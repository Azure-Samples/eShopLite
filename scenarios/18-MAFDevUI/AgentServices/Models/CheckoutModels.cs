using CartEntities;
using DataEntities;

namespace AgentServices.Models;

/// <summary>
/// DEMO: Request model for the Agent Checkout Orchestrator.
/// Contains all information needed for the multi-agent checkout workflow.
/// </summary>
public class AgentCheckoutRequest
{
    /// <summary>
    /// The shopping cart to process.
    /// </summary>
    public Cart Cart { get; set; } = new();

    /// <summary>
    /// The customer's membership tier for discount calculation.
    /// </summary>
    public MembershipTier MembershipTier { get; set; } = MembershipTier.Normal;
}

/// <summary>
/// DEMO: Response model from the Agent Checkout Orchestrator.
/// Contains the complete checkout result including stock validation, discounts, and agent steps.
/// </summary>
public class AgentCheckoutResult
{
    /// <summary>
    /// Updated subtotal (may change if stock adjustments were made).
    /// </summary>
    public decimal Subtotal { get; set; }

    /// <summary>
    /// The discount amount applied.
    /// </summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>
    /// Human-readable reason for the discount.
    /// </summary>
    public string? DiscountReason { get; set; }

    /// <summary>
    /// The total after discount (before tax).
    /// </summary>
    public decimal TotalAfterDiscount { get; set; }

    /// <summary>
    /// Log of agent steps for demo visibility.
    /// </summary>
    public List<AgentStep> AgentSteps { get; set; } = new();

    /// <summary>
    /// Whether the entire checkout process was successful.
    /// </summary>
    public bool Success { get; set; } = true;

    /// <summary>
    /// Error message if the checkout failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Represents the workflow in Mermaid syntax for visualization in the demo.
    /// </summary>
    public string? WorkFlowMermaid { get; set; }
}
