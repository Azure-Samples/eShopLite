using System.Text;

namespace StoreRealtime;

public partial class ConversationManager
{
    // Internal state object keeps streaming buffers together (improves readability vs scattered fields)
    private sealed class ConversationState
    {
        public StringBuilder UserPartial { get; } = new();
        public StringBuilder AssistantPartial { get; } = new();
    }
}
