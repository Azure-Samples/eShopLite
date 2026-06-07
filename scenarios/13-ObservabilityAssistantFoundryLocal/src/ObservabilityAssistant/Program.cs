using ElBruno.MAF.FoundryLocal;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddProblemDetails();

// Bind and validate the full FoundryLocal section.
// This catalog drives model selection using:
// - FoundryLocal:SelectedModel (active key)
// - FoundryLocal:Models:{key}:* (catalog entries)
builder.Services
    .AddOptions<FoundryLocalModelCatalogOptions>()
    .Bind(builder.Configuration.GetSection("FoundryLocal"))
    .Validate(options => !string.IsNullOrWhiteSpace(options.SelectedModel),
        "FoundryLocal:SelectedModel is required.")
    .Validate(options => options.Models.Count > 0,
        "FoundryLocal:Models must contain at least one model entry.")
    .Validate(options =>
    {
        if (!options.Models.TryGetValue(options.SelectedModel, out var selected))
        {
            return false;
        }

        return !string.IsNullOrWhiteSpace(selected.ModelAlias);
    }, "FoundryLocal:SelectedModel must match a model key with a non-empty ModelAlias.")
    .ValidateOnStart();

// Bind runtime FoundryLocal options from the same section, then map the selected
// catalog entry into executable values (ModelAlias, optional flags).
// To switch local models for demos, set FoundryLocal:SelectedModel to another key
// and define/update that key under FoundryLocal:Models (with ModelAlias, etc.).
builder.Services
    .AddOptions<FoundryLocalOptions>()
    .Bind(builder.Configuration.GetSection("FoundryLocal"))
    .PostConfigure<IOptions<FoundryLocalModelCatalogOptions>>((foundryOptions, catalogOptionsAccessor) =>
    {
        var catalog = catalogOptionsAccessor.Value;
        if (!catalog.Models.TryGetValue(catalog.SelectedModel, out var selectedModel))
        {
            return;
        }

        foundryOptions.ModelAlias = selectedModel.ModelAlias;
        if (selectedModel.DownloadIfMissing.HasValue)
        {
            foundryOptions.DownloadIfMissing = selectedModel.DownloadIfMissing.Value;
        }

        if (selectedModel.UnloadOnExit.HasValue)
        {
            foundryOptions.UnloadOnExit = selectedModel.UnloadOnExit.Value;
        }
    })
    .Validate(options => !string.IsNullOrWhiteSpace(options.ModelAlias),
        "FoundryLocal model alias could not be resolved from FoundryLocal:SelectedModel.")
    .ValidateOnStart();

builder.Services.Configure<ChatRuntimeOptions>(builder.Configuration.GetSection("Chat"));

// Local-embeddings log clustering options (ElBruno.LocalEmbeddings, ONNX, no cloud).
// Read from the "Embeddings" section of appsettings.json:
// - Embeddings:Enabled            toggle the embeddings step on/off (demo before/after)
// - Embeddings:ModelName          HuggingFace embedding model (downloaded once, cached)
// - Embeddings:SimilarityThreshold cosine threshold to fold similar log lines together
builder.Services.Configure<LogClusteringOptions>(builder.Configuration.GetSection("Embeddings"));

builder.Services.AddSingleton<FoundryLocalModelLifecycleService>();
builder.Services.AddSingleton<IChatClient>(sp =>
{
    var lifecycleService = sp.GetRequiredService<FoundryLocalModelLifecycleService>();
    var adapterLogger = sp.GetRequiredService<ILogger<FoundryLocalChatClientAdapter>>();
    return new FoundryLocalChatClientAdapter(lifecycleService, adapterLogger);
});

builder.Services.AddSingleton<InMemoryLogStore>();
builder.Services.AddSingleton<LogClusteringService>();
builder.Services.AddSingleton<ObservabilityAnalyzer>();

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapObservabilityEndpoints();

app.Run();
