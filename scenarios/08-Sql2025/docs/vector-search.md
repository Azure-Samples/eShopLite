# Vector Search (SQL Server 2025)

## Overview

The 08-Sql2025 scenario leverages SQL Server 2025's native vector search capabilities combined with Azure OpenAI embeddings to provide high-performance semantic search directly within the database engine.

## Native Vector Storage

### SQL Server 2025 Vector Types
```sql
-- Native vector column definitions
ALTER TABLE Products ADD 
    NameEmbedding vector(1536),
    DescriptionEmbedding vector(1536);

-- Vector indexes for optimal performance
CREATE INDEX IX_Products_NameEmbedding_Vector 
ON Products(NameEmbedding) 
USING VECTOR;

CREATE INDEX IX_Products_DescriptionEmbedding_Vector 
ON Products(DescriptionEmbedding) 
USING VECTOR;
```

### Entity Framework Integration
```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    
    [Column(TypeName = "vector(1536)")]
    public float[] NameEmbedding { get; set; }
    
    [Column(TypeName = "vector(1536)")]
    public float[] DescriptionEmbedding { get; set; }
}
```

## Vector Search Operations

### Similarity Search
```csharp
public async Task<List<ProductSearchResult>> VectorSimilaritySearchAsync(
    string query, 
    int maxResults = 10, 
    double threshold = 0.7)
{
    // Generate query embedding using Azure OpenAI
    var queryEmbedding = await embeddingClient.GenerateEmbeddingAsync(query);
    
    // SQL Server 2025 native vector search
    var results = await context.Products
        .FromSqlRaw(@"
            SELECT 
                Id, Name, Description, Price,
                VECTOR_DISTANCE('cosine', DescriptionEmbedding, @queryVector) as Similarity
            FROM Products
            WHERE VECTOR_DISTANCE('cosine', DescriptionEmbedding, @queryVector) >= @threshold
            ORDER BY VECTOR_DISTANCE('cosine', DescriptionEmbedding, @queryVector) DESC
            OFFSET 0 ROWS FETCH NEXT @maxResults ROWS ONLY",
            new SqlParameter("@queryVector", queryEmbedding.Value.Vector.ToArray()),
            new SqlParameter("@threshold", threshold),
            new SqlParameter("@maxResults", maxResults))
        .Select(p => new ProductSearchResult
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            Score = p.Similarity
        })
        .ToListAsync();
    
    return results;
}
```

### Hybrid Search Implementation
```csharp
public async Task<List<ProductSearchResult>> HybridSearchAsync(
    string query, 
    int maxResults = 10)
{
    // Generate embedding for semantic component
    var embedding = await embeddingClient.GenerateEmbeddingAsync(query);
    
    // Combine full-text search with vector similarity
    var hybridResults = await context.Products
        .FromSqlRaw(@"
            WITH FullTextResults AS (
                SELECT p.*, 
                       KEY_TBL.RANK as FTSScore
                FROM Products p
                INNER JOIN FREETEXTTABLE(Products, (Name, Description), @query) KEY_TBL 
                    ON p.Id = KEY_TBL.[KEY]
            ),
            VectorResults AS (
                SELECT p.*,
                       VECTOR_DISTANCE('cosine', p.DescriptionEmbedding, @embedding) as VectorSimilarity
                FROM Products p
                WHERE VECTOR_DISTANCE('cosine', p.DescriptionEmbedding, @embedding) >= 0.5
            )
            SELECT 
                p.Id, p.Name, p.Description, p.Price,
                COALESCE(fts.FTSScore, 0) as KeywordScore,
                COALESCE(vec.VectorSimilarity, 0) as SemanticScore,
                (COALESCE(fts.FTSScore, 0) * 0.3 + COALESCE(vec.VectorSimilarity, 0) * 0.7) as CombinedScore
            FROM Products p
            LEFT JOIN FullTextResults fts ON p.Id = fts.Id
            LEFT JOIN VectorResults vec ON p.Id = vec.Id
            WHERE fts.Id IS NOT NULL OR vec.Id IS NOT NULL
            ORDER BY CombinedScore DESC
            OFFSET 0 ROWS FETCH NEXT @maxResults ROWS ONLY",
            new SqlParameter("@query", query),
            new SqlParameter("@embedding", embedding.Value.Vector.ToArray()),
            new SqlParameter("@maxResults", maxResults))
        .ToListAsync();
    
    return hybridResults;
}
```

## Performance Optimization

