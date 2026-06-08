using System.Text.Json;
using DataEntities;

namespace Store.Services;

public class IntelligenceService : IIntelligenceService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<IntelligenceService> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public IntelligenceService(HttpClient httpClient, ILogger<IntelligenceService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<StoreSignal>> GetSignals(int max = 50)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/intelligence/signals?max={max}");
            if (response.IsSuccessStatusCode)
            {
                var signals = await response.Content.ReadFromJsonAsync<List<StoreSignal>>(JsonOptions);
                return signals ?? new List<StoreSignal>();
            }

            _logger.LogWarning("GetSignals failed: {StatusCode}", response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during GetSignals.");
        }

        return new List<StoreSignal>();
    }

    public async Task<StoreIntelligenceReport?> GetReport()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/intelligence/report");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<StoreIntelligenceReport>(JsonOptions);
            }

            _logger.LogWarning("GetReport failed: {StatusCode}", response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during GetReport.");
        }

        return null;
    }
}
