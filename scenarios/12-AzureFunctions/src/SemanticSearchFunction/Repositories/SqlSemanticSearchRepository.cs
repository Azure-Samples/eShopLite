using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using SearchEntities;
using SemanticSearchFunction.Functions;

namespace SemanticSearchFunction.Repositories;

public class SqlSemanticSearchRepository : ISemanticSearchRepository
{
    private readonly ILogger<SearchFunction> _logger;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
    private readonly int _dimensions = 1536;
    private readonly Context _db;

    public SqlSemanticSearchRepository(
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
        Context db,
        ILogger<SearchFunction> logger,
        int dimensions = 1536)
    {
        _logger = logger;
        _embeddingGenerator = embeddingGenerator ?? throw new ArgumentNullException(nameof(embeddingGenerator));
        _dimensions = dimensions;
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task<SearchResponse> SearchAsync(SearchRequest searchRequest, CancellationToken cancellationToken = default)
    {
        var response = new SearchResponse();

        // Generate embedding for query using the provided delegate
        var queryVector = await _embeddingGenerator.GenerateVectorAsync(searchRequest.query, new() { Dimensions = _dimensions });

        var vectorSearch = queryVector.ToArray();
        var products = await _db.Product
            .OrderBy(p => EF.Functions.VectorDistance("cosine", p.Embedding, vectorSearch))
            .Take(3)
            .ToListAsync();

        // remove the embedding from the results to avoid sending unnecessary data
        foreach (var product in products)
        {
            product.Embedding = null; // Clear the embedding to avoid sending it back
        }

        response.Products = products;
        response.Response = products.Count > 0 ?
                $"{products.Count} Products found for [{searchRequest.query}]" :
                $"No products found for [{searchRequest.query}]";       
        return response;
    }
}