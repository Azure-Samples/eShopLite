internal sealed record ObservabilityAnalysisResponse(
    string RequestId,
    DateTimeOffset GeneratedAtUtc,
    int Minutes,
    DateTimeOffset WindowStartUtc,
    DateTimeOffset WindowEndUtc,
    int EntriesAnalyzed,
    string Details,
    string AnalysisMarkdown,
    string AnalysisSource,
    ObservabilityClusteringMetadata? Clustering,
    ObservabilityModelMetadata? Model);

// Local-embeddings clustering summary: how many raw entries were folded into how
// many representative groups before the model summarized them.
internal sealed record ObservabilityClusteringMetadata(
    bool Enabled,
    int OriginalEntryCount,
    int ClusterCount);

internal sealed record ObservabilityModelMetadata(
    string SelectedModelKey,
    string ConfiguredModelAlias,
    string? DisplayName,
    string? Description,
    bool? ConfiguredDownloadIfMissing,
    bool? ConfiguredUnloadOnExit,
    string RuntimeModelAlias,
    bool RuntimeModelLoaded,
    bool RuntimeDownloadedThisSession,
    bool RuntimeStreamingEnabled,
    bool RuntimeUsingRestServer,
    IReadOnlyList<string> RuntimeWarnings);
