using Contracts;
using System.ComponentModel;

namespace ShoppingAgent;

public sealed class ProductTools(HttpClient http, bool demo)
{
    [Description("Find outdoor products in the catalog. Returns authoritative product IDs, names and prices. Use semantic search for natural language, or keyword search for exact words.")]
    public async Task<ProductSearchResult> SearchProducts(
        [Description("Search text, 1 to 300 characters.")] string query,
        [Description("Maximum price, or null for no price filter.")] decimal? maxPrice = null,
        [Description("Number of results, between 1 and 9.")] int limit = 3,
        [Description("Whether to use semantic search.")] bool semantic = true,
        CancellationToken cancellationToken = default)
    {
        query = SearchInput.Validate(query, maxPrice, limit);
        var uri = FormattableString.Invariant(
            $"/api/product/search?query={Uri.EscapeDataString(query)}&limit={limit}&semantic={(!demo && semantic).ToString().ToLowerInvariant()}");
        if (maxPrice is not null) { uri += FormattableString.Invariant($"&maxPrice={maxPrice.Value}"); }
        return await http.GetFromJsonAsync<ProductSearchResult>(uri, cancellationToken)
            ?? throw new HttpRequestException("Products API returned no search result.");
    }

    [Description("Read authoritative catalog details for one product before discussing its price or proposing it for the cart.")]
    public async Task<Product> GetProduct(int productId, CancellationToken cancellationToken = default)
    {
        if (productId <= 0) { throw new ArgumentOutOfRangeException(nameof(productId)); }
        return await http.GetFromJsonAsync<Product>($"/api/product/{productId}", cancellationToken)
            ?? throw new HttpRequestException("Products API returned no product.");
    }
}
