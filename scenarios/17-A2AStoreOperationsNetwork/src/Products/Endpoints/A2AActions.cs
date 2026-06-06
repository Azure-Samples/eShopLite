using Products.Services;
using SearchEntities;

namespace Products.Endpoints;

public static class A2AActions
{
    public static async Task<IResult> A2ASearch(string search, IA2AOrchestrationService orchestrationService)
    {
        var result = await orchestrationService.ExecuteA2ASearchAsync(search);
        return Results.Ok(result);
    }
}