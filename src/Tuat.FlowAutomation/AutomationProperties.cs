namespace Tuat.FlowAutomation;

public class AutomationProperties
{
    public string? PreStartAction { get; set; }

    public List<Step> Steps { get; set; } = [];
    public List<Transition> Transitions { get; set; } = [];
}
