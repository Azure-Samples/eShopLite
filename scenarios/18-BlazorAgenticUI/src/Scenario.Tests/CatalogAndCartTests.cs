using Contracts;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Hosting;
using Products;
using ShoppingAgent;
using Store.Services;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Scenario.Tests;

public sealed class CatalogAndCartTests
{
    [Theory]
    [InlineData("", null, 3)]
    [InlineData("camping", -1, 3)]
    [InlineData("camping", null, 0)]
    [InlineData("camping", null, 10)]
    public void Search_rejects_invalid_input(string query, int? maxPrice, int limit) =>
        Assert.ThrowsAny<ArgumentException>(() => SearchInput.Validate(query, maxPrice, limit));

    [Fact]
    public void Search_rejects_overlong_query() =>
        Assert.Throws<ArgumentException>(() => SearchInput.Validate(new string('x', 301), null, 3));

    [Fact]
    public async Task Keyword_results_respect_price_and_limit()
    {
        var result = await new CatalogSearch(null).SearchAsync(CatalogSeed.Products, "camping", 30, 2, false, default);
        Assert.Equal(2, result.Products.Length);
        Assert.All(result.Products, product => Assert.True(product.Price <= 30));
        Assert.All(result.Products, product => Assert.Contains(product, CatalogSeed.Products));
    }

    [Fact]
    public async Task No_match_is_an_empty_grounded_result()
    {
        var result = await new CatalogSearch(null).SearchAsync(CatalogSeed.Products, "submarine", null, 3, false, default);
        Assert.Empty(result.Products);
    }

    [Fact]
    public async Task Semantic_search_without_embeddings_fails_explicitly() =>
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new CatalogSearch(null).SearchAsync(CatalogSeed.Products, "camping", null, 3, true, default));

    [Fact]
    public async Task Semantic_search_ranks_vectors_and_respects_price_and_limit()
    {
        using var embeddings = new TestEmbeddings();
        var search = new CatalogSearch(embeddings);
        await search.InitializeAsync(CatalogSeed.Products, default);
        var result = await search.SearchAsync(CatalogSeed.Products, "precipitation protection", 60, 1, true, default);
        Assert.Equal(3, Assert.Single(result.Products).Id);
        Assert.Equal(10, embeddings.Inputs);
    }

    [Theory]
    [InlineData("https://models.test")]
    [InlineData("https://models.test/")]
    [InlineData("https://models.test/openai/v1")]
    [InlineData("https://models.test/openai/v1/")]
    public void Model_endpoint_is_normalized_once(string endpoint) =>
        Assert.Equal("https://models.test/openai/v1/", Extensions.GetModelEndpoint(endpoint).AbsoluteUri);

    [Theory]
    [InlineData("http://models.test")]
    [InlineData("https://user:password@models.test")]
    [InlineData("https://models.test/?key=secret")]
    [InlineData("https://models.test/#fragment")]
    [InlineData("https://models.test/api/projects/example")]
    public void Model_endpoint_rejects_unsafe_or_non_inference_urls(string endpoint) =>
        Assert.Throws<InvalidOperationException>(() => Extensions.GetModelEndpoint(endpoint));

    [Fact]
    public async Task Rejected_cart_action_never_reads_or_mutates_catalog()
    {
        var handler = new CatalogHandler();
        var cart = new CartState(new CatalogClient(new HttpClient(handler) { BaseAddress = new Uri("https://catalog.test") }));
        var result = await cart.RespondAsync(9, false);
        Assert.Contains("rejected", result);
        Assert.Empty(cart.Items);
        Assert.Equal(0, handler.Calls);
    }

    [Fact]
    public async Task Approved_cart_uses_catalog_price_and_is_session_scoped()
    {
        var client = new CatalogClient(new HttpClient(new CatalogHandler()) { BaseAddress = new Uri("https://catalog.test") });
        var cart = new CartState(client);
        var anotherSession = new CartState(client);
        await cart.RespondAsync(9, true);
        Assert.Equal(99.99m, cart.Total);
        Assert.Equal(1, Assert.Single(cart.Items).Quantity);
        Assert.Empty(anotherSession.Items);
        cart.Remove(9);
        Assert.Empty(cart.Items);
    }

    [Fact]
    public async Task Cart_limits_quantity_and_does_not_hide_catalog_failure()
    {
        var handler = new CatalogHandler();
        var cart = new CartState(new CatalogClient(new HttpClient(handler) { BaseAddress = new Uri("https://catalog.test") }));
        for (var index = 0; index < 10; index++) { await cart.RespondAsync(9, true); }
        await Assert.ThrowsAsync<InvalidOperationException>(() => cart.RespondAsync(9, true));
        Assert.Equal(10, Assert.Single(cart.Items).Quantity);
        handler.StatusCode = HttpStatusCode.ServiceUnavailable;
        await Assert.ThrowsAsync<HttpRequestException>(() => cart.RespondAsync(8, true));
        Assert.Single(cart.Items);
    }

    [Fact]
    public async Task Tool_validates_before_http_and_propagates_errors()
    {
        var handler = new CatalogHandler();
        var tools = new ProductTools(new HttpClient(handler) { BaseAddress = new Uri("https://catalog.test") }, false);
        await Assert.ThrowsAnyAsync<ArgumentException>(() => tools.SearchProducts("camping", limit: 100));
        Assert.Equal(0, handler.Calls);
        handler.StatusCode = HttpStatusCode.ServiceUnavailable;
        await Assert.ThrowsAsync<HttpRequestException>(() => tools.GetProduct(9));
    }

    [Fact]
    public async Task Demo_client_emits_real_tool_calls_not_invented_products()
    {
        using var client = new DemoChatClient();
        var response = await client.GetResponseAsync([new ChatMessage(ChatRole.User, "find camping equipment")]);
        var call = Assert.Single(response.Messages.SelectMany(message => message.Contents).OfType<FunctionCallContent>());
        Assert.Equal("search_products", call.Name);
        Assert.Equal("camping", call.Arguments!["query"]);
    }

    private sealed class TestEmbeddings : IEmbeddingGenerator<string, Embedding<float>>
    {
        public int Inputs { get; private set; }

        public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(IEnumerable<string> values,
            EmbeddingGenerationOptions? options = null, CancellationToken cancellationToken = default)
        {
            var results = values.Select(value =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                Inputs++;
                var relevant = value.Contains("Rain Jacket", StringComparison.Ordinal) ||
                    value.Contains("Survival Kit", StringComparison.Ordinal) || value == "precipitation protection";
                return new Embedding<float>(relevant ? new float[] { 1, 0 } : new float[] { 0, 1 });
            }).ToArray();
            return Task.FromResult(new GeneratedEmbeddings<Embedding<float>>(results));
        }

        public object? GetService(Type serviceType, object? serviceKey = null) =>
            serviceKey is null && serviceType.IsInstanceOfType(this) ? this : null;

        public void Dispose() { }
    }

    private sealed class CatalogHandler : HttpMessageHandler
    {
        public int Calls { get; private set; }
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls++;
            return Task.FromResult(new HttpResponseMessage(StatusCode) { Content = JsonContent.Create(CatalogSeed.Products[8]) });
        }
    }
}
