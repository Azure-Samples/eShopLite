using AGUI.Client;
using Contracts;
using Microsoft.AspNetCore.Components.AI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Products;
using ShoppingAgent;
using Store.Components;
using Xunit;

namespace Scenario.Tests;

public sealed class ProtocolTests : IAsyncLifetime
{
    private readonly string databasePath = Path.Combine(Path.GetTempPath(), $"eshoplite18-tests-{Guid.NewGuid():N}.db");
    private WebApplicationFactory<CatalogDb> products = null!;
    private WebApplicationFactory<DemoChatClient> agent = null!;

    public Task InitializeAsync()
    {
        products = new WebApplicationFactory<CatalogDb>().WithWebHostBuilder(host =>
        {
            host.UseSetting("Demo", "true");
            host.UseSetting("ConnectionStrings:DemoCatalog", $"Data Source={databasePath};Pooling=False");
        });
        _ = products.CreateClient();
        agent = new WebApplicationFactory<DemoChatClient>().WithWebHostBuilder(host =>
        {
            host.UseSetting("Demo", "true");
            host.ConfigureServices(services => services.AddHttpClient("products")
                .ConfigurePrimaryHttpMessageHandler(() => products.Server.CreateHandler()));
        });
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Catalog_api_bounds_search_and_exposes_no_write_routes()
    {
        using var http = products.CreateClient();
        var catalog = await http.GetFromJsonAsync<Product[]>("/api/product");
        Assert.Equal(9, catalog!.Length);
        using var invalid = await http.GetAsync("/api/product/search?query=camping&limit=100");
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, invalid.StatusCode);
        using var semantic = await http.GetAsync("/api/product/search?query=camping&semantic=true");
        Assert.Equal(System.Net.HttpStatusCode.ServiceUnavailable, semantic.StatusCode);
        using var write = await http.DeleteAsync("/api/product/9");
        Assert.Equal(System.Net.HttpStatusCode.MethodNotAllowed, write.StatusCode);
    }

    [Fact]
    public async Task Agui_stream_executes_catalog_tool_and_generates_typed_cards()
    {
        using var http = agent.CreateClient();
        using var chat = new AGUIChatClient(new AGUIChatClientOptions(http, "/agent"));
        using var ui = ShoppingConversation.Create(chat);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var blocks = new List<ContentBlock>();
        await foreach (var block in ui.SendMessageAsync(new ChatMessage(ChatRole.User, "find camping equipment"),
            cancellationToken: timeout.Token))
        {
            blocks.Add(block);
        }
        var search = Assert.Single(blocks.OfType<ProductSearchBlock>());
        Assert.True(search.HasResult);
        Assert.NotNull(search.SearchResult);
        Assert.True(search.SearchResult.Products is not null,
            $"Tool result shape: {System.Text.Json.JsonSerializer.Serialize(search.Result?.Result)}");
        Assert.Equal(3, search.SearchResult.Products.Length);
        Assert.All(search.SearchResult.Products, product => Assert.Contains(product, CatalogSeed.Products));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Ui_action_waits_for_human_before_invocation(bool accepted)
    {
        using var http = agent.CreateClient();
        using var chat = new AGUIChatClient(new AGUIChatClientOptions(http, "/agent"));
        var invocations = 0;
        bool? decision = null;
        using var ui = new UIAgent(chat, options => options.RegisterUIAction(AIFunctionFactory.Create(
            (int productId, bool accepted = false) =>
            {
                Assert.Equal(9, productId);
                invocations++;
                decision = accepted;
                return accepted ? "Added one catalog product." : "Rejected without mutation.";
            }, "confirm_add_to_cart")));
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        await foreach (var block in ui.SendMessageAsync(new ChatMessage(ChatRole.User, "add tent to cart"),
            cancellationToken: timeout.Token))
        {
            if (block is UIActionBlock action)
            {
                Assert.Equal(0, invocations);
                Assert.False(action.IsComplete);
                action.Call.Arguments ??= new Dictionary<string, object?>();
                action.Call.Arguments["accepted"] = accepted;
                await action.InvokeAsync();
            }
        }
        Assert.Equal(1, invocations);
        Assert.Equal(accepted, decision);
    }

    public async Task DisposeAsync()
    {
        await agent.DisposeAsync();
        await products.DisposeAsync();
        if (File.Exists(databasePath)) { File.Delete(databasePath); }
    }
}
