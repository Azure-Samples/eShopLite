using SearchEntities;

namespace Store.Services;

public class ObservabilityAnalysisClientResult
{
    public ObservabilityAnalysisResponse? Response { get; init; }
    public string Summary { get; init; } = string.Empty;
    public int Minutes { get; init; }
    public DateTimeOffset WindowStartUtc { get; init; }
    public DateTimeOffset WindowEndUtc { get; init; }
    public int EntriesAnalyzed { get; init; }
    public string AnalysisSource { get; init; } = string.Empty;
    public int? StatusCode { get; init; }
    public string Endpoint { get; init; } = string.Empty;
    public string? ErrorMessage { get; init; }
    public DateTimeOffset ReceivedAtUtc { get; init; }
    public bool IsSuccess => StatusCode is >= 200 and < 300 && (Response is not null || !string.IsNullOrWhiteSpace(Summary));
}
