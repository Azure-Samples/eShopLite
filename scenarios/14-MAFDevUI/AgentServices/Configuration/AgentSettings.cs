namespace AgentServices.Configuration;

/// <summary>
/// DEMO: Configuration for the Agent Framework integration.
/// Supports both local (Azure OpenAI) mode and optional Azure AI Foundry workflows.
/// </summary>
public class AgentSettings
{
    /// <summary>
    /// Section name in configuration (appsettings.json or user secrets)
    /// </summary>
    public const string SectionName = "AgentSettings";

    /// <summary>
    /// When true, uses Azure AI Foundry persistent agents/workflows instead of local agent mode.
    /// Default is false for local demo mode.
    /// </summary>
    public bool UseFoundryAgents { get; set; } = false;

    /// <summary>
    /// Azure OpenAI endpoint URL. If not set, uses the microsoftfoundry connection string.
    /// </summary>
    public string? AzureOpenAIEndpoint { get; set; }

    /// <summary>
    /// Azure OpenAI chat model deployment name (e.g., "gpt-5-mini").
    /// </summary>
    public string ChatDeploymentName { get; set; } = "gpt-5-mini";

    /// <summary>
    /// Azure AI Foundry project endpoint (only used when UseFoundryAgents is true).
    /// </summary>
    public string? MicrosoftFoundryProjectEndpoint { get; set; }

    /// <summary>
    /// Azure AI Foundry model ID (only used when UseFoundryAgents is true).
    /// </summary>
    public string? FoundryModelId { get; set; }

    /// <summary>
    /// Connection string for Azure OpenAI / Microsoft Foundry.
    /// </summary>
    public string? MicrosoftFoundryConnectionString { get; set; }

    /// <summary>
    /// Whether agent features are enabled. If false, checkout runs in standard mode.
    /// </summary>
    public bool AgentsEnabled => !string.IsNullOrEmpty(MicrosoftFoundryConnectionString) ||
                                  !string.IsNullOrEmpty(AzureOpenAIEndpoint);
}
