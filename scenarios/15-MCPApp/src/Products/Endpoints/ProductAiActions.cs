using Products.Memory;
using DataEntities;
using SearchEntities;
using Microsoft.AspNetCore.Http;
using Products.Models;

namespace Products.Endpoints;

public static class ProductAiActions
{
    public static async Task<IResult> AISearch(string search, Context db, MemoryContext mc)
    {
        var result = await mc.Search(search, db);
        return Results.Ok(result);
    }
}
