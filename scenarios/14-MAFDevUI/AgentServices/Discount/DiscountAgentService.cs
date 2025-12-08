using System.Text.Json;
using AgentServices.Models;
using DataEntities;
using Microsoft.Agents.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AgentServices.Discount;

/// <summary>
/// DEMO: Discount Agent Service
/// Uses Microsoft Agent Framework with Azure OpenAI to compute membership-based discounts.
/// Demonstrates the AIAgent pattern for intent-driven business logic.
/// </summary>
public class DiscountAgentService
{
    private readonly ILogger<DiscountAgentService> _logger;
    private readonly AIAgent _discountAgent;

    // Agent name for identification in logs and debugging
    public const string AgentName = "DiscountAgent";

    // Agent instructions (system prompt) for the discount agent
    // This defines the discount rules that the AI agent will follow
    public const string AgentInstructions = """
        You are an e-commerce pricing assistant.
        
        Rules:
        - If the customer is GOLD, apply 20% discount to the subtotal.
        - If SILVER, apply 10% discount to the subtotal.
        - For NORMAL (Regular) or any other tier, no discount.
        - Never apply negative discounts.
        - Calculate the discount amount based on the subtotal provided.
        
        Respond strictly with JSON in this format:
        { "discountAmount": <number>, "reason": "<string>" }
        
        Example for GOLD with $100 subtotal:
        { "discountAmount": 20.00, "reason": "Gold member 20% discount applied" }
        
        Example for NORMAL with $100 subtotal:
        { "discountAmount": 0, "reason": "No discount - Standard membership" }
        """;

    public DiscountAgentService(
        IServiceProvider serviceProvider,
        ILogger<DiscountAgentService> logger)
    {
        _logger = logger;
        _discountAgent = serviceProvider.GetRequiredKeyedService<AIAgent>(AgentName);
    }

    /// <summary>
    /// DEMO: Compute discount using the Microsoft Agent Framework AIAgent.
    /// The AIAgent wraps the chat client and provides a higher-level abstraction
    /// for building agentic applications with intent-driven logic.
    /// </summary>
    public async Task<DiscountResult> ComputeDiscountAsync(DiscountRequest request)
    {
        _logger.LogInformation("DEMO: {AgentName} starting - Tier: {Tier}, Subtotal: {Subtotal:C}", 
            AgentName, request.Tier, request.Subtotal);

        // If chat client is not available, use fallback deterministic logic
        if (_discountAgent == null)
        {
            _logger.LogWarning("DEMO: AI not available, using fallback discount logic");
            return ComputeFallbackDiscount(request);
        }

        try
        {
            // DEMO: Build the user message with cart context
            var itemsSummary = string.Join(", ", request.Items.Select(i => $"{i.Name} (${i.Price:F2} x {i.Quantity})"));
            var userMessage = $"""
                Customer membership tier: {request.Tier}
                Cart subtotal: ${request.Subtotal:F2}
                Items: {itemsSummary}
                
                Calculate the discount based on the membership tier rules.
                """;

            _logger.LogInformation("DEMO: Sending request to {AgentName} via Agent Framework", AgentName);
            
            // DEMO: Use the Agent Framework's RunAsync method
            // This is the key change from direct ChatClient calls - the agent handles 
            // conversation context, system prompts, and message formatting internally
            var response = await _discountAgent.RunAsync(userMessage);
            var content = response.Text ?? "";

            _logger.LogInformation("DEMO: {AgentName} response: {Response}", AgentName, content);

            // DEMO: Parse the JSON response
            var result = ParseAgentResponse(content, request.Subtotal);
            _logger.LogInformation("DEMO: {AgentName} computed - Amount: {Amount:C}, Reason: {Reason}", 
                AgentName, result.DiscountAmount, result.DiscountReason);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DEMO: {AgentName} error, using fallback", AgentName);
            return ComputeFallbackDiscount(request);
        }
    }

    private DiscountResult ParseAgentResponse(string content, decimal subtotal)
    {
        try
        {
            // Extract JSON from response (handle markdown code blocks if present)
            var jsonContent = content.Trim();
            if (jsonContent.Contains("```json"))
            {
                var start = jsonContent.IndexOf("```json") + 7;
                var end = jsonContent.IndexOf("```", start);
                if (end > start)
                {
                    jsonContent = jsonContent.Substring(start, end - start).Trim();
                }
            }
            else if (jsonContent.Contains("```"))
            {
                var start = jsonContent.IndexOf("```") + 3;
                var end = jsonContent.IndexOf("```", start);
                if (end > start)
                {
                    jsonContent = jsonContent.Substring(start, end - start).Trim();
                }
            }

            using var doc = JsonDocument.Parse(jsonContent);
            var root = doc.RootElement;

            var discountAmount = root.GetProperty("discountAmount").GetDecimal();
            var reason = root.GetProperty("reason").GetString() ?? "Discount applied";

            // Validate and clamp discount
            if (discountAmount < 0) discountAmount = 0;
            if (discountAmount > subtotal) discountAmount = subtotal;

            return new DiscountResult
            {
                DiscountAmount = discountAmount,
                DiscountReason = reason,
                TotalAfterDiscount = subtotal - discountAmount,
                Success = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "DEMO: Failed to parse agent response, extracting manually");
            
            // Simple fallback parsing
            return new DiscountResult
            {
                DiscountAmount = 0,
                DiscountReason = "Could not parse agent response",
                TotalAfterDiscount = subtotal,
                Success = false
            };
        }
    }

    /// <summary>
    /// DEMO: Deterministic fallback when AI is unavailable.
    /// </summary>
    private DiscountResult ComputeFallbackDiscount(DiscountRequest request)
    {
        var (percentage, reason) = request.Tier switch
        {
            MembershipTier.Gold => (0.20m, "Gold member 20% discount applied"),
            MembershipTier.Silver => (0.10m, "Silver member 10% discount applied"),
            _ => (0m, "No discount - Standard membership")
        };

        var discountAmount = request.Subtotal * percentage;

        return new DiscountResult
        {
            DiscountAmount = discountAmount,
            DiscountReason = reason,
            TotalAfterDiscount = request.Subtotal - discountAmount,
            Success = true
        };
    }
}
