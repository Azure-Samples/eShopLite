# Semantic Search Implementation

## Overview

The semantic search feature enables intelligent product discovery by understanding the meaning and context of search queries, going beyond simple keyword matching to find relevant products based on semantic similarity.

## Architecture

### Vector Store
The implementation uses an in-memory vector store powered by `CommunityToolkit.VectorData.InMemory`:

```csharp
var vectorProductStore = new InMemoryVectorStore();
_productsCollection = vectorProductStore.GetCollection<int, ProductVector>("products");
await _productsCollection.EnsureCollectionExistsAsync();
```

### Product Vector Model
Products are represented as vectors using the `ProductVector` entity:

```csharp
public class ProductVector
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public string Brand { get; set; }
    public string Category { get; set; }
    
    // Vector representation of the product
    public ReadOnlyMemory<float> EmbeddingVector { get; set; }
}
```

## Search Process

### 1. Query Embedding Generation
When a user submits a search query, it's converted to a vector representation:

```csharp
public async Task<SearchResult> Search(string query, Context db)
{
    if (_embeddingClient == null || !_isMemoryCollectionInitialized)
    {
        return new SearchResult 
        { 
            Query = query, 
            Message = "Embedding client not initialized" 
        };
    }

    // Generate embedding for the search query
    var queryEmbedding = await _embeddingClient.GenerateEmbeddingAsync(query);
}
```

### 2. Vector Similarity Search
The query embedding is compared against stored product embeddings:

```csharp
// Perform vector search to find similar products
var searchOptions = new VectorSearchOptions
{
    VectorPropertyName = nameof(ProductVector.EmbeddingVector),
    Top = 5 // Return top 5 most similar products
};

var vectorSearchResults = await _productsCollection.VectorizedSearchAsync(
    queryEmbedding.Value, 
    searchOptions);
```

### 3. Result Processing
Similar products are retrieved and formatted for the response:

```csharp
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
```

## Initialization Process

### Product Embedding Generation
During application startup, all products are processed to generate embeddings:

```csharp
public async Task<bool> InitMemoryContextAsync(Context db)
{
    // Get all products from database
    var products = await db.Product.ToListAsync();
    
    foreach (var product in products)
    {
        // Create searchable text from product properties
        var searchableText = $"{product.Name} {product.Description} {product.Brand} {product.Category}";
        
        // Generate embedding for the product
        var embedding = await _embeddingClient.GenerateEmbeddingAsync(searchableText);
        
        // Store in vector collection
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
        
        await _productsCollection.UpsertAsync(productVector);
    }
    
    _isMemoryCollectionInitialized = true;
    return true;
}
```

## API Integration

### Semantic Search Endpoint
The search functionality is exposed via a REST API endpoint:

```csharp
public static async Task<IResult> AISearch(string search, Context db, MemoryContext mc)
{
    var result = await mc.Search(search, db);
    return Results.Ok(result);
}
```

### Endpoint Mapping
```csharp
routes.MapGet("/api/aisearch/{search}", ProductAiActions.AISearch)
    .WithName("AISearch")
    .WithOpenApi();
```

## Search Results

### Result Structure
```csharp
public class SearchResult
{
    public string Query { get; set; }
    public List<Product> Products { get; set; }
    public string Message { get; set; }
    public bool IsSuccessful { get; set; }
}
```

### Response Format
- **Query**: The original search query
- **Products**: List of semantically similar products
- **Message**: Status or error information
- **IsSuccessful**: Boolean indicating search success

## Benefits

### Intelligent Understanding
- **Context Awareness**: Understands product relationships and categories
- **Semantic Matching**: Finds products even when exact keywords don't match
- **Natural Language**: Supports conversational search queries

### Examples
- Query: "gear for hiking" → Returns camping equipment, backpacks, boots
- Query: "waterproof jacket" → Returns rain jackets, outdoor clothing
- Query: "cooking outdoors" → Returns camping stoves, cookware, utensils

## Performance Considerations

### Memory Usage
- In-memory vector store for fast retrieval
- Vectors stored in RAM for optimal search performance
- Consider persistent storage for large product catalogs

### Scalability
- Current implementation suitable for moderate product catalogs
- For larger datasets, consider Azure AI Search with vector capabilities
- Batch processing for embedding generation during startup

## Dependencies

### Required Packages
- `CommunityToolkit.VectorData.InMemory` (1.0.0-preview.3) — in-memory vector store
- `Microsoft.Extensions.AI.OpenAI` (10.6.0) — `IEmbeddingGenerator` and `IChatClient`
- `Microsoft.Extensions.VectorData.Abstractions` (10.6.0) — vector operations

### External Services
- Azure OpenAI text-embedding-ada-002 model
- SQL Server for product data storage