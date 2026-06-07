namespace Products.Endpoints;

public static class ProductFailureInjection
{
    private const int DefaultFailureProbabilityPercent = 30;

    public static void ThrowIfInjectedFailure(
        HttpContext httpContext,
        ILogger logger,
        string context)
    {
        var query = httpContext.Request.Query;

        var injectFailure = false;
        if (query.TryGetValue("injectFailure", out var injectFailureParam))
        {
            _ = bool.TryParse(injectFailureParam.ToString(), out injectFailure);
        }

        var failureProbabilityPercent = DefaultFailureProbabilityPercent;
        if (query.TryGetValue("failureProbabilityPercent", out var probabilityParam) &&
            int.TryParse(probabilityParam.ToString(), out var parsedProbability) &&
            parsedProbability is >= 0 and <= 100)
        {
            failureProbabilityPercent = parsedProbability;
        }

        if (!injectFailure)
        {
            return;
        }

        if (Random.Shared.Next(100) < failureProbabilityPercent)
        {
            logger.LogError(
                "Demo-injected failure triggered. Context: {Context}. Route: {Route}. ProbabilityPercent: {ProbabilityPercent}",
                context,
                httpContext.Request.Path.Value,
                failureProbabilityPercent);

            throw new InvalidOperationException(
                $"Demo-injected failure in {context} with probability {failureProbabilityPercent}%.");
        }
    }
}
