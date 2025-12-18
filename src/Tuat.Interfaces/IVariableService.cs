using Tuat.Models;

namespace Tuat.Interfaces;

public interface IVariableService: IDisposable
{
    Task StartAsync();
    List<VariableInfo> GetVariables();
    VariableInfo? GetVariable(int variableId);
    Task<bool> SetVariableValuesAsync(List<(int variableId, bool isMocking, string? mockingValue)> vaiableValues);
    Task<bool> SetVariableValuesAsync(List<(int variableId, string? value)> vaiableValues);
    Task DeleteVariableAsync(int variableId);
    Task<int?> CreateVariableAsync(string name, int clientId, int? automationId, bool persistant, string? data, List<string>? mockingOptions);
    Task Handle(Client client);
    Task Handle(Automation automation);
}
