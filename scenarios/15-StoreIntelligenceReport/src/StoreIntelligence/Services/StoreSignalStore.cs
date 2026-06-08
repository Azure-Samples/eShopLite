using System.Collections.Concurrent;
using DataEntities;

namespace StoreIntelligence.Services;

/// <summary>
/// Thread-safe, in-memory store of recent store signals (search events). This is the raw
/// material the Store Intelligence Report summarizes.
///
/// Design notes:
/// • Uses a <see cref="ConcurrentQueue{T}"/> so multiple HTTP threads can record signals
///   without locking.
/// • Bounded to 500 entries — oldest signals are evicted when the buffer is full, so
///   memory stays constant regardless of traffic.
/// • Seeded with deterministic sample signals at startup so the report is never empty
///   before the presenter has run a search.
/// </summary>
public class StoreSignalStore
{
    private const int MaxSignals = 500;
    private readonly ConcurrentQueue<StoreSignal> _signals = new();

    public StoreSignalStore()
    {
        SeedSampleSignals();
    }

    /// <summary>
    /// Records a search event. The term is trimmed; blank terms are silently dropped.
    /// </summary>
    public void Record(string term, bool semantic, int resultCount)
    {
        if (string.IsNullOrWhiteSpace(term))
        {
            return;
        }

        _signals.Enqueue(new StoreSignal
        {
            Term = term.Trim(),
            Semantic = semantic,
            ResultCount = resultCount,
            TimestampUtc = DateTime.UtcNow
        });

        // Evict the oldest entry when the ring buffer is full.
        while (_signals.Count > MaxSignals && _signals.TryDequeue(out _))
        {
        }
    }

    /// <summary>Returns up to <paramref name="max"/> signals, newest first.</summary>
    public IReadOnlyList<StoreSignal> GetRecent(int max = 100)
        => _signals.Reverse().Take(max).ToList();

    /// <summary>All signals, oldest first (needed for aggregation in the report service).</summary>
    public IReadOnlyList<StoreSignal> GetAll() => _signals.ToList();

    /// <summary>
    /// Deterministic sample signals so the report has content immediately on startup.
    /// These mirror the session demo narrative (seasonal/outdoor intent + a few failed searches).
    /// </summary>
    private void SeedSampleSignals()
    {
        var now = DateTime.UtcNow;
        var samples = new (string Term, bool Semantic, int ResultCount, int MinutesAgo)[]
        {
            // Rain topic (intended to collapse into one grouped intent).
            ("rainy day gear",                           true,  3, 28),
            ("do you have something for a rainy day",    true,  2, 27),
            ("something for camping while it's raining", true,  2, 26),
            ("rain jacket",                              true,  3, 25),
            ("something for rainy days while I'm camping", true, 3, 24),

            // Camping topic.
            ("camp cooking",            true,  4, 22),
            ("do you have something for cooking", true, 2, 21),
            ("camp stove",              false, 2, 20),
            ("four season tent",        true,  2, 18),
            ("winter camping tent",     false, 2, 16),

            // Deliberate no-result examples for product-gap narrative.
            ("hiking boots size 12",                    false, 0, 14), // failed — no size match
            ("paint my room white",                     true,  0, 12), // failed — off-catalog
            ("something to paint my room white",        true,  0, 10), // failed — same intent wording
            ("something to decorate my backyard with racoons", true, 0, 8), // failed — off-catalog
        };

        foreach (var s in samples)
        {
            _signals.Enqueue(new StoreSignal
            {
                Term = s.Term,
                Semantic = s.Semantic,
                ResultCount = s.ResultCount,
                TimestampUtc = now.AddMinutes(-s.MinutesAgo)
            });
        }
    }
}
