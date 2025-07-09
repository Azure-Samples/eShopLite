# Application Insights Telemetry - Realtime Audio

## Overview
The Realtime Audio scenario integrates Azure Application Insights for comprehensive monitoring of both traditional web interactions and real-time audio conversations.

## Configuration

### Application Insights Setup
```csharp
var appInsights = builder.AddAzureApplicationInsights("appInsights");
```

**Purpose**: Provides distributed tracing, logging, and performance monitoring for all services including real-time audio features.

**Service Name**: `appInsights`

### Service Integration

#### Products Service Telemetry
```csharp
products.WithReference(appInsights)
```

**Capabilities**:
- API request/response tracking
- Database query performance monitoring
- AI service call telemetry
- Exception tracking and logging

#### Store Web Application Telemetry
```csharp
store.WithReference(appInsights)
```

**Capabilities**:
- User interaction tracking
- Page load performance
- Search query analytics
- Client-side error monitoring

#### Realtime Store Telemetry
```csharp
storeRealtime.WithReference(appInsights)
```

**Capabilities**:
- Real-time audio session monitoring
- WebRTC connection quality tracking
- AI conversation analytics
- Audio processing performance metrics

## Telemetry Features
- **Distributed Tracing**: Track requests across all services including audio streams
- **Performance Metrics**: Monitor response times for both web and audio interactions
- **AI Integration Monitoring**: Track Azure OpenAI API calls including real-time audio
- **Custom Events**: Monitor audio conversation patterns and user engagement
- **Error Tracking**: Capture audio-specific errors and connection issues
- **Real-time Analytics**: Monitor concurrent audio sessions and resource usage

## Audio-Specific Monitoring
- **Session Duration**: Track length of voice conversations
- **Audio Quality**: Monitor WebRTC connection stability
- **Latency Metrics**: Measure real-time response times
- **Conversation Success**: Track completed vs. dropped audio sessions

## External Dependencies
- Azure Application Insights service
- Application Insights connection string or instrumentation key
- Properly configured Azure subscription
- WebRTC telemetry integration