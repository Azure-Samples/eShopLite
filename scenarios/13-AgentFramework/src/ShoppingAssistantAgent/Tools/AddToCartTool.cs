using System.ComponentModel;
using System.Text;
using System.Text.Json;

namespace ShoppingAssistantAgent.Tools;

/// <summary>
/// Tool for adding products to the shopping cart
/// </summary>
public class AddToCartTool
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AddToCartTool> _logger;

    public AddToCartTool(HttpClient httpClient, ILogger<AddToCartTool> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    [Description("Add a product to the shopping cart")]
    public async Task<string> AddProductToCartAsync(
        [Description("The ID of the product to add to cart")] string productId,
        [Description("The quantity to add (default is 1)")] int quantity = 1)
    {
        try
        {
            _logger.LogInformation("Adding product {ProductId} (quantity: {Quantity}) to cart", productId, quantity);

            if (quantity <= 0)
            {
                return "Quantity must be greater than zero.";
            }

            // First check if product exists
            var checkResponse = await _httpClient.GetAsync($"/api/products/{productId}");
            if (!checkResponse.IsSuccessStatusCode)
            {
                return $"Product with ID {productId} not found.";
            }

            // Add to cart
            var cartRequest = new { ProductId = productId, Quantity = quantity };
            var content = new StringContent(
                JsonSerializer.Serialize(cartRequest),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("/api/cart/add", content);

            if (response.IsSuccessStatusCode)
            {
                return $"✅ Successfully added {quantity} unit(s) of product {productId} to your cart!";
            }
            else
            {
                return "Sorry, I couldn't add the product to your cart at this time. Please try again.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding product {ProductId} to cart", productId);
            return "Sorry, I encountered an error while adding the product to your cart.";
        }
    }
}
