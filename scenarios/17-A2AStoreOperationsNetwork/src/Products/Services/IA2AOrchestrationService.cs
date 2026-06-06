using SearchEntities;

namespace Products.Services;

public interface IA2AOrchestrationService
{
    Task<A2ASearchResponse> ExecuteA2ASearchAsync(string searchTerm);
}