#pragma warning disable OPENAI001

using Azure;
using Azure.AI.Agents.Persistent;
using Azure.AI.Projects;
using Azure.Core;
using Azure.Identity;
using McpToolsEntities;
using OnlineResearcher.Controllers;
using OpenAI.Assistants;

namespace OnlineResearcher.EndPoints;

public static class OnlineResearchEndPoints
{
    public static void MapOnlineResearchEndpoints(
        this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api");

        routes.MapGet("/", () => $"Online Research API - {DateTime.Now}").ExcludeFromDescription();

        routes.MapGet("/searchonline/{query}",
            async (string query,
            ILogger<Program> logger,
            IConfiguration config) =>
            {
                return await SearchOnlineAsync(query, logger, config);
            })
            .WithName("SearchOnline")
            .Produces<OnlineSearchToolResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    internal static async Task<OnlineSearchToolResponse> SearchOnlineAsync(string query, ILogger<Program> logger, IConfiguration config)
    {
        logger.LogInformation("==========================");
        var sanitizedQuery = query.Replace(Environment.NewLine, "").Replace("\n", "").Replace("\r", "");
        logger.LogInformation($"Search online for the query: {sanitizedQuery}");

        // read settings from user secrets
        var tenantid = config["aifoundryproject_tenantid"];
        var searchagentid = config["aifoundryproject_searchagentid"];
        var bingsearchconnectionName = config["aifoundryproject_groundingcnnname"];
        var aifoundryproject_endpoint = config["aifoundryproject_endpoint"];
        

        // show in the log the values from config
        logger.LogInformation($"Configuration values:");
        logger.LogInformation($"AI Foundry Project - tenantid: {tenantid}");
        logger.LogInformation($"AI Foundry Project - searchagentid: {searchagentid}");
        var sanitizedBingSearchConnectionName = bingsearchconnectionName?.Replace(Environment.NewLine, "").Replace("\n", "").Replace("\r", "");
        logger.LogInformation($"AI Foundry Project - bingsearchconnectionName: {sanitizedBingSearchConnectionName}");
        var sanitizedEndpoint = aifoundryproject_endpoint?.Replace(Environment.NewLine, "").Replace("\n", "").Replace("\r", "");
        logger.LogInformation($"AI Foundry Project - endpoint: {sanitizedEndpoint}");

        // create credential
        var options = new DefaultAzureCredentialOptions();
        if (!string.IsNullOrEmpty(tenantid))
            options.TenantId = tenantid;
        PersistentAgentsClient persistentClient = new(aifoundryproject_endpoint, new DefaultAzureCredential(options));

        PersistentAgent searchOnlineAgent = null;
        if (string.IsNullOrEmpty(searchagentid))
        {
            BingGroundingToolDefinition bingGroundingTool = new(
                new BingGroundingSearchToolParameters(
                    [new BingGroundingSearchConfiguration(bingsearchconnectionName)]));

            searchOnlineAgent = await persistentClient.Administration.CreateAgentAsync(
               model: "gpt-4.1",
               name: "my-agent",
               instructions: "You are a helpful agent.",
               tools: [bingGroundingTool]);
        }
        else
        {
            searchOnlineAgent = persistentClient.Administration.GetAgent(searchagentid).Value;
        }

        // Create thread for communication
        PersistentAgentThread thread = await persistentClient.Threads.CreateThreadAsync();

        // Create message to thread
        PersistentThreadMessage message = await persistentClient.Messages.CreateMessageAsync(
            thread.Id,
            Azure.AI.Agents.Persistent.MessageRole.User,
            $"{query}");

        // Run the agent
        var runResponse = await persistentClient.Runs.CreateRunAsync(thread, searchOnlineAgent);
        do
        {
            await Task.Delay(TimeSpan.FromMilliseconds(500));
            runResponse = await persistentClient.Runs.GetRunAsync(thread.Id, runResponse.Value.Id);
        }
        while (runResponse.Value.Status == Azure.AI.Agents.Persistent.RunStatus.Queued
            || runResponse.Value.Status == Azure.AI.Agents.Persistent.RunStatus.InProgress);

        string searchResult = "";
        logger.LogInformation("==========================");        
        logger.LogInformation($"Search for '{sanitizedQuery}'");

        AsyncPageable<PersistentThreadMessage> messages = persistentClient.Messages.GetMessagesAsync(
threadId: thread.Id, order: ListSortOrder.Ascending);

        await foreach (PersistentThreadMessage threadMessage in messages)
        {
            Console.Write($"{threadMessage.CreatedAt:yyyy-MM-dd HH:mm:ss} - {threadMessage.Role,10}: ");
            foreach (Azure.AI.Agents.Persistent.MessageContent contentItem in threadMessage.ContentItems)
            {
                if (contentItem is MessageTextContent textItem)
                {
                    Console.Write(textItem.Text);
                    searchResult += textItem.Text;
                    searchResult += "\n";

                }
                else if (contentItem is MessageImageFileContent imageFileItem)
                {
                    Console.Write($"<image from ID: {imageFileItem.FileId}");
                }
                Console.WriteLine();
            }
        }


        logger.LogInformation($"Search result:");
        logger.LogInformation(searchResult);
        logger.LogInformation("==========================");

        return new OnlineSearchToolResponse()
        {
            SearchTerm = query,
            SearchResults = searchResult
        };
    }
}
