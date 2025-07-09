# Realtime Audio Documentation

## Overview
The 03-RealtimeAudio scenario demonstrates an innovative eCommerce application with real-time audio interaction capabilities using .NET Aspire, Azure OpenAI real-time API, and voice-based product discovery.

## Features

### Core Services
- **Aspire Orchestration**: Coordinated microservices architecture with dependency management
- **Realtime Audio Integration**: Voice-based AI interactions for product discovery
- **Application Insights**: Comprehensive telemetry including audio session monitoring

### Feature Documentation
- [Aspire Orchestration](aspire-orchestration.md) - Service coordination including realtime store
- [Realtime Audio Integration](realtime-audio-integration.md) - Voice AI and audio streaming
- [Application Insights](application-insights.md) - Telemetry and audio session monitoring

## Architecture
The scenario consists of four main components:
1. **SQL Server Database** - Persistent data storage
2. **Products Service** - REST API with semantic search capabilities
3. **Store Web Application** - Traditional Blazor frontend
4. **Realtime Store Application** - Voice-enabled Blazor frontend

## AI Capabilities
- **Real-time Audio Conversations**: Natural voice interaction with AI assistant
- **Semantic Search**: Uses text-embedding-ada-002 for intelligent product discovery
- **Chat Completion**: GPT-4o Mini for text-based recommendations
- **Voice Product Discovery**: Audio-based product recommendations and assistance

## Real-time Features
- **Bidirectional Audio Streaming**: Low-latency voice conversations
- **Contextual Understanding**: AI maintains conversation context across interactions
- **WebRTC Integration**: High-quality audio streaming in the browser
- **Real-time Responses**: Immediate AI responses to voice queries

## Configuration Requirements
- Azure OpenAI Service with real-time preview models
- WebRTC-compatible browser
- Microphone access permissions
- User secrets for API keys and endpoints
- .NET 9 runtime environment

## Screenshots

> **Note**: The screenshots below represent the expected user interface when running the scenario. Due to infrastructure requirements (Azure OpenAI with real-time API, SQL Server), actual screenshots would require a fully configured environment.

### Aspire Dashboard
*The Aspire Dashboard showing all services including the realtime store*

### Products Listing
*The main products page with both traditional and voice search options*

### Audio Interface
*The real-time audio interface for voice-based product discovery*

## Getting Started
1. Set up Azure OpenAI Service with real-time preview models
2. Configure user secrets for API keys
3. Run `dotnet run --project src/eShopAppHost/eShopAppHost.csproj`
4. Navigate to the Realtime Store application
5. Grant microphone permissions
6. Start a voice conversation for product recommendations