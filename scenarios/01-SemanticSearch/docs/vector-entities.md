# Vector Entities

## Overview

The Vector Entities library provides the data models and functionality for storing and retrieving product embeddings in vector format. This enables semantic search capabilities by converting product information into numerical vectors that can be compared for similarity.

## Core Components

### ProductVector Class
```csharp
public class ProductVector : IVectorData<int>
{
    [VectorStoreRecordKey]
    public int Id { get; set; }
    
    [VectorStoreRecordData]
    public string Name { get; set; }
    
    [VectorStoreRecordData]  
    public string Description { get; set; }
    
    [VectorStoreRecordVector(Dimensions: 1536)]
    public ReadOnlyMemory<float> NameEmbedding { get; set; }
    
    [VectorStoreRecordVector(Dimensions: 1536)]
    public ReadOnlyMemory<float> DescriptionEmbedding { get; set; }
}
```

### Key Features
- **Dual Embeddings**: Separate vectors for product names and descriptions
- **Vector Dimensions**: 1536-dimensional vectors matching Azure OpenAI embeddings
- **Metadata Storage**: Preserves original text alongside vector representations
- **Type Safety**: Strongly-typed vector data with attributes for validation

## Integration with Semantic Kernel

### Vector Store Collection
```csharp
// In-memory vector store setup
var vectorProductStore = new InMemoryVectorStore();
_productsCollection = vectorProductStore.GetCollection<int, ProductVector>("products");
await _productsCollection.CreateCollectionIfNotExistsAsync();
```

### Vector Generation Process
```csharp
public async Task<bool> AddProductsToMemoryAsync(List<Product> products)
{
    var vectorProducts = new List<ProductVector>();
    
    foreach (var product in products)
    {
        // Generate embeddings for name and description
        var nameEmbedding = await _embeddingClient.GenerateEmbeddingAsync(product.Name);
        var descEmbedding = await _embeddingClient.GenerateEmbeddingAsync(product.Description);
        
        var vectorProduct = new ProductVector
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            NameEmbedding = nameEmbedding.Value.Vector.ToArray(),
            DescriptionEmbedding = descEmbedding.Value.Vector.ToArray()
        };
        
        vectorProducts.Add(vectorProduct);
    }
    
    await _productsCollection.UpsertBatchAsync(vectorProducts);
    return true;
}
```

## Similarity Search

### Vector Search Implementation
```csharp
public async Task<List<ProductSearchResult>> Search(string query, Context db)
{
    // Generate embedding for search query
    var queryEmbedding = await _embeddingClient.GenerateEmbeddingAsync(query);
    
    // Perform vector similarity search
    var searchOptions = new VectorSearchOptions
    {
        VectorPropertyName = "DescriptionEmbedding",
        Top = 10
    };
    
    var searchResults = await _productsCollection.VectorizedSearchAsync(
        queryEmbedding.Value.Vector, searchOptions);
    
    // Convert to search results with scores
    var results = new List<ProductSearchResult>();
    await foreach (var result in searchResults.Results)
    {
        results.Add(new ProductSearchResult
        {
            Id = result.Record.Id,
            Name = result.Record.Name,
            Description = result.Record.Description,
            Score = result.Score ?? 0.0
        });
    }
    
    return results.OrderByDescending(r => r.Score).ToList();
}
```

## Performance Optimizations

### Memory Management
- **ReadOnlyMemory<float>**: Efficient memory usage for vector storage
- **Batch Operations**: Bulk insert/update operations for better performance
- **Lazy Loading**: Vector generation only when needed

### Search Optimization
- **Configurable Results**: Adjustable result count based on requirements
- **Score Thresholding**: Filter results below similarity threshold
- **Caching**: In-memory collection for fast retrieval

## Data Flow

### Vector Creation Pipeline
```
Product Data → Embedding Generation → Vector Storage → Search Index
     ↓              ↓                    ↓              ↓
  Text Fields → Azure OpenAI API → ProductVector → Collection
```

### Search Pipeline  
```
User Query → Query Embedding → Similarity Search → Ranked Results
     ↓            ↓               ↓                ↓
  Natural Text → Vector → Cosine Similarity → ProductSearchResult
```

## Configuration

### Embedding Model Settings
- **Model**: text-embedding-ada-002
- **Dimensions**: 1536 (standard for Ada-002)
- **Input Limits**: Up to 8192 tokens per embedding request

### Vector Store Configuration
```csharp
// Collection setup with proper typing
var collection = vectorStore.GetCollection<int, ProductVector>("products");
await collection.CreateCollectionIfNotExistsAsync();
```

## Error Handling

### Robust Vector Operations
```csharp
try
{
    var embedding = await _embeddingClient.GenerateEmbeddingAsync(text);
    return embedding.Value.Vector.ToArray();
}
catch (Exception ex)
{
    _logger.LogError(ex, "Failed to generate embedding for text: {Text}", text);
    return Array.Empty<float>();
}
```

### Graceful Degradation
- **Fallback Search**: Keyword search when vector search fails
- **Partial Results**: Return available results even if some vectors fail
- **Error Logging**: Comprehensive error tracking for debugging

## Future Enhancements

### Potential Improvements
- **Hybrid Search**: Combine vector and keyword search results
- **Dynamic Reindexing**: Update vectors when product data changes
- **Multiple Embedding Models**: Support for different embedding strategies
- **Persistent Storage**: Database-backed vector storage for production scenarios