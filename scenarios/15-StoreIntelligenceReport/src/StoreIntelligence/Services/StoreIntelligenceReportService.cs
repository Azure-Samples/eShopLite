using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using DataEntities;
using Microsoft.Extensions.AI;

namespace StoreIntelligence.Services;

/// <summary>
/// Builds the Store Intelligence Report from captured store signals.
///
/// Two-phase generation:
///   1. Deterministic aggregation — always runs, produces topSearches, failedSearches,
///      topCustomerIntents, productOpportunities, operationalIssues, and a rule-based narrative. This is the
///      "fallback" that works with no AI model configured.
///   2. AI narrative upgrade — if a <see cref="IChatClient"/> is available, the executive
///      summary, recommended actions, and customer-intent grouping are replaced with model-generated
///      output. If the
///      call fails for any reason, the deterministic narrative is kept and <c>Source</c>
///      remains "fallback".
/// </summary>
public class StoreIntelligenceReportService
{
    private readonly StoreSignalStore _signals;
    private readonly IChatClient? _chatClient;
    private readonly ILogger<StoreIntelligenceReportService> _logger;

    public StoreIntelligenceReportService(
        StoreSignalStore signals,
        ILogger<StoreIntelligenceReportService> logger,
        IChatClient? chatClient = null)  // null → deterministic-only mode
    {
        _signals = signals;
        _logger = logger;
        _chatClient = chatClient;
    }

