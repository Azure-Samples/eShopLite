using System.Net.Http.Json;

namespace Store.Services;

public class ShoppingAgentChatService(HttpClient httpClient, ILogger<ShoppingAgentChatService> logger) : IShoppingAgentChatService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ILogger<ShoppingAgentChatService> _logger = logger;

    public Task<ChatResponse?> SendAsync(string message, CancellationToken cancellationToken = default)
        => SendAsync(message, new List<ChatMessage>(), null, cancellationToken);

    public async Task<ChatResponse?> SendAsync(string message, IList<ChatMessage> history, string? conversationId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Convert ChatMessage records to serializable format
            var historyList = history?.Select(m => new 
            {
                Role = m.Role,
                Content = m.Content,
                Timestamp = m.Timestamp
            }).ToList();

            var request = new
            {
                Message = message,
                ConversationId = conversationId,
                History = historyList
            };

            var response = await _httpClient.PostAsJsonAsync("/api/agent/chat", request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Chat request failed with status {StatusCode}", response.StatusCode);
                return null;
            }
            var chatResponse = await response.Content.ReadFromJsonAsync<ChatResponse>(cancellationToken: cancellationToken);
            return chatResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending chat message");
            return null;
        }
    }
}
