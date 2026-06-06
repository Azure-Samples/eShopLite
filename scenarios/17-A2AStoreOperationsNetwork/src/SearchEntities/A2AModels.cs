using System.Text.Json.Serialization;

namespace SearchEntities;

public class A2ASearchResponse
{
    public A2ASearchResponse()
    {
        Products = new List<A2AEnrichedProduct>();
        Response = string.Empty;
    }

    [JsonPropertyName("response")]
    public string Response { get; set; }

    [JsonPropertyName("products")]
    public List<A2AEnrichedProduct> Products { get; set; }
}

public class A2AEnrichedProduct
{
    [JsonPropertyName("productId")]
    public string ProductId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("imageUrl")]
    public string ImageUrl { get; set; } = string.Empty;

    [JsonPropertyName("stock")]
    public int Stock { get; set; }

    [JsonPropertyName("promotions")]
    public List<A2APromotion> Promotions { get; set; } = new();

    [JsonPropertyName("insights")]
    public List<A2AInsight> Insights { get; set; } = new();
}

public class A2APromotion
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("discount")]
    public int Discount { get; set; }
}

public class A2AInsight
{
    [JsonPropertyName("review")]
    public string Review { get; set; } = string.Empty;

    [JsonPropertyName("rating")]
    public double Rating { get; set; }
}

[JsonSerializable(typeof(A2ASearchResponse))]
public sealed partial class A2ASearchResponseSerializerContext : JsonSerializerContext
{
}