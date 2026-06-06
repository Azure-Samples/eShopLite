using System.Text.Json.Serialization;

namespace McpToolsEntities;

public class ParkInformationToolResponse : ToolResponse
{
    public ParkInformationToolResponse()
    {
        ToolName = "ParkInformationTool";
    }

    [JsonPropertyName("ParkName")]
    public string ParkName { get; set; } = string.Empty;

    [JsonPropertyName("ParkInformation")]
    public string ParkInformation { get; set; } = string.Empty;

    [JsonPropertyName("OpeningHours")]
    public string OpeningHours { get; set; } = string.Empty;

    [JsonPropertyName("Location")]
    public string Location { get; set; } = string.Empty;

    [JsonPropertyName("TransportationType")]
    public string TransportationType { get; set; } = string.Empty;

    [JsonPropertyName("Facilities")]
    public string Facilities { get; set; } = string.Empty;

    [JsonPropertyName("ParkDescription")]
    public string ParkDescription { get; set; } = string.Empty;
}
