# Azure OpenAI Integration (SQL 2025 Scenario)

## Overview

The 08-Sql2025 scenario integrates Azure OpenAI services with SQL Server 2025's enhanced capabilities, including native vector search and modern database features. This combination provides a powerful foundation for AI-enhanced eCommerce applications.

## Azure OpenAI Configuration

### GPT-4.1-mini
- **Model**: gpt-4.1-mini
- **Version**: 2025-04-14
- **Purpose**: Advanced chat capabilities and product recommendations
- **Capacity**: 10 units with GlobalStandard SKU

### Text Embedding 3 Small
- **Model**: text-embedding-3-small
- **Version**: 1
- **Purpose**: Enhanced embeddings with improved quality and efficiency
- **Benefits**: Better semantic understanding with smaller footprint

## Service Integration

### Production Configuration
```csharp
if (builder.ExecutionContext.IsPublishMode)
{
    var appInsights = builder.AddAzureApplicationInsights("appInsights");
    var chatDeploymentName = "gpt-41-mini";
    var embeddingsDeploymentName = "text-embedding-3-small";
    var aoai = builder.AddAzureOpenAI("openai");

    var gpt41mini = aoai.AddDeployment(name: chatDeploymentName,
            modelName: chatDeploymentName,
            modelVersion: "2025-04-14");
    gpt41mini.Resource.SkuCapacity = 10;
    gpt41mini.Resource.SkuName = "GlobalStandard";

    var embeddingsDeployment = aoai.AddDeployment(name: embeddingsDeploymentName,
        modelName: embeddingsDeploymentName,
        modelVersion: "1");
}
```

### Development Configuration
```csharp
else
{
    openai = builder.AddConnectionString("openai");
}

products.WithReference(openai)
    .WithEnvironment("AI_ChatDeploymentName", chatDeploymentName)
    .WithEnvironment("AI_embeddingsDeploymentName", embeddingsDeploymentName);
```

## SQL Server 2025 + AI Integration

### Enhanced Vector Storage
```csharp
// EF Core with SQL Server 2025 vector capabilities
<PackageReference Include="EFCore.SqlServer.VectorSearch" Version="9.0.0-preview.2" />

// Vector entity with SQL Server 2025 optimization
public class ProductVector
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    
    [Column(TypeName = "vector(1536)")]
    public float[] NameEmbedding { get; set; }
    
    [Column(TypeName = "vector(1536)")]
    public float[] DescriptionEmbedding { get; set; }
}
```

### Native Vector Search
```csharp
// SQL Server 2025 native vector similarity search
public async Task<List<ProductSearchResult>> VectorSearchAsync(string query)
{
    // Generate embedding using Azure OpenAI
    var queryEmbedding = await embeddingClient.GenerateEmbeddingAsync(query);
    
    // Use SQL Server 2025 vector search capabilities
    var results = await context.Products
        .FromSqlRaw(@"
            SELECT p.*, 
                   VECTOR_DISTANCE('cosine', p.DescriptionEmbedding, @queryVector) as Distance
            FROM Products p
            ORDER BY VECTOR_DISTANCE('cosine', p.DescriptionEmbedding, @queryVector)
            OFFSET 0 ROWS FETCH NEXT 10 ROWS ONLY",
            new SqlParameter("@queryVector", queryEmbedding.Value.Vector.ToArray()))
        .ToListAsync();
    
    return results;
}
```

## Architecture Benefits

### SQL Server 2025 Advantages
```
┌─────────────────────────────────────────┐
│              Products API               │
├─────────────────────────────────────────┤
│                                         │
│  ┌─────────────────────────────────────┐ │
│  │        Azure OpenAI                 │ │
│  │ - GPT-4.1-mini (Chat)              │ │
│  │ - text-embedding-3-small            │ │
│  └─────────────────────────────────────┘ │
│                    │                     │
│                    ▼                     │
│  ┌─────────────────────────────────────┐ │
│  │       SQL Server 2025               │ │
│  │ - Native Vector Storage             │ │
│  │ - Vector Similarity Search          │ │
│  │ - Optimized Indexing                │ │
│  │ - Integrated AI Functions           │ │
│  └─────────────────────────────────────┘ │
└─────────────────────────────────────────┘
```

### Performance Optimizations
- **Native Vector Types**: SQL Server 2025 vector data types
- **Optimized Indexes**: Database-level vector indexing
- **Reduced Data Movement**: Vectors stored directly in database
- **Query Optimization**: SQL Server query optimizer for vector operations

## Database Integration Patterns

### Vector Storage Strategy
```csharp
public class ProductVectorContext : DbContext
{
    public DbSet<Product> Products { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            // Configure vector columns for SQL Server 2025
            entity.Property(e => e.NameEmbedding)
                .HasColumnType("vector(1536)")
                .HasComment("Azure OpenAI text-embedding-3-small vector");
                
            entity.Property(e => e.DescriptionEmbedding)
                .HasColumnType("vector(1536)")
                .HasComment("Azure OpenAI text-embedding-3-small vector");
                
            // Vector indexes for performance
            entity.HasIndex(e => e.NameEmbedding)
                .HasDatabaseName("IX_Products_NameEmbedding_Vector");
                
            entity.HasIndex(e => e.DescriptionEmbedding)
                .HasDatabaseName("IX_Products_DescriptionEmbedding_Vector");
        });
    }
}
```

