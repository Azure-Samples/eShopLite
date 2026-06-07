using DataEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using CommunityToolkit.VectorData.InMemory;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Products.Models;
using SearchEntities;
using VectorEntities;

namespace Products.Memory;

public class MemoryContext(
    ILogger _logger,
    IChatClient? chatClientOpenAI,
    IChatClient? chatClientReasoningModel,
    IEmbeddingGenerator<string, Embedding<float>>? _embeddingGenerator)
{
    private readonly string _systemPrompt = "You are a useful assistant. You always reply with a short and funny message. If you do not know an answer, you say 'I don't know that.' You only answer questions related to outdoor camping products. For any other type of questions, explain to the user that you only answer outdoor camping products questions. Do not store memory of the chat conversation.";
    private bool _isMemoryCollectionInitialized = false;

    private VectorStoreCollection<int, ProductVector>? _productsCollection;

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

        foreach (var product in products)
        {
            try
            {
                _logger.LogInformation("Adding product to memory: {Product}", product.Name);
                var productInfo = $"[{product.Name}] is a product that costs [{product.Price}] and is described as [{product.Description}]";

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

    public async Task<SearchResponse> Search(string search, Context db, bool useReasoningModel = false)
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

            var sbFoundProducts = new StringBuilder();
            int productPosition = 1;

            await foreach (var searchItem in _productsCollection!.SearchAsync(vectorSearchQuery, top: 3))
            {
                if (searchItem.Score > 0.5)
                {
                    var product = await db.FindAsync<Product>(searchItem.Record.Id);
                    if (product != null)
                    {
                        response.Products!.Add(product);
                        sbFoundProducts.AppendLine($"- Product {productPosition}:");
                        sbFoundProducts.AppendLine($"  - Name: {product.Name}");
                        sbFoundProducts.AppendLine($"  - Description: {product.Description}");
                        sbFoundProducts.AppendLine($"  - Price: {product.Price}");
                        productPosition++;
                    }
                }
            }

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
                new(ChatRole.System, _systemPrompt),
                new(ChatRole.User, prompt)
            };

            _logger.LogInformation("{ChatHistory}", JsonSerializer.Serialize(messages));

            if (!useReasoningModel)
            {
                _logger.LogInformation("Generate response using standard chat model");
                var resultPrompt = await chatClientOpenAI!.GetResponseAsync(messages);
                response.Response = resultPrompt.Text!;
            }
            else
            {
                _logger.LogInformation("Generate response using reasoning model (DeepSeek-R1)");
                var resultPrompt = await chatClientReasoningModel!.GetResponseAsync(messages);
                var responseComplete = resultPrompt.Text!;

                var match = Regex.Match(responseComplete, @"<think>(.*?)<\/think>(.*)", RegexOptions.Singleline);
                if (match.Success)
                {
                    response.ResponseThink = match.Groups[1].Value.Trim();
                    response.Response = match.Groups[2].Value.Trim();
                }
                else
                {
                    response.Response = responseComplete;
                }
            }
        }
        catch (Exception ex)
        {
            response.Response = $"An error occurred: {ex.Message}";
            _logger.LogError(ex, "Error during search");
        }

        return response;
    }
}
