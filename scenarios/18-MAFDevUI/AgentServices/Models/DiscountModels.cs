using CartEntities;
using DataEntities;

namespace AgentServices.Models;

/// <summary>
/// DEMO: Request model for the Discount Agent.
/// Contains all information needed to compute membership-based discounts.
/// </summary>
public class DiscountRequest
{
    /// <summary>
    /// The customer's membership tier (Normal, Silver, Gold).
    /// </summary>
    public MembershipTier Tier { get; set; } = MembershipTier.Normal;

    /// <summary>
    /// Cart items for context (product names, quantities, prices).
    /// </summary>
    public IReadOnlyList<CartItem> Items { get; set; } = new List<CartItem>();

    /// <summary>
    /// The subtotal amount before any discounts.
    /// </summary>
    public decimal Subtotal { get; set; }
}

/// <summary>
/// DEMO: Response model from the Discount Agent.
/// Contains the calculated discount and explanation.
/// </summary>
public class DiscountResult
{
    /// <summary>
    /// The discount amount to apply.
    /// </summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>
    /// Human-readable explanation for the discount (e.g., "Gold member 20% discount applied").
    /// </summary>
    public string DiscountReason { get; set; } = string.Empty;

    /// <summary>
    /// The final total after discount.
    /// </summary>
    public decimal TotalAfterDiscount { get; set; }

    /// <summary>
    /// Whether the discount was successfully computed by the agent.
    /// </summary>
    public bool Success { get; set; } = true;
}
