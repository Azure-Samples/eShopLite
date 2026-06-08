using DataEntities;
using Microsoft.EntityFrameworkCore;
using A2A;
using Products.Memory;
using Products.Models;
using Products.Services.Agents;
using SearchEntities;
using System.Text.Json;

namespace Products.Services;

/// <summary>
/// Orchestrates an A2A (Agent-to-Agent) search pipeline:
///   1. DISCOVER  — find candidate products via semantic vector search (with keyword fallback)
///   2. MESSAGE   — wrap each product ID in an A2A Message and fan-out to three specialist agents
///   3. ENRICH    — aggregate inventory, promotions, and insights returned by the agents
/// </summary>
public class A2AOrchestrationService : IA2AOrchestrationService
{
    private readonly Context _db;
    private readonly MemoryContext _memoryContext;      // semantic vector store (PREFERRED search path)
    private readonly CatalogAgent _inventoryAgent;
    private readonly PromotionsAgent _promotionsAgent;
    private readonly BusinessInsightsAgent _researcherAgent;
    private readonly ILogger<A2AOrchestrationService> _logger;

    public A2AOrchestrationService(
        Context db,
        MemoryContext memoryContext,
        CatalogAgent inventoryAgent,
        PromotionsAgent promotionsAgent,
        BusinessInsightsAgent researcherAgent,
        ILogger<A2AOrchestrationService> logger)
    {
        _db = db;
        _memoryContext = memoryContext;
        _inventoryAgent = inventoryAgent;
        _promotionsAgent = promotionsAgent;
        _researcherAgent = researcherAgent;
        _logger = logger;
    }

    public async Task<A2ASearchResponse> ExecuteA2ASearchAsync(string searchTerm)
    {
        try
        {
            // ── STEP 1: DISCOVER ──────────────────────────────────────────────────────────
            // Get candidate products. Multi-word queries like "winter camping" would return
            // zero rows with a naïve substring match, so we prefer semantic search here.
            var products = await GetBaseProductsAsync(searchTerm);

            _logger.LogInformation("A2A search for '{term}' — {count} base products to enrich",
                searchTerm, products.Count);

            var enrichedProducts = new List<A2AEnrichedProduct>();

            // ── STEP 2 & 3: MESSAGE → ENRICH ─────────────────────────────────────────────
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

                // Wrap the product ID in an A2A-protocol Message so agents receive a
                // standardised envelope (they can be swapped for remote A2A agents later)
                var productMessage = CreateProductMessage(product.Id.ToString());

                // Fan-out: call all three specialist agents in parallel to minimise latency
                var inventoryTask  = _inventoryAgent.HandleInventoryCheckAsync(productMessage);
                var promotionsTask = _promotionsAgent.HandlePromotionsAsync(productMessage);
                var insightsTask   = _researcherAgent.HandleInsightsAsync(productMessage);

                await Task.WhenAll(inventoryTask, promotionsTask, insightsTask);

                // ── Aggregate responses ───────────────────────────────────────────────────
                enrichedProduct.Stock      = ParseStock(await inventoryTask,  product.Id);
                enrichedProduct.Promotions = ParsePromotions(await promotionsTask, product.Id);
                enrichedProduct.Insights   = ParseInsights(await insightsTask, product.Id);

                enrichedProducts.Add(enrichedProduct);
            }

            return new A2ASearchResponse
            {
                Products = enrichedProducts,
                Response  = $"Found {enrichedProducts.Count} products enriched with A2A agent data " +
                            "(inventory, promotions, and insights)."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing A2A search for term: {searchTerm}", searchTerm);
            return new A2ASearchResponse
            {
                Products = new List<A2AEnrichedProduct>(),
                Response  = "Error occurred during A2A search. Please try again."
            };
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────────────
    // BASE PRODUCT DISCOVERY
    // ─────────────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns candidate products for <paramref name="searchTerm"/> using a two-tier strategy:
    /// <list type="number">
    ///   <item>Semantic vector search via <see cref="MemoryContext"/> (handles multi-word, fuzzy queries)</item>
    ///   <item>Tokenised keyword OR-search against the SQL DB (fallback when AOAI is not configured)</item>
    /// </list>
    /// </summary>
    private async Task<List<Product>> GetBaseProductsAsync(string searchTerm)
    {
        // ── Preferred path: semantic vector search ────────────────────────────────────────
        // Works even for multi-word queries like "winter camping" because it compares
        // embedding vectors rather than doing literal substring matching.
        try
        {
            var semanticResults = await _memoryContext.SearchProductsAsync(searchTerm);
            if (semanticResults.Count > 0)
            {
                _logger.LogInformation("Semantic search returned {count} products for '{term}'",
                    semanticResults.Count, searchTerm);
                return semanticResults;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Semantic search failed for '{term}', falling back to keyword search", searchTerm);
        }

        // ── Fallback path: tokenised keyword search ───────────────────────────────────────
        // Split "winter camping" → ["winter", "camping"] and return products that contain
        // ANY token (OR logic), so multi-word queries still yield results even without AOAI.
        _logger.LogInformation("Keyword fallback search for '{term}'", searchTerm);

        var tokens = searchTerm.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 0)
            return [];

        // Collect matching IDs per token then union them (avoids un-translatable LINQ)
        var matchedIds = new HashSet<int>();
        foreach (var token in tokens)
        {
            var t = token; // capture loop variable for EF lambda translation
            var ids = await _db.Product
                .Where(p => p.Name.Contains(t) || p.Description.Contains(t))
                .Select(p => p.Id)
                .ToListAsync();
            matchedIds.UnionWith(ids);
        }

        // Fetch the deduplicated set of matching products
        return await _db.Product
            .Where(p => matchedIds.Contains(p.Id))
            .Take(10)
            .ToListAsync();
    }

    // ─────────────────────────────────────────────────────────────────────────────────────
    // A2A MESSAGE CONSTRUCTION
    // ─────────────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Wraps a product ID in an A2A SDK <see cref="Message"/> so each agent receives a
    /// protocol-conformant envelope (enabling future swap-out to remote A2A agents).
    /// </summary>
    private static Message CreateProductMessage(string productId)
    {
        return new Message
        {
            // Encode the product ID as JSON in the text part so agents can parse it reliably
            Parts = [new TextPart { Text = JsonSerializer.Serialize(new { productId }) }],
            Role  = MessageRole.User
        };
    }

    // ─────────────────────────────────────────────────────────────────────────────────────
    // RESPONSE PARSERS (isolated so failures in one agent don't abort the whole pipeline)
    // ─────────────────────────────────────────────────────────────────────────────────────

    private int ParseStock(string json, int productId)
    {
        try
        {
            return JsonSerializer.Deserialize<InventoryResponse>(json)?.Stock ?? 0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse inventory response for product {productId}", productId);
            return 0;
        }
    }

    private List<A2APromotion> ParsePromotions(string json, int productId)
    {
        try
        {
            return JsonSerializer.Deserialize<PromotionsResponse>(json)?.Promotions ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse promotions response for product {productId}", productId);
            return [];
        }
    }

    private List<A2AInsight> ParseInsights(string json, int productId)
    {
        try
        {
            return JsonSerializer.Deserialize<ResearchResponse>(json)?.Insights ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse insights response for product {productId}", productId);
            return [];
        }
    }
}