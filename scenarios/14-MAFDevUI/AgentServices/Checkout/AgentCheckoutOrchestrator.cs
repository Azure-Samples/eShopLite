using System.Text.Json;
using AgentServices.Configuration;
using AgentServices.Discount;
using AgentServices.Models;
using AgentServices.Stock;
using AgentServices.Triage;
using CartEntities;
using DataEntities;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AgentServices.Checkout;

/// <summary>
/// Agent Checkout Orchestrator using Microsoft Agent Framework Workflows Orchestrations - Sequential
/// Coordinates the multi-agent checkout workflow using AgentWorkflowBuilder.BuildSequential:
/// 1. StockAgent → validates availability
/// 2. DiscountAgent → applies tier-based discount
/// 3. Returns combined result with agent steps log
/// 
/// This implementation uses the registered AIAgents from the ServiceProvider directly,
/// following the pattern from: https://learn.microsoft.com/en-us/agent-framework/user-guide/workflows/orchestrations/sequential
/// </summary>
public class AgentCheckoutOrchestrator
{
    private readonly ILogger<AgentCheckoutOrchestrator> _logger;

    private readonly AIAgent _stockAgent;
    private readonly AIAgent _discountAgent;
    private readonly AIAgent _triageAgent;
    private Workflow _workflow;

    // Workflow name for identification in logs and debugging
    public const string WorkflowNameSequential = "CheckoutWorkflowSequential";
    public const string WorkflowNameHandOff = "CheckoutWorkflowHandOff";

    public AgentCheckoutOrchestrator(
        IServiceProvider serviceProvider,
        AgentSettings settings,
        ILogger<AgentCheckoutOrchestrator> logger)
    {
        _logger = logger;
        _stockAgent = serviceProvider.GetRequiredKeyedService<AIAgent>(StockAgentService.AgentName);
        _discountAgent = serviceProvider.GetRequiredKeyedService<AIAgent>(DiscountAgentService.AgentName);
        _triageAgent = serviceProvider.GetRequiredKeyedService<AIAgent>(TriageAgentService.AgentName);
        _workflow = serviceProvider.GetKeyedService<Workflow>(WorkflowNameSequential);
    }

