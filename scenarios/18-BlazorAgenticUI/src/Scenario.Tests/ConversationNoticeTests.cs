using AGUI.Client;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.AI;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Store.Components;
using Xunit;

namespace Scenario.Tests;

public sealed class ConversationNoticeTests
{
    [Fact]
    public async Task Failed_conversation_shows_safe_actionable_error_without_provider_details()
    {
        using var http = new HttpClient(new UnavailableAgent()) { BaseAddress = new Uri("https://agent.test") };
        using var chat = new AGUIChatClient(new AGUIChatClientOptions(http, "/agent"));
        using var agent = ShoppingConversation.Create(chat);
        using var context = new AgentContext(agent);
        await context.SendMessageAsync("Synthetic test");
        Assert.Equal(ConversationStatus.Error, context.Status);

        await using var services = new ServiceCollection().AddLogging().BuildServiceProvider();
        await using var renderer = new HtmlRenderer(services, services.GetRequiredService<ILoggerFactory>());
        var html = await renderer.Dispatcher.InvokeAsync(async () =>
        {
            RenderFragment notice = builder =>
            {
                builder.OpenComponent<ConversationNotice>(0);
                builder.CloseComponent();
            };
            var output = await renderer.RenderComponentAsync<CascadingValue<AgentContext>>(
                ParameterView.FromDictionary(new Dictionary<string, object?>
                {
                    ["Value"] = context,
                    ["ChildContent"] = notice
                }));
            return output.ToHtmlString();
        });

        Assert.Contains("role=\"alert\"", html);
        Assert.Contains("Check the cart before retrying", html);
        Assert.DoesNotContain("private provider diagnostic", html);
    }

    private sealed class UnavailableAgent : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.ServiceUnavailable)
            {
                Content = new StringContent("private provider diagnostic")
            });
    }
}
