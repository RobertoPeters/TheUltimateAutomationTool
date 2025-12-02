namespace Tuat.FlowAutomation.StepTypes.SubAutomation;

public class SubFlowParameter
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public bool IsScriptVariable { get; set; }
    public string? ScriptVariable { get; set; }
}
