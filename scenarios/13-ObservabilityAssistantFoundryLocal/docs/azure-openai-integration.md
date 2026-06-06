# Azure OpenAI Integration

## Overview

The semantic search scenario integrates Azure OpenAI services to provide chat completion and text embedding capabilities for intelligent product discovery and customer assistance.

## Client Configuration

### OpenAI Client Registration

The Products service registers Azure OpenAI clients using Aspire's integration:

```csharp
// Register Azure OpenAI client
var azureOpenAiClientName = "openai";
builder.AddAzureOpenAIClient(azureOpenAiClientName);
```

### Chat Client Setup

The chat client is configured for conversational interactions:

```csharp
builder.Services.AddSingleton<ChatClient>(serviceProvider =>
{
    var logger = serviceProvider.GetService<ILogger<Program>>()!;
    logger.LogInformation($"Chat client configuration, modelId: {chatDeploymentName}");
    
    ChatClient chatClient = null;
    try
    {
        OpenAIClient client = serviceProvider.GetRequiredService<OpenAIClient>();
        chatClient = client.GetChatClient(chatDeploymentName);
    }
    catch (Exception exc)
    {
        logger.LogError(exc, "Error creating chat client");
    }
    return chatClient;
});
```

### Embedding Client Setup

The embedding client generates vector representations of text for semantic search:

```csharp
builder.Services.AddSingleton<EmbeddingClient>(serviceProvider =>
{
    var logger = serviceProvider.GetService<ILogger<Program>>()!;
    logger.LogInformation($"Embeddings client configuration, modelId: {embeddingsDeploymentName}");
    
    EmbeddingClient embeddingsClient = null;
    try
    {
        OpenAIClient client = serviceProvider.GetRequiredService<OpenAIClient>();
        embeddingsClient = client.GetEmbeddingClient(embeddingsDeploymentName);
    }
    catch (Exception exc)
    {
        logger.LogError(exc, "Error creating embeddings client");
    }
    return embeddingsClient;
});
```

## Model Configurations

### Chat Model (GPT-4.1-mini)
- **Model Name**: gpt-4.1-mini
- **Version**: 2025-04-14
- **Purpose**: Conversational AI for product assistance
- **SKU**: GlobalStandard with 10 capacity units

### Embedding Model (text-embedding-ada-002)
- **Model Name**: text-embedding-ada-002
- **Version**: 2
- **Purpose**: Generate vector embeddings for semantic search
- **Dimensions**: 1536-dimensional vectors

## Usage Patterns

### Chat Completion
Used in the MemoryContext for intelligent product recommendations:

```csharp
private string _systemPrompt = "You are a useful assistant. You always reply with a short and funny message. If you do not know an answer, you say 'I don't know that.' You only answer questions related to outdoor camping products.";
```

### Text Embeddings
Used to convert product descriptions and search queries into vector representations:

```csharp
// Generate embeddings for products during initialization
var embeddings = await _embeddingClient.GenerateEmbeddingAsync(productDescription);
```

## Configuration Sources

### Development (Local)
- **User Secrets**: Connection strings with Azure OpenAI endpoint and API key
- **Aspire**: Service discovery and dependency injection

### Production (Azure)
- **Managed Identity**: Secure authentication to Azure OpenAI
- **Aspire**: Automatic deployment and configuration of Azure OpenAI resources
- **Environment Variables**: Model deployment names and configuration

## Security

### Authentication
- **Development**: API key authentication via user secrets
- **Production**: Azure Managed Identity for secure, credential-free access

### Best Practices
- Connection strings stored in secure configuration (user secrets, Azure Key Vault)
- Error handling and logging for failed client initialization
- Singleton pattern for efficient resource usage

## Telemetry

The integration includes OpenTelemetry support:

```csharp
AppContext.SetSwitch("OpenAI.Experimental.EnableOpenTelemetry", true);
```

This enables:
- Request/response logging
- Performance metrics
- Error tracking
- Integration with Azure Application Insights

## Dependencies

### NuGet Packages
- `Aspire.Azure.AI.OpenAI` (9.3.0-preview.1.25265.20)
- Standard OpenAI .NET SDK packages

### External Services
- Azure OpenAI Service endpoint
- Azure Application Insights (production)
- Azure Managed Identity (production)