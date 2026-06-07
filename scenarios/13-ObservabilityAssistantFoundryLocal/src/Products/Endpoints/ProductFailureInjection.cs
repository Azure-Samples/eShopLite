namespace Products.Endpoints;

public static class ProductFailureInjection
{
    // Demo-friendly behavior: when the Store sends "injectFailure=true"
    // (the "Inject Search Failure" checkbox), the endpoint ALWAYS throws.
    // This guarantees an error shows up in the logs during the live demo
    // so the Observability Assistant has something to detect.
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

        if (!injectFailure)
        {
            return;
        }

        logger.LogError(
            "Demo-injected failure triggered. Context: {Context}. Route: {Route}",
            context,
            httpContext.Request.Path.Value);

        throw new InvalidOperationException(
            $"Demo-injected failure in {context}.");
    }
}
