using System.Text.Json.Serialization;

namespace SearchEntities;

public class SearchResponse
{
    public SearchResponse()
    {
        Products = new List<DataEntities.Product>();
        Response = string.Empty;
        McpFunctionCallId = string.Empty;
        McpFunctionCallName = string.Empty;
    }

    [JsonPropertyName("Response")]
    public string Response { get; set; }

    [JsonPropertyName("McpFunctionCallId")]
    public string McpFunctionCallId { get; set; }

    [JsonPropertyName("McpFunctionCallName")]
    public string McpFunctionCallName { get; set; }

    [JsonPropertyName("Products")]
    public List<DataEntities.Product>? Products { get; set; }
}

[JsonSerializable(typeof(SearchResponse))]
public sealed partial class SearchResponseSerializerContext : JsonSerializerContext
{
}