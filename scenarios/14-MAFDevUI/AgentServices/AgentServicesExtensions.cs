using AgentServices.Checkout;
using AgentServices.Configuration;
using AgentServices.Discount;
using AgentServices.Stock;
using AgentServices.Stock.Tools;
using AgentServices.Triage;
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

        // Also check for connection string directly if not in AgentSettings section
        if (string.IsNullOrEmpty(settings.MicrosoftFoundryConnectionString))
        {
            settings.MicrosoftFoundryConnectionString = builder.Configuration.GetConnectionString("microsoftfoundry");
        }

        if (string.IsNullOrEmpty(settings.MicrosoftFoundryProjectEndpoint))
        {
            settings.MicrosoftFoundryProjectEndpoint = builder.Configuration.GetConnectionString("microsoftfoundryproject");
        }

        // Get deployment name from configuration
        var chatDeploymentName = builder.Configuration["AI_ChatDeploymentName"];
        if (!string.IsNullOrEmpty(chatDeploymentName))
        {
            settings.ChatDeploymentName = chatDeploymentName;
        }

        // Register settings as singleton (configuration doesn't change at runtime)
        builder.Services.AddSingleton(settings);

        return builder;
    }

    /// <summary>
    /// Registers the StockSearchTool as a scoped service with an HttpClient configured 
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
            return chatClient.CreateAIAgent(
                name: DiscountAgentService.AgentName,
                instructions: DiscountAgentService.AgentInstructions);
        });

        // add Stock Agent with external stock search tool
        builder.AddAIAgent(StockAgentService.AgentName, (sp, key) =>
        {
            // create agent and add tool
            var chatClient = sp.GetRequiredService<IChatClient>();
            var stockSearchTool = sp.GetService<StockSearchTool>();
            if (stockSearchTool != null)
            {
                return chatClient.CreateAIAgent(
                    name: StockAgentService.AgentName,
                    instructions: StockAgentService.AgentInstructions,
                    tools: [AIFunctionFactory.Create(stockSearchTool.SearchProductStockAsync)]);
            }

            // Fallback: Create agent without tools
            return chatClient.CreateAIAgent(
                name: StockAgentService.AgentName,
                instructions: StockAgentService.AgentInstructions);
        });

        // add Triage Agent
        builder.AddAIAgent(TriageAgentService.AgentName, (sp, key) =>
        {
            var chatClient = sp.GetRequiredService<IChatClient>();
            return chatClient.CreateAIAgent(
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
        });

        return builder;
    }


    public static WebApplicationBuilder AddeShopLiteFoundryAgents(this WebApplicationBuilder builder)
    {
        var serviceProvider = builder.Services.BuildServiceProvider();
        var settings = serviceProvider.GetRequiredService<AgentSettings>();

        var microsoftFoundryProjectEndpoint = settings.MicrosoftFoundryProjectEndpoint;

        AIProjectClient projectClient = new(
            endpoint: new Uri(microsoftFoundryProjectEndpoint),
            tokenProvider: new DefaultAzureCredential());

        // add Discount Agent from Microsoft Foundry
        builder.AddAIAgent(DiscountAgentService.AgentName, (sp, key) =>
        {
            return projectClient.GetAIAgent(DiscountAgentService.AgentName);
        });

        // add Stock Agent from Microsoft Foundry
        builder.AddAIAgent(StockAgentService.AgentName, (sp, key) =>
        {
            return projectClient.GetAIAgent(StockAgentService.AgentName);
        });

        // add Triage Agent from Microsoft Foundry
        builder.AddAIAgent(TriageAgentService.AgentName, (sp, key) =>
        {
            return projectClient.GetAIAgent(TriageAgentService.AgentName);
        });

        builder.AddWorkflow(AgentCheckoutOrchestrator.WorkflowNameSequential, (sp, key) =>
        {
            var stockAgent = sp.GetRequiredKeyedService<AIAgent>(StockAgentService.AgentName);
            var discountAgent = sp.GetRequiredKeyedService<AIAgent>(DiscountAgentService.AgentName);

            var workflow = AgentWorkflowBuilder.BuildSequential(
                workflowName: AgentCheckoutOrchestrator.WorkflowNameSequential,
                agents: [stockAgent, discountAgent]);

            return workflow;
        });

        return builder;
    }
}
