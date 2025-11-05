using Tuat.Interfaces;
using Tuat.Models;

namespace Tuat.MessageHandlers;

public static class ClientServiceMessageHandler
{
    public static async Task Handle(Client client, IClientService clientService)
    {
        await clientService.Handle(client);
    }

    public static async Task Handle(VariableInfo variable, IClientService clientService)
    {
        await clientService.Handle([variable]);
    }

    public static async Task Handle(List<VariableInfo> variables, IClientService clientService)
    {
        await clientService.Handle(variables);
    }
}
