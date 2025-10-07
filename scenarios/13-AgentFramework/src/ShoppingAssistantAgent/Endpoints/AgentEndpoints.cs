using Microsoft.Extensions.AI;
using Microsoft.Agents.AI;
using ShoppingAssistantAgent.Models;
using ShoppingAssistantAgent.Services;
using System.Collections.Concurrent;
using AIMessage = Microsoft.Extensions.AI.ChatMessage;

namespace ShoppingAssistantAgent.Endpoints;

public static class AgentEndpoints
{
    // In-memory conversation storage (for demo purposes)
    private static readonly ConcurrentDictionary<string, List<AIMessage>> ConversationHistory = new();

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
        [FromKeyedServices("ZavaAssistant")] AIAgent zavaAgent,
        TelemetryAgent telemetryAgent,
        ILogger<Program> logger)
    {
        var conversationId = request.ConversationId ?? Guid.NewGuid().ToString();
        
        try
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return Results.BadRequest("Message cannot be empty");
            }

            logger.LogInformation("Received chat message: {Message}", request.Message);

            // Get or create conversation history
            var history = ConversationHistory.GetOrAdd(conversationId, _ => new List<AIMessage>());

            // Add user message to history
            var userMessage = new AIMessage(ChatRole.User, request.Message);
            history.Add(userMessage);

            // Track telemetry
            telemetryAgent.RecordMessage(conversationId, "user", request.Message.Length);

            logger.LogInformation("Calling ZavaAssistant with {MessageCount} messages", history.Count);

            // Run the agent using Microsoft Agent Framework pattern
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            AgentRunResponse response = await zavaAgent.RunAsync(request.Message);
            stopwatch.Stop();

            telemetryAgent.RecordResponseTime(conversationId, stopwatch.ElapsedMilliseconds);

            var assistantText = response.Text ?? "I'm sorry, I couldn't process that request.";

            // Add assistant message to history
            var assistantMessage = new AIMessage(ChatRole.Assistant, assistantText);
            history.Add(assistantMessage);

            // Keep only last 20 messages
            if (history.Count > 20)
            {
                history.RemoveRange(0, history.Count - 20);
            }

            telemetryAgent.RecordMessage(conversationId, "assistant", assistantText.Length);

            logger.LogInformation("ZavaAssistant response: {ResponsePreview}",
                assistantText.Substring(0, Math.Min(100, assistantText.Length)));

            return Results.Ok(new Models.ChatResponse
            {
                Message = assistantText,
                ConversationId = conversationId
            });
        }
        catch (Exception ex)
        {
            telemetryAgent.RecordError(conversationId, "ChatError", ex.Message);
            logger.LogError(ex, "Error processing chat request");
            return Results.Problem("An error occurred while processing your request.");
        }
    }

    private static async Task<IResult> ChatWithAgentAndImage(
        HttpRequest httpRequest,
        [FromKeyedServices("ImageAgent")] AIAgent imageAgent,
        TelemetryAgent telemetryAgent,
        ILogger<Program> logger)
    {
        string conversationId = "unknown";
        
        try
        {
            var form = await httpRequest.ReadFormAsync();
            var message = form["message"].ToString();
            conversationId = form["conversationId"].ToString();
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
            var history = ConversationHistory.GetOrAdd(conversationId, _ => new List<AIMessage>());

            // Read image data
            byte[] imageData;
            using (var memoryStream = new MemoryStream())
            {
                await imageFile.CopyToAsync(memoryStream);
                imageData = memoryStream.ToArray();
            }

            // Convert image to base64 for AI processing
            var base64Image = Convert.ToBase64String(imageData);
            var imageContentType = imageFile.ContentType ?? "image/jpeg";

            // Create multimodal message with image using DataContent
            var imageDataUri = $"data:{imageContentType};base64,{base64Image}";
            var contentParts = new List<AIContent>
            {
                new TextContent(message),
                new DataContent(imageDataUri, imageContentType)
            };

            var userMessage = new AIMessage(ChatRole.User, contentParts);
            history.Add(userMessage);

            telemetryAgent.RecordMessage(conversationId, "user", message.Length + imageData.Length);

            logger.LogInformation("Calling ImageAgent with image and {MessageCount} messages", history.Count);

            // Run the image agent
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            AgentRunResponse response = await imageAgent.RunAsync(message);
            stopwatch.Stop();

            telemetryAgent.RecordResponseTime(conversationId, stopwatch.ElapsedMilliseconds);

            var assistantText = response.Text ?? "I couldn't analyze the image.";

            // Add assistant message to history
            var assistantMessage = new AIMessage(ChatRole.Assistant, assistantText);
            history.Add(assistantMessage);

            // Keep only last 20 messages
            if (history.Count > 20)
            {
                history.RemoveRange(0, history.Count - 20);
            }

            telemetryAgent.RecordMessage(conversationId, "assistant", assistantText.Length);

            logger.LogInformation("ImageAgent response: {ResponsePreview}",
                assistantText.Substring(0, Math.Min(100, assistantText.Length)));

            return Results.Ok(new Models.ChatResponse
            {
                Message = assistantText,
                ConversationId = conversationId
            });
        }
        catch (Exception ex)
        {
            telemetryAgent.RecordError(conversationId, "ImageChatError", ex.Message);
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
