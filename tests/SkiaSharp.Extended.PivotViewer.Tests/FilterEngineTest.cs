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

    [Fact]
    public void CombinedFilters_StringAndNumeric()
    {
        var engine = new FilterEngine();
        var strProp = new PivotViewerStringProperty("Color") { DisplayName = "Color" };
        var numProp = new PivotViewerNumericProperty("Year") { DisplayName = "Year" };

        var item1 = new PivotViewerItem("1");
        item1.Set(strProp, new object[] { "Red" });
        item1.Set(numProp, new object[] { 2020.0 });

        var item2 = new PivotViewerItem("2");
        item2.Set(strProp, new object[] { "Blue" });
        item2.Set(numProp, new object[] { 2022.0 });

        var item3 = new PivotViewerItem("3");
        item3.Set(strProp, new object[] { "Red" });
        item3.Set(numProp, new object[] { 2023.0 });

        engine.SetSource(new[] { item1, item2, item3 }, new PivotViewerProperty[] { strProp, numProp });

        engine.AddStringFilter("Color", "Red");
        engine.AddNumericRangeFilter("Year", 2021, 2025);

        var filtered = engine.GetFilteredItems();
        Assert.Single(filtered);
        Assert.Equal("3", filtered[0].Id);
    }

    [Fact]
    public void CombinedFilters_MultipleStringProperties()
    {
        var engine = new FilterEngine();
        var colorProp = new PivotViewerStringProperty("Color") { DisplayName = "Color" };
        var sizeProp = new PivotViewerStringProperty("Size") { DisplayName = "Size" };

        var item1 = new PivotViewerItem("1");
        item1.Set(colorProp, new object[] { "Red" });
        item1.Set(sizeProp, new object[] { "Large" });

        var item2 = new PivotViewerItem("2");
        item2.Set(colorProp, new object[] { "Red" });
        item2.Set(sizeProp, new object[] { "Small" });

        engine.SetSource(new[] { item1, item2 }, new PivotViewerProperty[] { colorProp, sizeProp });

        engine.AddStringFilter("Color", "Red");
        engine.AddStringFilter("Size", "Large");

        var filtered = engine.GetFilteredItems();
        Assert.Single(filtered);
        Assert.Equal("1", filtered[0].Id);
    }

    [Fact]
    public void NumericRangeFilter_ExactBounds_IncludesBothEnds()
    {
        var engine = new FilterEngine();
        var numProp = new PivotViewerNumericProperty("Value") { DisplayName = "Value" };

        var item1 = new PivotViewerItem("1");
        item1.Set(numProp, new object[] { 10.0 });
        var item2 = new PivotViewerItem("2");
        item2.Set(numProp, new object[] { 20.0 });
        var item3 = new PivotViewerItem("3");
        item3.Set(numProp, new object[] { 30.0 });

        engine.SetSource(new[] { item1, item2, item3 }, new PivotViewerProperty[] { numProp });
        engine.AddNumericRangeFilter("Value", 10, 20);

        var filtered = engine.GetFilteredItems();
        Assert.Equal(2, filtered.Count);
    }

    [Fact]
    public void ClearAll_AfterCombinedFilters_RestoresAll()
    {
        var engine = new FilterEngine();
        var strProp = new PivotViewerStringProperty("Color") { DisplayName = "Color" };
        var numProp = new PivotViewerNumericProperty("Year") { DisplayName = "Year" };

        var item1 = new PivotViewerItem("1");
        item1.Set(strProp, new object[] { "Red" });
        item1.Set(numProp, new object[] { 2020.0 });
        var item2 = new PivotViewerItem("2");
        item2.Set(strProp, new object[] { "Blue" });
        item2.Set(numProp, new object[] { 2022.0 });

        engine.SetSource(new[] { item1, item2 }, new PivotViewerProperty[] { strProp, numProp });
        engine.AddStringFilter("Color", "Red");
        engine.AddNumericRangeFilter("Year", 2021, 2025);
        Assert.Empty(engine.GetFilteredItems());

        engine.ClearAll();
        Assert.Equal(2, engine.GetFilteredItems().Count);
    }

    // --- Tests for previously untested public methods ---

    [Fact]
    public void RemoveAllFilters_ClearsPropertyFilters()
    {
        var (engine, items, _) = CreateTestData();
        engine.AddStringFilter("Category", "Sports");
        Assert.Equal(2, engine.GetFilteredItems().Count);

        engine.RemoveAllFilters("Category");
        Assert.Equal(4, engine.GetFilteredItems().Count);
    }

    [Fact]
    public void RemoveAllFilters_LeavesOtherPropertyFiltersIntact()
    {
        var (engine, items, _) = CreateTestData();
        engine.AddStringFilter("Category", "Sports");
        engine.AddNumericRangeFilter("Year", 2000, 2003);

        engine.RemoveAllFilters("Category");

        // Year filter still active: 2001, 2002 pass
        var filtered = engine.GetFilteredItems();
        Assert.Equal(2, filtered.Count);
        Assert.All(filtered, i => Assert.True(
            ((double)(i["Year"]![0]!)) <= 2003));
    }

    [Fact]
    public void RemoveAllFilters_NonexistentProperty_NoOp()
    {
        var (engine, items, _) = CreateTestData();
        engine.AddStringFilter("Category", "Sports");

        engine.RemoveAllFilters("Nonexistent");

        // Original filter still active
        Assert.Equal(2, engine.GetFilteredItems().Count);
    }

    [Fact]
    public void GetActiveFilters_AfterRemoveAllFilters_ReturnsEmpty()
    {
        var (engine, items, _) = CreateTestData();
        engine.AddStringFilter("Category", "Sports");
        engine.AddStringFilter("Category", "Electric");
        Assert.Equal(2, engine.GetActiveFilters("Category").Count);

        engine.RemoveAllFilters("Category");
        Assert.Empty(engine.GetActiveFilters("Category"));
    }

    [Fact]
    public void ComputeNumericHistogram_SingleValue_ReturnsSingleBucket()
    {
        var prop = new PivotViewerNumericProperty("Val") { DisplayName = "Val" };
        var items = new List<PivotViewerItem>();
        for (int j = 0; j < 5; j++)
        {
            var item = new PivotViewerItem(j.ToString());
            item.Set(prop, new object[] { 42.0 });
            items.Add(item);
        }
        var engine = new FilterEngine();
        engine.SetSource(items, new[] { prop });

        var buckets = engine.ComputeNumericHistogram("Val", items);
        Assert.Single(buckets);
        Assert.Equal(5, buckets[0].Count);
    }

    [Fact]
    public void ComputeNumericHistogram_EmptyItems_ReturnsEmpty()
    {
        var prop = new PivotViewerNumericProperty("Val") { DisplayName = "Val" };
        var engine = new FilterEngine();
        engine.SetSource(Array.Empty<PivotViewerItem>(), new[] { prop });

        var buckets = engine.ComputeNumericHistogram("Val", Array.Empty<PivotViewerItem>().ToList());
        Assert.Empty(buckets);
    }

    [Fact]
    public void StringFilterPredicate_RemoveValue_NarrowsFilter()
    {
        var pred = new StringFilterPredicate("Category");
        pred.AddValue("Sports");
        pred.AddValue("Electric");
        Assert.Equal(2, pred.Values.Count);

        pred.RemoveValue("Sports");
        Assert.Single(pred.Values);
        Assert.Contains("Electric", pred.Values);
    }

    [Fact]
    public void StringFilterPredicate_RemoveValue_NonexistentIsNoOp()
    {
        var pred = new StringFilterPredicate("Category");
        pred.AddValue("Sports");

        pred.RemoveValue("Nonexistent");
        Assert.Single(pred.Values);
    }

    [Fact]
    public void StringFilterPredicate_Matches_EmptyValues_MatchesAll()
    {
        var pred = new StringFilterPredicate("Category");
        var item = new PivotViewerItem("1");
        item.Set(new PivotViewerStringProperty("Category"), new object[] { "Sports" });

        Assert.True(pred.Matches(item));
    }

    [Fact]
    public void StringFilterPredicate_Matches_NullItemValues_ReturnsFalse()
    {
        var pred = new StringFilterPredicate("Category");
        pred.AddValue("Sports");
        var item = new PivotViewerItem("1");

        Assert.False(pred.Matches(item));
    }

    [Fact]
    public void NumericRangeFilterPredicate_Matches_InRange()
    {
        var pred = new NumericRangeFilterPredicate("Year", 2000, 2010);
        var item = new PivotViewerItem("1");
        item.Set(new PivotViewerNumericProperty("Year"), new object[] { 2005.0 });

        Assert.True(pred.Matches(item));
    }

    [Fact]
    public void NumericRangeFilterPredicate_Matches_OutOfRange()
    {
        var pred = new NumericRangeFilterPredicate("Year", 2000, 2010);
        var item = new PivotViewerItem("1");
        item.Set(new PivotViewerNumericProperty("Year"), new object[] { 2015.0 });

        Assert.False(pred.Matches(item));
    }

    [Fact]
    public void NumericRangeFilterPredicate_Matches_BoundaryValues()
    {
        var pred = new NumericRangeFilterPredicate("Val", 10.0, 20.0);
        var itemMin = new PivotViewerItem("1");
        itemMin.Set(new PivotViewerNumericProperty("Val"), new object[] { 10.0 });
        var itemMax = new PivotViewerItem("2");
        itemMax.Set(new PivotViewerNumericProperty("Val"), new object[] { 20.0 });

        Assert.True(pred.Matches(itemMin));
        Assert.True(pred.Matches(itemMax));
    }

    [Fact]
    public void DateTimeRangeFilterPredicate_Matches_InRange()
    {
        var pred = new DateTimeRangeFilterPredicate("Date",
            new DateTime(2023, 1, 1), new DateTime(2023, 12, 31));
        var item = new PivotViewerItem("1");
        item.Set(new PivotViewerDateTimeProperty("Date"), new object[] { new DateTime(2023, 6, 15) });

        Assert.True(pred.Matches(item));
    }

    [Fact]
    public void DateTimeRangeFilterPredicate_Matches_OutOfRange()
    {
        var pred = new DateTimeRangeFilterPredicate("Date",
            new DateTime(2023, 1, 1), new DateTime(2023, 12, 31));
        var item = new PivotViewerItem("1");
        item.Set(new PivotViewerDateTimeProperty("Date"), new object[] { new DateTime(2024, 6, 15) });

        Assert.False(pred.Matches(item));
    }

    [Fact]
    public void HistogramBucket_StoresProperties()
    {
        var bucket = new HistogramBucket("10–20", 10.0, 20.0, 5);
        Assert.Equal("10–20", bucket.Label);
        Assert.Equal(10.0, bucket.Min);
        Assert.Equal(20.0, bucket.Max);
        Assert.Equal(5, bucket.Count);
    }

    [Fact]
    public void AddNumericRangeFilter_MinGreaterThanMax_MatchesNothing()
    {
        var (engine, _, _) = CreateTestData();
        engine.AddNumericRangeFilter("Year", 2020, 2000); // min > max
        var result = engine.GetFilteredItems();
        Assert.Empty(result);
    }

    [Fact]
    public void AddDateTimeRangeFilter_MinGreaterThanMax_MatchesNothing()
    {
        var dateP = new PivotViewerDateTimeProperty("Date") { DisplayName = "Date" };
        var items = new List<PivotViewerItem>();

        var i1 = new PivotViewerItem("1");
        i1.Set(dateP, new object[] { new DateTime(2021, 6, 15) });
        items.Add(i1);

        var engine = new FilterEngine();
        engine.SetSource(items, new[] { dateP });
        engine.AddDateTimeRangeFilter("Date", new DateTime(2023, 1, 1), new DateTime(2020, 1, 1)); // min > max

        var result = engine.GetFilteredItems();
        Assert.Empty(result);
    }

    [Fact]
    public void ComputeNumericHistogram_ReturnsCorrectBuckets()
    {
        var prop = new PivotViewerNumericProperty("Score") { DisplayName = "Score" };
        var items = new List<PivotViewerItem>();

        // Create items with known numeric values: 10, 20, 30, 40, 50
        foreach (var val in new[] { 10.0, 20.0, 30.0, 40.0, 50.0 })
        {
            var item = new PivotViewerItem(val.ToString());
            item.Set(prop, new object[] { val });
            items.Add(item);
        }

        var engine = new FilterEngine();
        engine.SetSource(items, new[] { prop });

        var buckets = engine.ComputeNumericHistogram("Score", items);

        Assert.NotEmpty(buckets);

        // Bucket ranges should cover all values
        Assert.True(buckets[0].Min <= 10.0, "First bucket min should cover smallest value");
        Assert.True(buckets[buckets.Count - 1].Max >= 50.0, "Last bucket max should cover largest value");

        // Total count across all buckets should equal item count
        int totalCount = buckets.Sum(b => b.Count);
        Assert.Equal(5, totalCount);

        // Each bucket should have valid range: Min < Max
        Assert.All(buckets, b => Assert.True(b.Min <= b.Max));
    }

    [Fact]
    public void StringFilterPredicate_Matches_ExactAndPartial()
    {
        var pred = new StringFilterPredicate("Category");
        pred.AddValue("Sports");

        // Exact match
        var item1 = new PivotViewerItem("1");
        item1.Set(new PivotViewerStringProperty("Category"), new object[] { "Sports" });
        Assert.True(pred.Matches(item1));

        // Case-insensitive match
        var item2 = new PivotViewerItem("2");
        item2.Set(new PivotViewerStringProperty("Category"), new object[] { "sports" });
        Assert.True(pred.Matches(item2));

        // No match
        var item3 = new PivotViewerItem("3");
        item3.Set(new PivotViewerStringProperty("Category"), new object[] { "Hybrid" });
        Assert.False(pred.Matches(item3));

        // Multi-value item: matches if any value matches
        var item4 = new PivotViewerItem("4");
        item4.Set(new PivotViewerStringProperty("Category"), new object[] { "Hybrid", "Sports" });
        Assert.True(pred.Matches(item4));
    }
}
