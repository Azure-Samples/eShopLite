using ElBruno.MAF.FoundryLocal;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;

internal sealed class ObservabilityAnalyzer(
    IChatClient chatClient,
    InMemoryLogStore logStore,
    IOptions<FoundryLocalModelCatalogOptions> modelCatalogOptions,
    FoundryLocalModelLifecycleService modelLifecycleService,
    ILogger<ObservabilityAnalyzer> logger)
{
    public async Task<ObservabilityAnalysisResponse?> AnalyzeAsync(int minutes, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var from = now.AddMinutes(-minutes);
        var logs = logStore.GetWindow(from, now);

        if (logs.Count == 0)
        {
            return null;
        }

        var summary = await SummarizeAsync(logs, minutes, cancellationToken);
        var severityBreakdown = logs
            .GroupBy(entry => entry.Severity)
            .ToDictionary(group => group.Key, group => group.Count());

        var details = string.Join(", ", severityBreakdown.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
        var modelCatalog = modelCatalogOptions.Value;
        modelCatalog.Models.TryGetValue(modelCatalog.SelectedModel, out var selectedModel);
        var diagnostics = modelLifecycleService.GetDiagnosticsSnapshot();
        var modelMetadata = selectedModel is null
            ? null
            : new ObservabilityModelMetadata(
                SelectedModelKey: modelCatalog.SelectedModel,
                ConfiguredModelAlias: selectedModel.ModelAlias,
                DisplayName: selectedModel.DisplayName,
                Description: selectedModel.Description,
                ConfiguredDownloadIfMissing: selectedModel.DownloadIfMissing,
                ConfiguredUnloadOnExit: selectedModel.UnloadOnExit,
                RuntimeModelAlias: diagnostics.ModelAlias,
                RuntimeModelLoaded: diagnostics.ModelLoaded,
                RuntimeDownloadedThisSession: diagnostics.DownloadedThisSession,
                RuntimeStreamingEnabled: diagnostics.StreamingEnabled,
                RuntimeUsingRestServer: diagnostics.UsingRestServer,
                RuntimeWarnings: diagnostics.Warnings);

        return new ObservabilityAnalysisResponse(
            RequestId: Guid.NewGuid().ToString("N"),
            GeneratedAtUtc: now,
            Minutes: minutes,
            WindowStartUtc: from,
            WindowEndUtc: now,
            EntriesAnalyzed: logs.Count,
            Details: details,
            AnalysisMarkdown: summary.Text,
            AnalysisSource: summary.Source,
            Model: modelMetadata);
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
}
