# Products Service

## Overview

The Products Service is a REST API built with ASP.NET Core that manages product data and provides AI-powered search capabilities. It serves as the backend for the eCommerce application with integration to SQL Server and Azure OpenAI.

## Key Features

### Core Product Operations
- **Get All Products**: Retrieve complete product catalog
- **Get Product by ID**: Fetch individual product details
- **Create Product**: Add new products to catalog
- **Update Product**: Modify existing product information
- **Delete Product**: Remove products from catalog

### AI-Powered Search
- **Semantic Search**: Natural language search using vector embeddings
- **Keyword Search**: Traditional text-based search
- **Vector Storage**: In-memory vector store for product embeddings

## API Endpoints

### Standard CRUD Operations

```csharp
// Get all products
GET /products
// Returns: List<Product>

// Get product by ID
GET /products/{id}
// Returns: Product | 404 Not Found

// Create new product
POST /products
// Body: Product object
// Returns: Created product

// Update existing product
PUT /products/{id}
// Body: Product object
// Returns: 200 OK | 404 Not Found

// Delete product
DELETE /products/{id}
// Returns: 204 No Content | 404 Not Found
```

### AI Search Endpoint

```csharp
// AI-powered semantic search
GET /ai-search?search={query}
// Returns: List<ProductSearchResult>
```

## Code Structure

### Service Registration

```csharp
// Add DbContext service
builder.AddSqlServerDbContext<Context>("sqldb");

// Azure OpenAI clients
builder.AddAzureOpenAIClient("openai");
builder.Services.AddSingleton<ChatClient>(/* chat client configuration */);
builder.Services.AddSingleton<EmbeddingClient>(/* embedding client configuration */);

// Memory context for AI operations
builder.Services.AddSingleton<MemoryContext>(sp =>
{
    var logger = sp.GetService<ILogger<Program>>();
    return new MemoryContext(logger, sp.GetService<ChatClient>(), sp.GetService<EmbeddingClient>());
});
```

### API Actions Implementation

#### Standard Product Operations (`ProductApiActions.cs`)
```csharp
public static async Task<IResult> GetAllProducts(Context db)
{
    var products = await db.Product.ToListAsync();
    return Results.Ok(products);
}

public static async Task<IResult> GetProductById(int id, Context db)
{
    var model = await db.Product.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
    return model is not null ? Results.Ok(model) : Results.NotFound();
}
```

#### AI Search Operations (`ProductAiActions.cs`)
```csharp
public static async Task<IResult> AISearch(string search, Context db, MemoryContext mc)
{
    var result = await mc.Search(search, db);
    return Results.Ok(result);
}
```

## Dependencies

### NuGet Packages
- `Aspire.Azure.AI.OpenAI` (9.3.0-preview.1.25265.20) - Azure OpenAI integration
- `Aspire.Microsoft.EntityFrameworkCore.SqlServer` (9.3.0) - SQL Server Entity Framework
- `Microsoft.SemanticKernel.Connectors.InMemory` (1.54.0-preview) - In-memory vector storage

### Project References
- `DataEntities` - Product data models
- `SearchEntities` - Search result models  
- `VectorEntities` - Vector embedding models
- `eShopServiceDefaults` - Aspire service configuration

## Database Integration

The service uses Entity Framework Core with SQL Server for persistent storage:

```csharp
public class Context : DbContext
{
    public DbSet<Product> Product { get; set; }
    // Additional entity sets...
}
```

## Memory Context Integration

The `MemoryContext` service provides AI capabilities:
- Initializes in-memory vector store for products
- Generates embeddings for product descriptions
- Performs semantic similarity searches
- Integrates with Azure OpenAI Chat and Embedding clients

## Configuration

### Environment Variables
- `AI_ChatDeploymentName`: GPT model deployment name
- `AI_embeddingsDeploymentName`: Embedding model deployment name

### Connection Strings
- `sqldb`: SQL Server database connection
- `openai`: Azure OpenAI service connection

## Error Handling

The service implements comprehensive error handling:
- Database connection failures
- Azure OpenAI service unavailability  
- Invalid product data validation
- Proper HTTP status codes for all scenarios

## Performance Considerations

- Asynchronous operations for all database and AI service calls
- Entity Framework no-tracking queries for read operations
- Singleton registration for expensive AI clients
- In-memory vector store for fast similarity searches