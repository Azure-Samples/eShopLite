# Azure OpenAI Real-time Integration

## Overview
The Realtime Audio scenario integrates with Azure OpenAI to provide advanced AI capabilities including real-time audio conversations, chat completion, and text embeddings for enhanced customer interactions.

## AI Models Configuration

### GPT-4o Mini Chat Model
```csharp
var chatDeploymentName = "gpt-4o-mini";
.AddDeployment(new AzureOpenAIDeployment(chatDeploymentName,
"gpt-4o-mini",
"2024-07-18",
"GlobalStandard",
10))
```

**Purpose**: Provides chat completion capabilities for product recommendations and customer interactions.

**Configuration**: 
- Model: `gpt-4o-mini`
- Version: `2024-07-18`
- SKU: GlobalStandard with capacity of 10

### GPT-4o Mini Real-time Preview Model
```csharp
var reatimeDeploymentName = "gpt-4o-mini-realtime-preview";
.AddDeployment(new AzureOpenAIDeployment(reatimeDeploymentName,
"gpt-4o-mini-realtime-preview",
"2024-12-17",
"GlobalStandard",
1))
```

**Purpose**: Enables real-time audio conversations with customers for product discovery and recommendations.

**Configuration**:
- Model: `gpt-4o-mini-realtime-preview`
- Version: `2024-12-17`
- SKU: GlobalStandard with capacity of 1
- Supports bidirectional audio streaming

### Text Embeddings Model
```csharp
var embeddingsDeploymentName = "text-embedding-ada-002";
.AddDeployment(new AzureOpenAIDeployment(embeddingsDeploymentName,
"text-embedding-ada-002",
"2"))
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
    .WithEnvironment("AI_RealtimeDeploymentName", reatimeDeploymentName)
    .WithEnvironment("AI_embeddingsDeploymentName", embeddingsDeploymentName);
```

### Realtime Store Configuration
```csharp
storeRealtime.WithReference(appInsights)
    .WithReference(aoai)
    .WithEnvironment("AI_ChatDeploymentName", chatDeploymentName)
    .WithEnvironment("AI_RealtimeDeploymentName", reatimeDeploymentName)
    .WithEnvironment("AI_embeddingsDeploymentName", embeddingsDeploymentName);
```

**Environment Variables**:
- `AI_ChatDeploymentName`: Set to "gpt-4o-mini"
- `AI_RealtimeDeploymentName`: Set to "gpt-4o-mini-realtime-preview"
- `AI_embeddingsDeploymentName`: Set to "text-embedding-ada-002"

## Real-time Audio Features
- **Voice Conversations**: Natural voice interaction with AI assistant
- **Product Recommendations**: Audio-based product discovery
- **Real-time Responses**: Low-latency audio processing
- **Contextual Understanding**: AI maintains conversation context

## External Dependencies
- Azure OpenAI Service with real-time API access
- WebRTC for audio streaming
- Azure Application Insights for telemetry
- User secrets or environment variables for authentication