using Microsoft.Extensions.AI;
using ShoppingAssistantAgent.Models;
using ShoppingAssistantAgent.Services;
using System.Collections.Concurrent;

namespace ShoppingAssistantAgent.Endpoints;

public static class AgentEndpoints
{
    // In-memory conversation storage (for demo purposes)
    private static readonly ConcurrentDictionary<string, List<Models.ChatMessage>> ConversationHistory = new();

    /// <summary>
    /// Configures the agent-related endpoints for the application.
    /// </summary>
    public static void MapAgentEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/agent");

        group.MapPost("/chat", ChatWithAgent)
            .WithName("AgentChat")
            .WithOpenApi()
            .Produces<Models.ChatResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/chat-with-image", ChatWithAgentAndImage)
            .WithName("AgentChatWithImage")
            .WithOpenApi()
            .Produces<Models.ChatResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .DisableAntiforgery(); // Allow file uploads

        group.MapDelete("/conversation/{conversationId}", DeleteConversation)
            .WithName("DeleteConversation")
            .WithOpenApi()
            .Produces(StatusCodes.Status204NoContent);

        group.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }))
            .WithName("AgentHealthCheck")
            .WithOpenApi();
    }

    private static async Task<IResult> ChatWithAgent(
        ChatRequest request,
        IAgentOrchestrator agentOrchestrator,
        ILogger<Program> logger)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return Results.BadRequest("Message cannot be empty");
            }

            logger.LogInformation("Received chat message: {Message}", request.Message);

            var conversationId = request.ConversationId ?? Guid.NewGuid().ToString();

            // Get or create conversation history
            var history = ConversationHistory.GetOrAdd(conversationId, _ => new List<Models.ChatMessage>());

            // Add user message to history
            var userMessage = new Models.ChatMessage
            {
                Role = "user",
                Content = request.Message,
                Timestamp = DateTime.UtcNow
            };
            history.Add(userMessage);

            // Process with agent
            var response = await agentOrchestrator.ProcessMessageAsync(
                request.Message,
                conversationId,
                history);

            // Add assistant message to history
            var assistantMessage = new Models.ChatMessage
            {
                Role = "assistant",
                Content = response.Message,
                Timestamp = DateTime.UtcNow
            };
            history.Add(assistantMessage);

            // Keep only last 20 messages
            if (history.Count > 20)
            {
                history.RemoveRange(0, history.Count - 20);
            }

            response.ConversationId = conversationId;
            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing chat request");
            return Results.Problem("An error occurred while processing your request.");
        }
    }

    private static async Task<IResult> ChatWithAgentAndImage(
        HttpRequest httpRequest,
        IImageAgentOrchestrator imageAgentOrchestrator,
        ILogger<Program> logger)
    {
        try
        {
            var form = await httpRequest.ReadFormAsync();
            var message = form["message"].ToString();
            var conversationId = form["conversationId"].ToString();
            var imageFile = form.Files.GetFile("image");

            if (string.IsNullOrWhiteSpace(message))
            {
                return Results.BadRequest("Message cannot be empty");
            }

            if (imageFile == null || imageFile.Length == 0)
            {
                return Results.BadRequest("Image is required");
            }

            logger.LogInformation("Received chat message with image: {Message}, ImageSize: {Size}", 
                message, imageFile.Length);

            conversationId = string.IsNullOrWhiteSpace(conversationId) 
                ? Guid.NewGuid().ToString() 
                : conversationId;

            // Get or create conversation history
            var history = ConversationHistory.GetOrAdd(conversationId, _ => new List<Models.ChatMessage>());

            // Read image data
            byte[] imageData;
            using (var memoryStream = new MemoryStream())
            {
                await imageFile.CopyToAsync(memoryStream);
                imageData = memoryStream.ToArray();
            }

            // Add user message to history
            var userMessage = new Models.ChatMessage
            {
                Role = "user",
                Content = $"{message} [Image uploaded: {imageFile.FileName}]",
                Timestamp = DateTime.UtcNow
            };
            history.Add(userMessage);

            // Process with image agent
            var response = await imageAgentOrchestrator.ProcessMessageWithImageAsync(
                message,
                imageData,
                imageFile.ContentType,
                conversationId,
                history);

            // Add assistant message to history
            var assistantMessage = new Models.ChatMessage
            {
                Role = "assistant",
                Content = response.Message,
                Timestamp = DateTime.UtcNow
            };
            history.Add(assistantMessage);

            // Keep only last 20 messages
            if (history.Count > 20)
            {
                history.RemoveRange(0, history.Count - 20);
            }

            response.ConversationId = conversationId;
            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing chat request with image");
            return Results.Problem("An error occurred while processing your request with image.");
        }
    }

    private static IResult DeleteConversation(string conversationId, ILogger<Program> logger)
    {
        try
        {
            ConversationHistory.TryRemove(conversationId, out _);
            logger.LogInformation("Deleted conversation: {ConversationId}", conversationId);
            return Results.NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting conversation");
            return Results.Problem("An error occurred while deleting the conversation.");
        }
    }
}
