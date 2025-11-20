using Tuat.Interfaces;
using Tuat.Models;

namespace Tuat.AutomationHelper;

public abstract class BaseAutomationHandler<T> where T: new()
{
    private Automation _automation;
    protected IClientService _clientService;
    protected IDataService _dataService;
    protected IVariableService _variableService;
    protected IMessageBusService _messageBusService;

    private IScriptEngine? _engine;
    private Guid _instance;
    private IAutomationHandler? _subAutomationHandler = null;
    private bool _isRunningAsSubAutomation = false;

    private readonly object _lockEngineObject = new object();
    private bool _readyForTriggers = false;

    private static System.Text.Json.JsonSerializerOptions logJsonOptions = new System.Text.Json.JsonSerializerOptions
    {
        WriteIndented = true,
        IncludeFields = true
    };

    protected T _automationProperties = new();

    public Automation Automation => _automation;
    public string? ErrorMessage { get; protected set; }
    public event EventHandler<List<AutomationOutputVariable>>? OnAutomationFinished;
    public event EventHandler<LogEntry>? OnLogEntry;

    public AutomationRunningState RunningState
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                PublishAutomationStateInfo();
            }
        }
    } = AutomationRunningState.NotActive;

    public BaseAutomationHandler(Automation automation, IClientService clientService, IDataService dataService, IVariableService variableService, IMessageBusService messageBusService)
    {
        _automation = automation;
        _clientService = clientService;
        _dataService = dataService;
        _variableService = variableService;
        _messageBusService = messageBusService;

        _automationProperties = GetAutomationProperties(automation.Data);
    }

    public static T GetAutomationProperties(string? data)
    {
        if (!string.IsNullOrWhiteSpace(data))
        {
            return System.Text.Json.JsonSerializer.Deserialize<T>(data) ?? new();
        }
        return new();
    }

    public void SetAutomationFinished(List<AutomationOutputVariable> OutputValues)
    {
        if (Automation.IsSubAutomation && !_isRunningAsSubAutomation)
        {
            Stop();
        }
        else
        {
            RunningState = AutomationRunningState.Finished;
            OnAutomationFinished?.Invoke(this, OutputValues);
        }
    }

    public void StartSubAutomation(int automationId, List<AutomationInputVariable> InputValues)
    {
        DisposeSubAutomation();

        var automation = _dataService.GetAutomations().First(x => x.Id == automationId);
        var asm = (from a in AppDomain.CurrentDomain.GetAssemblies()
                   where a.GetTypes().Any(x => x.FullName == automation.AutomationType)
                   select a).FirstOrDefault();

        var type = asm!.GetTypes().First(x => x.FullName == automation.AutomationType);
        _subAutomationHandler = (IAutomationHandler?)Activator.CreateInstance(type, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance, null, new object[] { automation, _clientService, _dataService, _variableService, _messageBusService }, null);
        _subAutomationHandler!.OnAutomationFinished += OnSubAutomationFinished;
        _subAutomationHandler.OnLogEntry += OnSubAutomationLogEntry;
        _subAutomationHandler.Start(_instance, InputValues);
    }

    public bool IsSubAutomationRunning()
    {
        return _subAutomationHandler != null && _subAutomationHandler.RunningState == AutomationRunningState.Active;
    }

    public string? ExecuteScript(string script)
    {
        string? result = null;
        if (_engine != null)
        {
            var autoResetEvent = new AutoResetEvent(false);
            Task.Run(() =>
            {
                lock (_lockEngineObject)
                {
                    try
                    {
                        result = _engine.Evaluate(script)?.ToString();
                    }
                    catch (Exception e)
                    {
                        result = $"Error: {e.Message}";
                    }
                }
                autoResetEvent.Set();
            });
            autoResetEvent.WaitOne();
            autoResetEvent.Dispose();
            RequestTriggerProcess();
        }
        return result;
    }

    public List<IScriptEngine.ScriptVariable> GetScriptVariables()
    {
        List<IScriptEngine.ScriptVariable> result;
        lock (_lockEngineObject)
        {
            try
            {
                if (_engine == null)
                {
                    return [];
                }
                result = _engine.GetScriptVariables();
            }
            catch
            {
                result = [];
            }
        }
        return result;
    }

    public void Restart()
    {
        Stop();
        Start();
    }

    public void Start(Guid? instanceId = null, List<AutomationInputVariable>? InputValues = null)
    {
        ErrorMessage = null;
        _readyForTriggers = false;
        if (Automation.Enabled || Automation.IsSubAutomation)
        {
            lock (_lockEngineObject)
            {
                _isRunningAsSubAutomation = instanceId != null;
                var scriptEngine = GetScriptEngine(Automation.ScriptType);
                if (scriptEngine == null)
                {
                    DisposeEngines();
                    ErrorMessage = $"Error initializing flow: Unknown script type";
                    RunningState = AutomationRunningState.Error;
                }
                else
                {
                    _engine = scriptEngine;
                    try
                    {
                        _instance = instanceId ?? Guid.NewGuid();
                        RunStart(scriptEngine, _instance, InputValues);
                        RunningState = AutomationRunningState.Active;
                        RequestTriggerProcess();
                    }
                    catch (Exception e)
                    {
                        DisposeEngines();
                        ErrorMessage = $"Error initializing automation: {e.Message}";
                        RunningState = AutomationRunningState.Error;
                    }
                }
                _readyForTriggers = true;
            }
        }
        else
        {
            RunningState = AutomationRunningState.NotActive;
        }
    }

    public async Task AddLogAsync(string instanceId, object? logObject)
    {
        if (logObject != null)
        {
            var logEvent = new LogEntry
            {
                Timestamp = DateTime.UtcNow,
                AutomationId = Automation.Id,
            };

            if (logObject is string txt)
            {
                logEvent.Message = txt;
            }
            else
            {
                logEvent.Message = System.Text.Json.JsonSerializer.Serialize(logObject, logJsonOptions);
            }
            logEvent.Message = $"[{Automation.Name}]: {logEvent.Message}";

            if (_isRunningAsSubAutomation)
            {
                OnLogEntry?.Invoke(this, logEvent);
            }
            else
            {
                await _messageBusService.PublishAsync(logEvent);
            }
        }
    }

    public Task UpdateAsync(Automation automation)
    {
        Stop();
        _automation = automation;
        _automationProperties = GetAutomationProperties(automation.Data);
        if (!_automation.IsSubAutomation)
        {
            Start();
        }
        return Task.CompletedTask;
    }

    public Task Handle(List<VariableValueInfo> variableValueInfos)
    {
        RequestTriggerProcess();
        return Task.CompletedTask;
    }

    public Task Handle(List<VariableInfo> variableInfos)
    {
        RequestTriggerProcess();
        return Task.CompletedTask;
    }

    public static IScriptEngine? GetScriptEngine(string scriptType)
    {
        IScriptEngine? scriptEngine = null;

        var asm = (from a in AppDomain.CurrentDomain.GetAssemblies()
                   where a.GetTypes().Any(x => x.FullName == scriptType)
                   select a).FirstOrDefault();

        if (asm == null)
        {
            return null;
        }

        var type = asm.GetTypes().First(x => x.FullName == scriptType);
        scriptEngine = (IScriptEngine?)Activator.CreateInstance(type, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance, null, null, null);
        return scriptEngine;
    }

    public void TriggerProcess()
    {
        if (!_readyForTriggers) return;

        lock (_lockEngineObject)
        {
            if (RunningState == AutomationRunningState.Active && _engine != null)
            {
                try
                {
                    _subAutomationHandler?.TriggerProcess();
                    RunProcess(_engine);
                }
                catch (Exception e)
                {
                    ErrorMessage = $"Error in script{e.Message}";
                    RunningState = AutomationRunningState.Error;
                }
            }
        }

        _messageBusService.PublishAsync(new AutomationTriggered(Automation.Id));
    }

    public void StopSubAutomation()
    {
        DisposeSubAutomation();
    }


    protected virtual void RunStart(IScriptEngine scriptEngine, Guid instanceId, List<AutomationInputVariable>? InputValues = null)
    {
    }

    protected virtual void RunProcess(IScriptEngine scriptEngine)
    {
    }

    protected async Task AddLogAsync(object? logObject)
    {
        await AddLogAsync(_instance.ToString(), logObject);
    }

    private void PublishAutomationStateInfo()
    {
        var info = new AutomationStateInfo
        {
            AutomationId = Automation.Id,
            AutomationRunningState = RunningState
        };
        _messageBusService.PublishAsync(info);
    }

    private void OnSubAutomationLogEntry(object? sender, LogEntry logEntry)
    {
        logEntry.Message = $"[{Automation.Name}].{logEntry.Message}";
        logEntry.AutomationId = Automation.Id;
        if (_isRunningAsSubAutomation)
        {
            OnLogEntry?.Invoke(this, logEntry);
        }
        else
        {
            _messageBusService.PublishAsync(logEntry);
        }
    }

    private void OnSubAutomationFinished(object? sender, List<AutomationOutputVariable> outputValues)
    {
        if (_subAutomationHandler != null && _engine != null)
        {
            var subAutomationResultParameters = (from subAutomationParameter in _subAutomationHandler.Automation.SubAutomationParameters
                                                 from outputValue in outputValues.Where(x => x.Name == subAutomationParameter.Name).DefaultIfEmpty()
                                                 where subAutomationParameter.IsOutput
                                                 select new AutomationOutputVariable
                                                 {
                                                     Name = subAutomationParameter.ScriptVariableName,
                                                     Value = outputValue == null ? subAutomationParameter.DefaultValue : outputValues.FirstOrDefault(x => x.Name == subAutomationParameter.Name)?.Value
                                                 }).ToList();
            if (subAutomationResultParameters.Any())
            {
                _engine.HandleSubAutomationOutputVariables(subAutomationResultParameters);
            }
        }
        DisposeSubAutomation();
        RequestTriggerProcess();
    }

    private long _triggering = 0;
    private readonly object _triggerLock = new object();
    private void RequestTriggerProcess()
    {
        var count = Interlocked.Read(ref _triggering);
        if (count < 3)
        {
            Interlocked.Increment(ref _triggering);
            Task.Factory.StartNew(() =>
            {
                lock (_triggerLock)
                {
                    TriggerProcess();
                }
                Interlocked.Decrement(ref _triggering);
            });
        }
    }

    private void Stop()
    {
        _readyForTriggers = false;
        lock (_lockEngineObject)
        {
            RunningState = AutomationRunningState.NotActive;
            DisposeEngines();
        }
    }

    private void DisposeEngines()
    {
        DisposeSubAutomation();
        _engine?.Dispose();
        _engine = null;
    }

    private void DisposeSubAutomation()
    {
        if (_subAutomationHandler != null)
        {
            _subAutomationHandler.OnLogEntry -= OnSubAutomationLogEntry;
            _subAutomationHandler.OnAutomationFinished -= OnSubAutomationFinished;
            _subAutomationHandler.Dispose();
            _subAutomationHandler = null;
        }
    }

    public void Dispose()
    {
        Stop();
    }
}
