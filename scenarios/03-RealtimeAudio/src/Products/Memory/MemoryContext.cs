using DataEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using CommunityToolkit.VectorData.InMemory;
using System.Text.Json;
using Products.Models;
using SearchEntities;
using System.Text;
using VectorEntities;

namespace Products.Memory;

public class MemoryContext
{
    private const string SystemPrompt = "You are a useful assistant. You always reply with a short and funny message. If you do not know an answer, you say 'I don't know that.' You only answer questions related to outdoor camping products. For any other type of questions, explain to the user that you only answer outdoor camping products questions. Do not store memory of the chat conversation.";

    private readonly ILogger _logger;
    public IChatClient? _chatClient;
    public IEmbeddingGenerator<string, Embedding<float>>? _embeddingGenerator;
    public VectorStoreCollection<int, ProductVector>? _productsCollection;
    private bool _isMemoryCollectionInitialized;

    public MemoryContext(ILogger logger, IChatClient? chatClient, IEmbeddingGenerator<string, Embedding<float>>? embeddingGenerator)
    {
        _logger = logger;
        _chatClient = chatClient;
        _embeddingGenerator = embeddingGenerator;

        _logger.LogInformation("Memory context created");
        _logger.LogInformation($"Chat Client is null: {_chatClient is null}");
        _logger.LogInformation($"Embedding Generator is null: {_embeddingGenerator is null}");
    }

    public async Task<bool> InitMemoryContextAsync(Context db)
    {
        if (_isMemoryCollectionInitialized)
        {
            _logger.LogInformation("Memory context already initialized");
            return true;
        }

        _logger.LogInformation("Initializing memory context");
        var vectorProductStore = new InMemoryVectorStore();
        _productsCollection = vectorProductStore.GetCollection<int, ProductVector>("products");
        await _productsCollection.EnsureCollectionExistsAsync();

        _logger.LogInformation("Get a copy of the list of products");
        var products = await db.Product.ToListAsync();

        _logger.LogInformation("Filling products in memory");

        // iterate over the products and add them to the memory
        foreach (var product in products)
        {
            try
            {
                _logger.LogInformation("Adding product to memory: {Product}", product.Name);
                var productInfo = $"[{product.Name}] is a product that costs [{product.Price}] and is described as [{product.Description}]";

                // new product vector
                var productVector = new ProductVector
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    ImageUrl = product.ImageUrl
                };
                var result = await _embeddingGenerator!.GenerateVectorAsync(productInfo);

                productVector.Vector = result.ToArray();
                await _productsCollection.UpsertAsync(productVector);
                _logger.LogInformation("Product added to memory: {Product}", product.Name);
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Error adding product {product.Name} to memory");
                _isMemoryCollectionInitialized = false;
                return false;
            }
        }

        _isMemoryCollectionInitialized = true;
        _logger.LogInformation("DONE! Filling products in memory");
        return true;
    }

    public async Task<SearchResponse> Search(string search, Context db)
    {
        if (!_isMemoryCollectionInitialized)
        {
            await InitMemoryContextAsync(db);
        }

        var response = new SearchResponse
        {
            Response = $"I don't know the answer for your question. Your question is: [{search}]"
        };
        try
        {
            var result = await _embeddingGenerator!.GenerateVectorAsync(search);
            var vectorSearchQuery = result.ToArray();

            // search the vector database for the most similar product
            var sbFoundProducts = new StringBuilder();
            int productPosition = 1;

            await foreach (var searchItem in _productsCollection!.SearchAsync(vectorSearchQuery, top: 2))
            {
                if (searchItem.Score > 0.5)
                {
                    var foundProduct = await db.FindAsync<Product>(searchItem.Record.Id);
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
                new(ChatRole.System, SystemPrompt),
                new(ChatRole.User, prompt)
            };

            _logger.LogInformation("{ChatHistory}", JsonSerializer.Serialize(messages));

            var resultPrompt = await _chatClient!.GetResponseAsync(messages);
            response.Response = resultPrompt.Text!;
        }
        catch (Exception ex)
        {
            // Handle exceptions (log them, rethrow, etc.)
            response.Response = $"An error occurred: {ex.Message}";
        }
        return response;
    }
}
