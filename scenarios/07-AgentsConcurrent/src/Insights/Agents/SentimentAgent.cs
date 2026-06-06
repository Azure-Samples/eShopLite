namespace Insights.Agents;

/// <summary>
/// Defines the SentimentAgent using Microsoft Agent Framework (MAF).
/// The agent is created from an IChatClient via chatClient.CreateAIAgent().
/// </summary>
public static class SentimentAgent
{
    public const string AgentName = "SentimentAgent";

    public const string Instructions = """
        You are an expert in sentiment analysis. Given a string, evaluate its sentiment and return one of the following values: positive, neutral, or negative. 
        The output should be in the format 'Sentiment:<detected sentiment>', in example: 'Sentiment:positive'
        """;
}
