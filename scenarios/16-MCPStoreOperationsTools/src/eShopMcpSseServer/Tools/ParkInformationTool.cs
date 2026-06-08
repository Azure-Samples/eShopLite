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
    /// 1. "I'm visiting Yellowstone National Park — what gear do I need?"
    /// 2. "Tell me about Central Park so I can plan what to bring"
    /// 3. "I need a destination guide for the Grand Canyon"
    /// 4. "What's it like at Yosemite — help me plan my outdoor kit"
    /// </summary>
    [McpServerTool(Name = "GetDestinationGuide"), 
        Description("Store operations: retrieves a destination/park guide (location, opening hours, facilities, how to get there) so the store assistant can recommend the right outdoor products for the trip. Use this when the user asks about a park or outdoor destination they plan to visit.")]
    public async Task<ParkInformationToolResponse> GetDestinationGuide(
        ParkInformationService parkInformationService,
        ILogger<ProductService> logger,
        IMcpServer currentMcpServer,
        [Description("The name of the park or destination to get the guide for")] string parkName)
    {
        Console.WriteLine("==========================");
        Console.WriteLine($"Function Start GetDestinationGuide called with parkName: {parkName}");

        var response = await parkInformationService.GetParkInformation(parkName);

        Console.WriteLine($"Function End ParkInformationTool");
        Console.WriteLine("==========================");

        return response;        
    }
}
