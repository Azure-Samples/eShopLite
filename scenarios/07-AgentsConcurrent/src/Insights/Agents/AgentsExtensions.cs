using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

namespace Insights.Agents;

public static class AgentsExtensions
{
    public static void AddAgents(this IServiceCollection services)
    {
        // Register SentimentAgent as a keyed MAF AIAgent
        services.AddKeyedSingleton<AIAgent>(SentimentAgent.AgentName, (sp, _) =>
        {
            var chatClient = sp.GetRequiredService<IChatClient>();
            return chatClient.CreateAIAgent(
                name: SentimentAgent.AgentName,
                instructions: SentimentAgent.Instructions);
        });

        // Register LanguageAgent as a keyed MAF AIAgent
        services.AddKeyedSingleton<AIAgent>(LanguageAgent.AgentName, (sp, _) =>
        {
            var chatClient = sp.GetRequiredService<IChatClient>();
            return chatClient.CreateAIAgent(
                name: LanguageAgent.AgentName,
                instructions: LanguageAgent.Instructions);
        });

        // Register the insights generator that uses the agents concurrently
        services.AddSingleton<Generator>();
    }
}
