using ElBruno.MAF.FoundryLocal;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;

internal sealed class ObservabilityAnalyzer(
    IChatClient chatClient,
    InMemoryLogStore logStore,
    LogClusteringService logClusteringService,
    IOptions<FoundryLocalModelCatalogOptions> modelCatalogOptions,
    IOptions<LogClusteringOptions> clusteringOptions,
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

        // Step 1: collapse semantically-similar log lines using local embeddings.
        var clusters = await logClusteringService.ClusterAsync(logs, cancellationToken);

        // Step 2: summarize the deduplicated representatives with the local model.
        var summary = await SummarizeAsync(clusters, minutes, cancellationToken);
        var severityBreakdown = logs
            .GroupBy(entry => entry.Severity)
            .ToDictionary(group => group.Key, group => group.Count());

        var details = string.Join(", ", severityBreakdown.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
        var clusteringMetadata = new ObservabilityClusteringMetadata(
            Enabled: clusteringOptions.Value.Enabled,
            OriginalEntryCount: logs.Count,
            ClusterCount: clusters.Count);

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
            Clustering: clusteringMetadata,
            Model: modelMetadata);
    }

    private async Task<(string Text, string Source)> SummarizeAsync(
        IReadOnlyList<LogCluster> clusters,
        int minutes,
        CancellationToken cancellationToken)
    {
        // Each representative line carries the number of similar entries it stands
        // in for (e.g. "(x12)"), so the model sees frequency without the raw noise.
        var clusterLines = clusters.Select(cluster =>
        {
            var rep = cluster.Representative;
            var occurrences = cluster.Count > 1 ? $" (x{cluster.Count})" : string.Empty;
            return $"{rep.Timestamp:O} [{rep.Severity}] {rep.Service} - {rep.Message}{occurrences}";
        });

        var prompt = $"""
            You are an observability assistant.
            Summarize incidents and customer impact for the last {minutes} minutes in at most 6 bullet points.
            The log lines below are grouped by similarity; "(xN)" means the line represents N similar events.
            Mention probable root cause and immediate next action.

            Logs:
            {string.Join(Environment.NewLine, clusterLines)}
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

            // Prefer an Error, then Warning, cluster as the headline concern so the
            // deterministic fallback still points at the most actionable signal.
            static int SeverityRank(string severity) => severity?.ToLowerInvariant() switch
            {
                "critical" or "fatal" => 0,
                "error" => 1,
                "warning" or "warn" => 2,
                _ => 3
            };

            var topCluster = clusters
                .OrderBy(c => SeverityRank(c.Representative.Severity))
                .ThenByDescending(c => c.Count)
                .First();

            var fallback = $"Analyzed {clusters.Sum(c => c.Count)} entries grouped into {clusters.Count} clusters " +
                           $"in the last {minutes} minutes. " +
                           $"Primary concern: [{topCluster.Representative.Severity}] {topCluster.Representative.Message} (x{topCluster.Count}). " +
                           "Model summary unavailable, review recent errors and retry.";
            return (fallback, "fallback");
        }
    }
}
