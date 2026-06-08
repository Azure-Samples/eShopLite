using System.Text.Json.Serialization;

namespace SearchEntities;

public class SearchResponse
{
    public SearchResponse()
    {
        Products = new List<DataEntities.Product>();
        Response = string.Empty;
    }

    [JsonPropertyName("id")]
    public string? Response { get; set; }

    [JsonPropertyName("products")]
    public List<DataEntities.Product>? Products { get; set; }

}


[JsonSerializable(typeof(SearchResponse))]
public sealed partial class SearchResponseSerializerContext : JsonSerializerContext
{
}