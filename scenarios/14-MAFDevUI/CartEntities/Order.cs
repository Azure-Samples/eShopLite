using System.Text.Json.Serialization;

namespace CartEntities;

public class Order
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public Customer Customer { get; set; } = new();
    public List<CartItem> Items { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; } = "Confirmed";

    // DEMO: Agentic checkout discount fields
    public decimal DiscountAmount { get; set; }
    public string? DiscountReason { get; set; }
    public decimal TotalAfterDiscount { get; set; }

    // DEMO: Agent steps log for visibility during demo
    public List<AgentStep> AgentSteps { get; set; } = new();
}