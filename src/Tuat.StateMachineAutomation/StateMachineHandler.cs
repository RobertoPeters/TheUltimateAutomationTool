using System.ComponentModel;
using Tuat.Interfaces;
using Tuat.Models;

namespace Tuat.StateMachineAutomation;

[DisplayName("State Machine")]
[Editor("Tuat.StateMachineAutomation.AutomationSettings", typeof(AutomationSettings))]
[Editor("Tuat.StateMachineAutomation.Editor", typeof(Editor))]
public class StateMachineHandler : IAutomationHandler
{
    private Automation _automation;
    private readonly IClientService _clientService;
    private readonly IDataService _dataService;
    private readonly IVariableService _variableService;
    private readonly IMessageBusService _messageBusService;

    private AutomationProperties _automationProperties = new();

    public Automation Automation => _automation;
    private IScriptEngine? _engine;
    private Guid _instance;
    private IAutomationHandler? _subAutomationHandler = null;
    private bool _isRunningAsSubAutomation = false;

    private readonly object _lockEngineObject = new object();
    private List<StateMachineEngineInfo> _engines = [];
    private bool _readyForTriggers = false;

    private static System.Text.Json.JsonSerializerOptions logJsonOptions = new System.Text.Json.JsonSerializerOptions
    {
        WriteIndented = true,
        IncludeFields = true
    };

    public string? ErrorMessage { get; private set; }
    public event EventHandler<List<AutomationOutputVariable>> OnAutomationFinished;
    public event EventHandler<LogEntry> OnLogEntry;

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

    public StateMachineHandler(Automation automation, IClientService clientService, IDataService dataService, IVariableService variableService, IMessageBusService messageBusService)
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

    public void SetAutomationFinished(List<AutomationOutputVariable> OutputValues)
    {
        RunningState = AutomationRunningState.Finished;
        OnAutomationFinished?.Invoke(this, OutputValues);
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

    public void StartSubAutomation(int automationId, List<AutomationInputVariable> InputValues)
    {
        DisposeSubAutomation();

        var automation = _dataService.GetAutomations().First(x => x.Id == automationId);
        var asm = (from a in AppDomain.CurrentDomain.GetAssemblies()
                   where a.GetTypes().Any(x => x.FullName == automation.AutomationType)
                   select a).FirstOrDefault();

        var type = asm.GetTypes().First(x => x.FullName == automation.AutomationType);
        _subAutomationHandler = (IAutomationHandler?)Activator.CreateInstance(type, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance, null, new object[] { automation, _clientService, _dataService, _variableService, _messageBusService }, null);
        _subAutomationHandler!.OnAutomationFinished += OnSubAutomationFinished;
        _subAutomationHandler.OnLogEntry += OnSubAutomationLogEntry;
        _subAutomationHandler.Start(_instance, InputValues);
    }

    public bool IsSubAutomationRunning()
    {
        return _subAutomationHandler != null && _subAutomationHandler.RunningState == AutomationRunningState.Active;
    }

    public Task AddLogAsync(string instanceId, object? logObject)
    {
        throw new NotImplementedException();
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

    public string? ExecuteScript(string script)
    {
        throw new NotImplementedException();
    }

    public List<IScriptEngine.ScriptVariable> GetScriptVariables()
    {
        return [];
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

    public void Restart()
    {
        Stop();
        Start();
    }

    public void Start(Guid? instanceId = null, List<AutomationInputVariable>? InputValues = null)
    {
        //todo
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
        //todo
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
}
