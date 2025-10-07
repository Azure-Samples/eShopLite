using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace ShoppingAssistantAgent.Services;

/// <summary>
/// Telemetry and monitoring agent for tracking agent interactions
/// </summary>
public class TelemetryAgent
{
    private static readonly ActivitySource ActivitySource = new("ShoppingAssistant.Telemetry");
    private static readonly Meter Meter = new("ShoppingAssistant.Metrics");
    
    private readonly Counter<long> _messageCounter;
    private readonly Counter<long> _errorCounter;
    private readonly Histogram<double> _responseTimeHistogram;
    private readonly ILogger<TelemetryAgent> _logger;

    // Sanitizes potentially user-provided values for logging
    private static string SanitizeForLog(string input) =>
        (input ?? string.Empty).Replace("\r", "").Replace("\n", "");

    public TelemetryAgent(ILogger<TelemetryAgent> logger)
    {
        _logger = logger;
        
        // Create metrics
        _messageCounter = Meter.CreateCounter<long>(
            "shopping_assistant.messages.total",
            description: "Total number of messages processed");
            
        _errorCounter = Meter.CreateCounter<long>(
            "shopping_assistant.errors.total",
            description: "Total number of errors encountered");
            
        _responseTimeHistogram = Meter.CreateHistogram<double>(
            "shopping_assistant.response_time",
            unit: "ms",
            description: "Response time for agent interactions");
    }

    public Activity? StartActivity(string operationName, string conversationId)
    {
        var activity = ActivitySource.StartActivity(operationName);
        activity?.SetTag("conversation.id", conversationId);
        activity?.SetTag("timestamp", DateTime.UtcNow.ToString("O"));
        
        _logger.LogDebug("Started activity {OperationName} for conversation {ConversationId}",
            operationName, conversationId);
            
        return activity;
    }

    public void RecordMessage(string conversationId, string messageType, int messageLength)
    {
        _messageCounter.Add(1, new KeyValuePair<string, object?>("message_type", messageType));
        
        _logger.LogInformation(
            "Message recorded - ConversationId: {ConversationId}, Type: {MessageType}, Length: {Length}",
            conversationId, messageType, messageLength);
    }

    public void RecordError(string conversationId, string errorType, string errorMessage)
    {
        _errorCounter.Add(1, new KeyValuePair<string, object?>("error_type", errorType));
        
        _logger.LogError(
            "Error recorded - ConversationId: {ConversationId}, Type: {ErrorType}, Message: {ErrorMessage}",
            SanitizeForLog(conversationId), errorType, errorMessage);
    }

    public void RecordResponseTime(string conversationId, double durationMs)
    {
        _responseTimeHistogram.Record(durationMs);
        
        _logger.LogInformation(
            "Response time recorded - ConversationId: {ConversationId}, Duration: {DurationMs}ms",
            conversationId, durationMs);
    }

    public void RecordToolInvocation(string conversationId, string toolName, bool success)
    {
        using var activity = ActivitySource.StartActivity("ToolInvocation");
        activity?.SetTag("conversation.id", conversationId);
        activity?.SetTag("tool.name", toolName);
        activity?.SetTag("tool.success", success);
        
        _logger.LogInformation(
            "Tool invocation - ConversationId: {ConversationId}, Tool: {ToolName}, Success: {Success}",
            conversationId, toolName, success);
    }
}
