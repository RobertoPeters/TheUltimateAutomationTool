using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace Tuat.Interfaces;

public interface IAIConversationService
{
    AIAgent CreateAIConversationAgent(int aIConversationId,
        Type? scriptEngineType,
        IAutomationHandler? automationHandler,
        string? userCode,
        string? systemCode,
        List<object>? tools,
        string? conversationAgentInstructions,
        Func<AIAgent, FunctionInvocationContext, Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>>, CancellationToken, ValueTask<object?>>? functionCallMiddleware);

    Workflow CreateAIConversationWorkflow(int aIConversationId,
        Type? scriptEngineType,
        IAutomationHandler? automationHandler,
        string? userCode,
        string? systemCode,
        List<object>? tools,
        string? conversationAgentInstructions,
        Func<AIAgent, FunctionInvocationContext, Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>>, CancellationToken, ValueTask<object?>>? functionCallMiddleware);
}
