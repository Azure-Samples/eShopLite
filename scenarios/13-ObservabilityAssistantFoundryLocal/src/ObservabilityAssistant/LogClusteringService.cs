using ElBruno.LocalEmbeddings;
using ElBruno.LocalEmbeddings.Options;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;

// Groups semantically-similar log lines using fully-local embeddings
// (ElBruno.LocalEmbeddings -> ONNX Runtime, no cloud calls).
//
// The embedding model is created lazily on first use and cached for the life of
// the process. If embeddings are disabled or fail for any reason, the service
// degrades gracefully to a pass-through (one cluster per log entry) so the
// scenario still runs fully offline.
internal sealed class LogClusteringService(
    IOptions<LogClusteringOptions> options,
    ILogger<LogClusteringService> logger)
{
    private readonly LogClusteringOptions _options = options.Value;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private LocalEmbeddingGenerator? _generator;
    private bool _initializationFailed;

    public async Task<IReadOnlyList<LogCluster>> ClusterAsync(
        IReadOnlyList<LogEntry> logs,
        CancellationToken cancellationToken)
    {
        if (!_options.Enabled || logs.Count <= 1)
        {
            return PassThrough(logs);
        }

        var generator = await GetGeneratorAsync(cancellationToken);
        if (generator is null)
        {
            return PassThrough(logs);
        }

        try
        {
            var entries = logs.Count > _options.MaxEntriesToEmbed
                ? logs.Take(_options.MaxEntriesToEmbed).ToList()
                : logs;

            var messages = entries
                .Select(e => $"[{e.Severity}] {e.Service}: {e.Message}")
                .ToList();

            var embeddings = await generator.GenerateAsync(messages, options: null, cancellationToken: cancellationToken);
            var vectors = embeddings.Select(e => e.Vector).ToList();

            return BuildClusters(entries, vectors);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Local embeddings clustering failed. Falling back to pass-through log grouping.");
            return PassThrough(logs);
        }
    }

    private List<LogCluster> BuildClusters(IReadOnlyList<LogEntry> entries, IReadOnlyList<ReadOnlyMemory<float>> vectors)
    {
        var representatives = new List<(ReadOnlyMemory<float> Vector, List<LogEntry> Members)>();

        for (var i = 0; i < entries.Count; i++)
        {
            var vector = vectors[i];
            var bestIndex = -1;
            var bestScore = float.MinValue;

            for (var c = 0; c < representatives.Count; c++)
            {
                var score = CosineSimilarity(vector.Span, representatives[c].Vector.Span);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestIndex = c;
                }
            }

            if (bestIndex >= 0 && bestScore >= _options.SimilarityThreshold)
            {
                representatives[bestIndex].Members.Add(entries[i]);
            }
            else
            {
                representatives.Add((vector, new List<LogEntry> { entries[i] }));
            }
        }

        return representatives
            .Select(group => new LogCluster(group.Members[0], group.Members.Count, group.Members))
            .OrderByDescending(cluster => cluster.Count)
            .ToList();
    }

    private async Task<LocalEmbeddingGenerator?> GetGeneratorAsync(CancellationToken cancellationToken)
    {
        if (_generator is not null)
        {
            return _generator;
        }

        if (_initializationFailed)
        {
            return null;
        }

        await _initLock.WaitAsync(cancellationToken);
        try
        {
            if (_generator is not null)
            {
                return _generator;
            }

            if (_initializationFailed)
            {
                return null;
            }

            logger.LogInformation(
                "Initializing local embedding model '{ModelName}' (first use downloads and caches the model).",
                _options.ModelName);

            // Async factory handles model download + ONNX session setup off the request path.
            _generator = await LocalEmbeddingGenerator.CreateAsync(
                new LocalEmbeddingsOptions
                {
                    ModelName = _options.ModelName,
                    EnsureModelDownloaded = true,
                    NormalizeEmbeddings = true
                },
                cancellationToken);

            return _generator;
        }
        catch (Exception ex)
        {
            _initializationFailed = true;
            logger.LogWarning(ex, "Could not initialize local embedding model. Clustering disabled for this session.");
            return null;
        }
        finally
        {
            _initLock.Release();
        }
    }

    private static List<LogCluster> PassThrough(IReadOnlyList<LogEntry> logs) =>
        logs.Select(entry => new LogCluster(entry, 1, new[] { entry })).ToList();

    private static float CosineSimilarity(ReadOnlySpan<float> a, ReadOnlySpan<float> b)
    {
        if (a.Length != b.Length || a.Length == 0)
        {
            return 0f;
        }

        double dot = 0, normA = 0, normB = 0;
        for (var i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            normA += a[i] * a[i];
            normB += b[i] * b[i];
        }

        if (normA == 0 || normB == 0)
        {
            return 0f;
        }

        return (float)(dot / (Math.Sqrt(normA) * Math.Sqrt(normB)));
    }
}
