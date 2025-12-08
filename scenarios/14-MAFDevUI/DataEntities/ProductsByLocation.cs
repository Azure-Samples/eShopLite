using System.Text.Json.Serialization;

namespace DataEntities;

public class ProductsByLocation
{
    public ProductsByLocation()
    {
        Id = 0;
        ProductId = 0;
        LocationId = 0;
        Quantity = 0;
    }

    [JsonPropertyName("id")]
    public virtual int Id { get; set; }

    [JsonPropertyName("productId")]
    public virtual int ProductId { get; set; }

    [JsonPropertyName("locationId")]
    public virtual int LocationId { get; set; }

    [JsonPropertyName("quantity")]
    public virtual int Quantity { get; set; }

    [JsonIgnore]
    public virtual Product? Product { get; set; }

    [JsonIgnore]
    public virtual Location? Location { get; set; }
}
