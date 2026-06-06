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
    /// 1. "What's the current weather in Seattle?"
    /// 2. "Tell me the weather forecast for New York City"
    /// 3. "How is the weather in Paris today?"
    /// </summary>
    [McpServerTool(Name = "GetWeatherForCity"), 
        Description("Retrieves the current weather conditions for a specified city. Use this function when the user asks about weather in a specific location. Returns both the city name and a textual description of current weather conditions.")]
    public async Task<WeatherToolResponse> GetWeatherForCity(
        WeatherService weatherService,
        ILogger<ProductService> logger,
        IMcpServer currentMcpServer,
        [Description("The name of the city to get the weather information")] string cityName)
    {
        Console.WriteLine("==========================");
        Console.WriteLine($"Function Start WeatherTool: GetWeatherForCity called with cityName: {cityName}");

        var response = await weatherService.GetWeather(cityName);

        Console.WriteLine($"Function End WeatherTool");
        Console.WriteLine("==========================");

        return response;        
    }
}
