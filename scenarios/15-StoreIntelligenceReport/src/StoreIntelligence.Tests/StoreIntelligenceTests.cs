using DataEntities;
using Microsoft.Extensions.Logging.Abstractions;
using StoreIntelligence.Services;

namespace StoreIntelligence.Tests;

/// <summary>
/// Unit tests for the StoreSignalStore and StoreIntelligenceReportService that now live in
/// the StoreIntelligence project. These tests run without any AI model (chatClient = null)
/// so they work offline and in CI.
/// </summary>
[TestClass]
public sealed class StoreIntelligenceTests
{
    [TestMethod]
    public void SignalStore_Seeds_Sample_Signals()
    {
        // The store seeds deterministic demo signals at construction time so the report
        // is never empty before the presenter runs a search.
        var store = new StoreSignalStore();
        var all = store.GetAll();

        Assert.IsTrue(all.Count > 0, "Store should seed deterministic sample signals.");
        Assert.IsTrue(all.Any(s => s.Failed), "Seed data should include at least one failed search.");
    }

    [TestMethod]
    public void SignalStore_Records_New_Signal_As_Most_Recent()
    {
        var store = new StoreSignalStore();
        store.Record("brand new term", semantic: true, resultCount: 0);

        // GetRecent returns newest-first, so index [0] is the just-recorded signal.
        var recent = store.GetRecent(1);
        Assert.AreEqual(1, recent.Count);
        Assert.AreEqual("brand new term", recent[0].Term);
        Assert.IsTrue(recent[0].Failed, "resultCount=0 → Failed should be true");
    }

    [TestMethod]
    public void SignalStore_Ignores_Empty_Term()
    {
        var store = new StoreSignalStore();
        var before = store.GetAll().Count;

        store.Record("   ", semantic: false, resultCount: 3);

        Assert.AreEqual(before, store.GetAll().Count, "Blank term should be silently dropped.");
    }

    [TestMethod]
    public async Task ReportService_Without_ChatClient_Produces_Fallback_Report()
    {
        // Build a store with a mix of successful and failed searches.
        var store = new StoreSignalStore();
        store.Record("hiking boots size 12", semantic: false, resultCount: 0);
        store.Record("hiking boots size 12", semantic: false, resultCount: 0);
        store.Record("rain jacket", semantic: true, resultCount: 3);

        // No IChatClient — report service should fall back to deterministic narrative.
        var service = new StoreIntelligenceReportService(
            store,
            NullLogger<StoreIntelligenceReportService>.Instance,
            chatClient: null);

        StoreIntelligenceReport report = await service.GenerateAsync();

        Assert.AreEqual("fallback", report.Source);
        Assert.IsTrue(report.SignalsAnalyzed > 0);
        Assert.IsTrue(report.ExecutiveSummary.Count > 0);
        Assert.IsTrue(report.RecommendedActions.Count > 0);
        Assert.IsTrue(report.TopSearches.Count > 0);
        Assert.IsTrue(report.FailedSearches.Count > 0, "Failed searches should surface in the report.");
        Assert.IsTrue(
            report.FailedSearches.Any(f => f.Contains("hiking boots size 12", StringComparison.OrdinalIgnoreCase)),
            "Repeated failed search should appear in FailedSearches.");
    }

    [TestMethod]
    public async Task ReportService_Groups_Rain_Queries_Into_One_Intent()
    {
        var store = new StoreSignalStore();
        store.Record("do you have something for a rainy day", semantic: true, resultCount: 2);
        store.Record("rain jacket", semantic: true, resultCount: 3);
        store.Record("something for camping while it's raining", semantic: true, resultCount: 2);

        var service = new StoreIntelligenceReportService(
            store,
            NullLogger<StoreIntelligenceReportService>.Instance,
            chatClient: null);

        StoreIntelligenceReport report = await service.GenerateAsync();

        Assert.IsTrue(report.TopCustomerIntents.Count > 0, "Report should include grouped customer intents.");

        var rainIntent = report.TopCustomerIntents.FirstOrDefault(i =>
            i.Theme.Contains("rain", StringComparison.OrdinalIgnoreCase) ||
            i.Terms.Any(t => t.Term.Contains("rain", StringComparison.OrdinalIgnoreCase)));

        Assert.IsNotNull(rainIntent, "Expected a rain-related grouped intent.");
        Assert.IsTrue(rainIntent.TotalSearches >= 3, "Rain intent should aggregate repeated rain searches.");
        Assert.IsTrue(
            rainIntent.Terms.Any(t => t.Term.Contains("rain jacket", StringComparison.OrdinalIgnoreCase)) &&
            rainIntent.Terms.Any(t => t.Term.Contains("rainy day", StringComparison.OrdinalIgnoreCase)),
            "Rain intent should include multiple differently-worded rain searches.");
    }
}
