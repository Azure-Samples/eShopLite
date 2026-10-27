using System.Text.Json.Serialization;

namespace Contracts;

public sealed record Product(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("price")] decimal Price,
    [property: JsonPropertyName("imageUrl")] string ImageUrl);

public sealed record ProductSearchResult(
    [property: JsonPropertyName("query")] string Query,
    [property: JsonPropertyName("products")] Product[] Products);

public static class SearchInput
{
    public const int MaximumQueryLength = 300;
    public const int MaximumResults = 9;

    public static string Validate(string query, decimal? maxPrice, int limit)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);
        query = query.Trim();
        if (query.Length > MaximumQueryLength)
        {
            throw new ArgumentException($"Search text must not exceed {MaximumQueryLength} characters.", nameof(query));
        }
        if (maxPrice is < 0 or > 100000)
        {
            throw new ArgumentOutOfRangeException(nameof(maxPrice));
        }
        if (limit is < 1 or > MaximumResults)
        {
            throw new ArgumentOutOfRangeException(nameof(limit));
        }
        return query;
    }
}
