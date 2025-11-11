using System.ComponentModel;
using Tuat.Interfaces;
using Tuat.Models;

namespace Tuat.FreeScriptAutomation;

[DisplayName("Free Script")]
[Editor("Tuat.FreeScriptAutomation.AutomationSettings", typeof(AutomationSettings))]
[Editor("Tuat.FreeScriptAutomation.Editor", typeof(Editor))]
public class FreeScriptHandler: IAutomationHandler
{
    public class AutomationProperties
    {
        public string Script { get; set; } = null!;
    }

    private sealed class ScriptEngineInfo
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Automation Automation { get; set; } = null!;
        public IScriptEngine Engine { get; set; } = null!;
    }

    private Automation _automation;
    private readonly IClientService _clientService;
    private readonly IDataService _dataService;
    private readonly IVariableService _variableService;
    private readonly IMessageBusService _messageBusService;

    private AutomationProperties _automationProperties = new();

    public Automation Automation => _automation;

    private readonly object _lockEngineObject = new object();
    private List<ScriptEngineInfo> _engines = [];
    private bool _readyForTriggers = false;

    private static System.Text.Json.JsonSerializerOptions logJsonOptions = new System.Text.Json.JsonSerializerOptions
    {
        WriteIndented = true,
        IncludeFields = true
    };

    public string? ErrorMessage { get; private set; }

    private AutomationRunningState _runningState = AutomationRunningState.NotActive;
    public AutomationRunningState RunningState
    {
        get => _runningState;
        set
        {
            if (_runningState != value)
            {
                _runningState = value;
                PublishAutomationStateInfo();
            }
        }
    }

    public FreeScriptHandler(Automation automation, IClientService clientService, IDataService dataService, IVariableService variableService, IMessageBusService messageBusService)
    {
        _automation = automation;
        _clientService = clientService;
        _dataService = dataService;
        _variableService = variableService;
        _messageBusService = messageBusService;

        _automationProperties = GetAutomationProperties(automation.Data);
    }

    public static AutomationProperties GetAutomationProperties(string? data)
    {
        if (!string.IsNullOrWhiteSpace(data))
        {
            return System.Text.Json.JsonSerializer.Deserialize<AutomationProperties>(data) ?? new();
        }
        return new();
    }

    public void PublishAutomationStateInfo()
    {
        var info = new AutomationStateInfo
        {
            AutomationId = Automation.Id,
            AutomationRunningState = RunningState
        };
        _messageBusService.PublishAsync(info);
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

    public void TriggerProcess()
    {
        if (!_readyForTriggers) return;

        lock (_lockEngineObject)
        {
            if (RunningState == AutomationRunningState.Active && _engines.Any())
            {
                try
                {
                    var index = 0;
                    while (index < _engines.Count)
                    {
                        _engines[index].Engine.CallVoidFunction("schedule", null);
                        index++;
                    }
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

    public void Start()
    {
        ErrorMessage = null;
        _readyForTriggers = false;
        if (Automation.Enabled || Automation.IsSubAutomation)
        {
            lock (_lockEngineObject)
            {
                var scriptEngine = GetScriptEngine(Automation.ScriptType);
                if (scriptEngine == null)
                {
                    DisposeEngines();
                    ErrorMessage = $"Error initializing flow: Unknown script type";
                    RunningState = AutomationRunningState.Error;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(_automationProperties.Script))
                    {
                        _automationProperties.Script = scriptEngine.GetDeclareFunction("schedule", false);
                    }

                    Guid id = Guid.NewGuid();
                    var engine = new ScriptEngineInfo()
                    {
                        Id = id,
                        Engine = scriptEngine,
                        Automation = Automation
                    };
                    _engines.Add(engine);

                    try
                    {
                        scriptEngine.Initialize(_clientService, _dataService, _variableService, this, id, _automationProperties.Script);
                        engine.Engine.Execute(_automationProperties.Script);
                        RunningState = AutomationRunningState.Active;
                        RequestTriggerProcess();
                    }
                    catch (Exception e)
                    {
                        DisposeEngines();
                        ErrorMessage = $"Error initializing flow: {e.Message}";
                        RunningState = AutomationRunningState.Error;
                    }
                }
            }
            _readyForTriggers = true;
        }
        else
        {
            RunningState = AutomationRunningState.NotActive;
        }
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

    public string? ExecuteScript(string script)
    {
        string? result = null;
        if (_engines.Any())
        {
            var autoResetEvent = new AutoResetEvent(false);
            Task.Run(() =>
            {
                lock (_lockEngineObject)
                {
                    try
                    {
                        result = _engines[0].Engine.Evaluate(script)?.ToString();
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
        }
        return result;

    }

    public void Restart()
    {
        Stop();
        Start();
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

    public async Task AddLogAsync(string instanceId, object? logObject)
    {
        if (logObject != null)
        {
            var indexOfEngine = _engines.FindIndex(_engines => _engines.Id.ToString() == instanceId);
            var prefix = $"[{string.Join("].[", _engines.Take(indexOfEngine + 1).Select(e => e.Automation.Name))}]";
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
            logEvent.Message = $"{prefix}: {logEvent.Message}";

            await _messageBusService.PublishAsync(logEvent);
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

    private void DisposeEngines()
    {
        foreach (var engine in _engines)
        {
            engine.Engine.Dispose();
        }
        _engines.Clear();
    }

    public void Dispose()
    {
        Stop();
    }
}
