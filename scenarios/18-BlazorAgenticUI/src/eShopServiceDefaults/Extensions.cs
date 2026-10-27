using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System.ClientModel;

namespace Microsoft.Extensions.Hosting;

public static class Extensions
{
    public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
    {
        builder.Services.AddServiceDiscovery();
        // Streaming agent requests must not be retried or buffered by a resilience pipeline.
        builder.Services.ConfigureHttpClientDefaults(http => http.AddServiceDiscovery());
        builder.Services.AddHealthChecks().AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);
        builder.Logging.AddFilter("Microsoft.AspNetCore.Hosting.Diagnostics", LogLevel.Warning);
        builder.Logging.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);
        builder.Logging.AddFilter("Microsoft.Agents.AI", LogLevel.Warning);
        builder.Logging.AddOpenTelemetry();
        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics => metrics.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation().AddRuntimeInstrumentation())
            .WithTracing(tracing => tracing.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation());
        if (!string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]))
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }
        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapHealthChecks("/health");
            app.MapHealthChecks("/alive", new HealthCheckOptions { Predicate = check => check.Tags.Contains("live") });
        }
        return app;
    }

    public static OpenAIClient CreateModelClient(this IConfiguration configuration)
    {
        var endpoint = GetModelEndpoint(configuration["AzureOpenAIEndpoint"]);
        var key = configuration["AzureOpenAIApiKey"];
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException("Configure AzureOpenAIApiKey through AppHost user-secrets.");
        }
        // Foundry's OpenAI-compatible v1 endpoint accepts deployment names as model IDs.
        return new OpenAIClient(new ApiKeyCredential(key), new OpenAIClientOptions
        {
            Endpoint = endpoint
        });
    }

    public static Uri GetModelEndpoint(string? endpoint)
    {
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var uri) || uri.Scheme != "https" ||
            !string.IsNullOrEmpty(uri.UserInfo) || !string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment) ||
            (uri.AbsolutePath.TrimEnd('/') is not ("" or "/openai/v1")))
        {
            throw new InvalidOperationException("Configure AzureOpenAIEndpoint as an HTTPS inference endpoint, not a Foundry project URL.");
        }
        return new Uri(uri.GetLeftPart(UriPartial.Authority) + "/openai/v1/");
    }
}
