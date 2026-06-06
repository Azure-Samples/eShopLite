using SearchEntities;
using DataEntities;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Products.Memory;
using Products.Models;

namespace Products.Endpoints;

public static class ProductEndpoints
{
    /// <summary>
    /// Configures the product-related endpoints for the application.
    /// </summary>
    /// <param name="routes">The route builder to add the endpoints to.</param>
    ///
    /// <remarks>
    /// This method sets up the following endpoints:
    /// 
    /// GET /api/Product/
    /// - Retrieves all products.
    /// - Response: 200 OK with a list of products.
    ///
    /// GET /api/Product/{id}
    /// - Retrieves a product by its ID.
    /// - Response: 200 OK with the product if found, 404 Not Found otherwise.
    ///
    /// PUT /api/Product/{id}
    /// - Updates an existing product by its ID.
    /// - Response: 200 OK if the product is updated, 404 Not Found otherwise.
    ///
    /// POST /api/Product/
    /// - Creates a new product.
    /// - Response: 201 Created with the created product.
    ///
    /// DELETE /api/Product/{id}
    /// - Deletes a product by its ID.
    /// - Response: 200 OK if the product is deleted, 404 Not Found otherwise.
    ///
    /// GET /api/Product/search/{search}
    /// - Searches for products by name.
    /// - Response: 200 OK with a list of matching products and search metadata.
    ///
    /// GET /api/aisearch/{search}
    /// - Searches for products using AI-based search.
    /// - Response: 200 OK with the search results, 404 Not Found otherwise.
    /// </remarks>
    public static void MapProductEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/Product");

        group.MapGet("/", async (Context db) =>
        {
            return await db.Product.ToListAsync();
        })
        .WithName("GetAllProducts")
        .Produces<List<Product>>(StatusCodes.Status200OK);

        group.MapGet("/{id}", async (int id, Context db) =>
        {
            return await db.Product.AsNoTracking()
                .FirstOrDefaultAsync(model => model.Id == id)
                is Product model
                    ? Results.Ok(model)
                    : Results.NotFound();
        })
        .WithName("GetProductById")
        .Produces<Product>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/{id}", async (int id, Product product, Context db) =>
        {
            var affected = await db.Product
                .Where(model => model.Id == id)
                .ExecuteUpdateAsync(setters => setters
                  .SetProperty(m => m.Id, product.Id)
                  .SetProperty(m => m.Name, product.Name)
                  .SetProperty(m => m.Description, product.Description)
                  .SetProperty(m => m.Price, product.Price)
                  .SetProperty(m => m.ImageUrl, product.ImageUrl)
                );

            return affected == 1 ? Results.Ok() : Results.NotFound();
        })
        .WithName("UpdateProduct")
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status204NoContent);

        group.MapPost("/", async (Product product, Context db) =>
        {
            db.Product.Add(product);
            await db.SaveChangesAsync();
            return Results.Created($"/api/Product/{product.Id}", product);
        })
        .WithName("CreateProduct")
        .Produces<Product>(StatusCodes.Status201Created);

        group.MapDelete("/{id}", async (int id, Context db) =>
        {
            var affected = await db.Product
                .Where(model => model.Id == id)
                .ExecuteDeleteAsync();

            return affected == 1 ? Results.Ok() : Results.NotFound();
        })
        .WithName("DeleteProduct")
        .Produces<Product>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/search/{search}", async (string search, Context db) =>
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            List<Product> products = await db.Product
            .Where(p => EF.Functions.Like(p.Name, $"%{search}%"))
            .ToListAsync();

            stopwatch.Stop();

            var response = new SearchResponse();
            response.Products = products;
            response.Response = products.Count > 0 ?
                $"{products.Count} Products found for [{search}]" :
                $"No products found for [{search}]";
            response.ElapsedTime = stopwatch.Elapsed;
            return response;
        })
            .WithName("SearchAllProducts")
            .Produces<List<Product>>(StatusCodes.Status200OK);

        #region AI Search Endpoint
        routes.MapGet("/api/aisearch/{search}",
            async (string search, Context db, MemoryContext mc) =>
            {
                var result = await mc.Search(search, db);
                return Results.Ok(result);
            })
            .WithName("AISearch")
            .Produces<SearchResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        #endregion
    }
}
