using AgentServices.Checkout;
using AgentServices.Configuration;
using AgentServices.Discount;
using AgentServices.Stock;
using AgentServices.Stock.Tools;
using AgentServices.Triage;
using Azure.AI.Extensions.OpenAI;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Workflows;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgentServices;

/// <summary>
/// Extension methods to register Agent Services in DI.
/// 
/// This approach follows the pattern from the Agent Framework's AgentWebChat sample:
/// https://github.com/microsoft/agent-framework/blob/main/dotnet/samples/AgentWebChat/AgentWebChat.AgentHost/Program.cs
/// </summary>
public static class AgentServicesExtensions
{
    public static WebApplicationBuilder AddAgentSettings(this WebApplicationBuilder builder)
    {
        var settings = new AgentSettings();
        builder.Configuration.GetSection(AgentSettings.SectionName).Bind(settings);

        // Also check for endpoint directly via explicit env var if not in AgentSettings section
        if (string.IsNullOrEmpty(settings.AzureOpenAIEndpoint))
        {
            settings.AzureOpenAIEndpoint = builder.Configuration["AzureOpenAIEndpoint"];
        }

        if (string.IsNullOrEmpty(settings.MicrosoftFoundryProjectEndpoint))
        {
            settings.MicrosoftFoundryProjectEndpoint = builder.Configuration.GetConnectionString("microsoftfoundryproject");
        }

        // Get deployment name from configuration
        var chatDeploymentName = builder.Configuration["AzureOpenAIDeploymentName"]
            ?? builder.Configuration["AI_ChatDeploymentName"];
        if (!string.IsNullOrEmpty(chatDeploymentName))
        {
            settings.ChatDeploymentName = chatDeploymentName;
        }

        if (settings.UseFoundryAgents &&
            (!Uri.TryCreate(settings.MicrosoftFoundryProjectEndpoint, UriKind.Absolute, out var projectEndpoint) ||
             projectEndpoint.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException(
                "Foundry agent mode requires an HTTPS AgentSettings:MicrosoftFoundryProjectEndpoint " +
                "or ConnectionStrings:microsoftfoundryproject.");
        }

        // Register settings as singleton (configuration doesn't change at runtime)
        builder.Services.AddSingleton(settings);

        return builder;
    }

    /// <summary>
    /// Registers the StockSearchTool as a typed client with an HttpClient configured
    /// to call the Products API.
    /// </summary>
    public static WebApplicationBuilder AddStockSearchTool(this WebApplicationBuilder builder, string productsApiBaseUrl)
    {
        builder.Services.AddHttpClient<StockSearchTool>(client =>
        {
            client.BaseAddress = new Uri(productsApiBaseUrl);
        });

        return builder;
    }

    public static WebApplicationBuilder AddeShopLiteAIAgents(this WebApplicationBuilder builder)
    {
        // add Discount Agent
        builder.AddAIAgent(DiscountAgentService.AgentName, (sp, key) =>
        {
            // create agent
            var chatClient = sp.GetRequiredService<IChatClient>();
            return new ChatClientAgent(chatClient,
                name: DiscountAgentService.AgentName,
                instructions: DiscountAgentService.AgentInstructions);
        });

        // add Stock Agent with external stock search tool
        builder.AddAIAgent(StockAgentService.AgentName, (sp, key) =>
        {
            // create agent and add tool
            var chatClient = sp.GetRequiredService<IChatClient>();
            var stockSearchTool = sp.GetRequiredService<StockSearchTool>();
            return new ChatClientAgent(chatClient,
                name: StockAgentService.AgentName,
                instructions: StockAgentService.AgentInstructions,
                tools: [AIFunctionFactory.Create(stockSearchTool.SearchProductStockAsync)]);
        });

        // add Triage Agent
        builder.AddAIAgent(TriageAgentService.AgentName, (sp, key) =>
        {
            var chatClient = sp.GetRequiredService<IChatClient>();
            return new ChatClientAgent(chatClient,
                name: TriageAgentService.AgentName,
                instructions: TriageAgentService.AgentInstructions);
        });

        builder.AddWorkflow(AgentCheckoutOrchestrator.WorkflowNameSequential, (sp, key) =>
        {
            var stockAgent = sp.GetRequiredKeyedService<AIAgent>(StockAgentService.AgentName);
            var discountAgent = sp.GetRequiredKeyedService<AIAgent>(DiscountAgentService.AgentName);

            // Build a sequential workflow that first checks stock and then computes discount
            return AgentWorkflowBuilder.BuildSequential(
                workflowName: AgentCheckoutOrchestrator.WorkflowNameSequential,
                agents: [stockAgent, discountAgent]);
        }).AddAsAIAgent(includeWorkflowOutputsInResponse: true);

        return builder;
    }


    public static WebApplicationBuilder AddeShopLiteFoundryAgents(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton(sp =>
        {
            var settings = sp.GetRequiredService<AgentSettings>();
            return new AIProjectClient(
                new Uri(settings.MicrosoftFoundryProjectEndpoint!),
                new DefaultAzureCredential());
        });

        // add Discount Agent from Microsoft Foundry
        builder.AddAIAgent(DiscountAgentService.AgentName, (sp, key) =>
        {
            return sp.GetRequiredService<AIProjectClient>().AsAIAgent(
                new AgentReference(DiscountAgentService.AgentName), services: sp);
        });

        // add Stock Agent from Microsoft Foundry
        builder.AddAIAgent(StockAgentService.AgentName, (sp, key) =>
        {
            var stockSearchTool = sp.GetRequiredService<StockSearchTool>();
            return sp.GetRequiredService<AIProjectClient>().AsAIAgent(
                new AgentReference(StockAgentService.AgentName),
                tools: [AIFunctionFactory.Create(stockSearchTool.SearchProductStockAsync)],
                services: sp);
        });

        // add Triage Agent from Microsoft Foundry
        builder.AddAIAgent(TriageAgentService.AgentName, (sp, key) =>
        {
            return sp.GetRequiredService<AIProjectClient>().AsAIAgent(
                new AgentReference(TriageAgentService.AgentName), services: sp);
        });

        builder.AddWorkflow(AgentCheckoutOrchestrator.WorkflowNameSequential, (sp, key) =>
        {
            var stockAgent = sp.GetRequiredKeyedService<AIAgent>(StockAgentService.AgentName);
            var discountAgent = sp.GetRequiredKeyedService<AIAgent>(DiscountAgentService.AgentName);

            var workflow = AgentWorkflowBuilder.BuildSequential(
                workflowName: AgentCheckoutOrchestrator.WorkflowNameSequential,
                agents: [stockAgent, discountAgent]);

            return workflow;
        }).AddAsAIAgent(includeWorkflowOutputsInResponse: true);

        return builder;
    }
}
