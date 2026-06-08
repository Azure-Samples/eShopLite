using McpToolsEntities;
using ModelContextProtocol.Server;
using SearchEntities;
using Services;
using System.ComponentModel;

namespace eShopMcpSseServer.Tools;

// DEMO TALKING POINT: This class is decorated with [McpServerToolType] so that the MCP SDK
// automatically discovers and registers every public method decorated with [McpServerTool].
// The tool Name and Description are what the model sees when deciding which tool to call —
// the Description is essentially the "function signature" the LLM reads to pick the right tool.
[McpServerToolType]
public class Products
{
    // SearchStoreCatalog — the primary discovery tool for product/gear queries.
    // [McpServerTool(Name = ...)] names the tool exactly as it will appear in the UI's
    // "Function Call Details" panel and in the MCP tool list with checkboxes.
    // The [Description] text is sent to the model as the tool's purpose; it deliberately
    // includes examples ("gear for a rainy hike") so the model reliably picks this tool
    // for concept-based outdoor product queries rather than answering conversationally.
    [McpServerTool(Name = "SearchStoreCatalog"),
        Description("Store operations: semantic search over the store's outdoor products catalog. " +
                    "Returns a text summary plus the matching Products. " +
                    "ALWAYS call this tool when the user asks for product recommendations, gear suggestions, " +
                    "or items by concept, need, activity, or category " +
                    "(examples: 'gear for a rainy hike', 'winter camping equipment', 'what do I need for a trail run'). " +
                    "Prefer this over LookupProductByName for open-ended or descriptive queries.")]
    public async Task<ProductsSearchToolResponse> SearchStoreCatalog(
        ProductService productService,
        ILogger<ProductService> logger,
        IMcpServer currentMcpServer,
        [Description("The search query to be used in the products semantic search")] string query)
    {
        logger.LogInformation("==========================");
        logger.LogInformation($"SearchStoreCatalog called with query: {query}");

        SearchResponse response = new();
        try
        {
            // Delegate to the Products API semantic search (useSemanticSearch: true)
            // which uses vector embeddings to find conceptually related products
            response = await productService.Search(query, true);

            // Set the tool name so the MCP client can display it in "Function Call Details"
            response.McpFunctionCallName = "SearchStoreCatalog";
        }
        catch (Exception ex)
        {
            logger.LogError($"Error during Semantic Search: {ex.Message}");
            response.Response = $"No response. {ex}";
        }

        logger.LogInformation($"Response: {response?.Response}");
        logger.LogInformation("==========================");

        // Wrap in ProductsSearchToolResponse so the MCP client can detect and deserialize
        // the product list (the SearchResponse property carries Products + Response text)
        return new ProductsSearchToolResponse()
        {
            SearchResponse = response
        };
    }

    // LookupProductByName — keyword-based lookup for when the user names a specific product.
    // The Description steers the model away from using this for concept/category queries
    // (that's SearchStoreCatalog's job) and toward explicit name matches.
    [McpServerTool(Name = "LookupProductByName"),
        Description("Store operations: looks up products by matching the query string against product names only. " +
                    "Use this when the user asks for a product by a specific name or brand keyword " +
                    "(example: 'do you have the TrailBlaze X1 jacket?'). " +
                    "Do NOT use for open-ended concept/category searches or gear recommendations — " +
                    "use SearchStoreCatalog for those. Returns matching products and their details.")]
    public async Task<ProductsSearchToolResponse> LookupProductByName(
        ProductService productService,
        ILogger<ProductService> logger,
        IMcpServer currentMcpServer,
        [Description("The exact or partial product name to look up")] string query)
    {
        logger.LogInformation("==========================");
        logger.LogInformation($"LookupProductByName called with query: {query}");

        SearchResponse response = new();
        try
        {
            // Delegate to the Products API keyword search (useSemanticSearch: false)
            response = await productService.Search(query, false);

            // Set the tool name so the MCP client can display it in "Function Call Details"
            response.McpFunctionCallName = "LookupProductByName";
        }
        catch (Exception ex)
        {
            logger.LogError($"Error during Keyword Search: {ex.Message}");
            response.Response = $"No response. {ex}";
        }

        logger.LogInformation($"Response: {response?.Response}");
        logger.LogInformation("==========================");

        return new ProductsSearchToolResponse()
        {
            SearchResponse = response
        };
    }
}
