# Semantic Search Engine

## Overview
The semantic search engine uses vector embeddings and in-memory vector storage to provide intelligent product search capabilities that understand context and meaning rather than just keyword matching.

## Components

### MemoryContext Service
```csharp
builder.Services.AddSingleton(sp =>
{
    var logger = sp.GetService<ILogger<Program>>();
    return new MemoryContext(logger, sp.GetService<ChatClient>(), sp.GetService<EmbeddingClient>());
});
```
- **Purpose**: Manages vector storage and semantic search operations
- **Configuration**: Depends on ChatClient and EmbeddingClient
- **Storage**: Uses Microsoft SemanticKernel InMemory vector store

### Vector Store Implementation
```csharp
var vectorProductStore = new InMemoryVectorStore();
_productsCollection = vectorProductStore.GetCollection<int, ProductVector>("products");
await _productsCollection.EnsureCollectionExistsAsync();
```
- **Purpose**: In-memory vector database for product embeddings
- **Collection**: "products" collection with ProductVector entities
- **Dimensions**: 384-dimensional vectors from text-embedding-ada-002

### Product Vectorization
```csharp
var productInfo = $"[{product.Name}] is a product that costs [{product.Price}] and is described as [{product.Description}]";
var result = await _embeddingClient.GenerateEmbeddingAsync(productInfo);
productVector.Vector = result.Value.ToFloats();
```
- **Purpose**: Converts product information to vector embeddings
- **Format**: Structured product description including name, price, and description
- **Storage**: ProductVector entities with 384-dimensional float arrays

## Search Process

### Vector Search Query
```csharp
var result = await _embeddingClient.GenerateEmbeddingAsync(search);
var vectorSearchQuery = result.Value.ToFloats();

await foreach (var resultItem in _productsCollection.SearchAsync(vectorSearchQuery, top: 3))
{
    if (resultItem.Score > 0.5)
    {
        // Process matching products
    }
}
```

### AI-Enhanced Response
```csharp
var prompt = @$"You are an intelligent assistant helping clients with their search about outdoor products. 
Generate a catchy and friendly message using the information below.
Add a comparison between the products found and the search criteria.
Include products details.
    - User Question: {search}
    - Found Products: 
{sbFoundProducts}";

var resultPrompt = await _chatClient.CompleteChatAsync(messages);
```

## Search API Endpoint
```csharp
routes.MapGet("/api/aisearch/{search}", ProductAiActions.AISearch)
    .WithName("AISearch")
    .Produces<SearchResponse>(StatusCodes.Status200OK);
```

## Configuration Notes
- **Similarity Threshold**: 0.5 minimum score for relevant results
- **Result Limit**: Top 3 most similar products
- **System Prompt**: Configured for outdoor camping products domain
- **Memory Initialization**: Automatic on first search request

## External Dependencies  
- Microsoft.SemanticKernel.Connectors.InMemory
- Azure OpenAI text-embedding-ada-002 model
- SQL Server database for product data