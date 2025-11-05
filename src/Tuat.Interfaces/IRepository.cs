using Tuat.Models;
namespace Tuat.Interfaces;

public interface IRepository
{
    Task SetupAsync();
    Task<List<Client>> GetClientsAsync();
    Task<List<Variable>> GetVariablesAsync();
    Task<List<Automation>> GetAutomationsAsync();
    Task<List<VariableValue>> GetVariableValuesAsync();
    Task AddClientAsync(Client client);
    Task AddVariableAsync(Variable variable);
    Task AddAutomationAsync(Automation automation);
    Task AddVariableValueAsync(VariableValue variableValue);
    Task UpdateClientAsync(Client client);
    Task UpdateVariableAsync(Variable variable);
    Task UpdateAutomationAsync(Automation automation);
    Task UpdateVariableValueAsync(VariableValue variableValue);
    Task DeleteClientAsync(Client client);
    Task DeleteVariableAsync(Variable variable);
    Task DeleteAutomationAsync(Automation automation);
    Task DeleteVariableValueAsync(VariableValue variableValue);
    Task DeleteVariablesAsync(List<int> ids);
    Task DeleteVariableValuesAsync(List<int> ids);
}
