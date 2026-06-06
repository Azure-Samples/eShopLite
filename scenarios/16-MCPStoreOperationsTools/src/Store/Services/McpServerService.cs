using McpToolsEntities;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using SearchEntities;
using Services;
using System.Text.Json;

namespace Store.Services;

public class McpServerService
{
    private readonly ILogger<ProductService> logger;
    IMcpClient mcpClient = null!;
    IList<McpClientTool> tools = null!;
    private IChatClient? chatClient;
    private IList<ChatMessage> ChatMessages = [];

    public McpServerService(ILogger<ProductService> _logger, IMcpClient _mcpClient, IChatClient? _chatClient)
    {
        logger = _logger;
        mcpClient = _mcpClient;
        chatClient = _chatClient;

        // get mcp server tools
        tools = mcpClient.ListToolsAsync().GetAwaiter().GetResult();
    }

    public IList<McpClientTool> GetTools() => tools;

    public async Task<SearchResponse?> GetResponseAsync(string searchTerm,
        IList<McpClientTool>? selectedTools = null)
    {
        try
        {
            ChatMessages.Clear();
            ChatMessages.Add(new ChatMessage(ChatRole.User, searchTerm));

            ChatOptions chatOptions = new ChatOptions
            {
                Tools = [.. selectedTools]
            };
            var responseComplete = await chatClient.GetResponseAsync(
                ChatMessages,
                chatOptions);
            logger.LogInformation($"Model Response: {responseComplete}");
            ChatMessages.AddMessages(responseComplete);

            // create search response
            SearchResponse searchResponse = new SearchResponse { Response = responseComplete.Text };


            // iterate through the messages
            foreach (var message in responseComplete.Messages.Where(m => m.Role == ChatRole.Tool))
            {
                if (message.Contents.FirstOrDefault() is FunctionResultContent functionResult)
                {
                    try
                    {
                        var functionResultJson = JsonDocument.Parse(functionResult.Result.ToString());
                        var searchResponseJson = functionResultJson.RootElement.GetProperty("content").EnumerateArray().FirstOrDefault().GetProperty("text").GetString();

                        var deserializedToolResponse = DeserializeResponseJson(searchResponseJson!);
                        if (deserializedToolResponse != null)
                        {
                            searchResponse.McpFunctionCallId = functionResult.CallId;
                            searchResponse.McpFunctionCallName ??= deserializedToolResponse.ToolName;

                            switch (deserializedToolResponse)
                            {
                                case ProductsSearchToolResponse productsSearchToolResponse:
                                    searchResponse.McpFunctionCallName = productsSearchToolResponse.SearchResponse.McpFunctionCallName;
                                    searchResponse.Products = productsSearchToolResponse.SearchResponse.Products;
                                    searchResponse.Response = productsSearchToolResponse.SearchResponse.Response;
                                    break;
                                case WeatherToolResponse weatherToolResponse:
                                    searchResponse.Response = weatherToolResponse.WeatherCondition;
                                    break;
                                case ParkInformationToolResponse parkInformationToolResponse:
                                    searchResponse.Response = parkInformationToolResponse.ParkInformation;
                                    break;
                            }
                        }
                    }
                    catch (Exception exc)
                    {
                        logger.LogError(exc, "Error deserializing function result JSON to SearchResponse object.");
                    }
                }
            }

            return searchResponse;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during Search.");
        }

        return new SearchResponse { Response = "No response" };
    }

    private ToolResponse? DeserializeResponseJson(string json)
    {
        try
        {
            using var jsonDoc = JsonDocument.Parse(json);
            var rootElement = jsonDoc.RootElement;

            if (rootElement.TryGetProperty("CityName", out _) || rootElement.TryGetProperty("WeatherCondition", out _))
                return JsonSerializer.Deserialize<WeatherToolResponse>(json);

            if (rootElement.TryGetProperty("ParkName", out _) || rootElement.TryGetProperty("ParkInformation", out _))
                return JsonSerializer.Deserialize<ParkInformationToolResponse>(json);

            if (rootElement.TryGetProperty("Products", out _) || rootElement.TryGetProperty("McpFunctionCallName", out _))
                return JsonSerializer.Deserialize<ProductsSearchToolResponse>(json);

            logger.LogWarning("Could not determine specific response type, defaulting to SearchResponse");
            return JsonSerializer.Deserialize<ProductsSearchToolResponse>(json);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deserializing JSON");
            return null;
        }
    }
}