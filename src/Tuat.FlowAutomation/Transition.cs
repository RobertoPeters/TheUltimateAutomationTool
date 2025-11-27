namespace Tuat.FlowAutomation;

public class Transition
{
    public Guid Id { get; set; }
    public string? UIData { get; set; }
    public Guid? FromStepId { get; set; }
    public Guid? ToStepId { get; set; }
    public Blazor.Diagrams.Core.Models.PortAlignment? FromStepPort { get; set; }
    public Blazor.Diagrams.Core.Models.PortAlignment? ToStepPort { get; set; }
}
