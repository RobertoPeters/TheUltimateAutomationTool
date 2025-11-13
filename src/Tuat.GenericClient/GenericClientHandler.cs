using System.ComponentModel;
using Tuat.Interfaces;
using Tuat.Models;

namespace Tuat.GenericClient;

[DisplayName("Generic")]
[Editor("Tuat.GenericClient.ClientSettings", typeof(ClientSettings)) ]
#pragma warning disable CS9113 // Parameter is unread.
public class GenericClientHandler(Client _client, IVariableService _variableService, IMessageBusService _messageBusService) : IClientHandler
#pragma warning restore CS9113 // Parameter is unread.
{
    public Client Client => _client;

    public List<(string methodName, bool isAutomationVariable, bool persistant, string description, string example)> CreateVariableOnClientMethods() => 
        [
            ("CreateVariable", true, false, "Create a non persistant automation variable with data as default value. Return value is id of variable", "CreateVariable(\"testVar\")"),
        ];

    public List<(string methodName, string command, string description, string example)> CreateExecuteOnClientMethods() => [];
    public Task AddOrUpdateVariableInfoAsync(List<VariableInfo> variables)
    {
        return Task.CompletedTask;
    }

    public Task DeleteVariableInfoAsync(List<VariableInfo> variables)
    {
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public Task StartAsync()
    {
        return Task.CompletedTask;
    }

    public async Task UpdateAsync(Client client)
    {
        _client = client;
        await StartAsync();
    }

    public Task<bool> ExecuteAsync(int? variableId, string command, object? parameter1, object? parameter2, object? parameter3)
    {
        return Task.FromResult(false);
    }

}

