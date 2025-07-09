# Telemetry and Monitoring

## Overview
The scenario includes comprehensive telemetry and monitoring through .NET Aspire's built-in observability features and Azure Application Insights integration.

## Aspire Dashboard

### Service Defaults
```csharp
builder.AddServiceDefaults();
```
- **Purpose**: Adds default Aspire services including telemetry, service discovery, and health checks
- **Configuration**: Automatic OpenTelemetry configuration for all services
- **Features**: Distributed tracing, metrics collection, and health monitoring

### Health Checks
```csharp
public static IHostApplicationBuilder AddDefaultHealthChecks(this IHostApplicationBuilder builder)
{
    builder.Services.AddHealthChecks()
        .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);
    return builder;
}
```
- **Purpose**: Provides health monitoring endpoints for service status
- **Endpoints**: `/health` (all checks), `/alive` (liveness only)
- **Configuration**: Development environment only for security

### Endpoint Mapping
```csharp
app.MapDefaultEndpoints();
```
- **Purpose**: Maps standard Aspire endpoints for health checks and telemetry
- **Configuration**: Automatic registration in development mode
- **Security**: Disabled in production environments

## Azure Application Insights

### Production Configuration
```csharp
if (builder.ExecutionContext.IsPublishMode)
{
    var appInsights = builder.AddAzureApplicationInsights("appInsights");
    
    products.WithReference(appInsights);
    store.WithReference(appInsights);
}
```
- **Purpose**: Production telemetry with Azure Application Insights
- **Configuration**: Automatic connection string management via Aspire
- **Scope**: Both Products and Store services instrumented

### OpenTelemetry Integration
```csharp
private static IHostApplicationBuilder AddOpenTelemetryExporters(this IHostApplicationBuilder builder)
{
    if (!string.IsNullOrEmpty(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
    {
        builder.Services.AddOpenTelemetry()
           .UseAzureMonitor();
    }
    return builder;
}
```

## OpenAI Telemetry

### Experimental Features
```csharp
AppContext.SetSwitch("OpenAI.Experimental.EnableOpenTelemetry", true);
```
- **Purpose**: Enables detailed tracing for OpenAI client operations
- **Configuration**: Experimental feature for Azure OpenAI calls
- **Data**: Request/response logging, latency metrics, error tracking

## Observability Features

### Distributed Tracing
- **Scope**: Cross-service request tracking from Blazor UI to Products API to Azure OpenAI
- **Context**: Automatic correlation ID propagation
- **Visualization**: Aspire Dashboard and Azure Application Insights

### Metrics Collection
- **HTTP Requests**: Request rates, response times, status codes
- **Database**: Entity Framework query performance and connection health  
- **AI Services**: Azure OpenAI request latency and token usage
- **Custom**: Application-specific business metrics

### Logging Integration
- **Structured Logging**: JSON-formatted logs with correlation context
- **Log Levels**: Configurable per service and environment
- **Sinks**: Console (development), Azure Application Insights (production)

## Configuration Notes
- **Local Development**: Uses Aspire Dashboard for real-time monitoring
- **Production**: Azure Application Insights for cloud-native observability
- **Correlation**: Automatic trace correlation across service boundaries
- **Security**: Health check endpoints restricted to development
- **Performance**: Minimal overhead with sampling and filtering

## External Dependencies
- Azure Application Insights workspace
- .NET Aspire Dashboard
- OpenTelemetry.Instrumentation.AspNetCore
- Azure.Monitor.OpenTelemetry.AspNetCore