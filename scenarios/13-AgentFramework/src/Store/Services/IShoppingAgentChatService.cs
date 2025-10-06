namespace Store.Services;

public record ChatMessage(string Role, string Content, DateTime Timestamp);
public class ChatRequest
{
    public string Message { get; set; } = string.Empty;
    public string? ConversationId { get; set; }
    public List<ChatMessage>? History { get; set; }
}
public class ProductCard
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public string? Description { get; set; }
}
public class ChatResponse
{
    public string Message { get; set; } = string.Empty;
    public string ConversationId { get; set; } = string.Empty;
    public List<ProductCard>? Products { get; set; }
}

public interface IShoppingAgentChatService
{
    Task<ChatResponse?> SendAsync(string message, CancellationToken cancellationToken = default);
    Task<ChatResponse?> SendAsync(string message, IList<ChatMessage> history, string? conversationId, CancellationToken cancellationToken = default);
}
