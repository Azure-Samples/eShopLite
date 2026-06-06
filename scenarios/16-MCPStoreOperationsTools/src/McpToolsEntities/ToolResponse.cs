using System.Text.Json.Serialization;

namespace McpToolsEntities;

public class ToolResponse
{
    [JsonPropertyName("ToolName")]
    public string ToolName { get; set; } = string.Empty;
    
    [JsonPropertyName("ToolCallId")]
    public string ToolCallId { get; set; } = string.Empty;
}
