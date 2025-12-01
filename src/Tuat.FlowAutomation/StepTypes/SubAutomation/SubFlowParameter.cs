namespace Tuat.FlowAutomation.StepTypes.SubAutomation;

public class SubFlowParameter
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? ScriptVariableName { get; set; }
}
