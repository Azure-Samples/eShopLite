using DataEntities;
using StoreIntelligence.Services;

namespace StoreIntelligence.Endpoints;

/// <summary>
/// Minimal API endpoints for the intelligence data plane.
///
///   POST /api/intelligence/signals  — Products service calls this to record a search signal.
///   GET  /api/intelligence/signals  — Returns recent signals (newest first), used by the UI.
///   GET  /api/intelligence/report   — Generates the full intelligence report (AI or fallback).
/// </summary>
public static class IntelligenceEndpoints
{
    public static void MapIntelligenceEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/intelligence");

        // ── Signal ingestion (called by Products on every search) ─────────────
        group.MapPost("/signals", (RecordSignalRequest req, StoreSignalStore store) =>
        {
            // Validate and record; StoreSignalStore silently ignores blank terms.
            store.Record(req.Term, req.Semantic, req.ResultCount);
            return Results.NoContent();
        })
        .WithName("RecordSignal")
        .Produces(StatusCodes.Status204NoContent);

        // ── Signal query (used by the dashboard page) ─────────────────────────
        group.MapGet("/signals", (StoreSignalStore store, int? max) =>
            Results.Ok(store.GetRecent(max is > 0 ? max.Value : 100)))
        .WithName("GetStoreSignals")
        .Produces<List<StoreSignal>>(StatusCodes.Status200OK);

        // ── Report generation ─────────────────────────────────────────────────
        group.MapGet("/report", async (StoreIntelligenceReportService reportService, CancellationToken ct) =>
            Results.Ok(await reportService.GenerateAsync(ct)))
        .WithName("GetStoreIntelligenceReport")
        .Produces<StoreIntelligenceReport>(StatusCodes.Status200OK);
    }
}

/// <summary>
/// Request body for POST /api/intelligence/signals.
/// Sent by the Products service whenever a customer search completes.
/// </summary>
public record RecordSignalRequest(string Term, bool Semantic, int ResultCount);
