namespace Store.Services;

public interface IObservabilityService
{
    Task<ObservabilityAnalysisClientResult> AnalyzeObservability(int minutes);

    Task IngestEventAsync(string service, string severity, string message);
}
