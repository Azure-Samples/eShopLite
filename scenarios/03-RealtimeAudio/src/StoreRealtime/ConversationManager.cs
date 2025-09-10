using DataEntities;
using Microsoft.Extensions.AI; // For AIFunction
using OpenAI.Realtime;
using SearchEntities;
using StoreRealtime.ContextManagers; // For ContosoProductContext service access
using StoreRealtime.Support;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Linq; // For LINQ projection when serializing compact product list

namespace StoreRealtime;

/// <summary>
/// Orchestrates a single realtime conversation session: 
///  * Starts the OpenAI realtime session (audio in + audio/text out)
///  * Registers available function (tool) calls backed by <see cref="ContosoProductContext"/>
///  * Streams microphone audio to the model
///  * Accumulates user + assistant partial transcripts producing ONE bubble per message
///  * Executes tool (function) calls, rendering product cards when returned
///  * Streams assistant audio back to the UI speaker component
///  
///  The original implementation concentrated all logic inside one large switch. This refactor
///  decomposes responsibilities into small, intention‑revealing handlers while preserving behavior.
/// </summary>
public partial class ConversationManager : IDisposable
{
    private readonly RealtimeClient _client;
    private readonly ContosoProductContext _contosoProductContext;
    private readonly ILogger _logger;
    private RealtimeSession? _session; // Nullable until started (clarifies lifecycle)
    private bool _disposed;

    public ConversationManager(RealtimeClient client, ContosoProductContext contosoProductContext, ILogger logger)
    {
        _client = client;
        _contosoProductContext = contosoProductContext;
        _logger = logger;
    }

    /// <summary>
    /// Entry point invoked by the UI layer to start streaming a realtime conversation.
    /// </summary>
    /// <param name="audioInput">PCM16 microphone input stream.</param>
    /// <param name="audioOutput">Speaker component receiving streamed assistant audio.</param>
    /// <param name="addMessageAsync">Adds a low‑level diagnostic / status message.</param>
    /// <param name="addChatMessageAsync">Adds a user/assistant chat bubble (bool indicates user=true).</param>
    /// <param name="addChatProductMessageAsync">Adds a product card style chat message.</param>
    /// <param name="cancellationToken">Cancellation for the whole session.</param>
    public async Task RunAsync(Stream audioInput, Components.Speaker audioOutput,
        Func<string, Task> addMessageAsync,
        Func<string, bool, Task> addChatMessageAsync,
        Func<List<Product>, Task> addChatProductMessageAsync,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(audioInput);
        ArgumentNullException.ThrowIfNull(audioOutput);

        // Prepare system instructions similar to original sample.
        var prompt = $"""
            You are a useful assistant.
            Respond as succinctly as possible, in just a few words.
            Your main field of expertise is outdoor products.
            You are able to answer questions about outdoor products, including their features, specifications, and availability.
            Check the product database and external sources for information.
            The current date is {DateTime.Now.ToLongDateString()}
            """;
        await addMessageAsync("Connecting...");
        await addChatMessageAsync("Hello, how can I help?", false);

        try
        {
            // Build tool functions (extracted for readability)
            List<AIFunction> tools = BuildTools();

            _session = await StartSessionAsync(prompt, tools, addMessageAsync);

            // Central streaming loop now delegated to small per-update handlers.
            var state = new ConversationState();
            await ProcessUpdatesAsync(state, audioInput, audioOutput, addMessageAsync, addChatMessageAsync, addChatProductMessageAsync, cancellationToken);
        }
        catch (Exception exc)
        {
            _logger.LogError(exc, "Error occurred while processing conversation updates");
            await addMessageAsync($"Error: {exc.Message}");
        }
    }


    private ConversationSessionOptions CreateConversationSessionOptions(string instructions, List<AIFunction> tools)
    {
        ConversationSessionOptions sessionOptions = new()
        {
            Instructions = instructions,
            Voice = ConversationVoice.Alloy,
            InputAudioFormat = RealtimeAudioFormat.Pcm16,
            OutputAudioFormat = RealtimeAudioFormat.Pcm16,
            // Input transcription options must be provided to enable transcribed feedback for input audio
            InputTranscriptionOptions = new()
            {
                Model = "whisper-1",
            },
        };

        // iterate through tools and add them to the session options tools
        foreach (var tool in tools)
        {
            sessionOptions.Tools.Add(tool.AsOpenAIConversationFunctionTool());
        }
        return sessionOptions;
    }

