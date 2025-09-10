namespace StoreRealtime.Models;

public enum LogCategory
{
    System,
    User,
    Assistant,
    Function,
    Audio,
    Error
}

public sealed class LogMessage
{
    public LogMessage(string message, LogCategory category = LogCategory.System, string? icon = null, string severity = "info")
    {
        Message = message;
        Category = category;
        Icon = icon ?? Category switch
        {
            LogCategory.User => "🧑",
            LogCategory.Assistant => "🤖",
            LogCategory.Function => "🛠️",
            LogCategory.Audio => "🔊",
            LogCategory.Error => "⚠️",
            _ => "ℹ️"
        };
        Severity = severity;
        Timestamp = DateTime.Now;
    }

    public string Message { get; init; }
    public LogCategory Category { get; init; }
    public string Icon { get; init; }
    public string Severity { get; init; }
    public DateTime Timestamp { get; init; }

    public override string ToString() => $"{Timestamp:HH:mm:ss} {Icon} {Message}";

    // Convenience factories
    public static LogMessage User(string msg) => new(msg, LogCategory.User);
    public static LogMessage Assistant(string msg) => new(msg, LogCategory.Assistant);
    public static LogMessage Function(string msg) => new(msg, LogCategory.Function);
    public static LogMessage Audio(string msg) => new(msg, LogCategory.Audio);
    public static LogMessage System(string msg) => new(msg, LogCategory.System);
    public static LogMessage Error(string msg) => new(msg, LogCategory.Error, severity: "error");
}
