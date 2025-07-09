# Azure OpenAI Integration

## Overview
The Semantic Search scenario integrates with Azure OpenAI to provide advanced AI capabilities including chat completion and text embeddings for semantic search functionality.

## AI Models Configuration

### GPT-4.1 Mini Chat Model
```csharp
var chatDeploymentName = "gpt-41-mini";
var gpt41mini = aoai.AddDeployment(name: chatDeploymentName,
        modelName: "gpt-4.1-mini",
        modelVersion: "2025-04-14");
gpt41mini.Resource.SkuCapacity = 10;
gpt41mini.Resource.SkuName = "GlobalStandard";
```

**Purpose**: Provides chat completion capabilities for product recommendations and customer interactions.

**Configuration**: 
- Model: `gpt-4.1-mini`
- Version: `2025-04-14`
- SKU: GlobalStandard with capacity of 10

### Text Embeddings Model
```csharp
var embeddingsDeploymentName = "text-embedding-ada-002";
var embeddingsDeployment = aoai.AddDeployment(name: embeddingsDeploymentName,
    modelName: "text-embedding-ada-002",
    modelVersion: "2");
```

**Purpose**: Generates vector embeddings for products and search queries to enable semantic search.

**Configuration**:
- Model: `text-embedding-ada-002`
- Version: `2`
- Used for vectorizing product descriptions and search terms

## Service Integration

### Products Service Configuration
```csharp
products.WithReference(appInsights)
    .WithReference(aoai)
    .WithEnvironment("AI_ChatDeploymentName", chatDeploymentName)
    .WithEnvironment("AI_embeddingsDeploymentName", embeddingsDeploymentName);
```

**Environment Variables**:
- `AI_ChatDeploymentName`: Set to "gpt-41-mini"
- `AI_embeddingsDeploymentName`: Set to "text-embedding-ada-002"

## External Dependencies
- Azure OpenAI Service endpoint and API key
- Azure Application Insights for telemetry
- User secrets or environment variables for authentication