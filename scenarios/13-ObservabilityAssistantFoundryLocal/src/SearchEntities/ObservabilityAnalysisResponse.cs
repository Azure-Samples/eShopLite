using System.Text.Json.Serialization;

namespace SearchEntities;

public class ObservabilityAnalysisResponse
{
    [JsonPropertyName("requestId")]
    public string RequestId { get; set; } = string.Empty;

    [JsonPropertyName("generatedAtUtc")]
    public DateTimeOffset GeneratedAtUtc { get; set; }

    [JsonPropertyName("minutes")]
    public int Minutes { get; set; }

    [JsonPropertyName("windowStartUtc")]
    public DateTimeOffset WindowStartUtc { get; set; }

    [JsonPropertyName("windowEndUtc")]
    public DateTimeOffset WindowEndUtc { get; set; }

    [JsonPropertyName("entriesAnalyzed")]
    public int EntriesAnalyzed { get; set; }

    [JsonPropertyName("details")]
    public string Details { get; set; } = string.Empty;

    [JsonPropertyName("analysisMarkdown")]
    public string AnalysisMarkdown { get; set; } = string.Empty;

    [JsonPropertyName("analysisSource")]
    public string AnalysisSource { get; set; } = string.Empty;

    [JsonPropertyName("clustering")]
    public ObservabilityClusteringMetadata? Clustering { get; set; }
}

public class ObservabilityClusteringMetadata
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    [JsonPropertyName("originalEntryCount")]
    public int OriginalEntryCount { get; set; }

    [JsonPropertyName("clusterCount")]
    public int ClusterCount { get; set; }
}
