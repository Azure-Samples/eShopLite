using ElBruno.MAF.FoundryLocal;
using Microsoft.Extensions.AI;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddProblemDetails();

builder.Services.Configure<FoundryLocalOptions>(builder.Configuration.GetSection("FoundryLocal"));
builder.Services.Configure<ChatRuntimeOptions>(builder.Configuration.GetSection("Chat"));
builder.Services.AddSingleton<FoundryLocalModelLifecycleService>();
builder.Services.AddSingleton<IChatClient>(sp =>
{
    var lifecycleService = sp.GetRequiredService<FoundryLocalModelLifecycleService>();
    var adapterLogger = sp.GetRequiredService<ILogger<FoundryLocalChatClientAdapter>>();
    return new FoundryLocalChatClientAdapter(lifecycleService, adapterLogger);
});

builder.Services.AddSingleton<ObservabilityAnalyzer>();

var app = builder.Build();

app.MapDefaultEndpoints();

var observability = app.MapGroup("/observability");
observability.MapGet("/windows", () =>
    Results.Ok(ObservabilityWindow.Supported.Select(m => new
    {
        minutes = m,
        analyzeEndpoint = $"/observability/analyze?minutes={m}"
    })));

observability.MapGet("/analyze", async (int? minutes, ObservabilityAnalyzer analyzer, CancellationToken cancellationToken) =>
{
    if (minutes is null)
    {
        return Results.BadRequest(new
        {
            error = "Query parameter 'minutes' is required.",
            supportedMinutes = ObservabilityWindow.Supported
        });
    }

    if (!ObservabilityWindow.Supported.Contains(minutes.Value))
    {
        return Results.BadRequest(new
        {
            error = "Invalid 'minutes' value. Allowed values are 5, 10, 15, and 30.",
            supportedMinutes = ObservabilityWindow.Supported
        });
    }

    var result = await analyzer.AnalyzeAsync(minutes.Value, cancellationToken);
    if (result is null)
    {
        return Results.NotFound(new
        {
            error = $"No log entries were found for the last {minutes.Value} minutes."
        });
    }

    return Results.Ok(result);
});

app.Run();

internal static class ObservabilityWindow
{
    public static readonly int[] Supported = [5, 10, 15, 30];
}

internal sealed class ObservabilityAnalyzer(
    IChatClient chatClient,
    ILogger<ObservabilityAnalyzer> logger)
{
    public async Task<ObservabilityAnalysisResponse?> AnalyzeAsync(int minutes, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var from = now.AddMinutes(-minutes);
        var logs = BuildLogs(now)
            .Where(entry => entry.Timestamp >= from)
            .OrderBy(entry => entry.Timestamp)
            .ToList();

        if (logs.Count == 0)
        {
            return null;
        }

        var summary = await SummarizeAsync(logs, minutes, cancellationToken);
        var severityBreakdown = logs
            .GroupBy(entry => entry.Severity)
            .ToDictionary(group => group.Key, group => group.Count());

        var details = string.Join(", ", severityBreakdown.Select(kvp => $"{kvp.Key}: {kvp.Value}"));

        return new ObservabilityAnalysisResponse(
            RequestId: Guid.NewGuid().ToString("N"),
            GeneratedAtUtc: now,
            Minutes: minutes,
            WindowStartUtc: from,
            WindowEndUtc: now,
            EntriesAnalyzed: logs.Count,
            Details: details,
            AnalysisMarkdown: summary.Text,
            AnalysisSource: summary.Source);
    }

    private async Task<(string Text, string Source)> SummarizeAsync(
        IReadOnlyList<LogEntry> logs,
        int minutes,
        CancellationToken cancellationToken)
    {
        var prompt = $"""
            You are an observability assistant.
            Summarize incidents and customer impact for the last {minutes} minutes in at most 6 bullet points.
            Mention probable root cause and immediate next action.

            Logs:
            {string.Join(Environment.NewLine, logs.Select(l => $"{l.Timestamp:O} [{l.Severity}] {l.Service} - {l.Message}"))}
            """;

        try
        {
            var response = await chatClient.GetResponseAsync(
                [new ChatMessage(ChatRole.User, prompt)],
                cancellationToken: cancellationToken);

            var text = string.IsNullOrWhiteSpace(response.Text)
                ? "No model summary generated."
                : response.Text.Trim();

            return (text, "foundry-local");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Foundry local summary generation failed. Returning deterministic fallback summary.");

            var fallback = $"Analyzed {logs.Count} entries in the last {minutes} minutes. " +
                           $"Primary concern: {logs.Last().Message}. " +
                           "Model summary unavailable, review recent errors and retry.";
            return (fallback, "fallback");
        }
    }

    private static IReadOnlyList<LogEntry> BuildLogs(DateTimeOffset now) =>
    [
        new(now.AddMinutes(-1), "store", "Warning", "Checkout retries are rising due to upstream timeout."),
        new(now.AddMinutes(-3), "products", "Error", "Vector search latency exceeded 2 seconds."),
        new(now.AddMinutes(-6), "products", "Information", "Recovered from transient SQL connection reset."),
        new(now.AddMinutes(-9), "store", "Error", "Payment callback returned 502 from mock gateway."),
        new(now.AddMinutes(-12), "store", "Warning", "Cart update queue depth crossed warning threshold."),
        new(now.AddMinutes(-17), "products", "Information", "Background index refresh completed."),
        new(now.AddMinutes(-23), "store", "Error", "Session cache miss ratio increased above baseline."),
        new(now.AddMinutes(-28), "products", "Warning", "Embedding request rate reached burst threshold."),
        new(now.AddMinutes(-34), "store", "Information", "Traffic spike detected after campaign launch.")
    ];
}

internal sealed record LogEntry(DateTimeOffset Timestamp, string Service, string Severity, string Message);

internal sealed record ObservabilityAnalysisResponse(
    string RequestId,
    DateTimeOffset GeneratedAtUtc,
    int Minutes,
    DateTimeOffset WindowStartUtc,
    DateTimeOffset WindowEndUtc,
    int EntriesAnalyzed,
    string Details,
    string AnalysisMarkdown,
    string AnalysisSource);
