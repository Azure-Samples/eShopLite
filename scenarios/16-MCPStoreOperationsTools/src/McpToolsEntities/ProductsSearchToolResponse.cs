using SearchEntities;
using System.Text.Json.Serialization;

namespace McpToolsEntities;

public class ProductsSearchToolResponse : ToolResponse
{
    public ProductsSearchToolResponse()
    {
        ToolName = "ProductsSearchTool";
    }

    [JsonPropertyName("SearchResponse")]
    public SearchResponse SearchResponse { get; set; } = new SearchResponse();
}
