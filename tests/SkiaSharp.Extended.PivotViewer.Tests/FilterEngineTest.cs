using SkiaSharp.Extended.PivotViewer;
using Xunit;

namespace SkiaSharp.Extended.PivotViewer.Tests;

public class FilterEngineTest
{
    private static (FilterEngine engine, List<PivotViewerItem> items, List<PivotViewerProperty> props) CreateTestData()
    {
        var nameP = new PivotViewerStringProperty("Name") { DisplayName = "Name" };
        var yearP = new PivotViewerNumericProperty("Year") { DisplayName = "Year" };
        var catP = new PivotViewerStringProperty("Category") { DisplayName = "Category" };

        var items = new List<PivotViewerItem>();

        var i1 = new PivotViewerItem("1");
        i1.Set(nameP, new object[] { "Ford GT" });
        i1.Set(yearP, new object[] { 2005.0 });
        i1.Set(catP, new object[] { "Sports" });
        items.Add(i1);

        var i2 = new PivotViewerItem("2");
        i2.Set(nameP, new object[] { "Ferrari Enzo" });
        i2.Set(yearP, new object[] { 2002.0 });
        i2.Set(catP, new object[] { "Sports" });
        items.Add(i2);

        var i3 = new PivotViewerItem("3");
        i3.Set(nameP, new object[] { "Toyota Prius" });
        i3.Set(yearP, new object[] { 2001.0 });
        i3.Set(catP, new object[] { "Hybrid" });
        items.Add(i3);

        var i4 = new PivotViewerItem("4");
        i4.Set(nameP, new object[] { "Tesla Model S" });
        i4.Set(yearP, new object[] { 2012.0 });
        i4.Set(catP, new object[] { "Electric" });
        items.Add(i4);

        var props = new List<PivotViewerProperty> { nameP, yearP, catP };
        var engine = new FilterEngine();
        engine.SetSource(items, props);

        return (engine, items, props);
    }

    [Fact]
    public void NoFilters_ReturnsAllItems()
    {
        var (engine, items, _) = CreateTestData();
        var result = engine.GetFilteredItems();
        Assert.Equal(4, result.Count);
    }

    [Fact]
    public void StringFilter_FiltersBySingleValue()
    {
        var (engine, _, _) = CreateTestData();
        engine.AddStringFilter("Category", "Sports");
        var result = engine.GetFilteredItems();
        Assert.Equal(2, result.Count);
        Assert.All(result, item => Assert.Equal("Sports", item["Category"]![0]!.ToString()));
    }

