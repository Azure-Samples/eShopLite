# Service Defaults

## Overview

The Service Defaults library provides common .NET Aspire services and configurations that are shared across all applications in the solution. This includes service discovery, resilience patterns, health checks, and OpenTelemetry integration.

## Core Features

### Aspire Service Integration
```csharp
public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
{
    builder.ConfigureOpenTelemetry();
    builder.AddDefaultHealthChecks();
    builder.Services.ConfigureHttpClientDefaults(http =>
    {
        // Add standard resilience handler
        http.AddStandardResilienceHandler();
        
        // Turn on service discovery by default
        http.AddServiceDiscovery();
    });
    
    return builder;
}
```

### OpenTelemetry Configuration
```csharp
public static IHostApplicationBuilder ConfigureOpenTelemetry(this IHostApplicationBuilder builder)
{
    builder.Logging.AddOpenTelemetry(logging =>
    {
        logging.IncludeFormattedMessage = true;
        logging.IncludeScopes = true;
    });

    builder.Services.AddOpenTelemetry()
        .WithMetrics(metrics =>
        {
            metrics.AddAspNetCoreInstrumentation()
                   .AddHttpClientInstrumentation()
                   .AddRuntimeInstrumentation();
        })
        .WithTracing(tracing =>
        {
            tracing.AddAspNetCoreInstrumentation()
                   .AddHttpClientInstrumentation();
        });

    builder.AddOpenTelemetryExporters();
    return builder;
}
```

## Health Checks

### Default Health Check Endpoints
```csharp
public static WebApplication MapDefaultEndpoints(this WebApplication app)
{
    // Adding health checks endpoints to applications in non-development environments has security implications.
    if (app.Environment.IsDevelopment())
    {
        // All health checks must pass for app to be considered ready
        app.MapHealthChecks("/health");

        // Only health checks tagged with the "live" tag must pass for app to be considered alive
        app.MapHealthChecks("/alive", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("live")
        });
    }

    return app;
}
```

### Health Check Configuration
- **Ready Endpoint**: `/health` - Comprehensive health verification
- **Live Endpoint**: `/alive` - Minimal liveness check
- **Development Only**: Health endpoints only exposed in development for security

## Resilience Patterns

### HTTP Client Resilience
```csharp
builder.Services.ConfigureHttpClientDefaults(http =>
{
    // Add standard resilience handler
    http.AddStandardResilienceHandler(resilienceOptions =>
    {
        // Configure retry policy
        resilienceOptions.Retry.MaxRetryAttempts = 3;
        resilienceOptions.Retry.BackoffType = DelayBackoffType.Exponential;
        
        // Configure circuit breaker
        resilienceOptions.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(10);
        resilienceOptions.CircuitBreaker.FailureRatio = 0.5;
        
        // Configure timeout
        resilienceOptions.AttemptTimeout.Timeout = TimeSpan.FromSeconds(30);
        resilienceOptions.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(2);
    });
});
```

### Resilience Features
- **Retry Logic**: Exponential backoff with configurable attempts
- **Circuit Breaker**: Fail-fast when services are unhealthy
- **Timeout Handling**: Request-level and attempt-level timeouts
- **Bulkhead Isolation**: Resource isolation between service calls

## Service Discovery

### Automatic Service Resolution
```csharp
// Automatic service discovery for HTTP clients
builder.Services.AddHttpClient<ProductService>(
    static client => client.BaseAddress = new("https+http://products"));
```

### Service Discovery Features
- **Automatic Resolution**: Services resolved by logical name
- **Load Balancing**: Built-in load balancing for multiple instances
- **Health-Aware Routing**: Routes traffic only to healthy instances
- **Configuration-Free**: No manual endpoint configuration required

## Telemetry and Observability

### Logging Configuration
```csharp
builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeFormattedMessage = true;
    logging.IncludeScopes = true;
});
```

### Metrics Collection
- **ASP.NET Core Metrics**: Request rates, response times, error rates
- **HTTP Client Metrics**: Outbound request statistics
- **Runtime Metrics**: GC, memory, thread pool statistics
- **Custom Metrics**: Application-specific performance counters

### Distributed Tracing
- **Request Tracing**: End-to-end request flow tracking
- **Service Correlation**: Track requests across service boundaries
- **Performance Analysis**: Identify bottlenecks and latency issues
- **Error Correlation**: Link errors to specific request traces

## Configuration Management

### Environment-Specific Settings
```csharp
// Configuration varies by environment
if (app.Environment.IsDevelopment())
{
    // Development-specific configurations
    app.MapHealthChecks("/health");
    app.UseDeveloperExceptionPage();
}
else
{
    // Production configurations
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
```

### Security Considerations
- **Health Check Security**: Health endpoints only in development
- **Exception Handling**: Detailed errors only in development
- **HTTPS Enforcement**: HSTS headers in production
- **Error Information**: Sanitized error responses in production

## Integration Patterns

### Service Registration Pattern
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults (required for all services)
builder.AddServiceDefaults();

// Add additional services
builder.Services.AddDbContext<MyContext>();
builder.Services.AddHttpClient<ExternalService>();

var app = builder.Build();

// Map default endpoints (required for all services)
app.MapDefaultEndpoints();
```

### Cross-Cutting Concerns
- **Consistent Logging**: Structured logging across all services
- **Standardized Metrics**: Common metric collection patterns
- **Unified Health Checks**: Consistent health reporting
- **Service Discovery**: Automatic service location and routing

## Best Practices

### Implementation Guidelines
1. **Always call AddServiceDefaults()** first in Program.cs
2. **Map default endpoints** for health checks and metrics
3. **Use service discovery** for inter-service communication
4. **Leverage resilience patterns** for external dependencies
5. **Configure appropriate timeouts** for your use case

### Monitoring Integration
- **Aspire Dashboard**: Local development monitoring
- **Azure Application Insights**: Production telemetry
- **Custom Dashboards**: Business-specific metrics visualization
- **Alerting**: Proactive issue detection and notification

## Troubleshooting

### Common Issues
- **Service Discovery Failures**: Check service naming and registration
- **Health Check Failures**: Verify dependencies are available
- **Timeout Issues**: Adjust timeout configurations for your scenario
- **Telemetry Gaps**: Ensure proper exporter configuration

### Debugging Features
- **Development Health Checks**: Detailed status information
- **Request Tracing**: Step-by-step request analysis
- **Performance Counters**: Real-time performance monitoring
- **Error Correlation**: Link errors to specific operations