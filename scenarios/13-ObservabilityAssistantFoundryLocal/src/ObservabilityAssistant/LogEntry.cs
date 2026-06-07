internal sealed record LogEntry(DateTimeOffset Timestamp, string Service, string Severity, string Message);
