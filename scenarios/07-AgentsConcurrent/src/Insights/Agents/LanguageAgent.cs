namespace Insights.Agents;

/// <summary>
/// Defines the LanguageAgent using Microsoft Agent Framework (MAF).
/// The agent is created from an IChatClient via chatClient.CreateAIAgent().
/// </summary>
public static class LanguageAgent
{
    public const string AgentName = "LanguageAgent";

    public const string Instructions = """
        You are an expert in language detection. Given a string, detect the language and return its standard language code (e.g., en for English, es for Spanish, fr for French, etc.).
        The output should be in the format 'Language:<detected language>', in example: 'Language:en'
        """;
}
