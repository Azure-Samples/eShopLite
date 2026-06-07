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
    ObservabilityModelMetadata? Model);

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
