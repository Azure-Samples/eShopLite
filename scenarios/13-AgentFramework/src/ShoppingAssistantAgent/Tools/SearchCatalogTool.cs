using SearchEntities;
using System.ComponentModel;

namespace ShoppingAssistantAgent.Tools;

/// <summary>
/// Tool for searching products in the catalog
/// </summary>
public class SearchCatalogTool
{
    private readonly HttpClient httpClient;
    private readonly ILogger<SearchCatalogTool> _logger;

    public SearchCatalogTool(HttpClient httpClient, ILogger<SearchCatalogTool> logger)
    {
        this.httpClient = httpClient;
        _logger = logger;
    }

    [Description("Search for products in the catalog by name or description")]
    public async Task<string> SearchProductsAsync(
        [Description("The search query to find products")] string query)
    {
        try
        {
            _logger.LogInformation("Searching products with query: {Query}", query);

            var response = await httpClient.GetAsync($"/api/Product/search/{Uri.EscapeDataString(query)}");
            response.EnsureSuccessStatusCode();

            var searchResponse = await response.Content.ReadFromJsonAsync<SearchResponse>();

            if (searchResponse?.Products == null || searchResponse.Products.Count == 0)
            {
                return "No products found matching your search.";
            }

            var resultText = $"Found {searchResponse.Products.Count} product(s):\n\n";
            foreach (var product in searchResponse.Products.Take(5))
            {
                resultText += $"- **{product.Name}** (ID: {product.Id})\n";
                resultText += $"  Price: ${product.Price:F2}\n";
                if (!string.IsNullOrEmpty(product.Description))
                {
                    resultText += $"  {product.Description}\n";
                }
                resultText += "\n";
            }

            return resultText;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching products");
            return "Sorry, I encountered an error while searching for products.";
        }
    }
}
