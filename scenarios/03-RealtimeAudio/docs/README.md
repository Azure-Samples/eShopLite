# eShopLite - Realtime Audio Documentation

This documentation provides detailed technical information about the eShopLite Realtime Audio scenario, including real-time conversation capabilities, audio processing, and advanced AI integrations.

## Overview

The 03-RealtimeAudio scenario extends the basic eCommerce platform with cutting-edge real-time audio conversation capabilities using OpenAI's GPT-4o Realtime Audio API. This scenario demonstrates how to integrate voice-based customer assistance into a modern web application.

## Features Documentation

- [Aspire Orchestration](./aspire-orchestration.md) - Aspire service composition with realtime services
- [Realtime Audio Integration](./realtime-audio-integration.md) - GPT-4o realtime conversation setup
- [Azure OpenAI Integration](./azure-openai-integration.md) - Multi-model OpenAI configuration
- [Store Realtime Service](./store-realtime-service.md) - Blazor Server app with audio capabilities
- [Product Context Management](./product-context-management.md) - Real-time product information retrieval
- [Conversation Management](./conversation-management.md) - Audio session handling and state management

## Architecture

```
       ┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
       │   Store (UI)    │───▶│  Products API   │───▶│   SQL Server    │
       └─────────────────┘    └─────────────────┘    └─────────────────┘
                                        │
       ┌─────────────────┐              ▼
       │ StoreRealtime   │       ┌─────────────────┐
       │ (Audio UI)      │──────▶│ Azure OpenAI    │
       └─────────────────┘       │ - GPT-4.1-mini  │
                │                │ - Embeddings    │
                │                └─────────────────┘
                ▼
       ┌─────────────────┐
       │ Azure OpenAI    │
       │ Realtime API    │
       │ - GPT-4o-mini   │
       │ - Audio I/O     │
       └─────────────────┘
```

### Core Services
- **Products Service**: API service with semantic search and product data
- **Store Service**: Traditional web frontend
- **StoreRealtime Service**: Blazor Server app with real-time audio capabilities
- **SQL Server**: Product database with Entity Framework
- **Azure OpenAI**: Multiple model deployments for different capabilities

### Key Technologies
- **Aspire**: Cloud-native orchestration and configuration
- **OpenAI Realtime API**: GPT-4o-mini-realtime-preview for voice conversations
- **Blazor Server**: Interactive web UI with SignalR for real-time updates
- **Azure OpenAI**: Chat completion and text embedding services
- **Real-time Communication**: WebRTC and SignalR for audio streaming

## Model Deployments

### Chat Model (GPT-4o-mini)
- **Model**: gpt-4o-mini
- **Version**: 2024-07-18
- **Purpose**: Standard text-based chat interactions

### Realtime Audio Model (GPT-4o-mini-realtime-preview)
- **Model**: gpt-4o-mini-realtime-preview
- **Version**: 2024-12-17
- **Purpose**: Real-time audio conversation processing
- **Capabilities**: Voice input/output, natural conversation flow

### Embedding Model (text-embedding-ada-002)
- **Model**: text-embedding-ada-002
- **Version**: 2
- **Purpose**: Semantic search and product matching

## Configuration

### Environment Variables
- `AI_ChatDeploymentName`: "gpt-4o-mini"
- `AI_RealtimeDeploymentName`: "gpt-4o-mini-realtime-preview"
- `AI_embeddingsDeploymentName`: "text-embedding-ada-002"


## Screenshots

### Aspire Dashboard
![Aspire Dashboard](./images/dashboard.jpg)

### Products Listing
![Products Listing](./images/products.jpg)

### Realtime Audio Interface
![Realtime Audio Search](./images/search.jpg)

## Key Features

### Real-time Audio Conversation
- Voice-to-voice product inquiries
- Natural language understanding for product searches
- Contextual responses based on product catalog
- Real-time audio processing and streaming

### Dual Interface Support
- Traditional web interface for standard browsing
- Audio-enabled interface for voice interactions
- Seamless integration between text and voice modalities

### Advanced Product Discovery
- Voice-based product search
- Conversational product recommendations
- Context-aware responses about outdoor camping products

## Getting Started

1. Configure Azure OpenAI user secrets with realtime model access
2. Run from the scenario's src directory:
   ```bash
   cd src/eShopAppHost
   dotnet run
   ```
3. Access both the traditional Store and RealtimeStore interfaces
4. Test voice interactions through the realtime audio interface

## Related Resources

- [Main Scenario README](../README.md)
- [OpenAI Realtime API Documentation](https://platform.openai.com/docs/guides/realtime)
- [Blazor Server Documentation](https://learn.microsoft.com/en-us/aspnet/core/blazor/hosting-models#blazor-server)
- [Aspire Documentation](https://aspire.dev/docs/)