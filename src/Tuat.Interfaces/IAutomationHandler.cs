namespace Tuat.Interfaces;

public interface IAutomationHandler : IDisposable
{
    Models.Automation Automation { get; }
    void RequestTriggerProcess();
    void TriggerProcess();
    void Start(Guid? instanceId = null, List<AutomationInputVariable>? InputValues = null, int? topAutomationId = null);
    void Restart();
    Task AddLogAsync(string instanceId, object? logObject);
    Task UpdateAsync(Models.Automation automation);
    Task Handle(List<VariableValueInfo> variableValueInfos);
    Task Handle(List<VariableInfo> variableInfos);
    string? ExecuteScript(string script);
    string? ErrorMessage { get; }
    AutomationRunningState RunningState { get; }
    List<IScriptEngine.ScriptVariable> GetScriptVariables();
    void SetAutomationFinished(List<AutomationOutputVariable> OutputValues);
    void StartSubAutomation(int automationId, List<AutomationInputVariable> InputValues);
    bool IsSubAutomationRunning();
    void StopSubAutomation();
    event EventHandler<List<AutomationOutputVariable>>? OnAutomationFinished;
    event EventHandler<LogEntry>? OnLogEntry;
}
