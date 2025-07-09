# Azure OpenAI Integration

## Overview

The 01-SemanticSearch scenario integrates Azure OpenAI services to provide advanced AI capabilities for the eCommerce platform. This includes both chat functionality and vector embeddings for semantic search.

## Features

### Chat Client (GPT-4.1-mini)
- **Model**: gpt-4.1-mini
- **Purpose**: Provides conversational AI capabilities for product inquiries and recommendations
- **Configuration**: Registered as a singleton service in dependency injection

### Embedding Client (text-embedding-ada-002)
- **Model**: text-embedding-ada-002  
- **Purpose**: Generates vector embeddings for products to enable semantic search
- **Configuration**: Registered as a singleton service in dependency injection

## Code Registration

```csharp
// Azure OpenAI client registration
var azureOpenAiClientName = "openai";
var chatDeploymentName = "gpt-4.1-mini";
var embeddingsDeploymentName = "text-embedding-ada-002";
builder.AddAzureOpenAIClient(azureOpenAiClientName);

// Chat client configuration
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

// Embedding client configuration
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

## Dependencies

- **Package**: `Aspire.Azure.AI.OpenAI` Version 9.3.0-preview.1.25265.20
- **Namespace**: `OpenAI.Chat`, `OpenAI.Embeddings`

## Configuration Sources

### Local Development
Configure via user secrets in the Products project:
```bash
cd src/Products
dotnet user-secrets set "ConnectionStrings:openai" "Endpoint=https://<endpoint>.openai.azure.com/;Key=<key>;"
```

### Production (Azure)
When `builder.ExecutionContext.IsPublishMode` is true, the application uses Azure OpenAI resources provisioned through .NET Aspire:

```csharp
var aoai = builder.AddAzureOpenAI("openai");
var gpt41mini = aoai.AddDeployment(name: chatDeploymentName,
        modelName: "gpt-4.1-mini",
        modelVersion: "2025-04-14");
gpt41mini.Resource.SkuCapacity = 10;
gpt41mini.Resource.SkuName = "GlobalStandard";

var embeddingsDeployment = aoai.AddDeployment(name: embeddingsDeploymentName,
    modelName: "text-embedding-ada-002",
    modelVersion: "2");
```

## Environment Variables

The following environment variables are set by the AppHost for service communication:
- `AI_ChatDeploymentName`: "gpt-4.1-mini"
- `AI_embeddingsDeploymentName`: "text-embedding-ada-002"

## Usage

The Azure OpenAI clients are used by the MemoryContext service to:
1. Generate embeddings for product descriptions
2. Perform semantic similarity searches
3. Process natural language queries for product recommendations

## Error Handling

Both client registrations include comprehensive error handling with logging to ensure graceful degradation if Azure OpenAI services are unavailable.