using SearchEntities;
using DataEntities;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Products.Memory;
using Products.Models;
using OpenAI.Embeddings;
using OpenAI.Chat;

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

        group.MapGet("/", ProductApiActions.GetAllProducts)
            .WithName("GetAllProducts")
            .Produces<List<Product>>(StatusCodes.Status200OK);

        group.MapGet("/{id}", ProductApiActions.GetProductById)
            .WithName("GetProductById")
            .Produces<Product>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/{id}", ProductApiActions.UpdateProduct)
            .WithName("UpdateProduct")
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status204NoContent);

        group.MapPost("/", ProductApiActions.CreateProduct)
            .WithName("CreateProduct")
            .Produces<Product>(StatusCodes.Status201Created);

        group.MapDelete("/{id}", ProductApiActions.DeleteProduct)
            .WithName("DeleteProduct")
            .Produces<Product>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/search/{search}", ProductApiActions.SearchAllProducts)
            .WithName("SearchAllProducts")
            .Produces<List<Product>>(StatusCodes.Status200OK);

        #region AI Search Endpoint
        routes.MapGet("/api/aisearch/{search}", ProductAiActions.AISearch)
            .WithName("AISearch")
            .Produces<SearchResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        #endregion
    }
}
