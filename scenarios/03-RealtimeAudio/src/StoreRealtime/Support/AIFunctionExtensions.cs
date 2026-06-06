using System.Text.Json;
using Microsoft.Extensions.AI;
using OpenAI.RealtimeConversation;

namespace StoreRealtime.Support;

public static class AIFunctionExtensions
{
    /// <summary>
    /// Converts a <see cref="AIFunction"/> into a <see cref="ConversationFunctionTool"/> so that
    /// it can be used with <see cref="RealtimeConversationClient"/>.
    /// </summary>
    public static ConversationFunctionTool ToConversationFunctionTool(this AIFunction aiFunction)
    {
        return new ConversationFunctionTool()
        {
            Name = aiFunction.Name,
            Description = aiFunction.Description,
            Parameters = BinaryData.FromString(aiFunction.JsonSchema.GetRawText())
        };
    }

    public static async Task<ConversationItem?> GetFunctionCallOutputAsync(this ConversationItemStreamingFinishedUpdate update, IReadOnlyList<AIFunction> tools)
    {
        if (!string.IsNullOrEmpty(update.FunctionName) && tools.FirstOrDefault(t => t.Name == update.FunctionName) is AIFunction aiFunction)
        {
            Dictionary<string, object?>? jsonArgs = null;
            try
            {
                jsonArgs = JsonSerializer.Deserialize<Dictionary<string, object?>>(update.FunctionCallArguments)!;
                var aiArgs = new AIFunctionArguments(jsonArgs!);
                var output = await aiFunction.InvokeAsync(aiArgs);
                var ci = ConversationItem.CreateFunctionCallOutput(update.FunctionCallId, output?.ToString() ?? "");
                return ci;
            }
            catch (JsonException)
            {
                return ConversationItem.CreateFunctionCallOutput(update.FunctionCallId, "Invalid JSON");
            }
            catch
            {
                return ConversationItem.CreateFunctionCallOutput(update.FunctionCallId, "Error calling tool");
            }
        }

        return null;
    }
}
