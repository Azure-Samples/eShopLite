using Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using System.Numerics.Tensors;

namespace Products;

public sealed class CatalogDb(DbContextOptions<CatalogDb> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().Property(product => product.Price).HasPrecision(18, 2);
    }
}

public static class CatalogSeed
{
    public static readonly Product[] Products =
    [
        new(1, "Solar Powered Flashlight", "A fantastic product for outdoor enthusiasts", 19.99m, "product1.png"),
        new(2, "Hiking Poles", "Ideal for camping and hiking trips", 24.99m, "product2.png"),
        new(3, "Outdoor Rain Jacket", "This product will keep you warm and dry in all weathers", 49.99m, "product3.png"),
        new(4, "Survival Kit", "A must-have for any outdoor adventurer", 99.99m, "product4.png"),
        new(5, "Outdoor Backpack", "This backpack is perfect for carrying all your outdoor essentials", 39.99m, "product5.png"),
        new(6, "Camping Cookware", "This cookware set is ideal for cooking outdoors", 29.99m, "product6.png"),
        new(7, "Camping Stove", "This stove is perfect for cooking outdoors", 49.99m, "product7.png"),
        new(8, "Camping Lantern", "This lantern is perfect for lighting up your campsite", 19.99m, "product8.png"),
        new(9, "Camping Tent", "This tent is perfect for camping trips", 99.99m, "product9.png")
    ];
}

public sealed class CatalogSearch(IEmbeddingGenerator<string, Embedding<float>>? embeddings)
{
    private readonly Dictionary<int, float[]> vectors = [];

    public async Task InitializeAsync(Product[] products, CancellationToken cancellationToken)
    {
        if (embeddings is null) { return; }
        foreach (var product in products)
        {
            var result = await embeddings.GenerateAsync($"{product.Name}: {product.Description}", cancellationToken: cancellationToken);
            vectors.Add(product.Id, result.Vector.ToArray());
        }
    }

    public async Task<ProductSearchResult> SearchAsync(Product[] products, string query, decimal? maxPrice,
        int limit, bool semantic, CancellationToken cancellationToken)
    {
        query = SearchInput.Validate(query, maxPrice, limit);
        var candidates = products.Where(product => maxPrice is null || product.Price <= maxPrice);
        if (semantic)
        {
            if (embeddings is null) { throw new InvalidOperationException("Semantic search requires a configured embedding model. Demo mode supports keyword search only."); }
            var vector = (await embeddings.GenerateAsync(query, cancellationToken: cancellationToken)).Vector.ToArray();
            candidates = candidates.OrderByDescending(product => TensorPrimitives.CosineSimilarity(vector, vectors[product.Id]));
        }
        else
        {
            var terms = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            candidates = candidates.Where(product => terms.Any(term =>
                $"{product.Name} {product.Description}".Contains(term, StringComparison.OrdinalIgnoreCase)))
                .OrderBy(product => product.Id);
        }
        return new ProductSearchResult(query, candidates.Take(limit).ToArray());
    }
}
