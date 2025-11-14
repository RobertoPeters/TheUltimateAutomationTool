using System.ComponentModel;
using Tuat.Interfaces;
using Tuat.Models;

namespace Tuat.StateMachineAutomation;

[DisplayName("State Machine")]
[Editor("Tuat.StateMachineAutomation.AutomationSettings", typeof(AutomationSettings))]
[Editor("Tuat.StateMachineAutomation.Editor", typeof(Editor))]
public class StateMachineHandler : IAutomationHandler
{
    public Automation Automation => throw new NotImplementedException();

    public string? ErrorMessage => throw new NotImplementedException();

    public AutomationRunningState RunningState => throw new NotImplementedException();

    public Task AddLogAsync(string instanceId, object? logObject)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public string? ExecuteScript(string script)
    {
        throw new NotImplementedException();
    }

    public List<IScriptEngine.ScriptVariable> GetScriptVariables()
    {
        throw new NotImplementedException();
    }

    public Task Handle(List<VariableValueInfo> variableValueInfos)
    {
        throw new NotImplementedException();
    }

    public Task Handle(List<VariableInfo> variableInfos)
    {
        throw new NotImplementedException();
    }

    public void Restart()
    {
        throw new NotImplementedException();
    }

    public void Start()
    {
        throw new NotImplementedException();
    }

    public void TriggerProcess()
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(Automation automation)
    {
        throw new NotImplementedException();
    }
}
