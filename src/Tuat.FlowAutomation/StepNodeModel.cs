using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using Tuat.Models;

namespace Tuat.FlowAutomation;

public interface IStepNodeModel
{
    Step BaseStep { get; }
}

public class StepNodeModel<T> : NodeModel, IStepNodeModel where T : Step
{
    public StepNodeModel(T step, Automation automation, Point? position = null) : base(position) 
    {
        Step = step;
        Automation = automation;
    }

    public T Step { get; set; }
    public Step BaseStep => Step;
    public Automation Automation { get; set; }
}