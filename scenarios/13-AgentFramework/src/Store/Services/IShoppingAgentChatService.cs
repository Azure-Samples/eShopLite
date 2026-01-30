namespace Store.Services;

public record ChatMessage(string Role, string Content, DateTime Timestamp, TelemetryData? Telemetry = null);

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
    public TelemetryData? Telemetry { get; set; }
}

public class TelemetryData
{
    public int? InputTokens { get; set; }
    public int? OutputTokens { get; set; }
    public int? TotalTokens { get; set; }
    public long ResponseTimeMs { get; set; }
    public List<ToolInvocation>? ToolsUsed { get; set; }
    public string? Model { get; set; }
}

public class ToolInvocation
{
    public string ToolName { get; set; } = string.Empty;
    public string? Arguments { get; set; }
    public string? Result { get; set; }
    public long DurationMs { get; set; }
}

public interface IShoppingAgentChatService
{
    Task<ChatResponse?> SendAsync(string message, CancellationToken cancellationToken = default);
    Task<ChatResponse?> SendAsync(string message, IList<ChatMessage> history, string? conversationId, CancellationToken cancellationToken = default);
}
