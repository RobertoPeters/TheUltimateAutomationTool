using System.Collections.Concurrent;
using System.Collections.Generic;
using Tuat.Interfaces;
using Tuat.Models;

namespace Tuat.Data;

public class DataService(IRepository _dataRepository, IMessageBusService _messageBusService): IDataService
{
    private readonly ConcurrentDictionary<int, Client> _clients = [];
    private readonly ConcurrentDictionary<int, Variable> _variables = [];
    private readonly ConcurrentDictionary<int, VariableValue> _variableValues = [];
    private readonly ConcurrentDictionary<int, Automation> _automations = [];
    private readonly ConcurrentDictionary<int, Library> _libraries = [];
    private readonly ConcurrentDictionary<int, AIProvider> _aiProviders = [];
    private readonly ConcurrentDictionary<int, AIConversation> _aiConversations = [];

    private int _lastUsedNonPersistentVariableValueId = int.MaxValue;

    public async Task StartAsync()
    {
        await _dataRepository.SetupAsync();

        var clients = await _dataRepository.GetClientsAsync();
        foreach (var client in clients)
        {
            _clients.TryAdd(client.Id, client);
        }

        var variables = await _dataRepository.GetVariablesAsync();
        foreach (var variable in variables)
        {
            _variables.TryAdd(variable.Id, variable);
        }

        var variableValues = await _dataRepository.GetVariableValuesAsync();
        foreach (var variableValue in variableValues)
        {
            if (_variables.ContainsKey(variableValue.VariableId))
            {
                _variableValues.TryAdd(variableValue.Id, variableValue);
            }
            else
            {
                await _dataRepository.DeleteVariableValueAsync(variableValue);
            }
        }

        var automations = await _dataRepository.GetAutomationsAsync();
        foreach (var automation in automations)
        {
            _automations.TryAdd(automation.Id, automation);
        }

        var libraries = await _dataRepository.GetLibrariesAsync();
        foreach (var library in libraries)
        {
            _libraries.TryAdd(library.Id, library);
        }

        var aiproviders = await _dataRepository.GetAIProvidersAsync();
        foreach (var aiprovider in aiproviders)
        {
            _aiProviders.TryAdd(aiprovider.Id, aiprovider);
        }

        var aiconversations = await _dataRepository.GetAIConversationsAsync();
        foreach (var aiconversation in aiconversations)
        {
            _aiConversations.TryAdd(aiconversation.Id, aiconversation);
        }
    }

    public List<Client> GetClients()
    {
        return _clients.Values.ToList();
    }

    public List<Variable> GetVariables()
    {
        return _variables.Values.ToList();
    }

    public List<VariableValue> GetVariableValues()
    {
        return _variableValues.Values.ToList();
    }

    public List<Automation> GetAutomations()
    {
        return _automations.Values.ToList();
    }

    public List<Library> GetLibraries()
    {
        return _libraries.Values.ToList();
    }

    public List<AIProvider> GetAIProviders()
    {
        return _aiProviders.Values.ToList();
    }

    public List<AIConversation> GetAIConversations()
    {
        return _aiConversations.Values.ToList();
    }

    public async Task AddOrUpdateClientAsync(Client client)
    {
        if (client.Id == 0)
        {
            await _dataRepository.AddClientAsync(client);
        }
        else
        {
            await _dataRepository.UpdateClientAsync(client);
        }
        _clients.AddOrUpdate(client.Id, client, (_, _) => client);
        await _messageBusService.PublishAsync(client);
    }

    public async Task AddOrUpdateVariableAsync(Variable variable)
    {
        if (variable.Id == 0)
        {
            await _dataRepository.AddVariableAsync(variable);
        }
        else
        {
            await _dataRepository.UpdateVariableAsync(variable);
        }
        _variables.AddOrUpdate(variable.Id, variable, (_, _) => variable);
        await _messageBusService.PublishAsync(variable);
    }

    public async Task AddOrUpdateVariableValueAsync(VariableValue variableValue)
    {
        if (variableValue.Id == 0)
        {
            if (_variables.TryGetValue(variableValue.VariableId, out var variable))
            {
                if (variable.Persistant)
                {
                    await _dataRepository.AddVariableValueAsync(variableValue);
                }
                else
                {
                    var newId = Interlocked.Decrement(ref _lastUsedNonPersistentVariableValueId);
                    variableValue.Id = newId;
                }
            }
        }
        else if (_variables.TryGetValue(variableValue.Id, out var variable) && variable.Persistant)
        {
            await _dataRepository.UpdateVariableValueAsync(variableValue);
        }
        _variableValues.AddOrUpdate(variableValue.Id, variableValue, (_, _) => variableValue);
        await _messageBusService.PublishAsync(variableValue);
    }

    public async Task DeleteVariableAsync(Variable variable)
    {
        if (_variables.TryRemove(Math.Abs(variable.Id), out var orgVariable))
        {
            _variableValues.TryRemove(orgVariable.Id, out var variableValue);

            if (variableValue != null)
            {
                await _dataRepository.DeleteVariableValueAsync(variableValue);
            }
            await _dataRepository.DeleteVariableAsync(orgVariable);
            orgVariable.Id = -Math.Abs(orgVariable.Id);
            await _messageBusService.PublishAsync(orgVariable);
        }
    }

    public async Task DeleteVariableValueAsync(VariableValue variableValue)
    {
        if (_variableValues.TryRemove(Math.Abs(variableValue.Id), out var orgVariableValue))
        {
            await _dataRepository.DeleteVariableValueAsync(variableValue);
            orgVariableValue.Id = -Math.Abs(orgVariableValue.Id);
            await _messageBusService.PublishAsync(orgVariableValue);
        }
    }

