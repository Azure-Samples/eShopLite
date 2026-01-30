namespace ShoppingAssistantAgent.Models;

/// <summary>
/// Request model for agent chat interactions
/// </summary>
public class ChatRequest
{
    public string Message { get; set; } = string.Empty;
    public string? ConversationId { get; set; }
    public List<ChatMessage>? History { get; set; }
}

/// <summary>
/// Response model for agent chat interactions
/// </summary>
public class ChatResponse
{
    public string Message { get; set; } = string.Empty;
    public string ConversationId { get; set; } = string.Empty;
    public List<ProductCard>? Products { get; set; }
    public TelemetryData? Telemetry { get; set; }
}

/// <summary>
/// Chat message in conversation history
/// </summary>
public class ChatMessage
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Product card for displaying product information in chat
/// </summary>
public class ProductCard
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// Telemetry data for monitoring agent performance
/// </summary>
public class TelemetryData
{
    public int? InputTokens { get; set; }
    public int? OutputTokens { get; set; }
    public int? TotalTokens { get; set; }
    public long ResponseTimeMs { get; set; }
    public List<ToolInvocation>? ToolsUsed { get; set; }
    public string? Model { get; set; }
}

/// <summary>
/// Information about tool invocations during agent execution
/// </summary>
public class ToolInvocation
{
    public string ToolName { get; set; } = string.Empty;
    public string? Arguments { get; set; }
    public string? Result { get; set; }
    public long DurationMs { get; set; }
}
