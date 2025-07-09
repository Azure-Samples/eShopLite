# Application Insights Telemetry - DeepSeek

## Overview
The DeepSeek scenario integrates Azure Application Insights for comprehensive monitoring of AI reasoning operations, multi-model interactions, and performance analytics.

## Configuration

### Application Insights Setup
```csharp
var appInsights = builder.AddAzureApplicationInsights("appInsights");
```

**Purpose**: Provides distributed tracing, logging, and performance monitoring with special focus on AI reasoning operations.

**Service Name**: `appInsights`

### Service Integration

#### Products Service Telemetry
```csharp
products.WithReference(appInsights)
```

**Capabilities**:
- API request/response tracking
- Database query performance monitoring
- Multi-model AI service call telemetry
- DeepSeek reasoning operation tracking
- Exception tracking and logging

#### Store Web Application Telemetry
```csharp
store.WithReference(appInsights)
```

**Capabilities**:
- User interaction tracking
- Page load performance
- Advanced search query analytics
- Client-side error monitoring
- AI reasoning result tracking

## Telemetry Features
- **Distributed Tracing**: Track requests across services and AI models
- **Performance Metrics**: Monitor response times for different AI models
- **AI Integration Monitoring**: Track both Azure OpenAI and DeepSeek API calls
- **Custom Events**: Monitor reasoning operation patterns and complexity
- **Error Tracking**: Capture AI-specific errors and model switching logic
- **Cost Tracking**: Monitor usage patterns across different AI services

## DeepSeek-Specific Monitoring
- **Reasoning Duration**: Track time for complex reasoning operations
- **Model Selection**: Monitor when DeepSeek vs GPT models are used
- **Reasoning Depth**: Measure complexity of thought chains
- **Accuracy Metrics**: Track success rates of reasoning operations

## Multi-Model Analytics
- **Model Performance Comparison**: Compare response times and accuracy
- **Cost Optimization**: Track usage patterns for cost-effective model selection
- **User Satisfaction**: Monitor user engagement with different AI responses
- **Error Analysis**: Compare error rates across AI services

## External Dependencies
- Azure Application Insights service
- Application Insights connection string or instrumentation key
- Properly configured Azure subscription
- Custom telemetry for DeepSeek API calls