### Hybrid Search Implementation
```csharp
public async Task<SearchResults> HybridSearchAsync(string query, int maxResults = 10)
{
    // Generate embedding for semantic search
    var embedding = await azureOpenAIService.GenerateEmbeddingAsync(query);
    
    // Perform hybrid search combining keyword and vector search
    var results = await context.Products
        .FromSqlRaw(@"
            WITH KeywordSearch AS (
                SELECT Id, Name, Description,
                       RANK() OVER (ORDER BY FTS_Score) as KeywordRank
                FROM Products
                WHERE CONTAINS((Name, Description), @query)
            ),
            VectorSearch AS (
                SELECT Id, Name, Description,
                       VECTOR_DISTANCE('cosine', DescriptionEmbedding, @embedding) as VectorDistance,
                       RANK() OVER (ORDER BY VECTOR_DISTANCE('cosine', DescriptionEmbedding, @embedding)) as VectorRank
                FROM Products
            )
            SELECT p.*, 
                   COALESCE(k.KeywordRank, 1000) as KeywordRank,
                   COALESCE(v.VectorRank, 1000) as VectorRank,
                   (COALESCE(k.KeywordRank, 1000) + COALESCE(v.VectorRank, 1000)) as CombinedScore
            FROM Products p
            LEFT JOIN KeywordSearch k ON p.Id = k.Id
            LEFT JOIN VectorSearch v ON p.Id = v.Id
            WHERE k.Id IS NOT NULL OR v.VectorRank <= 50
            ORDER BY CombinedScore
            OFFSET 0 ROWS FETCH NEXT @maxResults ROWS ONLY",
            new SqlParameter("@query", query),
            new SqlParameter("@embedding", embedding),
            new SqlParameter("@maxResults", maxResults))
        .ToListAsync();
    
    return results;
}
```

## SQL Server 2025 Features

### Container Optimization
```csharp
// Optimized SQL Server 2025 container setup
var sql = builder.AddSqlServer("sql")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithImageTag("2025-latest")
    .WithEnvironment("ACCEPT_EULA", "Y");

var productsDb = sql
    .WithDataVolume()
    .AddDatabase("productsDb");
```

### Advanced Data Types
- **Vector Types**: Native vector(n) data type
- **JSON Enhancements**: Improved JSON processing capabilities
- **Temporal Tables**: Built-in historical data tracking
- **Graph Database**: Enhanced graph relationship support

### AI-Enhanced Functions
```sql
-- SQL Server 2025 AI functions (example)
SELECT p.Name,
       p.Description,
       VECTOR_DISTANCE('cosine', p.DescriptionEmbedding, @queryVector) as Similarity,
       PREDICT(ML_MODEL('product_category_model'), p.Name, p.Description) as PredictedCategory
FROM Products p
WHERE VECTOR_DISTANCE('cosine', p.DescriptionEmbedding, @queryVector) < 0.3
ORDER BY Similarity;
```

## Performance Monitoring

### SQL Server 2025 Metrics
```csharp
public class SqlVectorPerformanceMonitor
{
    public async Task<VectorSearchMetrics> GetPerformanceMetricsAsync()
    {
        using var connection = new SqlConnection(connectionString);
        
        var metrics = await connection.QuerySingleAsync<VectorSearchMetrics>(@"
            SELECT 
                (SELECT COUNT(*) FROM sys.dm_exec_query_stats WHERE sql_handle IN 
                    (SELECT sql_handle FROM sys.dm_exec_sql_text WHERE text LIKE '%VECTOR_DISTANCE%')) as VectorQueries,
                (SELECT AVG(total_elapsed_time/execution_count) FROM sys.dm_exec_query_stats WHERE sql_handle IN 
                    (SELECT sql_handle FROM sys.dm_exec_sql_text WHERE text LIKE '%VECTOR_DISTANCE%')) as AvgVectorQueryTime,
                (SELECT COUNT(*) FROM sys.dm_db_index_usage_stats WHERE object_id IN 
                    (SELECT object_id FROM sys.indexes WHERE name LIKE '%Vector%')) as VectorIndexUsage
        ");
        
        return metrics;
    }
}
```

### Optimization Strategies
- **Index Tuning**: Optimize vector indexes for query patterns
- **Query Plan Analysis**: Monitor execution plans for vector operations
- **Memory Management**: Tune memory allocation for vector operations
- **Connection Pooling**: Optimize connection pools for concurrent vector searches

## Migration and Compatibility

### Upgrading to SQL Server 2025
```csharp
public class VectorMigrationService
{
    public async Task MigrateToNativeVectors()
    {
        // Migration from in-memory vectors to SQL Server 2025 native vectors
        await using var transaction = await context.Database.BeginTransactionAsync();
        
        try
        {
            // Create new vector columns
            await context.Database.ExecuteSqlRawAsync(@"
                ALTER TABLE Products 
                ADD NameEmbedding_New vector(1536),
                    DescriptionEmbedding_New vector(1536)");
            
            // Migrate existing vector data
            var products = await context.Products.ToListAsync();
            foreach (var product in products)
            {
                // Convert existing embeddings to SQL Server vector format
                await context.Database.ExecuteSqlRawAsync(@"
                    UPDATE Products 
                    SET NameEmbedding_New = @nameVector,
                        DescriptionEmbedding_New = @descVector
                    WHERE Id = @id",
                    new SqlParameter("@nameVector", product.NameEmbedding),
                    new SqlParameter("@descVector", product.DescriptionEmbedding),
                    new SqlParameter("@id", product.Id));
            }
            
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
```

## Best Practices

### Development Guidelines
1. **Use SQL Server vector types** for optimal performance
2. **Implement proper indexing** for vector columns
3. **Monitor query performance** with execution plans
4. **Cache embeddings** to reduce API calls
5. **Use hybrid search** for best relevance

### Production Considerations
- **Backup Strategy**: Include vector data in backup plans
- **Scaling**: Plan for vector storage growth
- **Monitoring**: Comprehensive performance monitoring
- **Security**: Protect vector data and AI endpoints