using SearchEntities;

namespace Store.Services;

public class ObservabilityService(HttpClient httpClient, ILogger<ObservabilityService> logger) : IObservabilityService
{
    public async Task<ObservabilityAnalysisClientResult> AnalyzeObservability(int minutes)
    {
        var endpoint = $"/observability/analyze?minutes={minutes}";
        try
        {
            var response = await httpClient.GetAsync(endpoint);
            var responseText = await response.Content.ReadAsStringAsync();

            logger.LogInformation("Observability status code: {StatusCode}", response.StatusCode);
            logger.LogInformation("Observability response content: {Response}", responseText);

            var endpointAddress = httpClient.BaseAddress is null
                ? endpoint
                : $"{httpClient.BaseAddress.ToString().TrimEnd('/')}{endpoint}";

            if (response.IsSuccessStatusCode)
            {
                var observabilityResponse = await response.Content.ReadFromJsonAsync<ObservabilityAnalysisResponse>();
                return new ObservabilityAnalysisClientResult
                {
                    Response = observabilityResponse,
                    Summary = observabilityResponse?.AnalysisMarkdown ?? string.Empty,
                    Minutes = observabilityResponse?.Minutes ?? minutes,
                    WindowStartUtc = observabilityResponse?.WindowStartUtc ?? DateTimeOffset.UtcNow.AddMinutes(-minutes),
                    WindowEndUtc = observabilityResponse?.WindowEndUtc ?? DateTimeOffset.UtcNow,
                    EntriesAnalyzed = observabilityResponse?.EntriesAnalyzed ?? 0,
                    AnalysisSource = observabilityResponse?.AnalysisSource ?? string.Empty,
                    FallbackReason = observabilityResponse?.FallbackReason,
                    Clustering = observabilityResponse?.Clustering,
                    StatusCode = (int)response.StatusCode,
                    Endpoint = endpointAddress,
                    ReceivedAtUtc = DateTimeOffset.UtcNow
                };
            }

            return new ObservabilityAnalysisClientResult
            {
                StatusCode = (int)response.StatusCode,
                Endpoint = endpointAddress,
                ReceivedAtUtc = DateTimeOffset.UtcNow,
                ErrorMessage = string.IsNullOrWhiteSpace(responseText)
                    ? "Observability analysis returned an error response."
                    : responseText
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during AnalyzeObservability.");
            return new ObservabilityAnalysisClientResult
            {
                StatusCode = null,
                Endpoint = endpoint,
                ReceivedAtUtc = DateTimeOffset.UtcNow,
                ErrorMessage = "Unable to contact observability assistant service."
            };
        }
    }

    // Forwards a single telemetry event into the assistant's in-memory log store
    // (POST /observability/events). This is what makes "Analyze" find real data:
    // each Store search emits events here so the selected time window is populated.
    // Failures are swallowed so observability never breaks the user-facing flow.
    public async Task IngestEventAsync(string service, string severity, string message)
    {
        try
        {
            var payload = new
            {
                timestamp = DateTimeOffset.UtcNow,
                service,
                severity,
                message
            };

            using var response = await httpClient.PostAsJsonAsync("/observability/events", payload);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Observability event ingestion returned {StatusCode} for service {Service}.",
                    response.StatusCode,
                    service);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to ingest observability event for service {Service}.", service);
        }
    }
}
