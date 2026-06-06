using McpToolsEntities;
using ModelContextProtocol.Server;
using SearchEntities;
using Services;
using System.ComponentModel;

namespace eShopMcpSseServer.Tools;

[McpServerToolType]
public class Products 
{
    [McpServerTool(Name = "SemanticSearchProducts"), 
        Description("Performs a search in the outdoor products catalog. Returns a text with the found products and the collection of Products in the Store. Use this function when the user asks for product recommendations or specific items.")]
    public async Task<ProductsSearchToolResponse> SemanticSearchProducts(
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
            response.McpFunctionCallName = "SemanticSearchProducts";
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

    [McpServerTool(Name = "KeyWordSearchProducts"),
    Description("Searches products in the database by matching the query string with product names only. Use this function when the user is looking for products by specific names or keywords that may appear in product names. Do not use this for semantic searches or when the user asks for product recommendations based on concepts or categories. Returns matching products and their details.")]
    public async Task<ProductsSearchToolResponse> KeyWordSearchProducts(
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
            response.McpFunctionCallName = "KeyWordSearchProducts";
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
