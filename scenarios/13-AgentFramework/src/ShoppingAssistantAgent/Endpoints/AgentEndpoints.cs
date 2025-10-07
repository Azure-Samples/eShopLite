using Microsoft.Extensions.AI;
using Microsoft.Agents.AI;
using ShoppingAssistantAgent.Models;
using ShoppingAssistantAgent.Services;
using System.Collections.Concurrent;
using System.Text.Json;
using AIMessage = Microsoft.Extensions.AI.ChatMessage;

namespace ShoppingAssistantAgent.Endpoints;

public static class AgentEndpoints
{
    // In-memory conversation storage using List<ChatMessage> (persisted conversation pattern)
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

            // Get or create conversation history using List<ChatMessage> (persisted conversation pattern)
            var chatHistory = ConversationHistory.GetOrAdd(conversationId, _ => new List<AIMessage>());

            // Add user message to history
            chatHistory.Add(new AIMessage(ChatRole.User, request.Message));

            // Track telemetry
            telemetryAgent.RecordMessage(conversationId, "user", request.Message.Length);

            logger.LogInformation("Calling ZavaAssistant with {MessageCount} messages", chatHistory.Count);

            // Run the agent using Microsoft Agent Framework pattern with conversation history
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // Run agent with the latest user message - the agent maintains conversation context internally
            AgentRunResponse response = await zavaAgent.RunAsync(request.Message);
            stopwatch.Stop();

            telemetryAgent.RecordResponseTime(conversationId, stopwatch.ElapsedMilliseconds);

            var assistantText = response.Text ?? "I'm sorry, I couldn't process that request.";

            // Add assistant message to history
            chatHistory.Add(new AIMessage(ChatRole.Assistant, assistantText));

            // Keep only last 20 messages to manage memory
            while (chatHistory.Count > 20)
            {
                chatHistory.RemoveAt(0);
            }

            telemetryAgent.RecordMessage(conversationId, "assistant", assistantText.Length);

            logger.LogInformation("ZavaAssistant response: {ResponsePreview}",
                assistantText.Substring(0, Math.Min(100, assistantText.Length)));

            // Extract telemetry data from response
            var telemetryData = new TelemetryData
            {
                ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                Model = "gpt-4o-mini",
                ToolsUsed = ExtractToolInvocations(response),
                InputTokens = ExtractTokenCount(response, "input"),
                OutputTokens = ExtractTokenCount(response, "output"),
                TotalTokens = ExtractTokenCount(response, "total")
            };

            return Results.Ok(new Models.ChatResponse
            {
                Message = assistantText,
                ConversationId = conversationId,
                Telemetry = telemetryData
            });
        }
        catch (Exception ex)
        {
            telemetryAgent.RecordError(conversationId, "ChatError", ex.Message);
            logger.LogError(ex, "Error processing chat request");
            return Results.Problem("An error occurred while processing your request.");
        }
    }

    private static List<ToolInvocation>? ExtractToolInvocations(AgentRunResponse response)
    {
        // Extract tool invocations from the response if available
        // This will capture which tools were called during agent execution
        var toolInvocations = new List<ToolInvocation>();
        
        try
        {
            // Check if response contains tool call information in AdditionalProperties
            if (response.AdditionalProperties?.TryGetValue("tool_calls", out var toolCallsObj) == true)
            {
                if (toolCallsObj is List<object> toolCallsList)
                {
                    foreach (var toolCall in toolCallsList)
                    {
                        if (toolCall is Dictionary<string, object> toolDict)
                        {
                            var toolName = toolDict.TryGetValue("name", out var name) ? name.ToString() : "Unknown";
                            var arguments = toolDict.TryGetValue("arguments", out var args) ? args.ToString() : null;
                            
                            toolInvocations.Add(new ToolInvocation
                            {
                                ToolName = toolName ?? "Unknown",
                                Arguments = arguments,
                                Result = "Executed",
                                DurationMs = 0
                            });
                        }
                    }
                }
            }
        }
        catch
        {
            // If tool extraction fails, return empty list
        }

        return toolInvocations.Count > 0 ? toolInvocations : null;
    }

    private static int? ExtractTokenCount(AgentRunResponse response, string type)
    {
        try
        {
            // Try to extract token usage from response AdditionalProperties
            if (response.AdditionalProperties?.TryGetValue("usage", out var usage) == true)
            {
                if (usage is Dictionary<string, object> usageDict)
                {
                    if (type == "input" && usageDict.TryGetValue("prompt_tokens", out var inputTokens))
                        return Convert.ToInt32(inputTokens);
                    if (type == "output" && usageDict.TryGetValue("completion_tokens", out var outputTokens))
                        return Convert.ToInt32(outputTokens);
                    if (type == "total" && usageDict.TryGetValue("total_tokens", out var totalTokens))
                        return Convert.ToInt32(totalTokens);
                }
            }
        }
        catch
        {
            // If extraction fails, return null
        }
        return null;
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

            // Get or create conversation history using List<ChatMessage>
            var chatHistory = ConversationHistory.GetOrAdd(conversationId, _ => new List<AIMessage>());

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

            chatHistory.Add(new AIMessage(ChatRole.User, contentParts));

            telemetryAgent.RecordMessage(conversationId, "user", message.Length + imageData.Length);

            logger.LogInformation("Calling ImageAgent with image and {MessageCount} messages", chatHistory.Count);

            // Run the image agent with conversation history
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // Run agent with the latest user message
            AgentRunResponse response = await imageAgent.RunAsync(message);
            stopwatch.Stop();

            telemetryAgent.RecordResponseTime(conversationId, stopwatch.ElapsedMilliseconds);

            var assistantText = response.Text ?? "I couldn't analyze the image.";

            // Add assistant message to history
            chatHistory.Add(new AIMessage(ChatRole.Assistant, assistantText));

            // Keep only last 20 messages
            while (chatHistory.Count > 20)
            {
                chatHistory.RemoveAt(0);
            }

            telemetryAgent.RecordMessage(conversationId, "assistant", assistantText.Length);

            logger.LogInformation("ImageAgent response: {ResponsePreview}",
                assistantText.Substring(0, Math.Min(100, assistantText.Length)));

            // Extract telemetry data
            var telemetryData = new TelemetryData
            {
                ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                Model = "gpt-4o-mini",
                ToolsUsed = ExtractToolInvocations(response),
                InputTokens = ExtractTokenCount(response, "input"),
                OutputTokens = ExtractTokenCount(response, "output"),
                TotalTokens = ExtractTokenCount(response, "total")
            };

            return Results.Ok(new Models.ChatResponse
            {
                Message = assistantText,
                ConversationId = conversationId,
                Telemetry = telemetryData
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
