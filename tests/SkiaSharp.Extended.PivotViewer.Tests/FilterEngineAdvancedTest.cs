using SkiaSharp.Extended.PivotViewer;
using Xunit;

namespace SkiaSharp.Extended.PivotViewer.Tests;

/// <summary>
/// Comprehensive tests for FilterEngine — all filter types and combinations.
/// </summary>
public class FilterEngineAdvancedTest
{
    [Fact]
    public void NumericRangeFilter_FiltersCorrectly()
    {
        var engine = CreateEngineWithNumericData();
        engine.AddNumericRangeFilter("Price", 20.0, 50.0);

        var filtered = engine.GetFilteredItems();
        Assert.True(filtered.Count > 0);
        Assert.True(filtered.Count < 10);

        foreach (var item in filtered)
        {
            var values = item["Price"];
            Assert.NotNull(values);
            var price = Convert.ToDouble(values[0]);
            Assert.InRange(price, 20.0, 50.0);
        }
    }

    [Fact]
    public void NumericRangeFilter_Replace_UpdatesRange()
    {
        var engine = CreateEngineWithNumericData();
        engine.AddNumericRangeFilter("Price", 10.0, 30.0);
        var count1 = engine.GetFilteredItems().Count;

        engine.AddNumericRangeFilter("Price", 10.0, 90.0);
        var count2 = engine.GetFilteredItems().Count;

        Assert.True(count2 >= count1, "Wider range should match more items");
    }

    [Fact]
    public void DateTimeRangeFilter_FiltersCorrectly()
    {
        var engine = CreateEngineWithDateData();
        var min = new DateTime(2020, 1, 1);
        var max = new DateTime(2022, 12, 31);
        engine.AddDateTimeRangeFilter("Date", min, max);

        var filtered = engine.GetFilteredItems();
        Assert.True(filtered.Count > 0);

        foreach (var item in filtered)
        {
            var values = item["Date"];
            Assert.NotNull(values);
            var date = (DateTime)values[0];
            Assert.InRange(date, min, max);
        }
    }

    [Fact]
    public void CombinedFilters_StringAndNumeric_AND()
    {
        var engine = CreateEngineWithMixedData();

        engine.AddStringFilter("Category", "A");
        var afterString = engine.GetFilteredItems().Count;

        engine.AddNumericRangeFilter("Score", 5.0, 10.0);
        var afterBoth = engine.GetFilteredItems().Count;

        Assert.True(afterBoth <= afterString, "AND logic: combined should be ≤ string-only");
    }

    [Fact]
    public void StringFilter_MultipleValues_OR()
    {
        var engine = CreateEngineWithMixedData();

        engine.AddStringFilter("Category", "A");
        var countA = engine.GetFilteredItems().Count;

        engine.AddStringFilter("Category", "B");
        var countAB = engine.GetFilteredItems().Count;

        Assert.True(countAB >= countA, "OR within category: A+B should be ≥ A alone");
    }

    [Fact]
    public void StringFilter_MultipleCategories_AND()
    {
        var engine = CreateEngineWithMixedData();

        engine.AddStringFilter("Category", "A");
        var afterCategory = engine.GetFilteredItems().Count;

        engine.AddStringFilter("Tag", "x");
        var afterBoth = engine.GetFilteredItems().Count;

        Assert.True(afterBoth <= afterCategory, "AND across categories: combined should be ≤ single");
    }

    [Fact]
    public void RemoveFilter_RestoresAll()
    {
        var engine = CreateEngineWithMixedData();
        var allCount = engine.GetFilteredItems().Count;

        engine.AddStringFilter("Category", "A");
        engine.RemoveFilter("Category");

        Assert.Equal(allCount, engine.GetFilteredItems().Count);
    }

    [Fact]
    public void ClearAll_RestoresAll()
    {
        var engine = CreateEngineWithMixedData();
        var allCount = engine.GetFilteredItems().Count;

        engine.AddStringFilter("Category", "A");
        engine.AddNumericRangeFilter("Score", 5, 10);
        engine.ClearAll();

        Assert.Equal(allCount, engine.GetFilteredItems().Count);
    }

    [Fact]
    public void HasActiveFilters_TracksProperly()
    {
        var engine = CreateEngineWithMixedData();
        Assert.False(engine.HasActiveFilters);

        engine.AddStringFilter("Category", "A");
        Assert.True(engine.HasActiveFilters);

        engine.ClearAll();
        Assert.False(engine.HasActiveFilters);
    }

