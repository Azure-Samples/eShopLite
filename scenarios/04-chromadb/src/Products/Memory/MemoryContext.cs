using ChromaDB.Client;
using DataEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Products.Models;
using SearchEntities;
using System.Text;
using System.Text.Json;

namespace Products.Memory;

public class MemoryContext
{
    private const string SystemPrompt = "You are a useful assistant. You always reply with a short and funny message. If you do not know an answer, you say 'I don't know that.' You only answer questions related to outdoor camping products. For any other type of questions, explain to the user that you only answer outdoor camping products questions. Do not store memory of the chat conversation.";

    private readonly ILogger _logger;
    private readonly IChatClient? _chatClient;
    private readonly IEmbeddingGenerator<string, Embedding<float>>? _embeddingGenerator;

    private bool _isMemoryCollectionInitialized = false;
    private ChromaCollectionClient? _collectionClient;

    public MemoryContext(ILogger logger, IChatClient? chatClient, IEmbeddingGenerator<string, Embedding<float>>? embeddingGenerator, ChromaCollectionClient? collectionClient)
    {
        _logger = logger;
        _chatClient = chatClient;
        _embeddingGenerator = embeddingGenerator;
        _collectionClient = collectionClient;
    }

    public async Task<bool> InitMemoryContextAsync(Context db)
    {
        _logger.LogInformation("Get a copy of the list of products");
        var products = await db.Product.ToListAsync();

        _logger.LogInformation("Filling products in memory");

        var productIds = new List<string>();
        var productDescriptionEmbeddings = new List<ReadOnlyMemory<float>>();
        var productMetadata = new List<Dictionary<string, object>>();

        // iterate over the products and add them to the memory
        foreach (var product in products)
        {
            try
            {
                _logger.LogInformation("Adding product to memory: {Product}", product.Name);
                var productInfo = $"[{product.Name}] is a product that costs [{product.Price}] and is described as [{product.Description}]";
                var embedding = await _embeddingGenerator!.GenerateVectorAsync(productInfo);
                productIds.Add(product.Id.ToString());
                productDescriptionEmbeddings.Add(embedding);
                _logger.LogInformation($"Product added to collections: {product.Name}");
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Error adding product to memory");
            }
        }

        // add the products to the memory
        await _collectionClient!.Upsert(productIds, productDescriptionEmbeddings, productMetadata);

        _logger.LogInformation("DONE! Filling products in memory");
        return true;
    }

    public async Task<SearchResponse> Search(string search, Context db)
    {
        if (!_isMemoryCollectionInitialized)
        {
            await InitMemoryContextAsync(db);
            _isMemoryCollectionInitialized = true;
        }

        var response = new SearchResponse
        {
            Response = $"I don't know the answer for your question. Your question is: [{search}]"
        };

        try
        {
            var embeddingsSearchQuery = await _embeddingGenerator!.GenerateVectorAsync(search);

            // search the vector database for the most similar product
            var queryResult = await _collectionClient!.Query(
                queryEmbeddings: embeddingsSearchQuery,
                nResults: 2,
                include: ChromaQueryInclude.Metadatas | ChromaQueryInclude.Distances);

            var sbFoundProducts = new StringBuilder();
            int productPosition = 1;
            foreach (var result in queryResult)
            {
                if (result.Distance > 0.3)
                {
                    var foundProductId = int.Parse(result.Id);
                    var foundProduct = await db.FindAsync<Product>(foundProductId);
                    if (foundProduct != null)
                    {
                        response.Products.Add(foundProduct);
                        sbFoundProducts.AppendLine($"- Product {productPosition}:");
                        sbFoundProducts.AppendLine($"  - Name: {foundProduct.Name}");
                        sbFoundProducts.AppendLine($"  - Description: {foundProduct.Description}");
                        sbFoundProducts.AppendLine($"  - Price: {foundProduct.Price}");
                        productPosition++;
                    }
                }
            }

            // let's improve the response message
            var prompt = @$"You are an intelligent assistant helping clients with their search about outdoor products. 
Generate a catchy and friendly message using the information below.
Respond using Markdown with concise sections and bullet lists when helpful.
Add a comparison between the products found and the search criteria.
Include products details.
    - User Question: {search}
    - Found Products: 
{sbFoundProducts}";

            var messages = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.System, SystemPrompt),
                new ChatMessage(ChatRole.User, prompt)
            };

            _logger.LogInformation("{ChatHistory}", JsonSerializer.Serialize(messages));

            var resultPrompt = await _chatClient!.GetResponseAsync(messages);
            response.Response = resultPrompt.Text ?? "";
        }
        catch (Exception ex)
        {
            // Handle exceptions (log them, rethrow, etc.)
            response.Response = $"An error occurred: {ex.Message}";
        }
        return response;
    }
}
