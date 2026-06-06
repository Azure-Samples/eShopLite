using A2A;
using Products.Models;
using SearchEntities;
using System.Text.Json;

namespace Products.Services.Agents;

/// <summary>
/// Researcher Agent using A2A .NET SDK
/// </summary>
public class ResearcherAgent
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ResearcherAgent> _logger;
    private readonly AgentSkill _skill;

    public ResearcherAgent(IHttpClientFactory httpClientFactory, ILogger<ResearcherAgent> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        
        // Define the agent skill using A2A SDK
        _skill = new AgentSkill
        {
            Id = "get_insights",
            Name = "Get Product Insights",
            Description = "Get product insights and reviews",
            Tags = new List<string> { "insights", "reviews", "analysis" },
            Examples = new List<string> { "Get insights for product 123" },
            InputModes = new List<string> { "text" },
            OutputModes = new List<string> { "json" }
        };
    }

    public AgentSkill Skill => _skill;

    public async Task<ResearchResponse?> GetInsightsAsync(string productId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ResearcherAgent");
            var request = new ResearchRequest(productId);
            
            var response = await client.PostAsJsonAsync("/api/researcher/insights", request);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ResearchResponse>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get insights for product {productId}", productId);
        }
        
        return null;
    }

    /// <summary>
    /// A2A SDK compatible method for handling insights messages
    /// </summary>
    public async Task<string> HandleInsightsAsync(Message message)
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

            var result = await GetInsightsAsync(productId);
            
            if (result != null)
            {
                return JsonSerializer.Serialize(result);
            }
            
            return JsonSerializer.Serialize(new { error = "Failed to retrieve insights" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling insights message");
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
public record ResearchRequest(string ProductId);
public record ResearchResponse(string ProductId, List<A2AInsight> Insights);