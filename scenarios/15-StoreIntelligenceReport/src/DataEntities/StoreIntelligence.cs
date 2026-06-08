using System.Text.Json.Serialization;

namespace DataEntities;

/// <summary>
/// A single store signal captured by the Products service. For this scenario a signal is a
/// search event: the term a customer searched, whether semantic search was used, and how many
/// products were returned. Zero results indicate a failed search (a catalog gap or off-catalog
/// intent).
/// </summary>
public class StoreSignal
{
    [JsonPropertyName("term")]
    public string Term { get; set; } = string.Empty;

    [JsonPropertyName("semantic")]
    public bool Semantic { get; set; }

    [JsonPropertyName("resultCount")]
    public int ResultCount { get; set; }

    [JsonPropertyName("timestampUtc")]
    public DateTime TimestampUtc { get; set; }

    [JsonIgnore]
    public bool Failed => ResultCount == 0;
}

/// <summary>
/// The Store Intelligence Report: a business-facing summary built from store signals. The
/// section list follows docs/report-schema.md. <see cref="Source"/> is "ai" when a chat model
/// wrote the summary, or "fallback" when it was assembled deterministically.
/// </summary>
public class StoreIntelligenceReport
{
    [JsonPropertyName("generatedAtUtc")]
    public DateTime GeneratedAtUtc { get; set; }

    [JsonPropertyName("source")]
    public string Source { get; set; } = "fallback";

    [JsonPropertyName("signalsAnalyzed")]
    public int SignalsAnalyzed { get; set; }

    [JsonPropertyName("executiveSummary")]
    public List<string> ExecutiveSummary { get; set; } = new();

    [JsonPropertyName("topSearches")]
    public List<string> TopSearches { get; set; } = new();

    [JsonPropertyName("failedSearches")]
    public List<string> FailedSearches { get; set; } = new();

    [JsonPropertyName("productOpportunities")]
    public List<string> ProductOpportunities { get; set; } = new();

    [JsonPropertyName("operationalIssues")]
    public List<string> OperationalIssues { get; set; } = new();

    [JsonPropertyName("recommendedActions")]
    public List<string> RecommendedActions { get; set; } = new();
}

[JsonSerializable(typeof(List<StoreSignal>))]
[JsonSerializable(typeof(StoreSignal))]
[JsonSerializable(typeof(StoreIntelligenceReport))]
public sealed partial class StoreIntelligenceSerializerContext : JsonSerializerContext
{
}