    public async Task DeleteClientAsync(Client client)
    {
        if (_clients.TryRemove(Math.Abs(client.Id), out var orgClient))
        {
            var variableIds = _variables.Values.Where(v => v.ClientId == orgClient.Id)
            .Select(x => x.Id)
            .ToList();

            var variableValueIds = _variableValues.Values.Where(v => variableIds.Contains(v.VariableId))
                .Select(x => x.Id)
                .ToList();

            await _dataRepository.DeleteVariableValuesAsync(variableValueIds);
            await _dataRepository.DeleteVariablesAsync(variableIds);
            await _dataRepository.DeleteClientAsync(orgClient);
            foreach (var variableValue in _variableValues.Values.Where(v => variableValueIds.Contains(v.Id)).ToList())
            {
                _variableValues.TryRemove(variableValue.Id, out _);
            }
            foreach (var variable in _variables.Values.Where(v => variableIds.Contains(v.Id)).ToList())
            {
                _variables.TryRemove(variable.Id, out _);
            }
            orgClient.Id = -Math.Abs(orgClient.Id);
            await _messageBusService.PublishAsync(orgClient);
        }
    }

    public async Task AddOrUpdateAutomationAsync(Automation automation)
    {
        if (automation.Id == 0)
        {
            await _dataRepository.AddAutomationAsync(automation);
        }
        else
        {
            await _dataRepository.UpdateAutomationAsync(automation);
        }
        _automations.AddOrUpdate(automation.Id, automation, (_, _) => automation);
        await _messageBusService.PublishAsync(automation);
    }

    public async Task AddOrUpdateLibraryAsync(Library library)
    {
        if (library.Id == 0)
        {
            await _dataRepository.AddLibraryAsync(library);
        }
        else
        {
            await _dataRepository.UpdateLibraryAsync(library);
        }
        _libraries.AddOrUpdate(library.Id, library, (_, _) => library);
        await _messageBusService.PublishAsync(library);
    }

    public async Task AddOrUpdateAIProviderAsync(AIProvider aIProvider)
    {
        if (aIProvider.Id == 0)
        {
            await _dataRepository.AddAIProviderAsync(aIProvider);
        }
        else
        {
            await _dataRepository.UpdateAIProviderAsync(aIProvider);
        }
        _aiProviders.AddOrUpdate(aIProvider.Id, aIProvider, (_, _) => aIProvider);
        await _messageBusService.PublishAsync(aIProvider);
    }

    public async Task AddOrUpdateAIConversationAsync(AIConversation aIConversation)
    {
        if (aIConversation.Id == 0)
        {
            await _dataRepository.AddAIConversationAsync(aIConversation);
        }
        else
        {
            await _dataRepository.UpdateAIConversationAsync(aIConversation);
        }
        _aiConversations.AddOrUpdate(aIConversation.Id, aIConversation, (_, _) => aIConversation);
        await _messageBusService.PublishAsync(aIConversation);
    }

    public async Task DeleteAutomationAsync(Automation automation)
    {
        if (_automations.TryRemove(Math.Abs(automation.Id), out var orgAutomation))
        {
            var variableIds = _variables.Values.Where(v => v.AutomationId == orgAutomation.Id)
                .Select(x => x.Id)
                .ToList();

            var variableValueIds = _variableValues.Values.Where(v => variableIds.Contains(v.VariableId))
                .Select(x => x.Id)
                .ToList();

            await _dataRepository.DeleteVariableValuesAsync(variableValueIds);
            await _dataRepository.DeleteVariablesAsync(variableIds);
            await _dataRepository.DeleteAutomationAsync(orgAutomation);
            foreach (var variableValue in _variableValues.Values.Where(v => variableValueIds.Contains(v.Id)).ToList())
            {
                _variableValues.TryRemove(variableValue.Id, out _);
            }
            foreach (var variable in _variables.Values.Where(v => variableIds.Contains(v.Id)).ToList())
            {
                _variables.TryRemove(variable.Id, out _);
            }
            orgAutomation.Id = -Math.Abs(orgAutomation.Id);
            await _messageBusService.PublishAsync(orgAutomation);
        }
    }

    public async Task DeleteLibraryAsync(Library library)
    {
        if (_libraries.TryRemove(Math.Abs(library.Id), out var orgLibrary))
        {
            await _dataRepository.DeleteLibraryAsync(library);
            orgLibrary.Id = -Math.Abs(orgLibrary.Id);
            await _messageBusService.PublishAsync(orgLibrary);
        }
    }

    public async Task DeleteAIProviderAsync(AIProvider aIProvider)
    {
        if (_aiProviders.TryRemove(Math.Abs(aIProvider.Id), out var orgAIProvider))
        {
            await _dataRepository.DeleteAIProviderAsync(aIProvider);
            orgAIProvider.Id = -Math.Abs(orgAIProvider.Id);
            await _messageBusService.PublishAsync(orgAIProvider);
        }
    }

    public async Task DeleteAIConversationAsync(AIConversation aIConversation)
    {
        if (_aiConversations.TryRemove(Math.Abs(aIConversation.Id), out var orgAIConversation))
        {
            await _dataRepository.DeleteAIConversationAsync(aIConversation);
            orgAIConversation.Id = -Math.Abs(orgAIConversation.Id);
            await _messageBusService.PublishAsync(orgAIConversation);
        }
    }

}

