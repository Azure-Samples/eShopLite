using A2A;
using Products.Models;
using System.Text.Json;

namespace Products.Services.Agents;

/// <summary>
/// Inventory Agent using A2A .NET SDK
/// </summary>
public class InventoryAgent
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<InventoryAgent> _logger;
    private readonly AgentSkill _skill;

    public InventoryAgent(IHttpClientFactory httpClientFactory, ILogger<InventoryAgent> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        
        // Define the agent skill using A2A SDK
        _skill = new AgentSkill
        {
            Id = "check_inventory",
            Name = "Check Inventory",
            Description = "Check inventory levels for a product",
            Tags = new List<string> { "inventory", "stock", "product" },
            Examples = new List<string> { "Check stock for product 123" },
            InputModes = new List<string> { "text" },
            OutputModes = new List<string> { "json" }
        };
    }

    public AgentSkill Skill => _skill;

    public async Task<InventoryResponse?> CheckInventoryAsync(string productId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("InventoryAgent");
            var request = new InventoryRequest(productId);
            
            var response = await client.PostAsJsonAsync("/api/inventory/check", request);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<InventoryResponse>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get inventory for product {productId}", productId);
        }
        
        return null;
    }

    /// <summary>
    /// A2A SDK compatible method for handling inventory check messages
    /// </summary>
    public async Task<string> HandleInventoryCheckAsync(Message message)
    {
        try
        {
            // Extract product ID from message parts
            var textPart = message.Parts?.OfType<TextPart>().FirstOrDefault();
            if (textPart == null || string.IsNullOrEmpty(textPart.Text))
            {
                return JsonSerializer.Serialize(new { error = "No product ID provided" });
            }

            // Parse the product ID from the message
            var productId = ExtractProductIdFromMessage(textPart.Text);
            if (string.IsNullOrEmpty(productId))
            {
                return JsonSerializer.Serialize(new { error = "Invalid product ID format" });
            }

            var result = await CheckInventoryAsync(productId);
            
            if (result != null)
            {
                return JsonSerializer.Serialize(result);
            }
            
            return JsonSerializer.Serialize(new { error = "Failed to retrieve inventory" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling inventory check message");
            return JsonSerializer.Serialize(new { error = "Internal error occurred" });
        }
    }

    private string? ExtractProductIdFromMessage(string messageContent)
    {
        // Try to parse JSON first
        try
        {
            var jsonDoc = JsonDocument.Parse(messageContent);
            if (jsonDoc.RootElement.TryGetProperty("productId", out var productIdElement))
            {
                return productIdElement.GetString();
            }
        }
        catch
        {
            // If not JSON, treat as plain text product ID
            return messageContent.Trim();
        }

        return null;
    }
}

// Agent request/response models
public record InventoryRequest(string ProductId);
public record InventoryResponse(string ProductId, int Stock);