    /// <summary>Generates the intelligence report from the current signal window.</summary>
    public async Task<StoreIntelligenceReport> GenerateAsync(CancellationToken cancellationToken = default)
    {
        var all = _signals.GetAll();
        var successfulTermStats = BuildSuccessfulTermStats(all);

        // ── Deterministic aggregation ────────────────────────────────────────
        var topSearches = successfulTermStats
            .OrderByDescending(t => t.Count)
            .Take(5)
            .Select(t => $"{t.Term} (x{t.Count})")
            .ToList();

        var failedSearches = all
            .Where(s => s.Failed)
            .GroupBy(s => s.Term, StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .Select(g => g.Count() > 1 ? $"{g.Key} (x{g.Count()})" : g.Key)
            .ToList();

        var report = new StoreIntelligenceReport
        {
            GeneratedAtUtc = DateTime.UtcNow,
            SignalsAnalyzed = all.Count,
            Source = "fallback",
            TopSearches = topSearches.Count > 0 ? topSearches : ["No successful searches yet."],
            TopCustomerIntents = BuildDeterministicIntents(successfulTermStats),
            FailedSearches = failedSearches.Count > 0 ? failedSearches : ["No failed searches in this window."],
            ProductOpportunities = BuildOpportunities(all),
            OperationalIssues = BuildOperationalIssues(all),
        };

        // Deterministic narrative (doubles as the fallback if AI fails).
        report.ExecutiveSummary = BuildDeterministicSummary(all, topSearches, failedSearches);
        report.RecommendedActions = BuildDeterministicActions(report);

        // ── Optional: upgrade narrative with the chat model ──────────────────
        if (_chatClient is not null)
        {
            try
            {
                var usedAi = false;

                var aiNarrative = await GenerateNarrativeWithAiAsync(report, cancellationToken);
                if (aiNarrative is not null)
                {
                    report.ExecutiveSummary = aiNarrative.ExecutiveSummary;
                    report.RecommendedActions = aiNarrative.RecommendedActions;
                    usedAi = true;
                }

                var aiIntents = await GenerateCustomerIntentsWithAiAsync(successfulTermStats, cancellationToken);
                if (aiIntents is not null && aiIntents.Count > 0)
                {
                    report.TopCustomerIntents = aiIntents;
                    usedAi = true;
                }

                if (usedAi)
                {
                    report.Source = "ai";
                }
            }
            catch (Exception ex)
            {
                // AI is best-effort — log and keep the deterministic narrative.
                _logger.LogWarning(ex, "AI narrative generation failed; using deterministic fallback.");
            }
        }

        return report;
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "a", "an", "and", "are", "for", "from", "have", "i", "in", "is", "it", "its", "me",
        "my", "of", "on", "or", "please", "show", "something", "that", "the", "this", "to",
        "we", "while", "with", "you", "your", "do"
    };

    private static readonly Regex NonAlphaNumericRegex = new("[^a-z0-9 ]", RegexOptions.Compiled);

    private static List<TermAggregate> BuildSuccessfulTermStats(IReadOnlyList<StoreSignal> all)
    {
        return all
            .Where(s => !s.Failed)
            .GroupBy(s => s.Term, StringComparer.OrdinalIgnoreCase)
            .Select(g =>
            {
                var canonicalTerm = g.First().Term.Trim();
                return new TermAggregate(
                    canonicalTerm,
                    g.Count(),
                    g.Max(x => x.ResultCount),
                    TokenizeTerm(canonicalTerm));
            })
            .ToList();
    }

    private static List<CustomerIntent> BuildDeterministicIntents(IReadOnlyList<TermAggregate> terms)
    {
        if (terms.Count == 0)
        {
            return [];
        }

        var clusters = new List<IntentCluster>();

        foreach (var term in terms.OrderByDescending(t => t.Count))
        {
            var target = clusters.FirstOrDefault(c => SharesTopicToken(c, term));
            if (target is null)
            {
                target = new IntentCluster();
                clusters.Add(target);
            }

            target.Terms.Add(term);
            foreach (var token in term.Tokens)
            {
                target.TopicTokens.Add(token);
            }
        }

        return clusters
            .Select(c => new CustomerIntent
            {
                Theme = BuildThemeLabel(c),
                TotalSearches = c.Terms.Sum(t => t.Count),
                Terms = c.Terms
                    .OrderByDescending(t => t.Count)
                    .Select(t => new IntentTerm
                    {
                        Term = t.Term,
                        Count = t.Count,
                        Results = t.RepresentativeResults
                    })
                    .ToList()
            })
            .OrderByDescending(c => c.TotalSearches)
            .ThenBy(c => c.Theme, StringComparer.OrdinalIgnoreCase)
            .Take(5)
            .ToList();
    }

    private async Task<List<CustomerIntent>?> GenerateCustomerIntentsWithAiAsync(
        IReadOnlyList<TermAggregate> terms,
        CancellationToken cancellationToken)
    {
        if (_chatClient is null || terms.Count == 0)
        {
            return null;
        }

        var termLines = string.Join('\n', terms.Select(t => $"- {t.Term} (x{t.Count}, results:{t.RepresentativeResults})"));
        var system = "You are a store intelligence analyst. Group search terms by customer intent meaning. " +
            "Terms that ask for the same thing (even with different wording) must be grouped together.";

        var prompt =
            "Group these search terms into themes and return STRICT JSON only.\n" +
            "Schema: {\"themes\":[{\"theme\":\"short title\",\"terms\":[\"exact original term\"]}]}\n" +
            "Rules:\n" +
            "1) Keep term text exactly as provided.\n" +
            "2) Every term must appear in exactly one theme.\n" +
            "3) Use concise theme names meaningful to a store manager.\n\n" +
            "Terms:\n" + termLines;

        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, system),
            new(ChatRole.User, prompt)
        };

        var response = await _chatClient.GetResponseAsync(messages, cancellationToken: cancellationToken);
        var text = response.Text?.Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        text = StripCodeFence(text);

