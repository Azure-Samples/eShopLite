using A2A;
using Products.Models;
using SearchEntities;
using System.Text.Json;

namespace Products.Services.Agents;

/// <summary>
/// Promotions Agent using A2A .NET SDK
/// </summary>
public class PromotionsAgent
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<PromotionsAgent> _logger;
    private readonly AgentSkill _skill;

    public PromotionsAgent(IHttpClientFactory httpClientFactory, ILogger<PromotionsAgent> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        
        // Define the agent skill using A2A SDK
        _skill = new AgentSkill
        {
            Id = "get_promotions",
            Name = "Get Promotions",
            Description = "Get active promotions for a product",
            Tags = new List<string> { "promotions", "discounts", "offers" },
            Examples = new List<string> { "Get promotions for product 123" },
            InputModes = new List<string> { "text" },
            OutputModes = new List<string> { "json" }
        };
    }

    public AgentSkill Skill => _skill;

    public async Task<PromotionsResponse?> GetPromotionsAsync(string productId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("PromotionsAgent");
            var request = new PromotionsRequest(productId);
            
            var response = await client.PostAsJsonAsync("/api/promotions/active", request);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<PromotionsResponse>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get promotions for product {productId}", productId);
        }
        
        return null;
    }

    /// <summary>
    /// A2A SDK compatible method for handling promotions messages
    /// </summary>
    public async Task<string> HandlePromotionsAsync(Message message)
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

            var result = await GetPromotionsAsync(productId);
            
            if (result != null)
            {
                return JsonSerializer.Serialize(result);
            }
            
            return JsonSerializer.Serialize(new { error = "Failed to retrieve promotions" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling promotions message");
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
public record PromotionsRequest(string ProductId);
public record PromotionsResponse(string ProductId, List<A2APromotion> Promotions);