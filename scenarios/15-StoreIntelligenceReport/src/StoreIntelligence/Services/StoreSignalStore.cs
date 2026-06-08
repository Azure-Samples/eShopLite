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
            // Rain topic.
            ("rainy day gear",                                true,  3, 45),
            ("do you have something for a rainy day",         true,  2, 44),
            ("something for camping while it's raining",      true,  2, 43),
            ("rain jacket",                                   true,  3, 42),
            ("something for rainy days while I'm camping",    true,  3, 41),
            ("waterproof jacket for hiking",                  true,  2, 40),

            // Camping + shelter topic.
            ("camp cooking",                                  true,  4, 38),
            ("do you have something for cooking",             true,  2, 37),
            ("camp stove",                                    false, 2, 36),
            ("four season tent",                              true,  2, 35),
            ("winter camping tent",                           false, 2, 34),
            ("lightweight backpacking tent",                  true,  3, 33),
            ("camp cookware set",                             true,  2, 32),
            ("portable propane stove",                        false, 2, 31),

            // Hiking + footwear topic.
            ("hiking backpack 50l",                           true,  3, 29),
            ("trekking poles",                                true,  2, 28),
            ("trail running hydration vest",                  true,  2, 27),
            ("hiking socks merino",                           true,  4, 26),

            // Deliberate no-result examples for gap narrative.
            ("hiking boots size 12",                          false, 0, 24),
            ("hiking boots size 13 wide",                     false, 0, 23),
            ("paint my room white",                           true,  0, 22),
            ("something to paint my room white",              true,  0, 21),
            ("something to decorate my backyard with racoons",true,  0, 20),
            ("raccoon garden statue",                         false, 0, 19),
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
