using AGUI.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;
using ShoppingAgent;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Scenario.Tests;

public sealed class ModelTransportTests
{
    [Fact]
    public async Task Real_mode_streams_via_responses_with_configured_model_and_catalog_tools()
    {
        using var handler = new ResponsesHandler();
        using var modelHttp = new HttpClient(handler);
        var modelClient = new OpenAIClient(new ApiKeyCredential("synthetic-test-key"), new OpenAIClientOptions
        {
            Endpoint = new Uri("https://models.test/openai/v1/"),
            Transport = new HttpClientPipelineTransport(modelHttp)
        });
        await using var server = new WebApplicationFactory<DemoChatClient>().WithWebHostBuilder(host =>
        {
            host.UseSetting("Demo", "false");
            host.UseSetting("AzureOpenAIDeploymentName", "configured-deployment");
            host.ConfigureServices(services => services.AddSingleton(modelClient));
        });
        using var http = server.CreateClient();
        using var chat = new AGUIChatClient(new AGUIChatClientOptions(http, "/agent"));
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var response = await chat.GetResponseAsync("Synthetic transport test", cancellationToken: timeout.Token);

        Assert.Equal("Synthetic response", response.Text);
        Assert.Equal("/openai/v1/responses", handler.Path);
        using var request = JsonDocument.Parse(handler.Body!);
        Assert.Equal("configured-deployment", request.RootElement.GetProperty("model").GetString());
        Assert.True(request.RootElement.GetProperty("stream").GetBoolean());
        var tools = request.RootElement.GetProperty("tools").EnumerateArray()
            .Select(tool => tool.GetProperty("name").GetString()).ToArray();
        Assert.Contains("search_products", tools);
        Assert.Contains("get_product", tools);
        Assert.DoesNotContain("reasoning_effort", request.RootElement.EnumerateObject().Select(property => property.Name));
    }

    private sealed class ResponsesHandler : HttpMessageHandler
    {
        public string? Path { get; private set; }
        public string? Body { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Path = request.RequestUri!.AbsolutePath;
            Body = await request.Content!.ReadAsStringAsync(cancellationToken);
            var events = """
                event: response.created
                data: {"type":"response.created","sequence_number":0,"response":{"id":"resp_test","object":"response","created_at":1,"status":"in_progress","model":"configured-deployment","output":[]}}

                event: response.output_item.added
                data: {"type":"response.output_item.added","sequence_number":1,"output_index":0,"item":{"id":"msg_test","type":"message","status":"in_progress","role":"assistant","content":[]}}

                event: response.content_part.added
                data: {"type":"response.content_part.added","sequence_number":2,"item_id":"msg_test","output_index":0,"content_index":0,"part":{"type":"output_text","text":"","annotations":[]}}

                event: response.output_text.delta
                data: {"type":"response.output_text.delta","sequence_number":3,"item_id":"msg_test","output_index":0,"content_index":0,"delta":"Synthetic response"}

                event: response.completed
                data: {"type":"response.completed","sequence_number":4,"response":{"id":"resp_test","object":"response","created_at":1,"status":"completed","model":"configured-deployment","output":[{"id":"msg_test","type":"message","status":"completed","role":"assistant","content":[{"type":"output_text","text":"Synthetic response","annotations":[]}]}],"usage":{"input_tokens":1,"output_tokens":1,"total_tokens":2}}}

                """;
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(events + "\n", Encoding.UTF8, "text/event-stream")
            };
        }
    }
}
