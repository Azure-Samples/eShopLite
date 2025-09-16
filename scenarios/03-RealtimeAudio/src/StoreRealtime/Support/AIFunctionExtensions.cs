using Microsoft.Extensions.AI;

namespace StoreRealtime.Support;

// Simplified placeholder extensions after migration away from RealtimeConversation* API.
// The previous implementation depended on OpenAI.RealtimeConversation.* types that no longer
// exist in the current SDK version being used with RealtimeClient. These are replaced with
// no-op helpers so the rest of the code can compile while a new tool invocation pattern
// (likely via model function calling) is designed.
public static class AIFunctionExtensions
{
    // Returns the original function; real conversion to a Realtime tool will be implemented later.
    public static AIFunction ToConversationFunctionTool(this AIFunction aiFunction) => aiFunction;
}
