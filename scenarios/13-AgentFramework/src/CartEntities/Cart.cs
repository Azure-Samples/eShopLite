using System.Text.Json.Serialization;

namespace CartEntities;

public class Cart
{
    public List<CartItem> Items { get; set; } = new();
    public decimal Subtotal => Items.Sum(item => item.Total);
    public decimal Tax => Subtotal * 0.08m; // 8% tax rate
    public decimal Total => Subtotal + Tax;
    public int ItemCount => Items.Sum(item => item.Quantity);
}