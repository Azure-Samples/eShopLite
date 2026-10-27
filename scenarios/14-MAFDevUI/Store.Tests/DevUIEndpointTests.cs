using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenAI;
using OpenAI.Responses;

namespace Store.Tests;

public class DevUIEndpointTests
{
    [Theory]
    [InlineData("https://model.example", "offline-test-key")]
    [InlineData("https://model.example/openai/v1/", "")]
    public async Task ConfiguredModelUsesResponsesWithKeyOrIdentity(string endpoint, string key)
    {
        await using var factory = new StoreFactory("Development", false, endpoint, key);
        using var client = factory.CreateClient();
        var modelClient = factory.Services.GetRequiredService<OpenAIClient>();
#pragma warning disable OPENAI001 // Verify the actual model transport without making a network request.
        Assert.Equal(new Uri("https://model.example/openai/v1/"), modelClient.Endpoint);
        Assert.NotNull(factory.Services.GetRequiredService<IChatClient>().GetService<ResponsesClient>());
#pragma warning restore OPENAI001
    }

    [Theory]
    [InlineData("http://model.example")]
    [InlineData("https://model.example/api/projects/demo")]
    [InlineData("https://model.example/?api-version=2024-10-21")]
    [InlineData("https://user@model.example")]
    public async Task ConfiguredModelRejectsInvalidInferenceEndpoint(string endpoint)
    {
        await using var factory = new StoreFactory("Development", false, endpoint);
        var error = Assert.Throws<InvalidOperationException>(() => factory.CreateClient());
        Assert.Contains("HTTPS inference endpoint", error.Message);
    }

    [Fact]
    public async Task DevelopmentListsAgentsAndExecutesWorkflowThroughResponses()
    {
        await using var factory = new StoreFactory("Development");
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/devui")).StatusCode);
        using var entities = await client.GetFromJsonAsync<JsonDocument>("/v1/entities");
        var ids = entities!.RootElement.GetProperty("entities").EnumerateArray()
            .Select(entity => entity.GetProperty("id").GetString()
                ?? throw new InvalidOperationException("Entity ID missing.")).ToArray();
        Assert.Equal(["CheckoutWorkflowSequential", "DiscountAgent", "StockAgent", "TriageAgent"], ids.Order().ToArray());

        using var response = await client.PostAsJsonAsync("/v1/responses", new
        {
            input = "GOLD membership, subtotal $100. Check inventory and calculate the discount.",
            metadata = new { entity_id = "CheckoutWorkflowSequential" },
            stream = false
        });
        var content = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, content);
        using var result = JsonDocument.Parse(content);
        Assert.Equal("completed", result.RootElement.GetProperty("status").GetString());
        Assert.Contains("Model discount", content);
        Assert.Equal(2, factory.ChatClient.Calls);
    }

    [Theory]
    [InlineData("/devui")]
    [InlineData("/v1/entities")]
    [InlineData("/v1/responses")]
    public async Task ProductionDoesNotExposeDevelopmentEndpoints(string path)
    {
        await using var factory = new StoreFactory("Production");
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        using var response = path == "/v1/responses"
            ? await client.PostAsJsonAsync(path, new { input = "Hello" })
            : await client.GetAsync(path);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private sealed class StoreFactory(string environment, bool replaceChatClient = true,
        string endpoint = "https://model.example", string key = "offline-test-key") : WebApplicationFactory<Program>
    {
        public AgentWorkflowTests.CheckoutChatClient ChatClient { get; } = new(20);

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment(environment);
            builder.UseSetting("AzureOpenAIEndpoint", endpoint);
            builder.UseSetting("AzureOpenAIApiKey", key);
            builder.UseSetting("AgentSettings:UseFoundryAgents", "false");
            builder.ConfigureTestServices(services =>
            {
                if (replaceChatClient)
                {
                    services.RemoveAll<IChatClient>();
                    services.AddSingleton<IChatClient>(ChatClient);
                }
                services.AddSingleton<IStartupFilter, LoopbackStartupFilter>();
            });
        }
    }

    private sealed class LoopbackStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
        {
            app.Use(nextMiddleware => context =>
            {
                context.Connection.RemoteIpAddress = IPAddress.Loopback;
                return nextMiddleware(context);
            });
            next(app);
        };
    }
}
