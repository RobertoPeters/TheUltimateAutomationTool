using Tuat.Models;

namespace Tuat.Interfaces;

public interface IDataService
{
    Task StartAsync();
    List<Client> GetClients();
    List<Variable> GetVariables();
    List<VariableValue> GetVariableValues();
    List<Automation> GetAutomations();
    List<Library> GetLibraries();
    List<AIProvider> GetAIProviders();
    List<AIConversation> GetAIConversations();
    Task AddOrUpdateClientAsync(Client client);
    Task AddOrUpdateVariableAsync(Variable variable);
    Task AddOrUpdateVariableValueAsync(VariableValue variableValue);
    Task DeleteVariableAsync(Variable variable);
    Task DeleteVariableValueAsync(VariableValue variableValue);
    Task DeleteClientAsync(Client client);
    Task AddOrUpdateAutomationAsync(Automation automation);
    Task DeleteAutomationAsync(Automation automation);
    Task AddOrUpdateLibraryAsync(Library library);
    Task DeleteLibraryAsync(Library library);
    Task AddOrUpdateAIProviderAsync(AIProvider aIProvider);
    Task DeleteAIProviderAsync(AIProvider aIProvider);
    Task AddOrUpdateAIConversationAsync(AIConversation aIConversation);
    Task DeleteAIConversationAsync(AIConversation aIConversation);
}
