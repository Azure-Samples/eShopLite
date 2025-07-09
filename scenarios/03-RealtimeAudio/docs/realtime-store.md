# Realtime Store (Audio Chat Interface)

## Overview

The RealtimeStore is an innovative Blazor Server application that provides real-time audio chat capabilities for product inquiries. It leverages GPT-4o Realtime API to enable natural voice conversations about products in the eCommerce catalog.

## Key Features

### Real-time Audio Processing
- **Voice Input**: Natural speech recognition for product queries
- **Voice Output**: AI-generated speech responses  
- **Low Latency**: Real-time conversation flow
- **Context Awareness**: Maintains conversation context across interactions

### Product Integration
- **Live Product Data**: Direct connection to Products API
- **Contextual Responses**: AI responses include specific product information
- **Search Integration**: Voice queries can trigger product searches
- **Recommendation Engine**: AI suggests relevant products based on conversation

## Technical Implementation

### Service Registration

```csharp
// Product service integration
builder.Services.AddSingleton<ProductService>();
builder.Services.AddHttpClient<ProductService>(
    static client => client.BaseAddress = new("https+http://products"));

// Azure OpenAI realtime client
var azureOpenAiClientName = "openai";
var chatDeploymentName = "gpt-4o-mini-realtime-preview";

builder.AddAzureOpenAIClient(azureOpenAiClientName, settings =>
{
    settings.DisableMetrics = false;
    settings.DisableTracing = false;
    settings.Endpoint = new Uri(aoaiEndpoint);
});

// Realtime conversation client
builder.Services.AddSingleton<RealtimeConversationClient>(serviceProvider =>
{
    var logger = serviceProvider.GetService<ILogger<Program>>()!;
    logger.LogInformation($"Realtime Chat client configuration, modelId: {chatDeploymentName}");
    RealtimeConversationClient realtimeConversationClient = null;
    try
    {
        AzureOpenAIClient client = serviceProvider.GetRequiredService<AzureOpenAIClient>();
        realtimeConversationClient = client.GetRealtimeConversationClient(chatDeploymentName);
    }
    catch (Exception exc)
    {
        logger.LogError(exc, "Error creating realtime conversation client");
    }
    return realtimeConversationClient;
});

// Product context for AI interactions
builder.Services.AddSingleton(serviceProvider =>
{
    ProductService productService = serviceProvider.GetRequiredService<ProductService>();
    return new ContosoProductContext(productService);
});
```

### Core Components

#### ContosoProductContext
Provides product-specific context for AI conversations:
- Integrates with ProductService API
- Maintains product knowledge for AI responses
- Enables product-aware conversation flows

#### Audio Chat Interface
- Interactive Blazor Server components for voice interaction
- Real-time audio streaming capabilities
- Visual feedback for conversation state

### Dependencies

#### NuGet Packages
- `Azure.AI.OpenAI` (2.1.0-beta.2) - Azure OpenAI with realtime capabilities
- `Microsoft.Extensions.AI` (9.1.0-preview.1.25064.3) - AI extensions
- `Aspire.Azure.AI.OpenAI` (9.0.0-preview.5.24551.3) - Aspire OpenAI integration

#### Project References
- `DataEntities` - Shared product data models
- `SearchEntities` - Search result models
- `eShopServiceDefaults` - Aspire service configuration

## Configuration

### Azure OpenAI Models Required
- **gpt-4o-mini-realtime-preview**: Real-time conversation model
- **gpt-4o-mini**: Standard chat model for fallback scenarios
- **text-embedding-ada-002**: Embedding model for semantic search

### Environment Variables
- `AI_ChatDeploymentName`: Chat model deployment name
- `AI_RealtimeDeploymentName`: Realtime model deployment name  
- `AI_embeddingsDeploymentName`: Embedding model deployment name

### Connection Configuration
```bash
# Local development user secrets
cd src/StoreRealtime
dotnet user-secrets set "ConnectionStrings:openai" "Endpoint=https://<endpoint>.openai.azure.com/;Key=<key>;"
```

## User Experience Features

### Voice Interaction Flow
1. **Audio Input**: User speaks product inquiry or question
2. **Real-time Processing**: AI processes speech and understands intent  
3. **Product Lookup**: System searches product database for relevant items
4. **AI Response**: Contextual response with product recommendations
5. **Audio Output**: AI responds with synthesized speech

### Conversation Capabilities
- Product search by description or features
- Price comparisons and recommendations
- Product availability inquiries
- General shopping assistance
- Context-aware follow-up questions

## Architecture Integration

### Service Communication
```
┌─────────────────┐    ┌─────────────────┐
│ RealtimeStore   │───▶│  Products API   │
│ (Audio UI)      │    │                 │
└─────────────────┘    └─────────────────┘
         │                       │
         ▼                       ▼
┌─────────────────┐    ┌─────────────────┐
│ GPT-4o Realtime │    │   SQL Server    │
│ Conversation    │    │   (Products)    │
└─────────────────┘    └─────────────────┘
```

### Performance Considerations
- Real-time audio requires low latency connections
- Optimized product data caching for quick AI responses
- Efficient conversation state management
- Audio streaming optimization for web browsers

## Error Handling
- Graceful fallback when realtime services are unavailable
- Audio device permission handling
- Network connectivity error recovery
- User-friendly error messages for audio issues