        try
        {
            using var doc = JsonDocument.Parse(text);
            if (!doc.RootElement.TryGetProperty("themes", out var themes) || themes.ValueKind != JsonValueKind.Array)
            {
                return null;
            }

            var byTerm = terms.ToDictionary(t => t.Term, StringComparer.OrdinalIgnoreCase);
            var assigned = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var result = new List<CustomerIntent>();

            foreach (var themeNode in themes.EnumerateArray())
            {
                if (!themeNode.TryGetProperty("theme", out var themeProperty) ||
                    themeProperty.ValueKind != JsonValueKind.String)
                {
                    continue;
                }

                if (!themeNode.TryGetProperty("terms", out var termsNode) ||
                    termsNode.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                var members = new List<TermAggregate>();
                foreach (var termNode in termsNode.EnumerateArray())
                {
                    if (termNode.ValueKind != JsonValueKind.String)
                    {
                        continue;
                    }

                    var termText = termNode.GetString();
                    if (string.IsNullOrWhiteSpace(termText))
                    {
                        continue;
                    }

                    if (assigned.Contains(termText))
                    {
                        continue;
                    }

                    if (byTerm.TryGetValue(termText.Trim(), out var member))
                    {
                        members.Add(member);
                        assigned.Add(member.Term);
                    }
                }

                if (members.Count == 0)
                {
                    continue;
                }

                var theme = themeProperty.GetString()?.Trim();
                if (string.IsNullOrWhiteSpace(theme))
                {
                    theme = BuildThemeLabel(new IntentCluster { Terms = members });
                }

                result.Add(new CustomerIntent
                {
                    Theme = theme!,
                    TotalSearches = members.Sum(m => m.Count),
                    Terms = members
                        .OrderByDescending(m => m.Count)
                        .Select(m => new IntentTerm
                        {
                            Term = m.Term,
                            Count = m.Count,
                            Results = m.RepresentativeResults
                        })
                        .ToList()
                });
            }

            var unassigned = terms.Where(t => !assigned.Contains(t.Term)).ToList();
            if (unassigned.Count > 0)
            {
                result.AddRange(BuildDeterministicIntents(unassigned));
            }

            return result
                .OrderByDescending(i => i.TotalSearches)
                .ThenBy(i => i.Theme, StringComparer.OrdinalIgnoreCase)
                .Take(5)
                .ToList();
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Could not parse AI customer intent JSON; using deterministic intent clusters.");
            return null;
        }
    }

    private static bool SharesTopicToken(IntentCluster cluster, TermAggregate term)
    {
        return cluster.TopicTokens.Overlaps(term.Tokens);
    }

    private static HashSet<string> TokenizeTerm(string term)
    {
        var normalized = NonAlphaNumericRegex.Replace(term.ToLowerInvariant(), " ");
        var tokens = normalized
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(NormalizeToken)
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Where(t => !StopWords.Contains(t))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return tokens;
    }

    private static string NormalizeToken(string token)
    {
        if (token.StartsWith("rain", StringComparison.Ordinal))
        {
            return "rain";
        }

        if (token.StartsWith("camp", StringComparison.Ordinal))
        {
            return "camp";
        }

        return token;
    }

