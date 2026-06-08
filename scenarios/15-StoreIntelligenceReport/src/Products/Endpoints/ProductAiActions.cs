using Products.Memory;
using DataEntities;
using SearchEntities;
using Microsoft.AspNetCore.Http;
using Products.Models; // Ensure Context is available

namespace Products.Endpoints;

public static class ProductAiActions
{
    public static async Task<IResult> AISearch(string search, Context db, MemoryContext mc, Products.Intelligence.StoreSignalStore signals)
    {
        var result = await mc.Search(search, db);

        // Capture a store signal (semantic search) for the Store Intelligence Report.
        signals.Record(search, semantic: true, result?.Products?.Count ?? 0);

        return Results.Ok(result);
    }
}
