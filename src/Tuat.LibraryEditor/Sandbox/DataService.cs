using System.Collections.Concurrent;
using Tuat.Interfaces;
using Tuat.Models;

namespace Tuat.LibraryEditor.Sandbox;

internal class DataService(IDataService _realDataService) : IDataService
{
    private readonly ConcurrentDictionary<int, Variable> _variables = [];
    private readonly ConcurrentDictionary<int, VariableValue> _variableValues = [];
    private readonly ConcurrentDictionary<int, Automation> _automations = [];
    private readonly ConcurrentDictionary<int, Library> _libraries = [];

    public Task AddOrUpdateAIConversationAsync(AIConversation aIConversation)
    {
        throw new NotImplementedException();
    }

    public Task AddOrUpdateAIProviderAsync(AIProvider aIProvider)
    {
        throw new NotImplementedException();
    }

    public Task AddOrUpdateAutomationAsync(Automation automation)
    {
        if (automation.Id == 0)
        {
            if (_automations.Count == 0)
            {
                automation.Id = 1;
            }
            else
            {
                automation.Id = _automations.Values.Max(x => x.Id) + 1;
            }
        }
        _automations.AddOrUpdate(automation.Id, automation, (_, _) => automation);
        return Task.CompletedTask;
    }

    public Task AddOrUpdateClientAsync(Client client)
    {
        throw new NotImplementedException();
    }

    public Task AddOrUpdateLibraryAsync(Library library)
    {
        throw new NotImplementedException();
    }

    public Task AddOrUpdateVariableAsync(Variable variable)
    {
        if (variable.Id == 0)
        {
            if (_variables.Count == 0)
            {
                variable.Id = 1;
            }
            else
            {
                variable.Id = _variables.Values.Max(x => x.Id) + 1;
            }
        }
        _variables.AddOrUpdate(variable.Id, variable, (_, _) => variable);
        return Task.CompletedTask;
    }

    public Task AddOrUpdateVariableValueAsync(VariableValue variableValue)
    {
        if (variableValue.Id == 0)
        {
            if (_variableValues.Count == 0)
            {
                variableValue.Id = 1;
            }
            else
            {
                variableValue.Id = _variableValues.Values.Max(x => x.Id) + 1;
            }
        }
        _variableValues.AddOrUpdate(variableValue.Id, variableValue, (_, _) => variableValue);
        return Task.CompletedTask;
    }

    public Task DeleteAIConversationAsync(AIConversation aIConversation)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAIProviderAsync(AIProvider aIProvider)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAutomationAsync(Automation automation)
    {
        throw new NotImplementedException();
    }

    public Task DeleteClientAsync(Client client)
    {
        throw new NotImplementedException();
    }

    public Task DeleteLibraryAsync(Library library)
    {
        throw new NotImplementedException();
    }

    public Task DeleteVariableAsync(Variable variable)
    {
        throw new NotImplementedException();
    }

    public Task DeleteVariableValueAsync(VariableValue variableValue)
    {
        throw new NotImplementedException();
    }

    public List<AIConversation> GetAIConversations()
    {
        throw new NotImplementedException();
    }

    public List<AIProvider> GetAIProviders()
    {
        throw new NotImplementedException();
    }

    public List<Automation> GetAutomations()
    {
        return _automations.Values.ToList();
    }

    public List<Client> GetClients()
    {
        return _realDataService.GetClients();
    }

    public List<Library> GetLibraries()
    {
        return _realDataService.GetLibraries();
    }

    public List<Models.Variable> GetVariables()
    {
        return _variables.Values.ToList();
    }

    public List<VariableValue> GetVariableValues()
    {
        return _variableValues.Values.ToList();
    }

    public Task StartAsync()
    {
        return Task.CompletedTask;
    }
}
