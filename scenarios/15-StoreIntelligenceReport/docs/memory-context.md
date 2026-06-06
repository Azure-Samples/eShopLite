# Memory Context Implementation

## Overview

The Memory Context serves as the core component for semantic search operations, managing vector embeddings, product data relationships, and chat interactions. It provides an abstraction layer over the in-memory vector store and integrates with OpenAI services for both embedding generation and conversational AI.

## Class Structure

### MemoryContext Class
The main class that orchestrates semantic search and chat functionality:

```csharp
public class MemoryContext
{
    private ILogger _logger;
    public ChatClient? _chatClient;
    public EmbeddingClient? _embeddingClient;
    public VectorStoreCollection<int, ProductVector> _productsCollection;
    private string _systemPrompt = "";
    private bool _isMemoryCollectionInitialized = false;
}
```

## Initialization Process

### Constructor Setup
The memory context requires proper dependency injection of AI clients:

```csharp
public MemoryContext(ILogger logger, ChatClient? chatClient, EmbeddingClient? embeddingClient)
{
    _logger = logger;
    _chatClient = chatClient;
    _embeddingClient = embeddingClient;

    _logger.LogInformation("Memory context created");
    _logger.LogInformation($"Chat Client is null: {_chatClient is null}");
    _logger.LogInformation($"Embedding Client is null: {_embeddingClient is null}");
}
```

### Vector Store Initialization
Setting up the in-memory vector collection for product embeddings:

```csharp
public async Task<bool> InitMemoryContextAsync(Context db)
{
    _logger.LogInformation("Initializing memory context");
    
    // Create in-memory vector store
    var vectorProductStore = new InMemoryVectorStore();
    _productsCollection = vectorProductStore.GetCollection<int, ProductVector>("products");
    await _productsCollection.EnsureCollectionExistsAsync();

    // Define system prompt for chat interactions
    _systemPrompt = "You are a useful assistant. You always reply with a short and funny message. If you do not know an answer, you say 'I don't know that.' You only answer questions related to outdoor camping products. For any other type of questions, explain to the user that you only answer outdoor camping products questions. Do not store memory of the chat conversation.";
}
```

## Embedding Generation

### Product Embedding Process
Converting product data into vector representations:

```csharp
public async Task InitializeProductEmbeddings(Context db)
{
    // Get all products from database
    var products = await db.Product.ToListAsync();
    
    if (_embeddingClient == null)
    {
        _logger.LogError("Embedding client is not initialized");
        return false;
    }

    _logger.LogInformation($"Processing {products.Count} products for embeddings");

    foreach (var product in products)
    {
        try
        {
            // Create searchable text combining multiple product fields
            var searchableText = $"{product.Name} {product.Description} {product.Brand} {product.Category}";
            
            // Generate embedding using Azure OpenAI
            var embedding = await _embeddingClient.GenerateEmbeddingAsync(searchableText);
            
            // Create ProductVector entity
            var productVector = new ProductVector
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Brand = product.Brand,
                Category = product.Category,
                EmbeddingVector = embedding.Value
            };

            // Store in vector collection
            await _productsCollection.UpsertAsync(productVector);
            _logger.LogInformation($"Processed product: {product.Name}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing product {product.Id}: {product.Name}");
        }
    }

    _isMemoryCollectionInitialized = true;
    _logger.LogInformation("Memory context initialization completed");
    return true;
}
```

## Search Operations

### Semantic Search Implementation
Core search functionality using vector similarity:

```csharp
public async Task<SearchResult> Search(string query, Context db)
{
    _logger.LogInformation($"Starting search for query: {query}");

    if (_embeddingClient == null || !_isMemoryCollectionInitialized)
    {
        return new SearchResult 
        { 
            Query = query, 
            Message = "Embedding client not initialized",
            IsSuccessful = false
        };
    }

    try
    {
        // Generate embedding for search query
        var queryEmbedding = await _embeddingClient.GenerateEmbeddingAsync(query);
        
        // Configure vector search options
        var searchOptions = new VectorSearchOptions
        {
            VectorPropertyName = nameof(ProductVector.EmbeddingVector),
            Top = 5  // Return top 5 most similar products
        };

        // Perform vector similarity search
        var vectorSearchResults = await _productsCollection.VectorizedSearchAsync(
            queryEmbedding.Value, 
            searchOptions);

        // Convert vector results to product entities
        var products = new List<Product>();
        await foreach (var result in vectorSearchResults.Results)
        {
            var productVector = result.Record;
            if (productVector != null)
            {
                var product = await db.Product.FindAsync(productVector.Id);
                if (product != null)
                {
                    products.Add(product);
                }
            }
        }

        return new SearchResult
        {
            Query = query,
            Products = products,
            Message = $"Found {products.Count} similar products",
            IsSuccessful = true
        };
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, $"Error during search for query: {query}");
        return new SearchResult
        {
            Query = query,
            Message = $"Search error: {ex.Message}",
            IsSuccessful = false
        };
    }
}
```

## Chat Integration

### Conversational AI Support
Integrating chat functionality with product search:

```csharp
public async Task<string> ProcessChatQuery(string userMessage, List<Product> contextProducts)
{
    if (_chatClient == null)
    {
        return "Chat service is not available.";
    }

    try
    {
        // Build context from products
        var productContext = string.Join("\n", 
            contextProducts.Select(p => $"- {p.Name}: {p.Description} (${p.Price})"));

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(_systemPrompt),
            new SystemChatMessage($"Available products:\n{productContext}"),
            new UserChatMessage(userMessage)
        };

        var chatCompletion = await _chatClient.CompleteChatAsync(messages);
        return chatCompletion.Value.Content[0].Text;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, $"Error in chat processing for message: {userMessage}");
        return "I'm sorry, I encountered an error while processing your request.";
    }
}
```

## Vector Data Models

### ProductVector Entity
Entity representing vectorized product data:

```csharp
public class ProductVector
{
    [VectorStoreRecordKey]
    public int Id { get; set; }

    [VectorStoreRecordData]
    public string Name { get; set; } = string.Empty;

    [VectorStoreRecordData]
    public string Description { get; set; } = string.Empty;

    [VectorStoreRecordData]
    public decimal Price { get; set; }

    [VectorStoreRecordData]
    public string Brand { get; set; } = string.Empty;

    [VectorStoreRecordData]
    public string Category { get; set; } = string.Empty;

    [VectorStoreRecordVector(1536)]  // text-embedding-ada-002 dimensions
    public ReadOnlyMemory<float> EmbeddingVector { get; set; }
}
```

### SearchResult Model
Result structure for search operations:

```csharp
public class SearchResult
{
    public string Query { get; set; } = string.Empty;
    public List<Product> Products { get; set; } = new();
    public string Message { get; set; } = string.Empty;
    public bool IsSuccessful { get; set; }
    public TimeSpan ProcessingTime { get; set; }
}
```

## Performance Considerations

### Memory Management
- **In-Memory Storage**: Fast access but limited by available RAM
- **Vector Dimensions**: 1536 dimensions for text-embedding-ada-002
- **Collection Size**: Suitable for moderate product catalogs (thousands of items)

### Search Optimization
- **Top-K Results**: Limit results to improve performance
- **Caching**: Consider caching frequently searched embeddings
- **Batch Processing**: Process multiple searches efficiently

### Error Handling
- **Graceful Degradation**: Handle missing AI services gracefully
- **Logging**: Comprehensive logging for debugging and monitoring
- **Retry Logic**: Implement retry for transient failures

## Service Registration

### Dependency Injection Setup
Registering the memory context in the DI container:

```csharp
builder.Services.AddSingleton(sp =>
{
    var logger = sp.GetService<ILogger<Program>>();
    logger.LogInformation("Creating memory context");
    
    var chatClient = sp.GetService<ChatClient>();
    var embeddingClient = sp.GetService<EmbeddingClient>();
    
    return new MemoryContext(logger, chatClient, embeddingClient);
});
```

## Best Practices

### Initialization
- Initialize memory context during application startup
- Verify all required AI clients are available
- Handle initialization failures gracefully

### Data Management
- Keep product data and vectors synchronized
- Implement incremental updates for large datasets
- Monitor memory usage and performance metrics

### Search Quality
- Use appropriate system prompts for chat interactions
- Combine multiple product fields for better embedding quality
- Test and tune search parameters for optimal results