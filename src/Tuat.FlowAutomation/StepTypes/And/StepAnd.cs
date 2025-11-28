using System.ComponentModel;

namespace Tuat.FlowAutomation.StepTypes.And;

[DisplayName("And")]
[Editor("Tuat.FlowAutomation.StepTypes.And.StepSettings", typeof(StepSettings))]
public class StepAnd: Step
{
    public override List<Blazor.Diagrams.Core.Models.PortAlignment> OutputPorts { get; set; } = [Blazor.Diagrams.Core.Models.PortAlignment.Right];

    public override List<Blazor.Diagrams.Core.Models.PortAlignment> InputPorts { get; set; } = [Blazor.Diagrams.Core.Models.PortAlignment.Left];

}