    // Attempts to extract audio bytes from a SDK update object in a resilient way.
    private static bool TryExtractAudioBytes(object update, out byte[] bytes)
    {
        bytes = Array.Empty<byte>();
        try
        {
            var t = update.GetType();
            // Common property names that might hold base64 data or raw byte[]
            string[] candidateNames = ["AudioChunk", "Audio", "Delta", "Bytes"];
            foreach (var name in candidateNames)
            {
                var prop = t.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (prop == null) continue;
                var val = prop.GetValue(update);
                if (val is byte[] b && b.Length > 0)
                {
                    bytes = b; return true;
                }
                if (val is string s && !string.IsNullOrWhiteSpace(s))
                {
                    // assume base64
                    try
                    {
                        bytes = Convert.FromBase64String(s);
                        return true;
                    }
                    catch { /* not base64 */ }
                }
            }
        }
        catch { }
        return false;
    }

    // Invokes a tool (function call) requested by the model, parsing a single string argument
    // and returning a short textual summary plus any products for UI rendering.
    private async Task<(string toolResultText, List<Product>? products)> InvokeToolAsync(string functionName, string? rawArgs)
    {
        try
        {
            string? extracted = null;
            if (!string.IsNullOrWhiteSpace(rawArgs))
            {
                try
                {
                    using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(rawArgs) ? "{}" : rawArgs);
                    if (doc.RootElement.ValueKind == JsonValueKind.Object)
                    {
                        if (doc.RootElement.TryGetProperty("searchCriteria", out var sc)) extracted = sc.GetString();
                        else if (doc.RootElement.TryGetProperty("name", out var nm)) extracted = nm.GetString();
                        else if (doc.RootElement.TryGetProperty("query", out var q)) extracted = q.GetString();
                        else if (doc.RootElement.EnumerateObject().FirstOrDefault().Value.ValueKind == JsonValueKind.String)
                        {
                            extracted = doc.RootElement.EnumerateObject().First().Value.GetString();
                        }
                    }
                }
                catch { /* ignore parse errors */ }
            }
            extracted ??= rawArgs?.Trim('"');
            if (string.IsNullOrWhiteSpace(extracted)) extracted = "outdoor"; // fallback

            if (functionName.Equals("SemanticSearchOutdoorProducts", StringComparison.OrdinalIgnoreCase))
            {
                var json = await _contosoProductContext.SemanticSearchOutdoorProductsAsync(extracted);
                List<Product>? prods = null;
                try
                {
                    prods = JsonSerializer.Deserialize<SearchResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })?.Products;
                }
                catch { }
                return ($"Semantic search results for '{extracted}'", prods ?? new List<Product>());
            }
            if (functionName.Equals("SearchOutdoorProductsByName", StringComparison.OrdinalIgnoreCase))
            {
                var text = await _contosoProductContext.SearchOutdoorProductsByNameAsync(extracted);
                List<Product>? prods = null;
                try
                {
                    var sr = JsonSerializer.Deserialize<SearchResponse>(text, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    prods = sr?.Products;
                }
                catch { }
                return (text, prods ?? new List<Product>());
            }
            return ($"Tool '{functionName}' not recognized", new List<Product>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tool invocation error for {Function}", functionName);
            return ($"Error invoking {functionName}: {ex.Message}", new List<Product>());
        }
    }


    #region Helper Methods (extracted for clarity)

    // Creates tool list (previously inline in RunAsync) – isolates knowledge of available functions
    private List<AIFunction> BuildTools()
    {
        var semanticSearchTool = AIFunctionFactory.Create(_contosoProductContext.SemanticSearchOutdoorProductsAsync).ToConversationFunctionTool();
        var searchByNameTool = AIFunctionFactory.Create(_contosoProductContext.SearchOutdoorProductsByNameAsync).ToConversationFunctionTool();
        return [semanticSearchTool, searchByNameTool];
    }

    // Starts session + configures options (separates lifecycle setup from streaming logic)
    private async Task<RealtimeSession> StartSessionAsync(string prompt, List<AIFunction> tools, Func<string, Task> addMessageAsync)
    {
        var modelName = Environment.GetEnvironmentVariable("AI_RealtimeDeploymentName")
                        ?? Environment.GetEnvironmentVariable("OPENAI_REALTIME_MODEL")
                        ?? "gpt-realtime"; // sensible default

        _logger.LogInformation("Starting realtime session with model {Model}", modelName);
        var session = await _client.StartConversationSessionAsync(model: modelName);
        await addMessageAsync("Session started...");
        ConversationSessionOptions conversationSessionOptions = CreateConversationSessionOptions(prompt, tools);
        await session.ConfigureConversationSessionAsync(conversationSessionOptions);
        return session;
    }

    // Core processing loop delegating each update to specific handlers for readability.
    private async Task ProcessUpdatesAsync(
        ConversationState state,
        Stream audioInput,
        Components.Speaker audioOutput,
        Func<string, Task> addMessageAsync,
        Func<string, bool, Task> addChatMessageAsync,
        Func<List<Product>, Task> addChatProductMessageAsync,
        CancellationToken cancellationToken)
    {
        if (_session is null) throw new InvalidOperationException("Session not started");

        await foreach (RealtimeUpdate update in _session.ReceiveUpdatesAsync(cancellationToken))
        {
            switch (update)
            {
                case ConversationSessionStartedUpdate:
                    await OnSessionStartedAsync(audioInput, addMessageAsync, cancellationToken);
                    break;
                case InputAudioSpeechStartedUpdate:
                    await OnInputSpeechStartedAsync(state, audioOutput, addMessageAsync);
                    break;
                case InputAudioTranscriptionDeltaUpdate userDelta:
                    OnUserTranscriptionDelta(state, userDelta);
                    break;
                case InputAudioTranscriptionFinishedUpdate userDone:
                    await OnUserTranscriptionFinishedAsync(state, userDone, addMessageAsync, addChatMessageAsync);
                    break;
                case InputAudioSpeechFinishedUpdate:
                    await addMessageAsync("Speech finished");
                    break;
                case OutputStreamingStartedUpdate started:
                    await OnAssistantStreamingStartedAsync(state, started, addMessageAsync);
                    break;
                case OutputDeltaUpdate delta:
                    OnAssistantDelta(state, delta);
                    if(delta.AudioBytes is not null && delta.AudioBytes.Length > 0)
                    {
                        // If we have audio bytes in the delta, play them immediately
                        await PlayAudio(delta.AudioBytes.ToArray(), audioOutput);
                    }
                    break;
                case OutputStreamingFinishedUpdate finished:
                    await OnAssistantStreamingFinishedAsync(
                        state, finished, audioOutput,
                        addMessageAsync, addChatMessageAsync, addChatProductMessageAsync);
                    break;
                default:
                    // Reflection based handling for dynamic audio related updates (keeps SDK future proof)
                    var typeName = update.GetType().Name;
                    if (typeName == "OutputAudioDeltaUpdate")
                    {
                        await OnAssistantAudioDeltaAsync(update, audioOutput);
                    }
                    else if (typeName == "OutputAudioTranscriptionFinishedUpdate")
                    {
                        OnAssistantAudioTranscriptFinished(state, update);
                    }
                    break;
            }
        }
    }

    private async Task OnSessionStartedAsync(Stream audioInput, Func<string, Task> addMessageAsync, CancellationToken ct)
    {
        await addMessageAsync("Conversation started");
        if (_session is null) return;
        _ = Task.Run(async () => await _session.SendInputAudioAsync(audioInput, ct)); // fire & forget
    }

    private async Task OnInputSpeechStartedAsync(ConversationState state, Components.Speaker audioOutput, Func<string, Task> addMessageAsync)
    {
        await addMessageAsync("Speech started");
        await audioOutput.ClearPlaybackAsync();
        state.UserPartial.Clear();
    }

    private static void OnUserTranscriptionDelta(ConversationState state, InputAudioTranscriptionDeltaUpdate delta)
    {
        state.UserPartial.Append(delta.Delta); // accumulate only (UI noise avoided)
    }

    private async Task OnUserTranscriptionFinishedAsync(ConversationState state, InputAudioTranscriptionFinishedUpdate done,
        Func<string, Task> addMessageAsync, Func<string, bool, Task> addChatMessageAsync)
    {
        var finalUser = done.Transcript?.Trim();
        if (!string.IsNullOrEmpty(finalUser))
        {
            await addMessageAsync($"User: {finalUser}");
            await addChatMessageAsync(finalUser, true);
        }
        state.UserPartial.Clear();
    }

    private async Task OnAssistantStreamingStartedAsync(ConversationState state, OutputStreamingStartedUpdate started, Func<string, Task> addMessageAsync)
    {
        state.AssistantPartial.Clear();
        if (!string.IsNullOrWhiteSpace(started.FunctionName))
            await addMessageAsync($"Calling function: {started.FunctionName}({started.FunctionCallArguments})");
        else
            await addMessageAsync("Assistant answering...");
    }

    private static void OnAssistantDelta(ConversationState state, OutputDeltaUpdate delta)
    {
        if (!string.IsNullOrEmpty(delta.AudioTranscript))
            state.AssistantPartial.Append(delta.AudioTranscript);
    }

    private async Task OnAssistantStreamingFinishedAsync(ConversationState state, 
        OutputStreamingFinishedUpdate finished,
        Components.Speaker audioOutput,
        Func<string, Task> addMessageAsync,
        Func<string, bool, Task> addChatMessageAsync,
        Func<List<Product>, Task> addChatProductMessageAsync)
    {
        if (_session is null) return;

        if (finished.FunctionCallId is not null)
        {
            // Tool invocation path
            var (toolText, products) = await InvokeToolAsync(finished.FunctionName ?? string.Empty, finished.FunctionCallArguments);
            var toolOutput = toolText;
            if (products?.Count > 0)
            {
                var compact = JsonSerializer.Serialize(products.Select(p => new { p.Name, p.Description, p.Price }));
                toolOutput = compact; // concise JSON for model
            }
            RealtimeItem functionOutputItem = RealtimeItem.CreateFunctionCallOutput(
                callId: finished.FunctionCallId,
                output: toolOutput);
            await _session.AddItemAsync(functionOutputItem);

            await addMessageAsync($"Tool executed: {finished.FunctionName}");
            if (products?.Count > 0)
            {
                await addMessageAsync($"Products returned: {products.Count}");
                await addChatProductMessageAsync(products);
            }
            else
            {
                await addChatMessageAsync(toolText, false);
            }
        }
        else
        {
            string assistantText;
            if (finished.MessageContentParts?.Count > 0)
            {
                var assembled = new StringBuilder();
                foreach (ConversationContentPart part in finished.MessageContentParts)
                {
                    if (!string.IsNullOrEmpty(part.AudioTranscript)) assembled.Append(part.AudioTranscript);
                    else if (!string.IsNullOrEmpty(part.Text)) assembled.Append(part.Text);
                }
                assistantText = assembled.Length > 0 ? assembled.ToString() : state.AssistantPartial.ToString();
            }
            else
            {
                assistantText = state.AssistantPartial.ToString();
            }
            assistantText = assistantText.Trim();
            if (!string.IsNullOrEmpty(assistantText))
            {
                await addMessageAsync($"Assistant: {assistantText}");
                await addChatMessageAsync(assistantText, false);
            }
        }
        state.AssistantPartial.Clear();
    }

    private async Task PlayAudio(byte[] audioBytes, Components.Speaker audioOutput)
    {
            try { await audioOutput.EnqueueAsync(audioBytes); } catch { /* swallow playback errors */ }
    }


    private async Task OnAssistantAudioDeltaAsync(object update, Components.Speaker audioOutput)
    {
        if (TryExtractAudioBytes(update, out var audioBytes))
        {
            try { await audioOutput.EnqueueAsync(audioBytes); } catch { /* swallow playback errors */ }
        }
    }

    private static void OnAssistantAudioTranscriptFinished(ConversationState state, object update)
    {
        var tProp = update.GetType().GetProperty("Transcript")
                   ?? update.GetType().GetProperty("AudioTranscript");
        var text = tProp?.GetValue(update) as string;
        if (!string.IsNullOrWhiteSpace(text) && state.AssistantPartial.Length == 0)
        {
            state.AssistantPartial.Append(text);
        }
    }

    #endregion

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        try
        {
            if (_session is IDisposable d) d.Dispose();
            else if (_session is IAsyncDisposable ad) ad.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }
        catch { /* ignore */ }
    }
}
