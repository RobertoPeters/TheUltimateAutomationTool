namespace Tuat.FlowAutomation.StepTypes.FinishFlow;

public class SubFlowParameter
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public bool IsScriptVariable { get; set; }
    public string? ScriptVariable { get; set; }
}
