using Products.Memory;
using DataEntities;
using SearchEntities;
using Microsoft.AspNetCore.Http;
using Products.Models; // Ensure Context is available
using Products.Services;

namespace Products.Endpoints;

public static class ProductAiActions
{
    public static async Task<IResult> AISearch(
        string search,
        Context db,
        MemoryContext mc,
        IIntelligenceSignalClient signalClient)  // Posts signal to StoreIntelligence service
    {
        var result = await mc.Search(search, db);

        // Record a semantic-search signal. Fire-and-forget: errors are swallowed by the client.
        await signalClient.RecordAsync(search, semantic: true, result?.Products?.Count ?? 0);

        return Results.Ok(result);
    }
}
