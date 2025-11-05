using Tuat.Interfaces;
using Tuat.Models;

namespace Tuat.MessageHandlers;

public static class VariableServiceMessageHandler
{
    public static async Task Handle(Client client, IVariableService variableService)
    {
        await variableService.Handle(client);
    }
    public static async Task Handle(Automation automation, IVariableService variableService)
    {
        await variableService.Handle(automation);
    }
}