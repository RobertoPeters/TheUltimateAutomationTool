using System.ComponentModel;
using Tuat.Interfaces;
using Tuat.Models;

namespace Tuat.StateMachineAutomation;

[DisplayName("State Machine")]
[Editor("Tuat.StateMachineAutomation.AutomationSettings", typeof(AutomationSettings))]
[Editor("Tuat.StateMachineAutomation.Editor", typeof(Editor))]
public class StateMachineHandler// : IAutomationHandler
{
    private Automation _automation;
    private readonly IClientService _clientService;
    private readonly IDataService _dataService;
    private readonly IVariableService _variableService;
    private readonly IMessageBusService _messageBusService;

    private AutomationProperties _automationProperties = new();

    public Automation Automation => _automation;

    private readonly object _lockEngineObject = new object();
    private List<StateMachineEngineInfo> _engines = [];
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

    public Task AddLogAsync(string instanceId, object? logObject)
    {
        throw new NotImplementedException();
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

    public void Start()
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
