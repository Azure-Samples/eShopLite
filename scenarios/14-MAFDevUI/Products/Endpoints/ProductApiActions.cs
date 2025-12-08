using Microsoft.EntityFrameworkCore;
using DataEntities;
using Products.Models;
using SearchEntities;
using Microsoft.AspNetCore.Http;

namespace Products.Endpoints;

public static class ProductApiActions
{
    public static async Task<IResult> GetAllProducts(Products.Models.Context db)
    {
        var products = await db.Product.ToListAsync();
        return Results.Ok(products);
    }

    public static async Task<IResult> GetProductById(int id, Products.Models.Context db)
    {
        var model = await db.Product.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
        return model is not null ? Results.Ok(model) : Results.NotFound();
    }

    public static async Task<IResult> UpdateProduct(int id, Product product, Products.Models.Context db)
    {
        var existing = await db.Product.FirstOrDefaultAsync(m => m.Id == id);
        if (existing == null)
            return Results.NotFound();
        existing.Name = product.Name;
        existing.Description = product.Description;
        existing.Price = product.Price;
        existing.ImageUrl = product.ImageUrl;
        existing.IsDefault = product.IsDefault;
        await db.SaveChangesAsync();
        return Results.Ok();
    }

    public static async Task<IResult> CreateProduct(Product product, Products.Models.Context db)
    {
        db.Product.Add(product);
        await db.SaveChangesAsync();
        return Results.Created($"/api/Product/{product.Id}", product);
    }

    public static async Task<IResult> DeleteProduct(int id, Products.Models.Context db)
    {
        var affected = await db.Product
            .Where(m => m.Id == id)
            .ExecuteDeleteAsync();
        return affected == 1 ? Results.Ok() : Results.NotFound();
    }

    public static async Task<IResult> SearchAllProducts(string search, Products.Models.Context db)
    {
        List<Product> products = await db.Product
            .Where(p => EF.Functions.Like(p.Name, $"%{search}%"))
            .ToListAsync();

        var response = new SearchResponse();
        response.Products = products;
        response.Response = products.Count > 0 ?
            $"{products.Count} Products found for [{search}]" :
            $"No products found for [{search}]";
        return Results.Ok(response);
    }

    public static async Task<IResult> GetDefaultProduct(Products.Models.Context db)
    {
        var model = await db.Product.AsNoTracking().FirstOrDefaultAsync(m => m.IsDefault);
        return model is not null ? Results.Ok(model) : Results.NotFound();
    }

    public static async Task<IResult> GetProductsByLocation(int locationId, Products.Models.Context db)
    {
        var productsAtLocation = await db.ProductsByLocation
            .Where(pl => pl.LocationId == locationId)
            .Include(pl => pl.Product)
            .Select(pl => new 
            {
                Product = pl.Product,
                Quantity = pl.Quantity
            })
            .ToListAsync();

        if (!productsAtLocation.Any())
            return Results.NotFound($"No products found at location {locationId}");

        return Results.Ok(productsAtLocation);
    }

    public static async Task<IResult> GetProductLocations(int productId, Products.Models.Context db)
    {
        var locations = await db.ProductsByLocation
            .Where(pl => pl.ProductId == productId)
            .Include(pl => pl.Location)
            .Select(pl => new 
            {
                Location = pl.Location,
                Quantity = pl.Quantity
            })
            .ToListAsync();

        if (!locations.Any())
            return Results.NotFound($"No locations found for product {productId}");

        return Results.Ok(locations);
    }

    /// <summary>
    /// Searches for product stock by name, returning products with their available quantities across all locations.
    /// </summary>
    public static async Task<IResult> SearchProductStock(string search, Products.Models.Context db)
    {
        const string UnknownLocation = "Unknown";

        // Search for products matching the name
        var matchingProducts = await db.Product
            .Where(p => EF.Functions.Like(p.Name, $"%{search}%"))
            .ToListAsync();

        if (!matchingProducts.Any())
        {
            return Results.Ok(new StockSearchResponse
            {
                Products = new List<ProductStockInfo>(),
                Message = $"No products found matching '{search}'"
            });
        }

        // Get stock information for matching products
        var productIds = matchingProducts.Select(p => p.Id).ToList();
        var stockByProduct = await db.ProductsByLocation
            .Where(pl => productIds.Contains(pl.ProductId))
            .Include(pl => pl.Product)
            .Include(pl => pl.Location)
            .GroupBy(pl => pl.ProductId)
            .Select(g => new ProductStockInfo
            {
                ProductId = g.Key,
                ProductName = g.First().Product!.Name,
                TotalQuantity = g.Sum(pl => pl.Quantity),
                LocationCount = g.Count(),
                Locations = g.Select(pl => new LocationStockInfo
                {
                    LocationId = pl.LocationId,
                    LocationName = pl.Location != null ? pl.Location.Name : UnknownLocation,
                    Quantity = pl.Quantity
                }).ToList()
            })
            .ToListAsync();

        // Add products with no stock info (not in any location)
        // Use HashSet for O(1) lookup instead of O(n) Any() check
        var productIdsWithStock = new HashSet<int>(stockByProduct.Select(s => s.ProductId));
        foreach (var product in matchingProducts.Where(p => !productIdsWithStock.Contains(p.Id)))
        {
            stockByProduct.Add(new ProductStockInfo
            {
                ProductId = product.Id,
                ProductName = product.Name,
                TotalQuantity = 0,
                LocationCount = 0,
                Locations = new List<LocationStockInfo>()
            });
        }

        return Results.Ok(new StockSearchResponse
        {
            Products = stockByProduct,
            Message = $"Found {stockByProduct.Count} product(s) matching '{search}'"
        });
    }
}

/// <summary>
/// Response model for stock search results.
/// </summary>
public class StockSearchResponse
{
    public List<ProductStockInfo> Products { get; set; } = new();
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Stock information for a single product.
/// </summary>
public class ProductStockInfo
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int TotalQuantity { get; set; }
    public int LocationCount { get; set; }
    public List<LocationStockInfo> Locations { get; set; } = new();
}

/// <summary>
/// Stock information for a product at a specific location.
/// </summary>
public class LocationStockInfo
{
    public int LocationId { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public int Quantity { get; set; }
}
