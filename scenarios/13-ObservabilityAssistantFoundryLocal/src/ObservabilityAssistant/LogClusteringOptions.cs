// Configuration for the local-embeddings log clustering step.
// Bound from the "Embeddings" section of appsettings.json.
internal sealed class LogClusteringOptions
{
    // When false, the analyzer skips embeddings entirely and sends raw log lines
    // to the model (useful to show the "before/after" difference during demos).
    public bool Enabled { get; set; } = true;

    // HuggingFace model id used by ElBruno.LocalEmbeddings (downloaded once, then
    // cached locally). all-MiniLM-L6-v2 is small (~90 MB) and 384-dimensional.
    public string ModelName { get; set; } = "sentence-transformers/all-MiniLM-L6-v2";

    // Cosine-similarity threshold (0..1). Log lines whose embeddings are at least
    // this similar are folded into the same cluster (deduplicated).
    public float SimilarityThreshold { get; set; } = 0.85f;

    // Safety cap: don't embed more than this many entries in a single analysis
    // (keeps a noisy window fast on CPU-only machines).
    public int MaxEntriesToEmbed { get; set; } = 400;
}