### Vector Index Strategies
```sql
-- Optimized vector index with specific parameters
CREATE INDEX IX_Products_DescriptionEmbedding_Optimized
ON Products(DescriptionEmbedding)
USING VECTOR
WITH (
    VECTOR_TYPE = 'FLAT',
    DISTANCE_FUNCTION = 'COSINE',
    DIMENSION = 1536
);

-- Composite index for filtered vector searches
CREATE INDEX IX_Products_CategoryVector
ON Products(CategoryId, DescriptionEmbedding)
USING VECTOR;
```

### Query Optimization
```csharp
public class OptimizedVectorSearch
{
    // Pre-compiled query for better performance
    private static readonly Func<ApplicationDbContext, float[], double, int, IAsyncEnumerable<ProductSearchResult>> 
        CompiledVectorSearch = EF.CompileAsyncQuery(
            (ApplicationDbContext context, float[] queryVector, double threshold, int maxResults) =>
                context.Products
                    .FromSqlRaw(@"
                        SELECT Id, Name, Description, Price,
                               VECTOR_DISTANCE('cosine', DescriptionEmbedding, @p0) as Similarity
                        FROM Products
                        WHERE VECTOR_DISTANCE('cosine', DescriptionEmbedding, @p0) >= @p1
                        ORDER BY VECTOR_DISTANCE('cosine', DescriptionEmbedding, @p0) DESC
                        OFFSET 0 ROWS FETCH NEXT @p2 ROWS ONLY",
                        queryVector, threshold, maxResults)
                    .Select(p => new ProductSearchResult
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        Price = p.Price,
                        Score = p.Similarity
                    }));
    
    public async Task<List<ProductSearchResult>> FastVectorSearchAsync(
        float[] queryVector, 
        double threshold = 0.7, 
        int maxResults = 10)
    {
        var results = new List<ProductSearchResult>();
        await foreach (var result in CompiledVectorSearch(context, queryVector, threshold, maxResults))
        {
            results.Add(result);
        }
        return results;
    }
}
```

## Advanced Vector Operations

### Multi-Vector Search
```csharp
public async Task<List<ProductSearchResult>> MultiVectorSearchAsync(
    string nameQuery,
    string descriptionQuery,
    double nameWeight = 0.3,
    double descWeight = 0.7)
{
    var nameEmbedding = await embeddingClient.GenerateEmbeddingAsync(nameQuery);
    var descEmbedding = await embeddingClient.GenerateEmbeddingAsync(descriptionQuery);
    
    var results = await context.Products
        .FromSqlRaw(@"
            SELECT 
                Id, Name, Description, Price,
                (VECTOR_DISTANCE('cosine', NameEmbedding, @nameVector) * @nameWeight +
                 VECTOR_DISTANCE('cosine', DescriptionEmbedding, @descVector) * @descWeight) as WeightedSimilarity
            FROM Products
            ORDER BY WeightedSimilarity DESC
            OFFSET 0 ROWS FETCH NEXT 10 ROWS ONLY",
            new SqlParameter("@nameVector", nameEmbedding.Value.Vector.ToArray()),
            new SqlParameter("@descVector", descEmbedding.Value.Vector.ToArray()),
            new SqlParameter("@nameWeight", nameWeight),
            new SqlParameter("@descWeight", descWeight))
        .ToListAsync();
    
    return results;
}
```

### Filtered Vector Search
```csharp
public async Task<List<ProductSearchResult>> FilteredVectorSearchAsync(
    string query,
    int? categoryId = null,
    decimal? minPrice = null,
    decimal? maxPrice = null)
{
    var embedding = await embeddingClient.GenerateEmbeddingAsync(query);
    
    var sql = new StringBuilder(@"
        SELECT Id, Name, Description, Price,
               VECTOR_DISTANCE('cosine', DescriptionEmbedding, @embedding) as Similarity
        FROM Products
        WHERE VECTOR_DISTANCE('cosine', DescriptionEmbedding, @embedding) >= 0.5");
    
    var parameters = new List<SqlParameter>
    {
        new("@embedding", embedding.Value.Vector.ToArray())
    };
    
    if (categoryId.HasValue)
    {
        sql.Append(" AND CategoryId = @categoryId");
        parameters.Add(new SqlParameter("@categoryId", categoryId.Value));
    }
    
    if (minPrice.HasValue)
    {
        sql.Append(" AND Price >= @minPrice");
        parameters.Add(new SqlParameter("@minPrice", minPrice.Value));
    }
    
    if (maxPrice.HasValue)
    {
        sql.Append(" AND Price <= @maxPrice");
        parameters.Add(new SqlParameter("@maxPrice", maxPrice.Value));
    }
    
    sql.Append(@"
        ORDER BY VECTOR_DISTANCE('cosine', DescriptionEmbedding, @embedding) DESC
        OFFSET 0 ROWS FETCH NEXT 10 ROWS ONLY");
    
    return await context.Products
        .FromSqlRaw(sql.ToString(), parameters.ToArray())
        .ToListAsync();
}
```

