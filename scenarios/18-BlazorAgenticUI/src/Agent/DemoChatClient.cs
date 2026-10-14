using Microsoft.Extensions.AI;
using System.Runtime.CompilerServices;

namespace ShoppingAgent;

// An explicit offline protocol fixture, never a fallback for a failing real model.
public sealed class DemoChatClient : IChatClient
{
    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages,
        ChatOptions? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var history = messages.ToList();
        var last = history.LastOrDefault();
        var id = Guid.NewGuid().ToString("N");
        if (last?.Role == ChatRole.Tool)
        {
            yield return new ChatResponseUpdate(ChatRole.Assistant, "Demo: the operation result is shown above. Prices and products come from the catalog, not from a model.")
            { MessageId = id, FinishReason = ChatFinishReason.Stop };
            yield break;
        }
        var cart = last?.Text.Contains("cart", StringComparison.OrdinalIgnoreCase) == true;
        yield return new ChatResponseUpdate(ChatRole.Assistant, cart
            ? "Demo: requesting your approval to add the Camping Tent (product 9)."
            : "Demo: searching the catalog for camping equipment.")
        { MessageId = id };
        await Task.Delay(50, cancellationToken);
        yield return new ChatResponseUpdate(ChatRole.Assistant, (string?)null)
        {
            MessageId = id,
            FinishReason = ChatFinishReason.ToolCalls,
            Contents = [new FunctionCallContent(Guid.NewGuid().ToString("N"), cart ? "confirm_add_to_cart" : "search_products",
                cart ? new Dictionary<string, object?> { ["productId"] = 9 } :
                    new Dictionary<string, object?> { ["query"] = "camping", ["limit"] = 3, ["semantic"] = false })]
        };
    }

    public async Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null,
        CancellationToken cancellationToken = default) =>
        await GetStreamingResponseAsync(messages, options, cancellationToken).ToChatResponseAsync(cancellationToken);

    public object? GetService(Type serviceType, object? serviceKey = null) => serviceType == typeof(DemoChatClient) ? this : null;
    public void Dispose() { }
}
