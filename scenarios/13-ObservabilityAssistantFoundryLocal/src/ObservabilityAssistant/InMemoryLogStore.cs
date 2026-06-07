using System.Collections.Generic;

internal sealed class InMemoryLogStore
{
    private readonly Queue<LogEntry> entries = new();
    private readonly object sync = new();
    private readonly int capacity;

    public InMemoryLogStore(int capacity = 5_000)
    {
        this.capacity = capacity;
    }

    public void Add(LogEntry entry)
    {
        lock (sync)
        {
            entries.Enqueue(entry);
            while (entries.Count > capacity)
            {
                entries.Dequeue();
            }
        }
    }

    public IReadOnlyList<LogEntry> GetWindow(DateTimeOffset fromInclusiveUtc, DateTimeOffset toInclusiveUtc)
    {
        lock (sync)
        {
            return entries
                .Where(entry => entry.Timestamp >= fromInclusiveUtc && entry.Timestamp <= toInclusiveUtc)
                .OrderBy(entry => entry.Timestamp)
                .ToList();
        }
    }
}
