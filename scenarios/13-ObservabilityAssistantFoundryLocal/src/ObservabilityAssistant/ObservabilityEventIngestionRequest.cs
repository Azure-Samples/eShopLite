internal sealed record ObservabilityEventIngestionRequest(
    DateTimeOffset? Timestamp,
    string Service,
    string Severity,
    string Message);
