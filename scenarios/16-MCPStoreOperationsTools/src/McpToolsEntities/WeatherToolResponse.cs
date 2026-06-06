using System.Text.Json.Serialization;

namespace McpToolsEntities;

public class WeatherToolResponse : ToolResponse
{
    public WeatherToolResponse()
    {
        ToolName = "WeatherTool";
    }

    [JsonPropertyName("CityName")]
    public string CityName { get; set; } = string.Empty;

    [JsonPropertyName("WeatherCondition")]
    public string WeatherCondition { get; set; } = string.Empty;
}
