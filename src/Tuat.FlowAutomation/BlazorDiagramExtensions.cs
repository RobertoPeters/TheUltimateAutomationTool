using Blazor.Diagrams;
using Blazor.Diagrams.Core.Models;

namespace Tuat.FlowAutomation;

public static class BlazorDiagramExtensions
{
    public static (NodeModel? source, NodeModel? target) GetNodes(this BlazorDiagram diagram, Transition transition, AutomationProperties automationProperties)
    {
        var source = diagram.GetNode(automationProperties.Steps.FirstOrDefault(y => y.Id == transition.FromStepId));
        var target = diagram.GetNode(automationProperties.Steps.FirstOrDefault(y => y.Id == transition.ToStepId));
        return (source, target);
    }

    public static NodeModel? GetNode(this BlazorDiagram diagram, Step? step)
    {
        if (step == null)
        {
            return null;
        }

        return diagram.Nodes.FirstOrDefault(x =>
        {
            if (x is IStepNodeModel node)
            {
                return node.BaseStep.Id == step.Id;
            }
            return false;
        });
    }

    public static Step? GetStep(this NodeModel node, AutomationProperties automationProperties)
    {
        if (node is IStepNodeModel smNode)
        {
            return automationProperties.Steps.First(x => x.Id == smNode.BaseStep.Id);
        }
        return null;
    }

    public static Step? GetStep(this NodeModel node, List<Step> steps)
    {
        if (node is IStepNodeModel smNode)
        {
            return steps.First(x => x.Id == smNode.BaseStep.Id);
        }
        return null;
    }    
}

