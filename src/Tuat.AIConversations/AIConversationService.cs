using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Tuat.Interfaces;

namespace Tuat.AIConversations;

public class AIConversationService(IClientService _clientService, IDataService _dataService, IVariableService _variableService) : IAIConversationService
{
    public AIAgent CreateAIConversationAgent(int aIConversationId, 
        Type? scriptEngineType, 
        IAutomationHandler? automationHandler, 
        string? userCode, 
        string? systemCode, 
        List<object>? tools,
        string? conversationAgentInstructions,
        Func<AIAgent, FunctionInvocationContext, Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>>, CancellationToken, ValueTask<object?>>? functionCallMiddleware)
    {
        List<AIAgent> agents = [];
        var aiConversation = _dataService.GetAIConversations().First(x => x.Id == aIConversationId);
        var allAIProviders = Helpers.Generics.Generic.AIProviderTypeDisplayNames.ToList();
        var defaultProvider = _dataService.GetAIProviders().First(x => x.Id == aiConversation.DefaultAIProviderId);

        var allAIAgentSettings = Helpers.Generics.Generic.AgentSettings.Values.ToList();
        foreach (var aiAgentSetting in allAIAgentSettings)
        {
            if (aiAgentSetting.ScriptEngineType != null && scriptEngineType != aiAgentSetting.ScriptEngineType)
            {
                continue;
            }

            var agentInstructions = aiAgentSetting.GetInstructions(_clientService, _dataService, _variableService, automationHandler, systemCode: systemCode, userCode: userCode);

            if (agentInstructions == null)
            {
                continue;
            }

            if (!aiConversation.AIAgentSettingAIProviderId.TryGetValue(aiAgentSetting.Id, out var aIProviderId))
            {
                aIProviderId = defaultProvider.Id;
            }
            var aIProvider = _dataService.GetAIProviders().First(x => x.Id == aIProviderId);
            var aiProviderType = allAIProviders.First(x => x.TypeName == aIProvider.ProviderType).Type;
            var providerInstance = (IAIProvider)Activator.CreateInstance(aiProviderType)!;

            var agent = providerInstance.CreateAIAgent(aIProvider, new IAIProvider.AIAgentSettings()
            {
                Name = aiAgentSetting.Name,
                Description = aiAgentSetting.Description,
                Instrunctions = agentInstructions!,
                Tools = tools,
                FunctionCallMiddleware = functionCallMiddleware,
            });

            agents.Add(agent);
        }

        var conversationaIProvider = _dataService.GetAIProviders().First(x => x.Id == defaultProvider.Id);
        var conversationaiProviderType = allAIProviders.First(x => x.TypeName == conversationaIProvider.ProviderType).Type;
        var conversationproviderInstance = (IAIProvider)Activator.CreateInstance(conversationaiProviderType)!;

        var conversationAgent = conversationproviderInstance.CreateAIAgent(conversationaIProvider, new IAIProvider.AIAgentSettings()
        {
            Name = "Conversation leader",
            Description = "Leads the conversation between multiple agents.",
            Instrunctions = conversationAgentInstructions ?? "You are the conversation agent and use all other agents when needed. Do not try to answer anything yourself.",
            Agents = agents,
            Tools = tools,
            FunctionCallMiddleware = functionCallMiddleware,
        });

        return conversationAgent;
    }

    public Workflow CreateAIConversationWorkflow(int aIConversationId,
    Type? scriptEngineType,
    IAutomationHandler? automationHandler,
    string? userCode,
    string? systemCode,
    List<object>? tools,
    string? conversationAgentInstructions,
    Func<AIAgent, FunctionInvocationContext, Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>>, CancellationToken, ValueTask<object?>>? functionCallMiddleware)
    {
        List<AIAgent> agents = [];
        var aiConversation = _dataService.GetAIConversations().First(x => x.Id == aIConversationId);
        var allAIProviders = Helpers.Generics.Generic.AIProviderTypeDisplayNames.ToList();
        var defaultProvider = _dataService.GetAIProviders().First(x => x.Id == aiConversation.DefaultAIProviderId);

        var allAIAgentSettings = Helpers.Generics.Generic.AgentSettings.Values.ToList();
        foreach (var aiAgentSetting in allAIAgentSettings)
        {
            if (aiAgentSetting.ScriptEngineType != null && scriptEngineType != aiAgentSetting.ScriptEngineType)
            {
                continue;
            }

            var agentInstructions = aiAgentSetting.GetInstructions(_clientService, _dataService, _variableService, automationHandler, systemCode: systemCode, userCode: userCode);

            if (agentInstructions == null)
            {
                continue;
            }

            if (!aiConversation.AIAgentSettingAIProviderId.TryGetValue(aiAgentSetting.Id, out var aIProviderId))
            {
                aIProviderId = defaultProvider.Id;
            }
            var aIProvider = _dataService.GetAIProviders().First(x => x.Id == aIProviderId);
            var aiProviderType = allAIProviders.First(x => x.TypeName == aIProvider.ProviderType).Type;
            var providerInstance = (IAIProvider)Activator.CreateInstance(aiProviderType)!;

            var agent = providerInstance.CreateAIAgent(aIProvider, new IAIProvider.AIAgentSettings()
            {
                Name = aiAgentSetting.Name,
                Description = aiAgentSetting.Description,
                Instrunctions = agentInstructions!,
                Tools = tools,
                FunctionCallMiddleware = functionCallMiddleware,
            });

            agents.Add(agent);
        }

        var conversationaIProvider = _dataService.GetAIProviders().First(x => x.Id == defaultProvider.Id);
        var conversationaiProviderType = allAIProviders.First(x => x.TypeName == conversationaIProvider.ProviderType).Type;
        var conversationproviderInstance = (IAIProvider)Activator.CreateInstance(conversationaiProviderType)!;

        var conversationAgent = conversationproviderInstance.CreateAIAgent(conversationaIProvider, new IAIProvider.AIAgentSettings()
        {
            Name = "Conversation leader",
            Description = "Leads the conversation between multiple agents.",
            Instrunctions = conversationAgentInstructions ?? "You are the conversation agent and use all other agents when needed. Do not try to answer anything yourself.",
            Tools = tools,
            FunctionCallMiddleware = functionCallMiddleware,
        });

        var workflow = AgentWorkflowBuilder.CreateHandoffBuilderWith(conversationAgent)
        .WithHandoffs(conversationAgent, agents)
        .WithHandoffs(agents, conversationAgent)
        .Build();

        return workflow;
    }
}
