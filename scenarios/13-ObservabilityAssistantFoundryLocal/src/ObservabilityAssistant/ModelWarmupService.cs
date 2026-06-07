using ElBruno.MAF.FoundryLocal;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;

// Warms up the selected Foundry Local model at startup by issuing a tiny priming
// chat through the same IChatClient the analyzer uses. This triggers the model
// load/download path once on boot, so the first "Analyze" click in the demo is a
// fast inference instead of a slow cold-start (and `runtimeModelLoaded` reports true).
// Runs in the background and never throws - if Foundry Local is offline, the analyzer
// still degrades gracefully to its deterministic fallback.
internal sealed class ModelWarmupService(
    IChatClient chatClient,
    IOptions<FoundryLocalModelCatalogOptions> catalogOptions,
    ILogger<ModelWarmupService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var catalog = catalogOptions.Value;
        catalog.Models.TryGetValue(catalog.SelectedModel, out var selected);
        var alias = selected?.ModelAlias ?? catalog.SelectedModel;

        logger.LogInformation(
            "Warming up Foundry Local model '{Alias}' (catalog key '{Key}')...",
            alias,
            catalog.SelectedModel);

        try
        {
            var response = await chatClient.GetResponseAsync(
                [new ChatMessage(ChatRole.User, "Reply with the single word: ready.")],
                cancellationToken: stoppingToken);

            logger.LogInformation(
                "Foundry Local model '{Alias}' is warm and ready. Priming reply: {Reply}",
                alias,
                string.IsNullOrWhiteSpace(response.Text) ? "(empty)" : response.Text.Trim());
        }
        catch (OperationCanceledException)
        {
            // App is shutting down before warm-up finished - nothing to do.
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Foundry Local warm-up for model '{Alias}' failed. The assistant will use the deterministic fallback until the model is available.",
                alias);
        }
    }
}
