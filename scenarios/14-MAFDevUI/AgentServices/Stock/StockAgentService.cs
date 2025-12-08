using AgentServices.Configuration;
using AgentServices.Models;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AgentServices.Stock;

/// <summary>
/// Stock Agent Service
/// Uses Microsoft Agent Framework to validate stock availability and generate human-friendly summaries.
/// For demo purposes, stock is always available (deterministic), but uses AI for friendly messages.
/// Supports location-based stock queries when a LocationId is provided.
/// </summary>
public class StockAgentService
{
    private readonly ILogger<StockAgentService> _logger;
    private readonly AIAgent _stockAgent;

    // Agent name for identification in logs and debugging
    public const string AgentName = "StockAgent";

    // Agent instructions (system prompt) for stock agent message generation
    public const string AgentInstructions = """
        You are a friendly e-commerce stock checker assistant.
        Given a list of items and their stock status, generate a brief, friendly summary message.
        Be concise and positive. If all items are available, say something like "Great news! All items are in stock and ready to ship."
        If there are issues, mention them briefly.
        If checking stock at a specific location, mention the location name in your response.
        Respond with just the summary message, no JSON or formatting.
        """;

    public StockAgentService(
        IServiceProvider serviceProvider,
        ILogger<StockAgentService> logger)
    {
        _logger = logger;
        _stockAgent = serviceProvider.GetRequiredKeyedService<AIAgent>(AgentName);
    }

    /// <summary>
    /// Check stock availability for cart items using Microsoft Agent Framework.
    /// For demo purposes, all items are considered in stock.
    /// Supports optional location-based queries.
    /// </summary>
    public async Task<StockCheckResult> CheckStockAsync(StockCheckRequest request)
    {
        var locationInfo = request.LocationId.HasValue 
            ? $" at location ID {request.LocationId}" 
            : " across all locations";
        
        _logger.LogInformation("DEMO: {AgentName} starting - Checking {ItemCount} items{LocationInfo}", 
            AgentName, request.Items.Count, locationInfo);

        // DEMO: For demo purposes, all items are in stock
        // In a real scenario, this would query the Products API or database
        var result = new StockCheckResult
        {
            HasStockIssues = false,
            Issues = new List<StockIssue>(),
            Success = true,
            CheckedLocationId = request.LocationId
        };

        // Generate a friendly summary message using the Agent Framework
        result.SummaryMessage = await GenerateSummaryMessage(request, result);
        
        _logger.LogInformation("DEMO: {AgentName} completed - HasIssues: {HasIssues}, Message: {Message}", 
            AgentName, result.HasStockIssues, result.SummaryMessage);

        return result;
    }

    /// <summary>
    /// Check stock availability at a specific location.
    /// </summary>
    public async Task<StockCheckResult> CheckStockAtLocationAsync(StockCheckRequest request, int locationId, string? locationName = null)
    {
        request.LocationId = locationId;
        
        var result = await CheckStockAsync(request);
        result.CheckedLocationId = locationId;
        result.CheckedLocationName = locationName;
        
        return result;
    }

    private async Task<string> GenerateSummaryMessage(StockCheckRequest request, StockCheckResult result)
    {
        // If AI is not available, use fallback message
        if (_stockAgent == null)
        {
            _logger.LogDebug("AI not available, using fallback stock message");
            return GenerateFallbackMessage(request, result);
        }

        try
        {
            var itemsList = string.Join("\n", request.Items.Select(i => $"- {i.Name} (Qty: {i.Quantity}): In Stock"));
            var locationContext = request.LocationId.HasValue 
                ? $"\nLocation: {result.CheckedLocationName ?? $"Location #{request.LocationId}"}"
                : "";
            
            var userMessage = $"""
                Items to check:
                {itemsList}
                {locationContext}
                
                All items are available. Generate a brief, friendly confirmation message.
                """;

            _logger.LogDebug("DEMO: Sending request to {AgentName} via Agent Framework for summary", AgentName);
            
            // DEMO: Use the Agent Framework's RunAsync method
            // This encapsulates the message building and response handling
            var response = await _stockAgent.RunAsync(userMessage);
            var content = response.Text?.Trim() ?? "";

            if (!string.IsNullOrEmpty(content))
            {
                return content;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "DEMO: {AgentName} error, using fallback message", AgentName);
        }

        // Fallback
        return GenerateFallbackMessage(request, result);
    }

    private static string GenerateFallbackMessage(StockCheckRequest request, StockCheckResult result)
    {
        var locationSuffix = request.LocationId.HasValue && !string.IsNullOrEmpty(result.CheckedLocationName)
            ? $" at {result.CheckedLocationName}"
            : request.LocationId.HasValue
                ? $" at location #{request.LocationId}"
                : "";

        return result.HasStockIssues 
            ? $"Some items have limited availability{locationSuffix}. Please review your cart."
            : $"All items are in stock and ready to ship{locationSuffix}!";
    }
}
