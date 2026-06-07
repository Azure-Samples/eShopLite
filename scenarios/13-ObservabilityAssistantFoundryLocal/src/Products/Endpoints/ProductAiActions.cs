using Products.Memory;
using DataEntities;
using SearchEntities;
using Microsoft.AspNetCore.Http;
using Products.Models; // Ensure Context is available

namespace Products.Endpoints;

public static class ProductAiActions
{
    public static async Task<IResult> AISearch(
        string search,
        Context db,
        MemoryContext mc,
        HttpContext? httpContext = null,
        ILogger<Program>? logger = null)
    {
        if (httpContext is not null)
        {
            ProductFailureInjection.ThrowIfInjectedFailure(
                httpContext,
                logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<Program>.Instance,
                "api/aisearch");
        }

        var result = await mc.Search(search, db);
        return Results.Ok(result);
    }
}
