using Tuat.Interfaces;
using Tuat.Models;

namespace Tuat.MessageHandlers;

public static class AutomationServiceMessageHandler
{
    public static async Task Handle(Automation automation, IAutomationService automationService)
    {
        await automationService.Handle(automation);
    }

    public static async Task Handle(VariableInfo variableInfo, IAutomationService automationService)
    {
        await automationService.Handle([variableInfo]);
    }

    public static async Task Handle(List<VariableInfo> variableInfos, IAutomationService automationService)
    {
        await automationService.Handle(variableInfos);
    }

    public static async Task Handle(List<VariableValueInfo> variableValueInfos, IAutomationService automationService)
    {
        await automationService.Handle(variableValueInfos);
    }
}
