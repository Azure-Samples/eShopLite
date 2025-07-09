# Azure OpenAI Integration (Realtime Audio)

## Overview

The 03-RealtimeAudio scenario extends Azure OpenAI integration with real-time audio capabilities using the GPT-4o Realtime API. This enables natural voice conversations with AI for product inquiries and recommendations.

## Enhanced AI Models

### GPT-4o-mini
- **Model**: gpt-4o-mini
- **Purpose**: Standard chat operations and product recommendations
- **Version**: 2024-07-18
- **Capacity**: 10 units with GlobalStandard SKU

### GPT-4o Realtime Preview
- **Model**: gpt-4o-mini-realtime-preview  
- **Purpose**: Real-time audio conversations
- **Version**: 2024-12-17
- **Capacity**: 1 unit with GlobalStandard SKU

### Text Embeddings
- **Model**: text-embedding-ada-002
- **Purpose**: Semantic search and product matching
- **Version**: 2

## Realtime Audio Configuration

### Azure OpenAI Client Setup
```csharp
var azureOpenAiClientName = "openai";
var chatDeploymentName = "gpt-4o-mini";
var realtimeDeploymentName = "gpt-4o-mini-realtime-preview";

builder.AddAzureOpenAIClient(azureOpenAiClientName, settings =>
{
    settings.DisableMetrics = false;
    settings.DisableTracing = false;
    settings.Endpoint = new Uri(aoaiEndpoint);
});
```

### Realtime Conversation Client
```csharp
builder.Services.AddSingleton<RealtimeConversationClient>(serviceProvider =>
{
    var logger = serviceProvider.GetService<ILogger<Program>>()!;
    RealtimeConversationClient realtimeConversationClient = null;
    try
    {
        AzureOpenAIClient client = serviceProvider.GetRequiredService<AzureOpenAIClient>();
        realtimeConversationClient = client.GetRealtimeConversationClient(realtimeDeploymentName);
    }
    catch (Exception exc)
    {
        logger.LogError(exc, "Error creating realtime conversation client");
    }
    return realtimeConversationClient;
});
```

## Audio Processing Features

### Voice Input Handling
- **Speech Recognition**: Real-time speech-to-text conversion
- **Audio Streaming**: Continuous audio input processing
- **Noise Handling**: Background noise filtering and clarity optimization
- **Language Detection**: Automatic language identification

### Voice Output Generation
- **Text-to-Speech**: AI-generated natural speech responses
- **Voice Characteristics**: Configurable voice parameters
- **Audio Quality**: High-fidelity audio output
- **Response Timing**: Low-latency audio generation

## Product Context Integration

### ContosoProductContext
```csharp
builder.Services.AddSingleton(serviceProvider =>
{
    ProductService productService = serviceProvider.GetRequiredService<ProductService>();
    return new ContosoProductContext(productService);
});
```

### AI-Product Integration
- **Product Knowledge**: AI has access to live product catalog
- **Contextual Responses**: Responses include specific product information
- **Recommendation Engine**: Voice-based product suggestions
- **Inventory Awareness**: Real-time availability information

## Deployment Configuration

### Azure Resource Provisioning
```csharp
var aoai = builder.AddAzureOpenAI("openai")
    .AddDeployment(new AzureOpenAIDeployment(chatDeploymentName,
    "gpt-4o-mini",
    "2024-07-18",
    "GlobalStandard",
    10))
    .AddDeployment(new AzureOpenAIDeployment(realtimeDeploymentName,
    "gpt-4o-mini-realtime-preview",
    "2024-12-17",
    "GlobalStandard",
    1))
    .AddDeployment(new AzureOpenAIDeployment(embeddingsDeploymentName,
    "text-embedding-ada-002",
    "2"));
```

### Service Environment Variables
```csharp
products.WithReference(appInsights)
    .WithReference(aoai)
    .WithEnvironment("AI_ChatDeploymentName", chatDeploymentName)
    .WithEnvironment("AI_RealtimeDeploymentName", realtimeDeploymentName)
    .WithEnvironment("AI_embeddingsDeploymentName", embeddingsDeploymentName);

storeRealtime.WithReference(appInsights)
    .WithReference(aoai)
    .WithEnvironment("AI_ChatDeploymentName", chatDeploymentName)
    .WithEnvironment("AI_RealtimeDeploymentName", realtimeDeploymentName)
    .WithEnvironment("AI_embeddingsDeploymentName", embeddingsDeploymentName);
```

## Audio Conversation Flow

### Conversation Lifecycle
1. **Audio Input**: User speaks into microphone
2. **Speech Processing**: Real-time audio streaming to AI
3. **Intent Understanding**: AI processes speech and identifies product intent
4. **Product Lookup**: System queries product database for relevant items
5. **Response Generation**: AI formulates contextual response with product info
6. **Audio Output**: AI responds with synthesized speech

### Real-time Capabilities
- **Interruption Handling**: User can interrupt AI responses
- **Context Preservation**: Maintains conversation context across turns
- **Multi-turn Conversations**: Extended product discussions
- **Clarification Requests**: AI asks for clarification when needed

## Performance Optimization

### Audio Streaming
- **Low Latency**: Optimized for real-time conversation
- **Bandwidth Efficiency**: Compressed audio streaming
- **Quality Adaptation**: Automatic quality adjustment based on connection
- **Buffer Management**: Intelligent audio buffering strategies

### Connection Management
- **WebSocket Communication**: Persistent connections for audio streaming
- **Reconnection Logic**: Automatic reconnection on network issues
- **Graceful Degradation**: Fallback to text chat when audio unavailable
- **Error Recovery**: Robust error handling for audio failures

## User Experience Features

### Voice Interaction Patterns
- **Natural Conversations**: Human-like conversation flow
- **Product Inquiries**: "Tell me about wireless headphones"
- **Price Comparisons**: "What's the cheapest laptop you have?"
- **Feature Searches**: "Show me waterproof cameras"
- **Purchase Guidance**: "What's best for gaming?"

### Accessibility Features
- **Voice-First Design**: Fully accessible via voice commands
- **Visual Feedback**: Screen indicators for conversation state
- **Keyboard Navigation**: Alternative input methods
- **Audio Transcription**: Real-time conversation transcripts

## Security and Privacy

### Audio Data Handling
- **Ephemeral Processing**: Audio not permanently stored
- **Secure Transmission**: Encrypted audio streaming
- **Privacy Controls**: User control over audio permissions
- **Data Retention**: Minimal retention of conversation data

### Authentication Integration
- **User Context**: Personalized responses based on user profile
- **Session Management**: Secure conversation sessions
- **Permission Controls**: Granular audio access permissions