using McpToolsEntities;
using ModelContextProtocol.Server;
using Services;
using System.ComponentModel;

namespace McpSample.AspNetCoreSseServer;

[McpServerToolType]
public class WeatherTool
{
    /// <summary>
    /// Sample prompts that trigger this function:
    /// 1. "What's the weather for my trip to Seattle so I know what gear to pack?"
    /// 2. "I'm heading to New York City this weekend — what should I wear outdoors?"
    /// 3. "Check the weather in Paris so you can recommend the right jacket"
    /// </summary>
    [McpServerTool(Name = "GetTripWeather"), 
        Description("Store operations: gets current weather conditions for a destination city so the store assistant can recommend appropriate outdoor gear for the trip. Use this when the user mentions a destination or asks what to pack/wear. Returns the city name and a description of current weather conditions.")]
    public async Task<WeatherToolResponse> GetTripWeather(
        WeatherService weatherService,
        ILogger<ProductService> logger,
        IMcpServer currentMcpServer,
        [Description("The name of the destination city to get the trip weather for")] string cityName)
    {
        Console.WriteLine("==========================");
        Console.WriteLine($"Function Start GetTripWeather called with cityName: {cityName}");

        var response = await weatherService.GetWeather(cityName);

        Console.WriteLine($"Function End WeatherTool");
        Console.WriteLine("==========================");

        return response;        
    }
}
