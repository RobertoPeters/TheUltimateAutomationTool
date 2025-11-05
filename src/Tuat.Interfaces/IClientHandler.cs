namespace Tuat.Interfaces;

public interface IClientHandler : IAsyncDisposable
{
    Models.Client Client { get; }
    Task StartAsync();
    Task UpdateAsync(Models.Client client);
    Task DeleteVariableInfoAsync(List<VariableInfo> variables);
    Task AddOrUpdateVariableInfoAsync(List<VariableInfo> variables);
    Task<bool> ExecuteAsync(int? variableId, string command, object? parameter1, object? parameter2, object? parameter3);

}
