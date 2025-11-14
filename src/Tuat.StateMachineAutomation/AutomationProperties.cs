namespace Tuat.StateMachineAutomation;

public class AutomationProperties
{
    public string? PreStartAction { get; set; }
    public string? PreScheduleAction { get; set; }
    public List<SubStateMachineParameter> SubStateMachineParameters { get; set; } = [];
    public List<State> States { get; set; } = [];
    public List<Transition> Transitions { get; set; } = [];
    public List<Information> Informations { get; set; } = [];
}
