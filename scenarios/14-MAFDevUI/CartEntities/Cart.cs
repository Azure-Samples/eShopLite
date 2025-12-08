namespace CartEntities;

public class Cart
{
    public List<CartItem> Items { get; set; } = new();
    public decimal Subtotal => Items.Sum(item => item.Total);
    public decimal Tax => TotalAfterDiscount * 0.08m; // 8% tax rate applied after discount
    public decimal Total => TotalAfterDiscount + Tax;
    public int ItemCount => Items.Sum(item => item.Quantity);

    // DEMO: Agentic checkout discount fields
    public decimal DiscountAmount { get; set; } = 0;
    public string? DiscountReason { get; set; }
    public decimal TotalAfterDiscount => Subtotal - DiscountAmount;

    // DEMO: Agent steps log for visibility during demo
    public List<AgentStep> AgentSteps { get; set; } = new();

    // Represents the workflow in Mermaid syntax for visualization in the demo.
    public string? WorkFlowMermaid { get; set; }
}
