using McpToolsEntities;
using ModelContextProtocol.Server;
using SearchEntities;
using Services;
using System.ComponentModel;

namespace eShopMcpSseServer.Tools;

[McpServerToolType]
public class Products 
{
    [McpServerTool(Name = "SearchStoreCatalog"), 
        Description("Store operations: semantic search over the store's outdoor products catalog. Returns a text summary plus the matching Products. Use this when the user asks for product recommendations or items by concept, need, or category (for example 'gear for a rainy hike').")]
    public async Task<ProductsSearchToolResponse> SearchStoreCatalog(
        ProductService productService,
        ILogger<ProductService> logger,
        IMcpServer currentMcpServer,
        [Description("The search query to be used in the products search")] string query)
    {
        logger.LogInformation("==========================");
        logger.LogInformation($"Function Semantic Search products: {query}");

        SearchResponse response = new();
        try
        {
            // call the desired Endpoint
            response = await productService.Search(query, true);
            response.McpFunctionCallName = "SearchStoreCatalog";
        }
        catch (Exception ex)
        {
            logger.LogError($"Error during Semantic Search: {ex.Message}");
            response.Response = $"No response. {ex}";
        }

        logger.LogInformation($"Response: {response?.Response}");
        logger.LogInformation("==========================");
        return new ProductsSearchToolResponse()
        {
            SearchResponse = response
        };
    }

    [McpServerTool(Name = "LookupProductByName"),
    Description("Store operations: looks up products by matching the query string against product names only. Use this when the user is looking for products by a specific name or keyword that appears in product names. Do not use for semantic/concept searches or recommendations. Returns matching products and their details.")]
    public async Task<ProductsSearchToolResponse> LookupProductByName(
    ProductService productService,
    ILogger<ProductService> logger,
    IMcpServer currentMcpServer,
    [Description("The search query to be used in the products search")] string query)
    {
        logger.LogInformation("==========================");
        logger.LogInformation($"Function Keyword Search products: {query}");

        SearchResponse response = new();
        try
        {
            // call the desired Endpoint
            response = await productService.Search(query, false);
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
