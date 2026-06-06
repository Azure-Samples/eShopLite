using McpToolsEntities;
using ModelContextProtocol.Server;
using Services;
using System.ComponentModel;

namespace McpSample.AspNetCoreSseServer;

[McpServerToolType]
public class ParkInformation
{
    /// <summary>
    /// Sample prompts that trigger this function:
    /// 1. "Tell me about Yellowstone National Park"
    /// 2. "What are the opening hours for Central Park?"
    /// 3. "I need information about Grand Canyon National Park"
    /// 4. "What facilities are available at Yosemite Park?"
    /// 5. "How do I get to Golden Gate Park?"
    /// </summary>
    [McpServerTool(Name = "GetParkInformation"), 
        Description("Retrieves comprehensive information about a specified park. Use this function when the user is asking questions about parks, their locations, opening hours, facilities, or how to get there. Returns detailed park information including name, description, location, opening hours, transportation options, and available facilities.")]
    public async Task<ParkInformationToolResponse> GetParkInformation(
        ParkInformationService parkInformationService,
        ILogger<ProductService> logger,
        IMcpServer currentMcpServer,
        [Description("The name of the park to get the information")] string parkName)
    {
        Console.WriteLine("==========================");
        Console.WriteLine($"Function Start ParkInformation: GetParkInformation called with parkName: {parkName}");

        var response = await parkInformationService.GetParkInformation(parkName);

        Console.WriteLine($"Function End ParkInformationTool");
        Console.WriteLine("==========================");

        return response;        
    }
}