## Performance Monitoring

### Vector Search Analytics
```csharp
public class VectorSearchAnalytics
{
    public async Task<VectorSearchMetrics> GetSearchPerformanceAsync()
    {
        var metrics = await context.Database.ExecuteScalarAsync<VectorSearchMetrics>(@"
            SELECT 
                COUNT(*) as TotalVectorQueries,
                AVG(total_elapsed_time / execution_count) as AvgQueryTimeMs,
                MAX(total_elapsed_time / execution_count) as MaxQueryTimeMs,
                SUM(execution_count) as TotalExecutions
            FROM sys.dm_exec_query_stats qs
            CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) st
            WHERE st.text LIKE '%VECTOR_DISTANCE%'
            AND qs.creation_time > DATEADD(hour, -24, GETDATE())");
        
        return metrics;
    }
    
    public async Task<List<PopularVectorQueries>> GetPopularSearchTermsAsync()
    {
        // This would require query store or custom logging
        return await context.SearchLogs
            .Where(l => l.SearchType == "Vector" && l.Timestamp > DateTime.UtcNow.AddDays(-7))
            .GroupBy(l => l.Query)
            .Select(g => new PopularVectorQueries
            {
                Query = g.Key,
                Count = g.Count(),
                AvgRelevanceScore = g.Average(l => l.TopResultScore)
            })
            .OrderByDescending(p => p.Count)
            .Take(10)
            .ToListAsync();
    }
}
```

## Data Maintenance

### Vector Index Maintenance
```sql
-- Rebuild vector indexes for optimal performance
ALTER INDEX IX_Products_DescriptionEmbedding_Vector ON Products REBUILD;

-- Update statistics for vector columns
UPDATE STATISTICS Products(IX_Products_DescriptionEmbedding_Vector);

-- Monitor vector index fragmentation
SELECT 
    i.name AS IndexName,
    s.avg_fragmentation_in_percent,
    s.fragment_count,
    s.page_count
FROM sys.dm_db_index_physical_stats(DB_ID(), OBJECT_ID('Products'), NULL, NULL, 'DETAILED') s
INNER JOIN sys.indexes i ON s.object_id = i.object_id AND s.index_id = i.index_id
WHERE i.name LIKE '%Vector%';
```

### Vector Data Synchronization
```csharp
public class VectorSynchronizationService
{
    public async Task SynchronizeProductVectorsAsync()
    {
        // Find products without embeddings
        var productsNeedingEmbeddings = await context.Products
            .Where(p => p.DescriptionEmbedding == null || p.NameEmbedding == null)
            .ToListAsync();
        
        foreach (var product in productsNeedingEmbeddings)
        {
            try
            {
                if (product.NameEmbedding == null && !string.IsNullOrEmpty(product.Name))
                {
                    var nameEmbedding = await embeddingClient.GenerateEmbeddingAsync(product.Name);
                    product.NameEmbedding = nameEmbedding.Value.Vector.ToArray();
                }
                
                if (product.DescriptionEmbedding == null && !string.IsNullOrEmpty(product.Description))
                {
                    var descEmbedding = await embeddingClient.GenerateEmbeddingAsync(product.Description);
                    product.DescriptionEmbedding = descEmbedding.Value.Vector.ToArray();
                }
                
                await context.SaveChangesAsync();
                
                // Rate limiting to avoid API throttling
                await Task.Delay(100);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to generate embeddings for product {ProductId}", product.Id);
            }
        }
    }
}
```

## Best Practices

### Vector Search Optimization
1. **Use appropriate thresholds** for similarity scores (typically 0.5-0.8)
2. **Implement proper indexing** on vector columns
3. **Cache embeddings** to reduce API calls
4. **Monitor query performance** regularly
5. **Use hybrid search** for best results

### Scalability Considerations
- **Partition large tables** by vector similarity ranges
- **Use read replicas** for read-heavy vector search workloads
- **Implement connection pooling** for concurrent searches
- **Consider memory allocation** for vector index caching

### Security and Compliance
- **Encrypt vector data** at rest and in transit
- **Implement access controls** for vector search endpoints
- **Audit vector search operations** for compliance
- **Protect embedding generation APIs** with proper authentication