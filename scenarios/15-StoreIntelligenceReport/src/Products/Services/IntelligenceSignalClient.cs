namespace Products.Services;

/// <summary>
/// Typed HttpClient that forwards search signals to the StoreIntelligence service
/// (POST /api/intelligence/signals). Products is the signal *producer*; StoreIntelligence
/// owns the store and the report UI.
///
/// Resilience contract:
///   Signal recording is best-effort. If the StoreIntelligence service is unavailable or
///   the HTTP call fails for any reason, the exception is caught, logged, and swallowed.
///   The Products search endpoints must NEVER return an error because of a failed signal.
/// </summary>
public class IntelligenceSignalClient : IIntelligenceSignalClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<IntelligenceSignalClient> _logger;

    // Constructor is called by the typed HttpClient factory; HttpClient.BaseAddress is
    // set to "https+http://intelligence" in Program.cs so Aspire service discovery resolves it.
    public IntelligenceSignalClient(HttpClient httpClient, ILogger<IntelligenceSignalClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task RecordAsync(string term, bool semantic, int resultCount)
    {
        try
        {
            // POST the signal as a JSON body to the intelligence service.
            var payload = new { term, semantic, resultCount };
            var response = await _httpClient.PostAsJsonAsync("/api/intelligence/signals", payload);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Signal record returned non-success status {Status} for term '{Term}'.",
                    response.StatusCode, term);
            }
        }
        catch (Exception ex)
        {
            // Swallow — a signal failure must never surface to the end user.
            _logger.LogWarning(ex, "Failed to record signal for term '{Term}'; continuing.", term);
        }
    }
}
