using System.Text.Json.Serialization;

namespace DataEntities;

public class Product
{
    public Product()
    {
        Id = 0;
        Name = "not defined";
        Description = "not defined";
        Price = 0;
        ImageUrl = "not defined";
    }

    [JsonPropertyName("id")]
    public virtual int Id { get; set; }

    [JsonPropertyName("name")]
    public virtual string Name { get; set; }

    [JsonPropertyName("description")]
    public virtual string Description { get; set; }

    [JsonPropertyName("price")]
    public virtual decimal Price { get; set; }

    [JsonPropertyName("imageUrl")]
    public virtual string ImageUrl { get; set; }
}


[JsonSerializable(typeof(List<Product>))]
public sealed partial class ProductSerializerContext : JsonSerializerContext
{
}