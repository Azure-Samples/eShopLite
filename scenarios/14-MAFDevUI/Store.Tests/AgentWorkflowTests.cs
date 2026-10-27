using System.Runtime.CompilerServices;
using AgentServices;
using AgentServices.Checkout;
using AgentServices.Configuration;
using AgentServices.Discount;
using AgentServices.Models;
using AgentServices.Stock;
using AgentServices.Triage;
using CartEntities;
using DataEntities;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace Store.Tests;

public class AgentWorkflowTests
{
    [Fact]
    public void SettingsHonorAppHostDeploymentAndDefaultToLocalMode()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["AzureOpenAIDeploymentName"] = "configured-chat",
            ["AI_ChatDeploymentName"] = "legacy-chat"
        });
        builder.AddAgentSettings();
        using var services = builder.Services.BuildServiceProvider();

        var settings = services.GetRequiredService<AgentSettings>();
        Assert.False(settings.UseFoundryAgents);
        Assert.Equal("configured-chat", settings.ChatDeploymentName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-an-endpoint")]
    [InlineData("http://foundry.example/projects/demo")]
    public void FoundryModeRejectsMissingOrInvalidEndpoint(string? endpoint)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["AgentSettings:UseFoundryAgents"] = "true",
            ["ConnectionStrings:microsoftfoundryproject"] = endpoint
        });

        var error = Assert.Throws<InvalidOperationException>(() => builder.AddAgentSettings());
        Assert.Contains("HTTPS", error.Message);
    }

    [Fact]
    public void FoundryModeRegistersExistingAgentsWithoutNetworkCalls()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["AgentSettings:UseFoundryAgents"] = "true",
            ["ConnectionStrings:microsoftfoundryproject"] = "https://foundry.example/api/projects/demo"
        });
        builder.AddAgentSettings();
        builder.AddStockSearchTool("https://products.example");
        builder.AddeShopLiteFoundryAgents();
        using var services = builder.Services.BuildServiceProvider(validateScopes: true);

        Assert.True(services.GetRequiredService<AgentSettings>().UseFoundryAgents);
        AssertAgentsAndWorkflow(services);
    }

    [Theory]
    [InlineData(MembershipTier.Gold, 20)]
    [InlineData(MembershipTier.Silver, 10)]
    [InlineData(MembershipTier.Normal, 0)]
    public async Task SequentialCheckoutUsesBothAgentsWithoutFallback(MembershipTier tier, int discount)
    {
        var builder = WebApplication.CreateBuilder();
        var chatClient = new CheckoutChatClient(discount);
        builder.Services.AddSingleton<IChatClient>(chatClient);
        builder.AddAgentSettings();
        builder.AddStockSearchTool("https://products.example");
        builder.AddeShopLiteAIAgents();
        await using var services = builder.Services.BuildServiceProvider(validateScopes: true);
        AssertAgentsAndWorkflow(services);
        var orchestrator = new AgentCheckoutOrchestrator(services,
            services.GetRequiredService<AgentSettings>(), NullLogger<AgentCheckoutOrchestrator>.Instance);

        var result = await orchestrator.ProcessCheckoutAsync(new AgentCheckoutRequest
        {
            MembershipTier = tier,
            Cart = new Cart { Items = [new CartItem { Name = "Test product", Price = 100, Quantity = 1 }] }
        });

        Assert.True(result.Success);
        Assert.Null(result.ErrorMessage);
        Assert.Equal(2, chatClient.Calls);
        Assert.Equal(discount, result.DiscountAmount);
        Assert.Equal(100 - discount, result.TotalAfterDiscount);
        Assert.Equal("Model discount", result.DiscountReason);
        Assert.Collection(result.AgentSteps,
            step =>
            {
                Assert.Equal(StockAgentService.AgentName, step.Name);
                Assert.Equal("Success", step.Status);
                Assert.Equal("Inventory verified.", step.Message);
            },
            step =>
            {
                Assert.Equal(DiscountAgentService.AgentName, step.Name);
                Assert.Equal("Success", step.Status);
            });
        Assert.NotEmpty(result.WorkFlowMermaid!);
    }

    private static void AssertAgentsAndWorkflow(IServiceProvider services)
    {
        foreach (var name in new[]
        {
            StockAgentService.AgentName, DiscountAgentService.AgentName,
            TriageAgentService.AgentName, AgentCheckoutOrchestrator.WorkflowNameSequential
        })
        {
            Assert.NotNull(services.GetRequiredKeyedService<AIAgent>(name));
        }

        Assert.NotNull(services.GetRequiredKeyedService<Workflow>(AgentCheckoutOrchestrator.WorkflowNameSequential));
    }

    internal sealed class CheckoutChatClient(int discount) : IChatClient
    {
        public int Calls { get; private set; }

        public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages,
            ChatOptions? options = null, CancellationToken cancellationToken = default)
        {
            var text = Calls++ == 0 ? "Inventory verified."
                : $$"""{"discountAmount":{{discount}},"reason":"Model discount"}""";
            return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, text)));
        }

        public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
            IEnumerable<ChatMessage> messages, ChatOptions? options = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var response = await GetResponseAsync(messages, options, cancellationToken);
            foreach (var update in response.ToChatResponseUpdates())
            {
                var text = update.Text;
                var midpoint = text.Length / 2;
                yield return new ChatResponseUpdate(update.Role, text[..midpoint])
                {
                    MessageId = update.MessageId,
                    ResponseId = update.ResponseId
                };
                yield return new ChatResponseUpdate(update.Role, text[midpoint..])
                {
                    MessageId = update.MessageId,
                    ResponseId = update.ResponseId
                };
            }
        }

        public object? GetService(Type serviceType, object? serviceKey = null) =>
            serviceKey is null && serviceType.IsInstanceOfType(this) ? this : null;

        public void Dispose() { }
    }
}
