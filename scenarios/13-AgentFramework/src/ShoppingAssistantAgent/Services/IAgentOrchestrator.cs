using ShoppingAssistantAgent.Models;

namespace ShoppingAssistantAgent.Services;

/// <summary>
/// Interface for orchestrating agent interactions
/// </summary>
public interface IAgentOrchestrator
{
    Task<ChatResponse> ProcessMessageAsync(
        string message,
        string conversationId,
        List<ChatMessage> history);
}

/// <summary>
/// Interface for orchestrating image-based agent interactions
/// </summary>
public interface IImageAgentOrchestrator
{
    Task<ChatResponse> ProcessMessageWithImageAsync(
        string message,
        byte[] imageData,
        string contentType,
        string conversationId,
        List<ChatMessage> history);
}
