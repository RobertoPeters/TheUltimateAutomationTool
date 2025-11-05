using Tuat.Models;

namespace Tuat.Interfaces;

public interface IDataService
{
    Task StartAsync();
    List<Client> GetClients();
    List<Variable> GetVariables();
    List<VariableValue> GetVariableValues();
    List<Automation> GetAutomations();
    Task AddOrUpdateClientAsync(Client client);
    Task AddOrUpdateVariableAsync(Variable variable);
    Task AddOrUpdateVariableValueAsync(VariableValue variableValue);
    Task DeleteVariableAsync(Variable variable);
    Task DeleteVariableValueAsync(VariableValue variableValue);
    Task DeleteClientAsync(Client client);
    Task AddOrUpdateAutomationAsync(Automation automation);
    Task DeleteAutomationAsync(Automation automation);
}
