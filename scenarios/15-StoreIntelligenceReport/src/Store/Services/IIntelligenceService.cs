using DataEntities;

namespace Store.Services;

public interface IIntelligenceService
{
    Task<List<StoreSignal>> GetSignals(int max = 50);
    Task<StoreIntelligenceReport?> GetReport();
}
