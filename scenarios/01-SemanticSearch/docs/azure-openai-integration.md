# Azure OpenAI Integration

## Overview
The Products service integrates with Azure OpenAI to provide both chat completion and text embedding capabilities for semantic search functionality.

## Components Registered

### OpenAI Client Registration
```csharp
var azureOpenAiClientName = "openai";
builder.AddAzureOpenAIClient(azureOpenAiClientName);
```
- **Purpose**: Main Azure OpenAI client for accessing deployed models
- **Configuration**: Connection string via Aspire configuration or user secrets
- **Model Support**: GPT-4.1-mini for chat, text-embedding-ada-002 for embeddings

### Chat Client
```csharp
builder.Services.AddSingleton<ChatClient>(serviceProvider =>
{
    var logger = serviceProvider.GetService<ILogger<Program>>()!;
    logger.LogInformation($"Chat client configuration, modelId: {chatDeploymentName}");
    OpenAIClient client = serviceProvider.GetRequiredService<OpenAIClient>();
    return client.GetChatClient(chatDeploymentName);
});
```
- **Purpose**: Provides AI-powered chat responses for search result enhancement
- **Configuration**: Uses GPT-4.1-mini deployment
- **Usage**: Generates friendly, contextual responses for search results

### Embedding Client
```csharp
builder.Services.AddSingleton<EmbeddingClient>(serviceProvider =>
{
    var logger = serviceProvider.GetService<ILogger<Program>>()!;
    logger.LogInformation($"Embeddings client configuration, modelId: {embeddingsDeploymentName}");
    OpenAIClient client = serviceProvider.GetRequiredService<OpenAIClient>();
    return client.GetEmbeddingClient(embeddingsDeploymentName);
});
```
- **Purpose**: Converts text to vector embeddings for semantic search
- **Configuration**: Uses text-embedding-ada-002 deployment
- **Usage**: Creates vector representations of products and search queries

## Sample Usage

### Chat Completion
```csharp
var messages = new List<ChatMessage>
{
    new SystemChatMessage(_systemPrompt),
    new UserChatMessage(prompt)
};

var resultPrompt = await _chatClient.CompleteChatAsync(messages);
string response = resultPrompt.Value.Content[0].Text!;
```

### Text Embeddings
```csharp
var result = await _embeddingClient.GenerateEmbeddingAsync(productInfo);
float[] vectorEmbedding = result.Value.ToFloats();
```

## Configuration Notes
- **Local Development**: Configure user secrets with Azure OpenAI endpoint and key
- **Production**: Uses Aspire Azure OpenAI resource bindings
- **Model Versions**: GPT-4.1-mini (2025-04-14), text-embedding-ada-002 (v2)
- **Environment Variables**: AI_ChatDeploymentName, AI_embeddingsDeploymentName set by Aspire

## External Dependencies
- Azure OpenAI Service
- Valid Azure subscription with OpenAI resource
- Appropriate model deployments in the Azure OpenAI resource