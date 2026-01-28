# Native Vector Search in SQL Server 2025

## Overview

SQL Server 2025 introduces [native vector search capabilities](https://learn.microsoft.com/en-us/sql/sql-server/ai/vectors) that eliminate the need for external vector databases. This implementation demonstrates how to leverage built-in vector data types, indexing, and similarity search directly within SQL Server.

> **Note**: In SQL Server 2025, vector features are in preview. To use vector indexes and `VECTOR_SEARCH`, you must enable [preview features](https://learn.microsoft.com/en-us/sql/t-sql/statements/alter-database-scoped-configuration-transact-sql#preview_features---on--off-):
>
> ```sql
> ALTER DATABASE SCOPED CONFIGURATION SET PREVIEW_FEATURES = ON;
> ```

## Vector Data Type

### Native Vector Support

SQL Server 2025 provides a native [`VECTOR` data type](https://learn.microsoft.com/en-us/sql/t-sql/data-types/vector-data-type) for storing high-dimensional embeddings. Vectors are stored in an optimized binary format but exposed as JSON arrays for convenience.

```sql
-- Vector column definition (default float32 precision)
CREATE TABLE Products (
    Id INT PRIMARY KEY,
    Name NVARCHAR(255),
    Description NVARCHAR(MAX),
    Embedding VECTOR(1536) NOT NULL  -- 1536 dimensions
);

-- Insert vector data as JSON array
INSERT INTO Products (Id, Name, Description, Embedding)
VALUES (1, 'Hiking Poles', 'Ideal for camping', '[0.1, 0.2, 0.3, ...]');
```

**Key characteristics:**

- Dimensions must be between 1 and 1998
- Each element is stored as single-precision float (float32) by default
- Half-precision (float16) is available as a preview feature
- Vectors can be inserted/retrieved as JSON arrays

### Entity Framework Core Integration

The scenario uses [EFCore.SqlServer.VectorSearch](https://www.nuget.org/packages/EFCore.SqlServer.VectorSearch/) for Entity Framework Core integration:

```csharp
// Enable vector search in Entity Framework
var productsDbConnectionString = builder.Configuration.GetConnectionString("productsDb");
builder.Services.AddDbContext<Context>(options =>
    options.UseSqlServer(productsDbConnectionString, o => o.UseVectorSearch()));
```

The `Product` entity (from `DataEntities/Product.cs`) stores embeddings as a `float[]`:

```csharp
public class Product
{
    public Product()
    {
        Id = 0;
        Name = "not defined";
        Description = "not defined";
        Price = 0;
        ImageUrl = "not defined";
    }

    [JsonPropertyName("id")]
    public virtual int Id { get; set; }

    [JsonPropertyName("name")]
    public virtual string Name { get; set; }

    [JsonPropertyName("description")]
    public virtual string Description { get; set; }

    [JsonPropertyName("price")]
    public virtual decimal Price { get; set; }

    [JsonPropertyName("imageUrl")]
    public virtual string ImageUrl { get; set; }

    // demo for SQL Server 2025 new vector type
    public float[] Embedding { get; set; } = [];
}
```

The `Context` class (from `Products/Models/Context.cs`) configures the column type:

```csharp
public class Context(DbContextOptions options) : DbContext(options)
{
    public DbSet<Product> Product => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure the float[] property as a vector:
        modelBuilder.Entity<Product>().Property(b => b.Embedding).HasColumnType("vector(1536)");
    }
}
```

## Vector Distance Calculation

### VECTOR_DISTANCE Function

The [`VECTOR_DISTANCE`](https://learn.microsoft.com/en-us/sql/t-sql/functions/vector-distance-transact-sql) function calculates the exact distance between two vectors. It performs an exact search and **does not use vector indexes**.

**Syntax:**

```sql
VECTOR_DISTANCE(distance_metric, vector1, vector2)
```

**Supported distance metrics:**

| Metric | Description | Range |
|--------|-------------|-------|
| `cosine` | Cosine (angular) distance | [0, 2] - 0 = identical, 2 = opposing |
| `euclidean` | Euclidean (L2) distance | [0, +∞] - 0 = identical |
| `dot` | Negative dot product | [-∞, +∞] - smaller = more similar |

**Example - Exact K-NN search:**

```sql
DECLARE @QueryVector VECTOR(1536) = '[0.1, 0.2, ...]';

SELECT TOP 10 
    p.Id, 
    p.Name, 
    p.Description,
    VECTOR_DISTANCE('cosine', p.Embedding, @QueryVector) AS Distance
FROM Products p
ORDER BY Distance ASC;
```

## Vector Indexing

### CREATE VECTOR INDEX

For larger datasets (50,000+ vectors), create a [vector index](https://learn.microsoft.com/en-us/sql/t-sql/statements/create-vector-index-transact-sql) to enable Approximate Nearest Neighbor (ANN) search using the [DiskANN algorithm](https://learn.microsoft.com/en-us/sql/sql-server/ai/vectors#approximate-vector-index-and-vector-search-approximate-nearest-neighbors):

**Syntax:**

```sql
CREATE VECTOR INDEX index_name
ON table_name (vector_column)
WITH (
    METRIC = { 'cosine' | 'dot' | 'euclidean' },
    TYPE = 'DiskANN',
    MAXDOP = max_degree_of_parallelism
);
```

**Example:**

```sql
-- Create a vector index for cosine similarity search
CREATE VECTOR INDEX IX_Products_Embedding
ON Products(Embedding)
WITH (METRIC = 'cosine', TYPE = 'DiskANN');
```

**Current limitations of vector indexes (preview):**

- Table must have a single-column, integer primary key clustered index
- Table becomes read-only while the vector index exists (in SQL Server 2025)
- Vector index must be dropped and recreated to incorporate new data
- No partition support
- Views and temporary tables not supported

## Approximate Nearest Neighbor Search

### VECTOR_SEARCH Function

The [`VECTOR_SEARCH`](https://learn.microsoft.com/en-us/sql/t-sql/functions/vector-search-transact-sql) function performs approximate nearest neighbor search using a vector index:

**Syntax:**

```sql
SELECT columns
FROM VECTOR_SEARCH(
    TABLE = table_name AS alias,
    COLUMN = vector_column,
    SIMILAR_TO = query_vector,
    METRIC = 'cosine' | 'dot' | 'euclidean',
    TOP_N = k
) AS result_alias;
```

**Example:**

```sql
DECLARE @QueryVector VECTOR(1536) = '[0.1, 0.2, ...]';

SELECT 
    t.Id,
    t.Name,
    t.Description,
    s.distance
FROM VECTOR_SEARCH(
    TABLE = Products AS t,
    COLUMN = Embedding,
    SIMILAR_TO = @QueryVector,
    METRIC = 'cosine',
    TOP_N = 10
) AS s
ORDER BY s.distance;
```

The result includes:

- All columns from the source table
- An additional `distance` column representing similarity

## Implementation in This Scenario

### Embedding Generation and Storage

The application generates embeddings using Azure OpenAI and stores them directly in SQL Server:

```csharp
// From DbInitializer.cs
public static async Task Initialize(
    Context context, 
    IEmbeddingGenerator<string, Embedding<float>> embeddingClient, 
    int dimensions = 1536)
{
    var products = new List<Product> { /* product data */ };

    foreach (var product in products)
    {
        var productInformation = $"Name = {product.Name} - Description = {product.Description} - Price = {product.Price}";
        var productInformationEmbedding = await embeddingClient.GenerateVectorAsync(
            productInformation, 
            new() { Dimensions = dimensions });
        product.Embedding = productInformationEmbedding.ToArray();
    }

    context.AddRange(products);
    context.SaveChanges();
}
```

### Semantic Search Implementation

Search queries use `VECTOR_DISTANCE` via Entity Framework:

```csharp
// From ProductApiActions.cs
public static async Task<Ok<SearchResponse>> AISearch(
    string search,
    Context db,
    IEmbeddingGenerator<string, Embedding<float>> embeddingClient,
    ILogger<ProductApiActions> logger,
    int dimensions = 1536)
{
    logger.LogInformation("Querying for similar products to {search}", search);

    var embeddingSearch = await embeddingClient.GenerateVectorAsync(
        search, 
        new() { Dimensions = dimensions });
    var vectorSearch = embeddingSearch.ToArray();
    
    var products = await db.Product
        .OrderBy(p => EF.Functions.VectorDistance("cosine", p.Embedding, vectorSearch))
        .Take(3)
        .ToListAsync();

    var response = new SearchResponse
    {
        Products = products,
        Response = products.Count > 0 ?
            $"{products.Count} Products found for [{search}]" :
            $"No products found for [{search}]"
    };
    return TypedResults.Ok(response);
}
```

## Combined Vector and SQL Filtering

You can combine vector similarity search with traditional SQL filtering. Note that with `VECTOR_SEARCH`, filtering is applied **after** the vector search (post-filtering):

```sql
-- Exact search with filtering (uses VECTOR_DISTANCE)
SELECT TOP 10 
    p.Id, 
    p.Name,
    VECTOR_DISTANCE('cosine', p.Embedding, @QueryVector) AS Distance
FROM Products p
WHERE p.Price BETWEEN 50 AND 500
ORDER BY Distance ASC;
```

## Dependencies

### Required Packages

- **EFCore.SqlServer.VectorSearch**: Version 9.0.0-preview.2 or later
- **.NET 9.0** or later

### SQL Server Requirements

- SQL Server 2025 container image (`mcr.microsoft.com/mssql/server:2025-latest`)
- Preview features enabled for vector indexes and `VECTOR_SEARCH`

### Container Configuration

```csharp
var sql = builder.AddSqlServer("sql")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithImageTag("2025-latest")
    .WithEnvironment("ACCEPT_EULA", "Y");
```

## References

- [Vector Data Type (Microsoft Learn)](https://learn.microsoft.com/en-us/sql/t-sql/data-types/vector-data-type)
- [VECTOR_DISTANCE Function (Microsoft Learn)](https://learn.microsoft.com/en-us/sql/t-sql/functions/vector-distance-transact-sql)
- [VECTOR_SEARCH Function (Microsoft Learn)](https://learn.microsoft.com/en-us/sql/t-sql/functions/vector-search-transact-sql)
- [CREATE VECTOR INDEX (Microsoft Learn)](https://learn.microsoft.com/en-us/sql/t-sql/statements/create-vector-index-transact-sql)
- [Vector Search Overview (Microsoft Learn)](https://learn.microsoft.com/en-us/sql/sql-server/ai/vectors)
- [EFCore.SqlServer.VectorSearch NuGet Package](https://www.nuget.org/packages/EFCore.SqlServer.VectorSearch/)
