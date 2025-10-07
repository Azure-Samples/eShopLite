using DataEntities;
using System.ComponentModel;

namespace ShoppingAssistantAgent.Tools;

/// <summary>
/// Tool for getting detailed product information
/// </summary>
public class ProductDetailsTool
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductDetailsTool> _logger;

    public ProductDetailsTool(HttpClient httpClient, ILogger<ProductDetailsTool> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    [Description("Get detailed information about a specific product by its ID")]
    public async Task<string> GetProductDetailsAsync(
        [Description("The ID of the product to get details for")] string productId)
    {
        try
        {
            _logger.LogInformation("Getting details for product: {ProductId}", productId);

            var response = await _httpClient.GetAsync($"/api/products/{productId}");

            if (!response.IsSuccessStatusCode)
            {
                return $"Product with ID {productId} not found.";
            }

            var product = await response.Content.ReadFromJsonAsync<Product>();

            if (product == null)
            {
                return $"Product with ID {productId} not found.";
            }

            var details = $"**{product.Name}**\n\n";
            details += $"- **Price:** ${product.Price:F2}\n";
            details += $"- **Description:** {product.Description}\n";

            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                details += $"- **Image:** {product.ImageUrl}\n";
            }

            return details;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product details for {ProductId}", productId);
            return $"Sorry, I encountered an error while retrieving details for product {productId}.";
        }
    }
}
