using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using Tuat.Models;

namespace Tuat.StateMachineAutomation;

public class StateMachineInformationNodeModel : NodeModel
{
    public StateMachineInformationNodeModel(Information information, Automation automation, Point? position = null) : base(position) 
    {
        Information = information;
        Automation = automation;
    }

    public Information Information { get; set; }
    public Automation Automation { get; set; }
}