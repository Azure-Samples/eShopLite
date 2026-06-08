using DataEntities;
using Microsoft.Extensions.Logging.Abstractions;
using Products.Intelligence;

namespace Products.Tests;

[TestClass]
public sealed class StoreIntelligenceTests
{
    [TestMethod]
    public void SignalStore_Seeds_Sample_Signals()
    {
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

        var recent = store.GetRecent(1);
        Assert.AreEqual(1, recent.Count);
        Assert.AreEqual("brand new term", recent[0].Term);
        Assert.IsTrue(recent[0].Failed);
    }

    [TestMethod]
    public void SignalStore_Ignores_Empty_Term()
    {
        var store = new StoreSignalStore();
        var before = store.GetAll().Count;
        store.Record("   ", semantic: false, resultCount: 3);
        Assert.AreEqual(before, store.GetAll().Count);
    }

    [TestMethod]
    public async Task ReportService_Without_ChatClient_Produces_Fallback_Report()
    {
        var store = new StoreSignalStore();
        store.Record("hiking boots size 12", semantic: false, resultCount: 0);
        store.Record("hiking boots size 12", semantic: false, resultCount: 0);
        store.Record("rain jacket", semantic: true, resultCount: 3);

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
            report.FailedSearches.Any(f => f.Contains("hiking boots size 12", System.StringComparison.OrdinalIgnoreCase)),
            "Repeated failed search should be reported.");
    }
}
