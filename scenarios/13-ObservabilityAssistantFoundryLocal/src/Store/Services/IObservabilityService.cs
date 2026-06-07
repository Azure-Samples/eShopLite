namespace Store.Services;

public interface IObservabilityService
{
    Task<ObservabilityAnalysisClientResult> AnalyzeObservability(int minutes);
}
