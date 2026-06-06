using DataEntities;
using Microsoft.EntityFrameworkCore;
using A2A;
using Products.Models;
using Products.Services.Agents;
using SearchEntities;
using System.Text.Json;

namespace Products.Services;

public class A2AOrchestrationService : IA2AOrchestrationService
{
    private readonly Context _db;
    private readonly InventoryAgent _inventoryAgent;
    private readonly PromotionsAgent _promotionsAgent;
    private readonly ResearcherAgent _researcherAgent;
    private readonly ILogger<A2AOrchestrationService> _logger;

    public A2AOrchestrationService(
        Context db,
        InventoryAgent inventoryAgent,
        PromotionsAgent promotionsAgent,
        ResearcherAgent researcherAgent,
        ILogger<A2AOrchestrationService> logger)
    {
        _db = db;
        _inventoryAgent = inventoryAgent;
        _promotionsAgent = promotionsAgent;
        _researcherAgent = researcherAgent;
        _logger = logger;
    }

    public async Task<A2ASearchResponse> ExecuteA2ASearchAsync(string searchTerm)
    {
        try
        {
            // Step 1: Find relevant products using standard search
            var products = await _db.Product
                .Where(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm))
                .Take(10)
                .ToListAsync();

            var enrichedProducts = new List<A2AEnrichedProduct>();

            // Step 2: For each product, orchestrate calls to agents using A2A SDK patterns
            foreach (var product in products)
            {
                var enrichedProduct = new A2AEnrichedProduct
                {
                    ProductId = product.Id.ToString(),
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    ImageUrl = product.ImageUrl
                };

                // Step 3: Create A2A messages for each agent
                var productMessage = CreateProductMessage(product.Id.ToString());

                // Step 4: Call agents using A2A SDK message pattern
                var inventoryTask = _inventoryAgent.HandleInventoryCheckAsync(productMessage);
                var promotionsTask = _promotionsAgent.HandlePromotionsAsync(productMessage);
                var insightsTask = _researcherAgent.HandleInsightsAsync(productMessage);

                // Wait for all agent calls to complete
                await Task.WhenAll(inventoryTask, promotionsTask, insightsTask);

                var inventoryResult = await inventoryTask;
                var promotionsResult = await promotionsTask;
                var insightsResult = await insightsTask;

                // Step 5: Parse and aggregate results
                try
                {
                    var inventoryResponse = JsonSerializer.Deserialize<InventoryResponse>(inventoryResult);
                    enrichedProduct.Stock = inventoryResponse?.Stock ?? 0;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse inventory response for product {productId}", product.Id);
                    enrichedProduct.Stock = 0;
                }

                try
                {
                    var promotionsResponse = JsonSerializer.Deserialize<PromotionsResponse>(promotionsResult);
                    enrichedProduct.Promotions = promotionsResponse?.Promotions ?? new List<A2APromotion>();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse promotions response for product {productId}", product.Id);
                    enrichedProduct.Promotions = new List<A2APromotion>();
                }

                try
                {
                    var insightsResponse = JsonSerializer.Deserialize<ResearchResponse>(insightsResult);
                    enrichedProduct.Insights = insightsResponse?.Insights ?? new List<A2AInsight>();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse insights response for product {productId}", product.Id);
                    enrichedProduct.Insights = new List<A2AInsight>();
                }

                enrichedProducts.Add(enrichedProduct);
            }

            return new A2ASearchResponse
            {
                Products = enrichedProducts,
                Response = $"Found {enrichedProducts.Count} products enriched with A2A agent data (inventory, promotions, and insights)."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing A2A search for term: {searchTerm}", searchTerm);
            return new A2ASearchResponse
            {
                Products = new List<A2AEnrichedProduct>(),
                Response = "Error occurred during A2A search. Please try again."
            };
        }
    }

    /// <summary>
    /// Creates an A2A SDK Message for product ID
    /// </summary>
    private Message CreateProductMessage(string productId)
    {
        return new Message
        {
            Parts = new List<Part>
            {
                new TextPart { Text = JsonSerializer.Serialize(new { productId = productId }) }
            },
            Role = MessageRole.User
        };
    }
}