using Tuat.Models;

namespace Tuat.Interfaces;

public interface IClientService: IAsyncDisposable
{
    Task StartAsync();
    List<IClientHandler> GetClients();
    List<T> GetClients<T>() where T : IClientHandler;
    Task Handle(List<VariableInfo> variables);
    Task Handle(Client client);
    Task<bool> ExecuteAsync(int clientId, int? variableId, string command, object? parameter1, object? parameter2, object? parameter3);
}
