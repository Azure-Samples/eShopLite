internal static class ObservabilityEndpoints
{
    public static IEndpointRouteBuilder MapObservabilityEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var observability = endpoints.MapGroup("/observability");

        observability.MapGet("/windows", () =>
            Results.Ok(ObservabilityWindow.Supported.Select(m => new
            {
                minutes = m,
                analyzeEndpoint = $"/observability/analyze?minutes={m}"
            })));

        observability.MapGet("/analyze", async (int? minutes, ObservabilityAnalyzer analyzer, CancellationToken cancellationToken) =>
        {
            if (minutes is null)
            {
                return Results.BadRequest(new
                {
                    error = "Query parameter 'minutes' is required.",
                    supportedMinutes = ObservabilityWindow.Supported
                });
            }

            if (!ObservabilityWindow.Supported.Contains(minutes.Value))
            {
                return Results.BadRequest(new
                {
                    error = "Invalid 'minutes' value. Allowed values are 5, 10, 15, and 30.",
                    supportedMinutes = ObservabilityWindow.Supported
                });
            }

            var result = await analyzer.AnalyzeAsync(minutes.Value, cancellationToken);
            if (result is null)
            {
                return Results.NotFound(new
                {
                    error = $"No log entries were found for the last {minutes.Value} minutes."
                });
            }

            return Results.Ok(result);
        });

        observability.MapPost("/events", (ObservabilityEventIngestionRequest request, InMemoryLogStore logStore) =>
        {
            if (string.IsNullOrWhiteSpace(request.Service) ||
                string.IsNullOrWhiteSpace(request.Severity) ||
                string.IsNullOrWhiteSpace(request.Message))
            {
                return Results.BadRequest(new
                {
                    error = "Fields 'service', 'severity', and 'message' are required."
                });
            }

            var entry = new LogEntry(
                request.Timestamp ?? DateTimeOffset.UtcNow,
                request.Service.Trim(),
                request.Severity.Trim(),
                request.Message.Trim());

            logStore.Add(entry);

            return Results.Accepted("/observability/events", new
            {
                status = "accepted",
                storedAtUtc = entry.Timestamp
            });
        });

        return endpoints;
    }
}