    /// <summary>
    /// Execute the multi-agent checkout workflow using Microsoft Agent Framework Sequential Orchestration.
    /// Uses AgentWorkflowBuilder.BuildSequential to chain the StockAgent and DiscountAgent.
    /// </summary>
    public async Task<AgentCheckoutResult> ProcessCheckoutAsync(AgentCheckoutRequest request)
    {
        _logger.LogInformation("Starting agentic checkout pipeline using Sequential Workflow Orchestration");
        _logger.LogInformation("Membership tier: {Tier}, Cart subtotal: {Subtotal:C}",
            request.MembershipTier, request.Cart.Subtotal);

        var result = new AgentCheckoutResult
        {
            Subtotal = request.Cart.Subtotal,
            AgentSteps = []
        };

        try
        {
            _logger.LogInformation("Building Sequential Workflow with StockAgent → DiscountAgent");

            // Build a handoff workflow using AgentWorkflowBuilder
            if(_workflow == null && _triageAgent != null)
            {
                var handoffWorkflow = AgentWorkflowBuilder.CreateHandoffBuilderWith(_triageAgent)
                    .WithHandoffs(_triageAgent, [_stockAgent, _discountAgent]) // Triage routes to specialists
                    .WithHandoff(_stockAgent, _triageAgent)                    // Stock can return to triage
                    .WithHandoff(_discountAgent, _triageAgent)                 // Discount can return to triage
                    .Build();
                _workflow = handoffWorkflow;
            }


            // Build the input message for the workflow
            var itemsSummary = string.Join(", ", request.Cart.Items.Select(i => $"{i.Name} (${i.Price:F2} x {i.Quantity})"));
            var inputMessage = $$"""
                Customer checkout request:
                Membership tier: {{request.MembershipTier}}
                Cart subtotal: ${{request.Cart.Subtotal:F2}}
                Items: {{itemsSummary}}
                
                Step 1 (StockAgent): Check stock availability for all items. All items are in stock for this demo.
                Step 2 (DiscountAgent): Calculate the discount based on the membership tier rules:
                - GOLD: 20% discount
                - SILVER: 10% discount
                - NORMAL: No discount
                
                Respond with JSON: { "discountAmount": <number>, "reason": "<string>" }
                """;

            _logger.LogInformation("Executing Sequential Workflow");

            // Execute the workflow using InProcessExecution
            var messages = new List<ChatMessage> { new(ChatRole.User, inputMessage) };
            var run = await InProcessExecution.RunAsync(_workflow, messages);

            // Process all workflow events from both agents
            var workflowEvents = run.OutgoingEvents
                .OfType<AgentRunResponseEvent>()
                .ToList();

            // Record StockAgent step from workflow events
            var stockEvent = workflowEvents.FirstOrDefault(e =>
                e.Response.Text?.Contains("stock", StringComparison.OrdinalIgnoreCase) == true ||
                e.Response.Text?.Contains("ship", StringComparison.OrdinalIgnoreCase) == true);

            var stockStep = new AgentStep
            {
                Name = StockAgentService.AgentName,
                Timestamp = DateTime.UtcNow,
                Status = "Success",
                Message = stockEvent?.Response.Text?.Trim() ?? "All items are in stock and ready to ship!"
            };
            result.AgentSteps.Add(stockStep);
            _logger.LogInformation("StockAgent completed - Status: {Status}", stockStep.Status);

            // Record DiscountAgent step from workflow events (last event is typically from the last agent)
            var discountEvent = workflowEvents.LastOrDefault();

            var discountStep = new AgentStep
            {
                Name = DiscountAgentService.AgentName,
                Timestamp = DateTime.UtcNow
            };

            if (discountEvent?.Response.Text != null)
            {
                _logger.LogInformation("Sequential Workflow completed. Parsing response...");
                var discountResult = ParseDiscountResponse(discountEvent.Response.Text, request.Cart.Subtotal, request.MembershipTier);

                result.DiscountAmount = discountResult.DiscountAmount;
                result.DiscountReason = discountResult.DiscountReason;
                result.TotalAfterDiscount = discountResult.TotalAfterDiscount;

                discountStep.Status = discountResult.Success ? "Success" : "Warning";
                discountStep.Message = discountResult.DiscountReason;

                _logger.LogInformation("DiscountAgent completed - Discount: {Discount:C}",
                    discountResult.DiscountAmount);
            }
            else
            {
                _logger.LogWarning("No response from workflow, using fallback discount calculation");
                var fallbackResult = ComputeFallbackDiscount(request.MembershipTier, request.Cart.Subtotal);

                result.DiscountAmount = fallbackResult.DiscountAmount;
                result.DiscountReason = fallbackResult.DiscountReason;
                result.TotalAfterDiscount = fallbackResult.TotalAfterDiscount;

                discountStep.Status = "Warning";
                discountStep.Message = fallbackResult.DiscountReason;
            }

            result.AgentSteps.Add(discountStep);

            // Finalize result
            result.Success = true;
            _logger.LogInformation("Sequential Workflow Orchestration completed successfully");
            _logger.LogInformation("Final - Subtotal: {Subtotal:C}, Discount: {Discount:C}, Total: {Total:C}",
                result.Subtotal, result.DiscountAmount, result.TotalAfterDiscount);

            // save the mermaid diagram for the workflow execution for debugging and visualization
            result.WorkFlowMermaid = _workflow.ToMermaidString();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sequential Workflow Orchestration failed");

            result.AgentSteps.Add(new AgentStep
            {
                Name = "Orchestrator",
                Status = "Error",
                Message = $"Workflow failed: {ex.Message}",
                Timestamp = DateTime.UtcNow
            });

            // Fallback to deterministic discount calculation
            var fallbackResult = ComputeFallbackDiscount(request.MembershipTier, request.Cart.Subtotal);
            result.DiscountAmount = fallbackResult.DiscountAmount;
            result.DiscountReason = $"Checkout running in standard mode - {fallbackResult.DiscountReason}";
            result.TotalAfterDiscount = fallbackResult.TotalAfterDiscount;
            result.Success = true; // Still mark as success with fallback
            result.ErrorMessage = ex.Message;

            return result;
        }
    }

    /// <summary>
    /// Parse the discount response from the workflow.
    /// </summary>
    private DiscountResult ParseDiscountResponse(string content, decimal subtotal, MembershipTier tier)
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
            _logger.LogWarning(ex, "Failed to parse workflow response, using fallback");
            return ComputeFallbackDiscount(tier, subtotal);
        }
    }

    /// <summary>
    /// Deterministic fallback when workflow or AI is unavailable.
    /// </summary>
    private static DiscountResult ComputeFallbackDiscount(MembershipTier tier, decimal subtotal)
    {
        var (percentage, reason) = tier switch
        {
            MembershipTier.Gold => (0.20m, "Gold member 20% discount applied"),
            MembershipTier.Silver => (0.10m, "Silver member 10% discount applied"),
            _ => (0m, "No discount - Standard membership")
        };

        var discountAmount = subtotal * percentage;

        return new DiscountResult
        {
            DiscountAmount = discountAmount,
            DiscountReason = reason,
            TotalAfterDiscount = subtotal - discountAmount,
            Success = true
        };
    }
}
