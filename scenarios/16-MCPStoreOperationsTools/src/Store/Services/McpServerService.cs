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

    // System prompt injected before every user query.
    // KEY INSIGHT: Without this, the model often answers conversationally for broad queries
    // like "what gear do I need for a rainy hike?" — giving text only, with no tool call,
    // so the product grid stays empty. This instruction tells the model it MUST call a
    // catalog tool (SearchStoreCatalog or LookupProductByName) to ground its answer in
    // real store inventory. Trip-context tools (GetTripWeather, GetDestinationGuide) may
    // also be called to enrich the answer, but never as a substitute for the catalog call.
    private const string CatalogSystemPrompt =
        "You are a helpful assistant for an outdoor gear store. " +
        "For ANY question about products, gear, equipment, or recommendations you MUST call " +
        "SearchStoreCatalog (for concept/category queries, e.g. 'gear for a rainy hike') or " +
        "LookupProductByName (when the user names a specific product) to ground your answer " +
        "in real store inventory. You may ALSO call GetTripWeather or GetDestinationGuide to " +
        "add trip context, but only IN ADDITION TO — never instead of — a catalog tool. " +
        "Always include real products from the catalog in your final answer.";

    public McpServerService(ILogger<ProductService> _logger, IMcpClient _mcpClient, IChatClient? _chatClient)
    {
        logger = _logger;
        mcpClient = _mcpClient;
        chatClient = _chatClient;

        // Discover all tools published by the MCP server at startup so the UI can list them
        tools = mcpClient.ListToolsAsync().GetAwaiter().GetResult();
    }

    public IList<McpClientTool> GetTools() => tools;

    public async Task<SearchResponse?> GetResponseAsync(string searchTerm,
        IList<McpClientTool>? selectedTools = null)
    {
        try
        {
            ChatMessages.Clear();

            // Prepend the system prompt so the model knows it must call a catalog tool.
            // This is what ensures product-query answers always populate the product grid.
            ChatMessages.Add(new ChatMessage(ChatRole.System, CatalogSystemPrompt));

            // Add the user's natural-language question
            ChatMessages.Add(new ChatMessage(ChatRole.User, searchTerm));

            // The ChatOptions.Tools list controls which MCP tools the model is ALLOWED to call.
            // Only the tools the user checked in the UI are passed here — this is the opt-in
            // tool-gating that lets the presenter demonstrate control by unchecking a tool.
            ChatOptions chatOptions = new ChatOptions
            {
                Tools = [.. selectedTools]
            };

            // Send the conversation to the chat client. The SDK will automatically:
            //   1. Call any tools the model requests (may happen multiple times in one turn)
            //   2. Feed tool results back to the model
            //   3. Return the final synthesized answer once the model stops calling tools
            var responseComplete = await chatClient.GetResponseAsync(
                ChatMessages,
                chatOptions);
            logger.LogInformation($"Model Response: {responseComplete}");
            ChatMessages.AddMessages(responseComplete);

            // The model's synthesized text is the richest response — use it as the primary answer.
            // Tool-level response text is supplementary and should not overwrite this.
            SearchResponse searchResponse = new SearchResponse { Response = responseComplete.Text };

            // Collect products from EVERY catalog-tool call in this turn.
            // Using an accumulator list prevents the "last-wins" bug where a later text-only
            // tool (WeatherTool, ParkInfoTool) would overwrite products found by an earlier one.
            var accumulatedProducts = new List<DataEntities.Product>();

            // Each Tool-role message corresponds to one MCP tool call the model made.
            // Iterate all of them so we capture results from every tool in a multi-tool turn.
            foreach (var message in responseComplete.Messages.Where(m => m.Role == ChatRole.Tool))
            {
                if (message.Contents.FirstOrDefault() is FunctionResultContent functionResult)
                {
                    try
                    {
                        // The MCP SDK wraps each tool return value in a JSON envelope:
                        //   { "content": [ { "text": "<serialized tool response JSON>" } ] }
                        var functionResultJson = JsonDocument.Parse(functionResult.Result.ToString());
                        var searchResponseJson = functionResultJson.RootElement
                            .GetProperty("content").EnumerateArray()
                            .FirstOrDefault()
                            .GetProperty("text").GetString();

                        // Detect the concrete response type from the JSON shape and deserialize
                        var deserializedToolResponse = DeserializeResponseJson(searchResponseJson!);
                        if (deserializedToolResponse != null)
                        {
                            // Track which tool ran so the UI can show it in "Function Call Details"
                            searchResponse.McpFunctionCallId ??= functionResult.CallId;
                            searchResponse.McpFunctionCallName ??= deserializedToolResponse.ToolName;

                            switch (deserializedToolResponse)
                            {
                                case ProductsSearchToolResponse productsSearchToolResponse:
                                    // Catalog tools (SearchStoreCatalog, LookupProductByName,
                                    // ResearchProductsOnline) return real products.
                                    // ADD to the running list — do NOT replace — so that two
                                    // catalog calls in one turn both contribute to the grid.
                                    searchResponse.McpFunctionCallName =
                                        productsSearchToolResponse.SearchResponse.McpFunctionCallName;
                                    if (productsSearchToolResponse.SearchResponse.Products?.Count > 0)
                                        accumulatedProducts.AddRange(
                                            productsSearchToolResponse.SearchResponse.Products);
                                    break;

                                case WeatherToolResponse:
                                    // Trip-context tool — text-only result.
                                    // Intentionally does NOT touch accumulatedProducts so products
                                    // found by an earlier catalog call are preserved.
                                    break;

                                case ParkInformationToolResponse:
                                    // Trip-context tool — text-only result.
                                    // Same as WeatherToolResponse: never clears accumulated products.
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

            // Deduplicate by product Id (or name as fallback) so the grid is clean when
            // multiple catalog tools return overlapping results (e.g. SearchStoreCatalog + ResearchProductsOnline)
            searchResponse.Products = accumulatedProducts
                .GroupBy(p => p.Id > 0 ? (object)p.Id : p.Name)
                .Select(g => g.First())
                .ToList();

            return searchResponse;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during Search.");
        }

        return new SearchResponse { Response = "No response" };
    }

    // Inspect the JSON returned by a tool call and deserialize it into the correct typed class.
    // Each tool method returns a different subtype of ToolResponse; we distinguish them by
    // checking for properties that are unique to each subtype:
    //   WeatherToolResponse         → "CityName" or "WeatherCondition" at root
    //   ParkInformationToolResponse → "ParkName" or "ParkInformation" at root
    //   ProductsSearchToolResponse  → "SearchResponse" wrapper at root (falls through to default)
    // The fallback deserializes to ProductsSearchToolResponse, which is the most common case.
    private ToolResponse? DeserializeResponseJson(string json)
    {
        try
        {
            using var jsonDoc = JsonDocument.Parse(json);
            var rootElement = jsonDoc.RootElement;

            // Weather tool: root object has CityName / WeatherCondition directly
            if (rootElement.TryGetProperty("CityName", out _) || rootElement.TryGetProperty("WeatherCondition", out _))
                return JsonSerializer.Deserialize<WeatherToolResponse>(json);

            // Park guide tool: root object has ParkName / ParkInformation directly
            if (rootElement.TryGetProperty("ParkName", out _) || rootElement.TryGetProperty("ParkInformation", out _))
                return JsonSerializer.Deserialize<ParkInformationToolResponse>(json);

            // Catalog tools wrap their payload in a nested "SearchResponse" object.
            // If we see that wrapper — or fall through — treat it as a ProductsSearchToolResponse.
            if (rootElement.TryGetProperty("SearchResponse", out _))
                return JsonSerializer.Deserialize<ProductsSearchToolResponse>(json);

            // Default: attempt to parse as a product response (covers legacy / unknown shapes)
            logger.LogWarning("Could not determine specific response type, defaulting to ProductsSearchToolResponse");
            return JsonSerializer.Deserialize<ProductsSearchToolResponse>(json);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deserializing JSON");
            return null;
        }
    }
}