using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OllamaSharp;
using System.ComponentModel;
using System.Reflection;
using Tuat.Interfaces;
using Tuat.Models;

namespace Tuat.AIProviderOllama;

[DisplayName("Ollama")]
[Editor("Tuat.AIProviderOllama.ProviderSettings", typeof(ProviderSettings))]
public class OllamaProvider : IAIProvider
{
    public AIAgent CreateAIAgent(AIProvider provider, IAIProvider.AIAgentSettings settings)
    {
        var providerProperties = GetProviderProperties(provider.Settings);
        var client = new OllamaApiClient(providerProperties.OllamaApiUrl, providerProperties.Model);
        List<AITool>? tools = null;

        if (settings.Agents?.Any() == true || settings.Tools?.Any() == true)
        {
            tools = [];

            if (settings.Agents?.Any() == true)
            {
                foreach(var agentTool in settings.Agents)
                {
                    tools.Add(agentTool.AsAIFunction(new AIFunctionFactoryOptions
                    {
                        Name = agentTool.Name
                    }));
                }
            }

            if (settings.Tools?.Any() == true)
            {
                foreach(var tool in settings.Tools)
                {
                    MethodInfo[] methods = tool.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);
                    List<AITool> listOfTools = methods.Select(x => AIFunctionFactory.Create(x, tool)).Cast<AITool>().ToList();
                    tools.AddRange(listOfTools);
                }
            }
        }

        var builder = new ChatClientAgent(client,
                name: settings.Name,
                instructions: settings.Instrunctions,
                tools: tools)
            .AsBuilder();

        if (settings.FunctionCallMiddleware != null)
        {
            builder = builder.Use(settings.FunctionCallMiddleware);
        }

        var agent =  builder.Build();

        return agent;
    }

    public static ProviderProperties GetProviderProperties(string? data)
    {
        if (!string.IsNullOrWhiteSpace(data))
        {
            return System.Text.Json.JsonSerializer.Deserialize<ProviderProperties>(data) ?? new();
        }
        return new();
    }
}
