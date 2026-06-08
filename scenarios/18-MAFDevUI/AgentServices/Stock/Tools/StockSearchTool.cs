using System.ComponentModel;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace AgentServices.Stock.Tools;

/// <summary>
/// External tool for searching product stock via the Products API.
/// This tool is used by the Stock Agent to search for product stock information.
/// </summary>
public class StockSearchTool
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<StockSearchTool> _logger;

    public StockSearchTool(HttpClient httpClient, ILogger<StockSearchTool> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Searches for product stock by product name.
    /// Returns stock information including quantities across all locations.
    /// </summary>
    /// <param name="productName">The name or partial name of the product to search for.</param>
    /// <returns>A JSON string containing stock information for matching products.</returns>
    [Description("Searches for product stock by name. Returns stock information including quantities across all locations.")]
    public async Task<string> SearchProductStockAsync(
        [Description("The name or partial name of the product to search for")] string productName)
    {
        _logger.LogInformation("StockSearchTool: Searching for stock of product '{ProductName}'", productName);

        try
        {
            var response = await _httpClient.GetAsync($"api/Product/stock/search/{Uri.EscapeDataString(productName)}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("StockSearchTool: Successfully retrieved stock information for '{ProductName}'", productName);
                return content;
            }
            else
            {
                var errorMessage = $"Failed to search stock for '{productName}'. Status: {response.StatusCode}";
                _logger.LogWarning("StockSearchTool: {ErrorMessage}", errorMessage);
                return JsonSerializer.Serialize(new { error = errorMessage });
            }
        }
        catch (Exception ex)
        {
            var errorMessage = $"Error searching stock for '{productName}': {ex.Message}";
            _logger.LogError(ex, "StockSearchTool: {ErrorMessage}", errorMessage);
            return JsonSerializer.Serialize(new { error = errorMessage });
        }
    }
}
