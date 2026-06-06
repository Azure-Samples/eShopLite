using SearchEntities;

namespace SemanticSearchFunction.Repositories;

public interface ISemanticSearchRepository
{
    Task<SearchResponse> SearchAsync(SearchRequest searchRequest, CancellationToken cancellationToken = default);
}
