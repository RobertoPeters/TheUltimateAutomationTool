namespace Tuat.Interfaces;

public interface IAutomationHandler : IDisposable
{
    Models.Automation Automation { get; }
    void TriggerProcess();
    void Start();
    void Restart();
    Task AddLogAsync(string instanceId, object? logObject);
    Task UpdateAsync(Models.Automation automation);
    Task Handle(List<VariableValueInfo> variableValueInfos);
    Task Handle(List<VariableInfo> variableInfos);
    string? ExecuteScript(string script);
    string? ErrorMessage { get; }
    AutomationRunningState RunningState { get; }

}
