# Application Insights Telemetry

## Overview
The Semantic Search scenario integrates Azure Application Insights for comprehensive application monitoring, performance tracking, and telemetry collection across all services.

## Configuration

### Application Insights Setup
```csharp
var appInsights = builder.AddAzureApplicationInsights("appInsights");
```

**Purpose**: Provides distributed tracing, logging, and performance monitoring for the entire application.

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

## Telemetry Features
- **Distributed Tracing**: Track requests across Products and Store services
- **Performance Metrics**: Monitor response times and throughput
- **AI Integration Monitoring**: Track Azure OpenAI API calls and performance
- **Custom Events**: Monitor semantic search usage patterns
- **Error Tracking**: Capture and analyze application exceptions

## External Dependencies
- Azure Application Insights service
- Application Insights connection string or instrumentation key
- Properly configured Azure subscription