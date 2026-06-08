using McpToolsEntities;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;
using SearchEntities;
using Services;
using System.ComponentModel;

namespace eShopMcpSseServer.Tools;

[McpServerToolType]
public class OnlineResearch
{
    [McpServerTool(Name = "ResearchProductsOnline"), 
        Description("Store operations: researches a topic online and then recommends matching catalog products. Performs an online search, derives a product query from the findings, and runs a semantic catalog search. Returns the online research summary plus matching store products.")]
    public async Task<ProductsSearchToolResponse> ResearchProductsOnline(
     ILogger<ProductService> logger,
     OnlineResearcherService researcherService,
     IChatClient chatClient,
     ProductService productService,
     [Description("The search query to be used in the online search")] string query)
    {
        // 1. Perform an online search using the Bing Search APIs
        var researchResponse = await researcherService.Search(query);

        // 2. Create a search query from the research response to search for products
        var prompt = @$"Analyze the following response from an online search and generate a query to be used on a semantic search with a vector database for outdoor products.
Return only the query without any other information.
---
Online Research Result: 
{researchResponse.SearchResults}";

        var messages = new List<ChatMessage>
        {
            new(ChatRole.User, prompt)
        };
        var resultPrompt = await chatClient.GetResponseAsync(messages);
        var queryFromChatClient = resultPrompt.Text ?? "";


        // 3. Search the products vector database using the query generated from the online search
        SearchResponse response = new();
        try
        {
            // get products
            response = await productService.Search(queryFromChatClient, true);
            // define tool name
            response.McpFunctionCallName = "ResearchProductsOnline";
            // set the response as the original response from the research agent
            response.Response = researchResponse.SearchResults;
        }
        catch (Exception ex)
        {
            logger.LogError($"Error during Search: {ex.Message}");
            response.Response = $"No response. {ex}";
        }

        // 4. Return the response
        return new ProductsSearchToolResponse()
        {
            SearchResponse = response
        };
    }
}
