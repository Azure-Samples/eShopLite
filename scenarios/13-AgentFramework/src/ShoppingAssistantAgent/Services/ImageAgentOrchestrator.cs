using Microsoft.Extensions.AI;
using ShoppingAssistantAgent.Models;
using System.Diagnostics;
using AIMessage = Microsoft.Extensions.AI.ChatMessage;

namespace ShoppingAssistantAgent.Services;

/// <summary>
/// Image processing agent that can analyze images and respond to queries about them
/// </summary>
public class ImageAgentOrchestrator : IImageAgentOrchestrator
{
    private readonly IChatClient _chatClient;
    private readonly ILogger<ImageAgentOrchestrator> _logger;
    private static readonly ActivitySource ActivitySource = new("ShoppingAssistant.ImageAgent");

    public ImageAgentOrchestrator(
        IChatClient chatClient,
        ILogger<ImageAgentOrchestrator> logger)
    {
        _chatClient = chatClient;
        _logger = logger;
    }

    public async Task<Models.ChatResponse> ProcessMessageWithImageAsync(
        string message,
        byte[] imageData,
        string contentType,
        string conversationId,
        List<Models.ChatMessage> history)
    {
        using var activity = ActivitySource.StartActivity("ProcessMessageWithImage");
        activity?.SetTag("conversation.id", conversationId);
        activity?.SetTag("message.length", message.Length);
        activity?.SetTag("image.size", imageData.Length);
        activity?.SetTag("image.contentType", contentType);

        try
        {
            _logger.LogInformation(
                "ImageAgent processing message with image for conversation {ConversationId}, Image size: {ImageSize}",
                conversationId,
                imageData.Length);

            // Build conversation messages
            var messages = new List<AIMessage>
            {
                new(ChatRole.System, @"You are an AI assistant that can analyze images of outdoor and camping products. 
You can help identify products, assess their condition, suggest similar items, and answer questions about what you see in images.
Be descriptive and helpful in your analysis.")
            };

            // Add recent conversation history
            if (history?.Any() == true)
            {
                foreach (var msg in history.TakeLast(5))
                {
                    var role = msg.Role.ToLower() == "user" ? ChatRole.User : ChatRole.Assistant;
                    messages.Add(new AIMessage(role, msg.Content));
                }
            }

            // Create message with image content
            var imageContent = new AIContent[]
            {
                new TextContent(message),
                new DataContent(imageData, contentType)
            };

            messages.Add(new AIMessage(ChatRole.User, imageContent));

            // Configure chat options for vision
            var chatOptions = new ChatOptions
            {
                Temperature = 0.7f,
                MaxOutputTokens = 1000
            };

            _logger.LogInformation(
                "Calling ImageAgent with {MessageCount} messages including image",
                messages.Count);

            // Call AI with image understanding
            var chatCompletion = await _chatClient.GetResponseAsync(messages, chatOptions);

            var assistantMessage = chatCompletion?.Text
                ?? "I'm sorry, I couldn't analyze that image.";

            activity?.SetTag("response.length", assistantMessage.Length);
            activity?.SetTag("response.success", true);

            _logger.LogInformation(
                "ImageAgent response generated: {ResponsePreview}",
                assistantMessage.Substring(0, Math.Min(100, assistantMessage.Length)));

            return new Models.ChatResponse
            {
                Message = assistantMessage,
                ConversationId = conversationId
            };
        }
        catch (Exception ex)
        {
            activity?.SetTag("response.success", false);
            activity?.SetTag("error.type", ex.GetType().Name);

            _logger.LogError(ex,
                "Error in ImageAgent processing message with image for conversation {ConversationId}",
                conversationId);

            throw;
        }
    }
}