    private static string BuildThemeLabel(IntentCluster cluster)
    {
        var tokenScores = cluster.Terms
            .SelectMany(t => t.Tokens.Select(token => new { token, t.Count }))
            .GroupBy(x => x.token, StringComparer.OrdinalIgnoreCase)
            .Select(g => new { Token = g.Key, Score = g.Sum(x => x.Count) })
            .OrderByDescending(g => g.Score)
            .ThenBy(g => g.Token, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (tokenScores.Count == 0)
        {
            return "General intent";
        }

        if (tokenScores[0].Token.Equals("rain", StringComparison.OrdinalIgnoreCase))
        {
            return "Rain questions";
        }

        return $"{char.ToUpper(tokenScores[0].Token[0])}{tokenScores[0].Token[1..]} questions";
    }

    private static List<string> BuildOpportunities(IReadOnlyList<StoreSignal> all)
    {
        // On-catalog-sounding failed searches are gaps worth filling.
        // Heuristic: exclude the obvious off-catalog demo term "paint".
        var gaps = all
            .Where(s => s.Failed && !s.Term.Contains("paint", StringComparison.OrdinalIgnoreCase))
            .GroupBy(s => s.Term, StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(g => g.Count())
            .Take(5)
            .Select(g => $"Demand for \"{g.Key}\" with no matching product — review catalog/size coverage.")
            .ToList();

        return gaps.Count > 0 ? gaps : ["No clear product gaps detected in this window."];
    }

    private static List<string> BuildOperationalIssues(IReadOnlyList<StoreSignal> all)
    {
        var issues = new List<string>();
        var failedCount = all.Count(s => s.Failed);
        if (all.Count > 0)
        {
            var failRate = (double)failedCount / all.Count;
            if (failRate >= 0.25)
            {
                issues.Add($"Elevated no-result rate ({failedCount}/{all.Count} searches) — verify catalog coverage and the search path.");
            }
        }

        return issues.Count > 0 ? issues : ["No blocking operational issues detected; monitor search latency during activity bursts."];
    }

    private static List<string> BuildDeterministicSummary(
        IReadOnlyList<StoreSignal> all,
        List<string> topSearches,
        List<string> failedSearches)
    {
        var summary = new List<string>
        {
            $"Analyzed {all.Count} store signals (searches) in the recent window.",
        };

        if (topSearches.Count > 0)
        {
            summary.Add($"Customers searched mostly for: {string.Join(", ", topSearches.Take(3))}.");
        }

        var failedCount = all.Count(s => s.Failed);
        summary.Add(failedCount > 0
            ? $"{failedCount} search(es) returned no results — real catalog gaps or off-catalog intent."
            : "Every search returned at least one product.");

        return summary;
    }

    private static List<string> BuildDeterministicActions(StoreIntelligenceReport report)
    {
        var actions = new List<string>();

        if (report.ProductOpportunities.Count > 0 &&
            !report.ProductOpportunities[0].StartsWith("No clear", StringComparison.OrdinalIgnoreCase))
        {
            actions.Add("Fill the top catalog gap from 'Product opportunities' (add SKU / size coverage).");
        }

        if (report.TopSearches.Count > 0 &&
            !report.TopSearches[0].StartsWith("No successful", StringComparison.OrdinalIgnoreCase))
        {
            actions.Add("Promote the most-searched categories on the storefront.");
        }

        actions.Add("Run a quick latency check on the products search path.");
        return actions;
    }

    private async Task<NarrativeResult?> GenerateNarrativeWithAiAsync(
        StoreIntelligenceReport report,
        CancellationToken cancellationToken)
    {
        // Build a tightly scoped fact sheet — the model must NOT invent content.
        var facts = new StringBuilder();
        facts.AppendLine($"Signals analyzed: {report.SignalsAnalyzed}");
        facts.AppendLine($"Top searches: {string.Join("; ", report.TopSearches)}");
        facts.AppendLine($"Failed searches (no results): {string.Join("; ", report.FailedSearches)}");
        facts.AppendLine($"Product opportunities: {string.Join("; ", report.ProductOpportunities)}");
        facts.AppendLine($"Operational issues: {string.Join("; ", report.OperationalIssues)}");

        var system = "You are a store intelligence analyst for an outdoor-camping eCommerce app. " +
            "Write a concise daily business summary for a store manager, grounded ONLY in the facts provided. " +
            "Do not invent products, numbers, or issues that are not in the facts.";

        var prompt = "Using only the facts below, return STRICT JSON with two arrays: " +
            "\"executiveSummary\" (2-4 short bullet strings) and \"recommendedActions\" (2-3 short bullet strings). " +
            "Return ONLY the JSON object, no markdown, no code fence.\n\nFacts:\n" + facts;

        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, system),
            new(ChatRole.User, prompt)
        };

        var response = await _chatClient!.GetResponseAsync(messages, cancellationToken: cancellationToken);
        var text = response.Text?.Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        text = StripCodeFence(text); // tolerate a stray markdown code fence

        try
        {
            using var doc = JsonDocument.Parse(text);
            var root = doc.RootElement;
            var exec = ReadStringArray(root, "executiveSummary");
            var actions = ReadStringArray(root, "recommendedActions");
            if (exec.Count == 0 && actions.Count == 0)
            {
                return null;
            }

            return new NarrativeResult(
                exec.Count > 0 ? exec : report.ExecutiveSummary,
                actions.Count > 0 ? actions : report.RecommendedActions);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Could not parse AI narrative JSON; using deterministic fallback.");
            return null;
        }
    }

    private static string StripCodeFence(string text)
    {
        if (text.StartsWith("```", StringComparison.Ordinal))
        {
            var firstNewline = text.IndexOf('\n');
            if (firstNewline >= 0)
            {
                text = text[(firstNewline + 1)..];
            }
            if (text.EndsWith("```", StringComparison.Ordinal))
            {
                text = text[..^3];
            }
        }
        return text.Trim();
    }

    private static List<string> ReadStringArray(JsonElement root, string property)
    {
        var list = new List<string>();
        if (root.TryGetProperty(property, out var arr) && arr.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in arr.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String)
                {
                    var value = item.GetString();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        list.Add(value!.Trim());
                    }
                }
            }
        }
        return list;
    }

    private sealed record NarrativeResult(List<string> ExecutiveSummary, List<string> RecommendedActions);

    private sealed record TermAggregate(string Term, int Count, int RepresentativeResults, HashSet<string> Tokens);

    private sealed class IntentCluster
    {
        public List<TermAggregate> Terms { get; set; } = [];
        public HashSet<string> TopicTokens { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }
}
