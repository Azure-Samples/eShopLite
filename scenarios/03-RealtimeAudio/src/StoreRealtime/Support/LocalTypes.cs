using System;
using System.Threading.Tasks;

namespace StoreRealtime.Stubs
{
    // These types are intentionally named with a `Stub` suffix and placed in a
    // separate namespace to avoid colliding with the application's real models
    // and components. They exist only to provide a minimal shape for compilation
    // if other files in the workspace reference them by fully-qualified name.

    public class SpeakerStub
    {
        public Task ClearPlaybackAsync() => Task.CompletedTask;
        public Task EnqueueAsync(byte[]? data) => Task.CompletedTask;
    }

    public record LogMessageStub(string Message)
    {
        public DateTime Timestamp { get; init; } = DateTime.Now;
    }

    public class RealtimeChatMessageStub
    {
        public string? Message { get; set; }
        public bool IsUser { get; set; }
        public DateTime Timestamp { get; set; }
        public System.Collections.Generic.List<DataEntities.Product>? Products { get; set; }
    }
}
