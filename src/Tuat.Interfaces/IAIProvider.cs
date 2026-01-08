using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Tuat.Models;

namespace Tuat.Interfaces;

public interface IAIProvider
{
    public class AIAgentSettings
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string Instrunctions { get; set; }
        public List<AIAgent>? Agents { get; set; }
        public List<object>? Tools { get; set; }
        public Func<AIAgent, FunctionInvocationContext, Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>>, CancellationToken, ValueTask<object?>>? FunctionCallMiddleware { get; set; }
    }

    AIAgent CreateAIAgent(AIProvider provider, AIAgentSettings settings);
}
