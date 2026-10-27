using Contracts;

namespace Store.Services;

public sealed class CatalogClient(HttpClient http)
{
    public async Task<Product[]> GetProductsAsync(CancellationToken ct = default) =>
        await http.GetFromJsonAsync<Product[]>("/api/product", ct)
        ?? throw new HttpRequestException("Catalog returned no products.");

    public async Task<Product> GetProductAsync(int id, CancellationToken ct = default)
    {
        if (id <= 0) { throw new ArgumentOutOfRangeException(nameof(id)); }
        return await http.GetFromJsonAsync<Product>($"/api/product/{id}", ct)
            ?? throw new HttpRequestException("Catalog returned no product.");
    }
}

public sealed class CartState(CatalogClient catalog)
{
    private readonly Dictionary<int, (Product Product, int Quantity)> items = [];
    public IReadOnlyList<(Product Product, int Quantity)> Items => items.Values.ToArray();
    public decimal Total => items.Values.Sum(item => item.Product.Price * item.Quantity);
    public event Action? Changed;

    public async Task<string> RespondAsync(int productId, bool accepted)
    {
        if (!accepted) { return "The user rejected the request. The cart was not changed."; }
        var product = await catalog.GetProductAsync(productId);
        var quantity = items.TryGetValue(product.Id, out var current) ? current.Quantity : 0;
        if (quantity >= 10) { throw new InvalidOperationException("The demo cart allows at most 10 units of each product."); }
        items[product.Id] = (product, quantity + 1);
        Changed?.Invoke();
        return $"Added one {product.Name} to the cart at the catalog price of {product.Price:0.00}. No order was placed.";
    }

    public void Remove(int id)
    {
        items.Remove(id);
        Changed?.Invoke();
    }
}
