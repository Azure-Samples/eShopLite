using McpToolsEntities;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;
using SearchEntities;
using Services;
using System.ComponentModel;

namespace eShopMcpSseServer.Tools;

// DEMO TALKING POINT: ResearchProductsOnline shows how an MCP tool can CHAIN capabilities:
// it does an online search, uses the LLM to derive a store-relevant query from the results,
// then runs a semantic catalog search — all inside a single named, callable tool.
[McpServerToolType]
public class OnlineResearch
{
    // ResearchProductsOnline is a "research-then-recommend" tool.
    // The [Description] is deliberately explicit about what it does internally (online search
    // → query derivation → catalog search) so the model knows it's heavier than SearchStoreCatalog
    // and should only be called when live online context genuinely adds value.
    // It still returns ProductsSearchToolResponse so the same product-grid rendering path works.
    [McpServerTool(Name = "ResearchProductsOnline"),
        Description("Store operations: researches a topic online and then recommends matching catalog products. " +
                    "Performs a live online search, derives a product query from the findings, " +
                    "and runs a semantic catalog search. " +
                    "Returns the online research summary plus matching store products. " +
                    "Use this when the question benefits from current online context " +
                    "(e.g. trending gear, recent trail reports). " +
                    "For general product queries, prefer SearchStoreCatalog.")]
    public async Task<ProductsSearchToolResponse> ResearchProductsOnline(
     ILogger<ProductService> logger,
     OnlineResearcherService researcherService,
     IChatClient chatClient,
     ProductService productService,
     [Description("The search query to be used in the online search")] string query)
    {
        // Step 1: Perform a live online search (Bing Search API) for current context
        var researchResponse = await researcherService.Search(query);

        // Step 2: Ask the LLM to distill the raw online research into a clean product query
        // that will work well against the vector database's outdoor-products index
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

        // Step 3: Run a semantic catalog search using the LLM-derived query
        SearchResponse response = new();
        try
        {
            // useSemanticSearch: true → vector embedding search over product descriptions
            response = await productService.Search(queryFromChatClient, true);

            // Tag the tool name so the MCP client shows it in "Function Call Details"
            response.McpFunctionCallName = "ResearchProductsOnline";

            // Use the original online research as the human-readable response text
            // so the audience sees the live research context alongside the product grid
            response.Response = researchResponse.SearchResults;
        }
        catch (Exception ex)
        {
            logger.LogError($"Error during Search: {ex.Message}");
            response.Response = $"No response. {ex}";
        }

        // Return the same ProductsSearchToolResponse wrapper used by the catalog tools
        // so McpServerService can accumulate its products into the grid alongside other tools
        return new ProductsSearchToolResponse()
        {
            SearchResponse = response
        };
    }
}
