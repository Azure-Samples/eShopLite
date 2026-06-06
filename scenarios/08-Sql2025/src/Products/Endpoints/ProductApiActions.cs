using DataEntities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Products.Models;
using SearchEntities;

namespace Products.Endpoints;

public class ProductApiActions
{
    public static async Task<Ok<List<Product>>> GetAllProducts(Context db)
    {
        var products = await db.Product.ToListAsync();
        return TypedResults.Ok(products);
    }

    public static async Task<IResult> GetProductById(int id, Context db)
    {
        var model = await db.Product.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
        return model is not null ? Results.Ok(model) : Results.NotFound();
    }

    public static async Task<IResult> UpdateProduct(int id, Product product, Context db)
    {
        var existing = await db.Product.FirstOrDefaultAsync(m => m.Id == id);
        if (existing is null)
            return Results.NotFound();

        existing.Name = product.Name;
        existing.Description = product.Description;
        existing.Price = product.Price;
        existing.ImageUrl = product.ImageUrl;
        await db.SaveChangesAsync();
        return Results.Ok();
    }

    public static async Task<Created<Product>> CreateProduct(Product product, Context db)
    {
        db.Product.Add(product);
        await db.SaveChangesAsync();
        return TypedResults.Created($"/api/Product/{product.Id}", product);
    }

    public static async Task<IResult> DeleteProduct(int id, Context db)
    {
        var affected = await db.Product
            .Where(m => m.Id == id)
            .ExecuteDeleteAsync();
        return affected == 1 ? Results.Ok() : Results.NotFound();
    }

    public static async Task<Ok<SearchResponse>> SearchAllProducts(string search, Context db)
    {
        List<Product> products = await db.Product
            .Where(p => EF.Functions.Like(p.Name, $"%{search}%"))
            .ToListAsync();

        var response = new SearchResponse
        {
            Products = products,
            Response = products.Count > 0 ?
                $"{products.Count} Products found for [{search}]" :
                $"No products found for [{search}]"
        };
        return TypedResults.Ok(response);
    }

    public static async Task<Ok<SearchResponse>> AISearch(
        string search,
        Context db,
        IEmbeddingGenerator<string, Embedding<float>> embeddingClient,
        ILogger<ProductApiActions>? logger = null,
        int dimensions = 1536)
    {
        logger?.LogInformation("Querying for similar products to {search}", search);

        var embeddingSearch = await embeddingClient.GenerateVectorAsync(search, new() { Dimensions = dimensions });
        var vectorSearch = embeddingSearch.ToArray();
        var products = await db.Product
            .OrderBy(p => EF.Functions.VectorDistance("cosine", p.Embedding, vectorSearch))
            .Take(3)
            .ToListAsync();

        var response = new SearchResponse
        {
            Products = products,
            Response = products.Count > 0 ?
                $"{products.Count} Products found for [{search}]" :
                $"No products found for [{search}]"
        };
        return TypedResults.Ok(response);
    }
}

