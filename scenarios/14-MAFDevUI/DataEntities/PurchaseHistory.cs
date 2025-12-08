using System.Text.Json.Serialization;

namespace DataEntities;

public class PurchaseHistory
{
    public PurchaseHistory()
    {
        Id = 0;
        CustomerId = 0;
        ProductId = 0;
        LocationId = 0;
        Quantity = 0;
        UnitPrice = 0;
        DiscountApplied = 0;
        TotalPrice = 0;
        PurchaseDate = DateTime.UtcNow;
    }

    [JsonPropertyName("id")]
    public virtual int Id { get; set; }

    [JsonPropertyName("customerId")]
    public virtual int CustomerId { get; set; }

    [JsonPropertyName("productId")]
    public virtual int ProductId { get; set; }

    [JsonPropertyName("locationId")]
    public virtual int LocationId { get; set; }

    [JsonPropertyName("quantity")]
    public virtual int Quantity { get; set; }

    [JsonPropertyName("unitPrice")]
    public virtual decimal UnitPrice { get; set; }

    [JsonPropertyName("discountApplied")]
    public virtual decimal DiscountApplied { get; set; }

    [JsonPropertyName("totalPrice")]
    public virtual decimal TotalPrice { get; set; }

    [JsonPropertyName("purchaseDate")]
    public virtual DateTime PurchaseDate { get; set; }

    [JsonIgnore]
    public virtual Customer? Customer { get; set; }

    [JsonIgnore]
    public virtual Product? Product { get; set; }

    [JsonIgnore]
    public virtual Location? Location { get; set; }
}
