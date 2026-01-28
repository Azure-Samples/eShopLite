# SQL Server 2025 Setup

## Overview

This scenario demonstrates the setup and configuration of [SQL Server 2025](https://learn.microsoft.com/en-us/sql/sql-server/sql-server-2025-overview) with [native vector search capabilities](https://learn.microsoft.com/en-us/sql/sql-server/ai/vectors). The setup includes container configuration, vector feature enablement, and integration with .NET Aspire for seamless orchestration.

## Container Configuration

### SQL Server 2025 Image

The scenario uses the latest SQL Server 2025 preview image with built-in vector support (from `eShopAppHost/Program.cs`):

```csharp
var sql = builder.AddSqlServer("sql")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithImageTag("2025-latest")  // SQL Server 2025 preview
    .WithEnvironment("ACCEPT_EULA", "Y");
```

### Key Configuration Parameters

- **Image Tag**: `2025-latest` provides the latest preview with vector capabilities
- **Lifetime**: `ContainerLifetime.Persistent` ensures data persistence across restarts
- **EULA**: Required acceptance for SQL Server container licensing
- **Data Volume**: Persistent storage for database files

## Database Setup

### Database Creation

The database is configured with vector search capabilities (from `eShopAppHost/Program.cs`):

```csharp
var productsDb = sql
    .WithDataVolume()  // Persistent storage volume
    .AddDatabase("productsDb");
```

### Entity Framework Configuration

Custom configuration enables vector search support (from `Products/Program.cs`):

```csharp
// Custom DbContext configuration for vector search
var productsDbConnectionString = builder.Configuration.GetConnectionString("productsDb");
builder.Services.AddDbContext<Context>(options =>
    options.UseSqlServer(productsDbConnectionString, o => o.UseVectorSearch()));
```

**Note**: The standard Aspire `AddSqlServerDbContext` method doesn't support vector search configuration, requiring this custom approach.

## Vector Search Features

### Native Vector Data Types

SQL Server 2025 introduces the native [`VECTOR` data type](https://learn.microsoft.com/en-us/sql/t-sql/data-types/vector-data-type):

```sql
-- Vector column definition in table
CREATE TABLE Products (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX),
    Price DECIMAL(10,2),
    ImageUrl NVARCHAR(255),
    Embedding VECTOR(1536)  -- Native vector type for embeddings
);
```

### Vector Index Creation

Create a [vector index](https://learn.microsoft.com/en-us/sql/t-sql/statements/create-vector-index-transact-sql) for optimized similarity search using the [DiskANN algorithm](https://learn.microsoft.com/en-us/sql/sql-server/ai/vectors#approximate-vector-index-and-vector-search-approximate-nearest-neighbors):

> **Note**: In SQL Server 2025, vector indexes are a preview feature. You must enable preview features first in the target database:
> ```sql
> ALTER DATABASE SCOPED CONFIGURATION SET PREVIEW_FEATURES = ON;
> ```

```sql
-- Vector index for cosine similarity search
CREATE VECTOR INDEX IX_Products_Embedding 
ON Products(Embedding)
WITH (METRIC = 'cosine', TYPE = 'DiskANN');
```

**Current limitations of vector indexes (preview):**

- Table must have a single-column, integer primary key clustered index
- Table becomes read-only while the vector index exists
- Vector index must be dropped and recreated to incorporate new data

## Development Environment Setup

### Prerequisites

- **Docker Desktop**: For container runtime
- **SQL Server 2025 Preview**: Container image access
- **.NET 9.0 SDK**: For application development
- **Entity Framework Core**: Vector search extensions ([EFCore.SqlServer.VectorSearch](https://www.nuget.org/packages/EFCore.SqlServer.VectorSearch/))

### Local Development Configuration

The setup automatically handles container lifecycle via .NET Aspire.

### Connection String Management

Aspire handles connection string generation and injection:

```csharp
// Connection string automatically provided by Aspire
var connectionString = builder.Configuration.GetConnectionString("productsDb");
```

## Container Management

### Lifecycle Management

SQL Server 2025 container is managed through Aspire:

- **Startup**: Automatic container download and initialization
- **Health Checks**: Built-in health monitoring
- **Persistence**: Data survives container restarts
- **Cleanup**: Managed container lifecycle

### Volume Management

Persistent data storage configuration:

```csharp
sql.WithDataVolume()  // Creates persistent volume for database files
```

## Vector Search Integration

### Entity Framework Vector Support

The `Product` entity (from `DataEntities/Product.cs`):

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

The `Context` class configures the vector column type (from `Products/Models/Context.cs`):

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

### Vector Operations

Using [`VECTOR_DISTANCE`](https://learn.microsoft.com/en-us/sql/t-sql/functions/vector-distance-transact-sql) via Entity Framework (from `ProductApiActions.cs`):

```csharp
var products = await db.Product
    .OrderBy(p => EF.Functions.VectorDistance("cosine", p.Embedding, vectorSearch))
    .Take(3)
    .ToListAsync();
```

## Database Initialization

### Schema Creation

Database schema is created automatically (from `Products/Program.cs`):

```csharp
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<Context>();
    try
    {
        app.Logger.LogInformation("Ensure database created");
        context.Database.EnsureCreated();
    }
    catch (Exception exc)
    {
        app.Logger.LogError(exc, "Error creating database");
    }
    await DbInitializer.Initialize(context, app.Services.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>());
}
```

### Data Seeding

Initial product data and embedding generation (from `Products/Models/DbInitializer.cs`):

```csharp
public static class DbInitializer
{
    public static async Task Initialize(Context context, IEmbeddingGenerator<string, Embedding<float>> embeddingClient, int dimensions = 1536)
    {
        if (context.Product.Any())
            return;

        var products = new List<Product>
        {
            new Product { Name = "Solar Powered Flashlight", Description = "A fantastic product for outdoor enthusiasts", Price = 19.99m, ImageUrl = "product1.png" },
            new Product { Name = "Hiking Poles", Description = "Ideal for camping and hiking trips", Price = 24.99m, ImageUrl = "product2.png" },
            new Product { Name = "Outdoor Rain Jacket", Description = "This product will keep you warm and dry in all weathers", Price = 49.99m, ImageUrl = "product3.png" },
            new Product { Name = "Survival Kit", Description = "A must-have for any outdoor adventurer", Price = 99.99m, ImageUrl = "product4.png" },
            // ... 5 more products (9 total)
        };

        // add embeddings
        foreach (var product in products)
        {
            var productInformation = $"Name = {product.Name} - Description = {product.Description} - Price = {product.Price}";
            var productInformationEmbedding = await embeddingClient.GenerateVectorAsync(productInformation, new() { Dimensions = dimensions });
            product.Embedding = productInformationEmbedding.ToArray();
        }

        context.AddRange(products);
        context.SaveChanges();
    }
}
```

## Monitoring and Diagnostics

### Aspire Dashboard Integration

SQL Server 2025 integrates with Aspire monitoring:

- **Container Status**: Real-time container health
- **Database Metrics**: Connection counts, query performance
- **Resource Usage**: Memory, CPU, storage metrics

### Debug Configuration

Enable detailed logging for troubleshooting:

```csharp
builder.Services.AddDbContext<Context>(options =>
{
    options.UseSqlServer(connectionString, o => o.UseVectorSearch())
           .EnableSensitiveDataLogging()  // Development only
           .LogTo(Console.WriteLine, LogLevel.Information);
});
```

## Troubleshooting

### Common Issues

1. **Container Download**: Large image size may require time
2. **Vector Support**: Ensure SQL Server 2025 preview image is used
3. **Memory Requirements**: Vector operations require adequate RAM
4. **Storage Performance**: SSD recommended for vector indexes

## Security Considerations

### Container Security

- Use specific image tags rather than 'latest' for production
- Configure appropriate resource limits
- Secure connection strings and credentials
- Regular security updates for container images

### Database Security

- Enable encryption at rest for sensitive data
- Configure appropriate user permissions
- Use strong passwords for SA account
- Network security for container communications

## Additional Resources

- [SQL Server 2025 Overview (Microsoft Learn)](https://learn.microsoft.com/en-us/sql/sql-server/sql-server-2025-overview)
- [Vector Search Overview (Microsoft Learn)](https://learn.microsoft.com/en-us/sql/sql-server/ai/vectors)
- [VECTOR Data Type (Microsoft Learn)](https://learn.microsoft.com/en-us/sql/t-sql/data-types/vector-data-type)
- [CREATE VECTOR INDEX (Microsoft Learn)](https://learn.microsoft.com/en-us/sql/t-sql/statements/create-vector-index-transact-sql)
- [VECTOR_DISTANCE Function (Microsoft Learn)](https://learn.microsoft.com/en-us/sql/t-sql/functions/vector-distance-transact-sql)
- [EFCore.SqlServer.VectorSearch NuGet Package](https://www.nuget.org/packages/EFCore.SqlServer.VectorSearch/)