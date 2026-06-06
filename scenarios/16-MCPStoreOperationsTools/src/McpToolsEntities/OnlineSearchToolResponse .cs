using System.Text.Json.Serialization;

namespace McpToolsEntities;

public class OnlineSearchToolResponse : ToolResponse
{
    public OnlineSearchToolResponse()
    {
        ToolName = "OnlineSearchToolResponse ";
    }

    [JsonPropertyName("SearchTerm")]
    public string SearchTerm { get; set; } = string.Empty;

    [JsonPropertyName("SearchResults")]
    public string SearchResults { get; set; } = string.Empty;
}
