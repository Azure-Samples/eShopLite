namespace Products.Services;

/// <summary>
/// Abstraction over the intelligence signal sender so the endpoint methods can be
/// unit-tested without a real HTTP client (the test project supplies a no-op implementation).
/// </summary>
public interface IIntelligenceSignalClient
{
    /// <summary>
    /// Records a search signal in the StoreIntelligence service.
    /// Implementations must NEVER throw — signal recording is fire-and-forget
    /// and must not affect the caller's response.
    /// </summary>
    Task RecordAsync(string term, bool semantic, int resultCount);
}
