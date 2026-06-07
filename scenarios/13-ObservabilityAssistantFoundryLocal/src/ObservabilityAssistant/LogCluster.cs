using System.Collections.Generic;

// A group of semantically-similar log entries collapsed into one representative
// line plus the number of occurrences it stands in for.
internal sealed record LogCluster(LogEntry Representative, int Count, IReadOnlyList<LogEntry> Members);