    [Fact]
    public void StringFilter_MultipleValues_UsesOrLogic()
    {
        var (engine, _, _) = CreateTestData();
        engine.AddStringFilter("Category", "Sports");
        engine.AddStringFilter("Category", "Hybrid");
        var result = engine.GetFilteredItems();
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void MultipleProperties_UseAndLogic()
    {
        var (engine, _, _) = CreateTestData();
        engine.AddStringFilter("Category", "Sports");
        engine.AddNumericRangeFilter("Year", 2003, 2010);
        var result = engine.GetFilteredItems();
        Assert.Single(result);
        Assert.Equal("Ford GT", result[0]["Name"]![0]!.ToString());
    }

    [Fact]
    public void NumericRangeFilter_InclRange()
    {
        var (engine, _, _) = CreateTestData();
        engine.AddNumericRangeFilter("Year", 2001, 2002);
        var result = engine.GetFilteredItems();
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void RemoveStringFilter_RemovesSpecificValue()
    {
        var (engine, _, _) = CreateTestData();
        engine.AddStringFilter("Category", "Sports");
        engine.AddStringFilter("Category", "Hybrid");
        Assert.Equal(3, engine.GetFilteredItems().Count);

        engine.RemoveStringFilter("Category", "Sports");
        var result = engine.GetFilteredItems();
        Assert.Single(result);
        Assert.Equal("Hybrid", result[0]["Category"]![0]!.ToString());
    }

    [Fact]
    public void RemoveFilter_RemovesAllForProperty()
    {
        var (engine, _, _) = CreateTestData();
        engine.AddStringFilter("Category", "Sports");
        engine.AddNumericRangeFilter("Year", 2005, 2010);
        Assert.Single(engine.GetFilteredItems());

        engine.RemoveFilter("Year");
        Assert.Equal(2, engine.GetFilteredItems().Count);
    }

    [Fact]
    public void ClearAll_RemovesAllFilters()
    {
        var (engine, _, _) = CreateTestData();
        engine.AddStringFilter("Category", "Sports");
        engine.AddNumericRangeFilter("Year", 2005, 2010);
        Assert.Single(engine.GetFilteredItems());

        engine.ClearAll();
        Assert.Equal(4, engine.GetFilteredItems().Count);
    }

    [Fact]
    public void FiltersChanged_EventFired()
    {
        var (engine, _, _) = CreateTestData();
        int firedCount = 0;
        engine.FiltersChanged += (s, e) => firedCount++;

        engine.AddStringFilter("Category", "Sports");
        Assert.Equal(1, firedCount);

        engine.RemoveStringFilter("Category", "Sports");
        Assert.Equal(2, firedCount);

        engine.ClearAll();
        Assert.Equal(3, firedCount);
    }

    [Fact]
    public void ComputeInScopeCounts_WithNoFilters_CountsAllItems()
    {
        var (engine, _, _) = CreateTestData();
        var counts = engine.ComputeInScopeCounts("Category");

        Assert.Equal(2, counts["Sports"]);
        Assert.Equal(1, counts["Hybrid"]);
        Assert.Equal(1, counts["Electric"]);
    }

    [Fact]
    public void ComputeInScopeCounts_WithOtherFilter_ShowsReducedCounts()
    {
        var (engine, _, _) = CreateTestData();
        engine.AddNumericRangeFilter("Year", 2000, 2005);

        var counts = engine.ComputeInScopeCounts("Category");

        Assert.Equal(2, counts["Sports"]);
        Assert.Equal(1, counts["Hybrid"]);
        Assert.False(counts.ContainsKey("Electric")); // Tesla 2012 is filtered out
    }

    [Fact]
    public void DateTimeRangeFilter_Works()
    {
        var dateP = new PivotViewerDateTimeProperty("Date") { DisplayName = "Date" };
        var items = new List<PivotViewerItem>();

        var i1 = new PivotViewerItem("1");
        i1.Set(dateP, new object[] { new DateTime(2020, 1, 1) });
        items.Add(i1);

        var i2 = new PivotViewerItem("2");
        i2.Set(dateP, new object[] { new DateTime(2021, 6, 15) });
        items.Add(i2);

        var i3 = new PivotViewerItem("3");
        i3.Set(dateP, new object[] { new DateTime(2023, 12, 31) });
        items.Add(i3);

        var engine = new FilterEngine();
        engine.SetSource(items, new[] { dateP });
        engine.AddDateTimeRangeFilter("Date", new DateTime(2020, 1, 1), new DateTime(2022, 1, 1));

        var result = engine.GetFilteredItems();
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void ComputeNumericHistogram_CreatesReasonableBuckets()
    {
        var (engine, items, _) = CreateTestData();
        var buckets = engine.ComputeNumericHistogram("Year", items);

        Assert.NotEmpty(buckets);
        Assert.True(buckets.Count <= 15, "Should not exceed max bucket count");

        int totalCount = buckets.Sum(b => b.Count);
        Assert.True(totalCount >= items.Count, "All items should be in buckets");
    }

    [Fact]
    public void StringFilter_CaseInsensitive()
    {
        var (engine, _, _) = CreateTestData();
        engine.AddStringFilter("Category", "sports"); // lowercase
        var result = engine.GetFilteredItems();
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Filter_NoMatchingItems_ReturnsEmpty()
    {
        var (engine, _, _) = CreateTestData();
        engine.AddStringFilter("Category", "Truck");
        var result = engine.GetFilteredItems();
        Assert.Empty(result);
    }

    [Fact]
    public void Filter_MissingProperty_ReturnsEmpty()
    {
        var (engine, _, _) = CreateTestData();
        engine.AddStringFilter("NonExistent", "anything");
        var result = engine.GetFilteredItems();
        Assert.Empty(result);
    }
}
