using DataEntities;
using Products.Intelligence;

namespace Products.Endpoints;

public static class IntelligenceEndpoints
{
    /// <summary>
    /// Store Intelligence endpoints.
    ///
    /// GET /api/intelligence/signals
    /// - Returns the most recent captured store signals (search events).
    ///
    /// GET /api/intelligence/report
    /// - Generates the Store Intelligence Report from captured signals (AI summary with a
    ///   deterministic fallback).
    /// </summary>
    public static void MapIntelligenceEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/intelligence");

        group.MapGet("/signals", (StoreSignalStore signals, int? max) =>
            Results.Ok(signals.GetRecent(max is > 0 ? max.Value : 100)))
            .WithName("GetStoreSignals")
            .Produces<List<StoreSignal>>(StatusCodes.Status200OK);

        group.MapGet("/report", async (StoreIntelligenceReportService reportService, CancellationToken ct) =>
            Results.Ok(await reportService.GenerateAsync(ct)))
            .WithName("GetStoreIntelligenceReport")
            .Produces<StoreIntelligenceReport>(StatusCodes.Status200OK);
    }
}
