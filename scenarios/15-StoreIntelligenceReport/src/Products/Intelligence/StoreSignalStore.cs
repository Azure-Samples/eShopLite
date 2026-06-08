using System.Collections.Concurrent;
using DataEntities;

namespace Products.Intelligence;

/// <summary>
/// Thread-safe, in-memory store of recent store signals (search events). This is the raw
/// material the Store Intelligence Report summarizes. It is intentionally in-memory and bounded
/// (a ring buffer) so the demo has no external dependency. A small set of deterministic sample
/// signals is seeded at startup so the report is never empty before the presenter searches.
/// </summary>
public class StoreSignalStore
{
    private const int MaxSignals = 500;
    private readonly ConcurrentQueue<StoreSignal> _signals = new();

    public StoreSignalStore()
    {
        SeedSampleSignals();
    }

    /// <summary>Record a search signal.</summary>
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

        // Bound the buffer.
        while (_signals.Count > MaxSignals && _signals.TryDequeue(out _))
        {
        }
    }

    /// <summary>Most recent signals first.</summary>
    public IReadOnlyList<StoreSignal> GetRecent(int max = 100)
    {
        return _signals
            .Reverse()
            .Take(max)
            .ToList();
    }

    /// <summary>All signals, oldest first (for aggregation).</summary>
    public IReadOnlyList<StoreSignal> GetAll() => _signals.ToList();

    /// <summary>
    /// Deterministic sample signals so the report has content out of the box. These mirror the
    /// session demo narrative (seasonal/outdoor intent + a couple of failed searches).
    /// </summary>
    private void SeedSampleSignals()
    {
        var now = DateTime.UtcNow;
        var samples = new (string Term, bool Semantic, int ResultCount, int MinutesAgo)[]
        {
            ("rainy day gear", true, 3, 22),
            ("camp cooking", true, 4, 19),
            ("four season tent", true, 2, 16),
            ("winter camping tent", false, 2, 14),
            ("hiking boots size 12", false, 0, 11),
            ("paint my room white", true, 0, 9),
            ("rain jacket", true, 3, 6),
            ("camp stove", false, 2, 3),
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