    [Fact]
    public void HasStringFilter_TracksValues()
    {
        var engine = CreateEngineWithMixedData();

        engine.AddStringFilter("Category", "A");
        Assert.True(engine.HasStringFilter("Category", "A"));
        Assert.False(engine.HasStringFilter("Category", "B"));

        engine.RemoveStringFilter("Category", "A");
        Assert.False(engine.HasStringFilter("Category", "A"));
    }

    [Fact]
    public void GetActiveFilters_ReturnsValues()
    {
        var engine = CreateEngineWithMixedData();

        engine.AddStringFilter("Category", "A");
        engine.AddStringFilter("Category", "B");

        var active = engine.GetActiveFilters("Category");
        Assert.Contains("A", active);
        Assert.Contains("B", active);

        var empty = engine.GetActiveFilters("Nonexistent");
        Assert.Empty(empty);
    }

    [Fact]
    public void InScopeCounts_ExcludesSameCategory()
    {
        var engine = CreateEngineWithMixedData();

        // Count for Category when no filters active
        var countsAll = engine.ComputeInScopeCounts("Category");
        Assert.True(countsAll.Count > 0);

        // Now filter on Tag — should reduce counts for Category
        engine.AddStringFilter("Tag", "x");
        var countsFiltered = engine.ComputeInScopeCounts("Category");

        // Tag filter should reduce available items for Category counts
        int totalAll = countsAll.Values.Sum();
        int totalFiltered = countsFiltered.Values.Sum();
        Assert.True(totalFiltered <= totalAll);
    }

    [Fact]
    public void FiltersChanged_EventFires()
    {
        var engine = CreateEngineWithMixedData();
        int fireCount = 0;
        engine.FiltersChanged += (s, e) => fireCount++;

        engine.AddStringFilter("Category", "A");
        engine.AddNumericRangeFilter("Score", 1, 10);
        engine.RemoveStringFilter("Category", "A");
        engine.ClearAll();

        Assert.Equal(4, fireCount);
    }

    [Fact]
    public void EmptyResult_NoException()
    {
        var engine = CreateEngineWithMixedData();
        engine.AddStringFilter("Category", "NonExistent");
        var filtered = engine.GetFilteredItems();
        Assert.Empty(filtered);
    }

    [Fact]
    public void DuplicateFilter_NoDoubleCount()
    {
        var engine = CreateEngineWithMixedData();

        engine.AddStringFilter("Category", "A");
        var count1 = engine.GetFilteredItems().Count;

        // Adding same value again shouldn't change result
        engine.AddStringFilter("Category", "A");
        var count2 = engine.GetFilteredItems().Count;

        Assert.Equal(count1, count2);
    }

    // --- Helper data builders ---

    private static FilterEngine CreateEngineWithNumericData()
    {
        var priceProp = new PivotViewerNumericProperty("Price") { DisplayName = "Price" };
        var items = new List<PivotViewerItem>();
        for (int i = 0; i < 10; i++)
        {
            var item = new PivotViewerItem(i.ToString());
            item.Set(priceProp, new object[] { (double)(i * 10) });
            items.Add(item);
        }
        var engine = new FilterEngine();
        engine.SetSource(items, new PivotViewerProperty[] { priceProp });
        return engine;
    }

    private static FilterEngine CreateEngineWithDateData()
    {
        var dateProp = new PivotViewerDateTimeProperty("Date") { DisplayName = "Date" };
        var items = new List<PivotViewerItem>();
        for (int i = 0; i < 10; i++)
        {
            var item = new PivotViewerItem(i.ToString());
            item.Set(dateProp, new object[] { new DateTime(2018 + i, 6, 15) });
            items.Add(item);
        }
        var engine = new FilterEngine();
        engine.SetSource(items, new PivotViewerProperty[] { dateProp });
        return engine;
    }

    private static FilterEngine CreateEngineWithMixedData()
    {
        var catProp = new PivotViewerStringProperty("Category") { DisplayName = "Category" };
        var tagProp = new PivotViewerStringProperty("Tag") { DisplayName = "Tag" };
        var scoreProp = new PivotViewerNumericProperty("Score") { DisplayName = "Score" };

        var items = new List<PivotViewerItem>();
        string[] categories = { "A", "B", "C" };
        string[] tags = { "x", "y", "z" };

        for (int i = 0; i < 30; i++)
        {
            var item = new PivotViewerItem(i.ToString());
            item.Set(catProp, new object[] { categories[i % 3] });
            item.Set(tagProp, new object[] { tags[i % 3] });
            item.Set(scoreProp, new object[] { (double)(i % 10) });
            items.Add(item);
        }

        var engine = new FilterEngine();
        engine.SetSource(items, new PivotViewerProperty[] { catProp, tagProp, scoreProp });
        return engine;
    }
}
