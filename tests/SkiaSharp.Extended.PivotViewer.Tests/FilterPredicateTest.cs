using System;
using SkiaSharp.Extended.PivotViewer;
using Xunit;

namespace SkiaSharp.Extended.PivotViewer.Tests;

public class FilterPredicateTest
{
    private static PivotViewerItem CreateItem(string id, string propertyId, params object[] values)
    {
        var prop = new PivotViewerStringProperty(propertyId) { DisplayName = propertyId };
        var item = new PivotViewerItem(id);
        item.Set(prop, values);
        return item;
    }

    private static PivotViewerItem CreateNumericItem(string id, string propertyId, params object[] values)
    {
        var prop = new PivotViewerNumericProperty(propertyId) { DisplayName = propertyId };
        var item = new PivotViewerItem(id);
        item.Set(prop, values);
        return item;
    }

    private static PivotViewerItem CreateDateItem(string id, string propertyId, params object[] values)
    {
        var prop = new PivotViewerDateTimeProperty(propertyId) { DisplayName = propertyId };
        var item = new PivotViewerItem(id);
        item.Set(prop, values);
        return item;
    }

    // --- StringFilterPredicate ---

    [Fact]
    public void StringFilter_MatchesSingleValue()
    {
        var pred = new StringFilterPredicate("Color");
        pred.AddValue("Red");

        var item = CreateItem("1", "Color", "Red");

        Assert.True(pred.Matches(item));
    }

    [Fact]
    public void StringFilter_MatchesAnyOfMultipleValues()
    {
        var pred = new StringFilterPredicate("Color");
        pred.AddValue("Red");
        pred.AddValue("Blue");

        var redItem = CreateItem("1", "Color", "Red");
        var blueItem = CreateItem("2", "Color", "Blue");
        var greenItem = CreateItem("3", "Color", "Green");

        Assert.True(pred.Matches(redItem));
        Assert.True(pred.Matches(blueItem));
        Assert.False(pred.Matches(greenItem));
    }

    [Fact]
    public void StringFilter_NoMatchReturnsFalse()
    {
        var pred = new StringFilterPredicate("Color");
        pred.AddValue("Red");

        var item = CreateItem("1", "Color", "Green");

        Assert.False(pred.Matches(item));
    }

    [Fact]
    public void StringFilter_CaseInsensitive()
    {
        var pred = new StringFilterPredicate("Color");
        pred.AddValue("red");

        var item = CreateItem("1", "Color", "RED");

        Assert.True(pred.Matches(item));
    }

    [Fact]
    public void StringFilter_EmptyValuesMatchesAll()
    {
        var pred = new StringFilterPredicate("Color");

        var item = CreateItem("1", "Color", "Red");

        Assert.True(pred.Matches(item));
    }

    [Fact]
    public void StringFilter_MissingPropertyReturnsFalse()
    {
        var pred = new StringFilterPredicate("Color");
        pred.AddValue("Red");

        var item = CreateItem("1", "Shape", "Circle");

        Assert.False(pred.Matches(item));
    }

    [Fact]
    public void StringFilter_RemoveValueWorks()
    {
        var pred = new StringFilterPredicate("Color");
        pred.AddValue("Red");
        pred.AddValue("Blue");

        pred.RemoveValue("Red");

        Assert.Single(pred.Values);
        Assert.Contains("Blue", pred.Values);
    }

    // --- NumericRangeFilterPredicate ---

    [Fact]
    public void NumericFilter_MatchesWithinRange()
    {
        var pred = new NumericRangeFilterPredicate("Price", 10.0, 50.0);

        var item = CreateNumericItem("1", "Price", 25.0);

        Assert.True(pred.Matches(item));
    }

    [Fact]
    public void NumericFilter_MatchesAtLowerBoundary()
    {
        var pred = new NumericRangeFilterPredicate("Price", 10.0, 50.0);

        var item = CreateNumericItem("1", "Price", 10.0);

        Assert.True(pred.Matches(item));
    }

    [Fact]
    public void NumericFilter_MatchesAtUpperBoundary()
    {
        var pred = new NumericRangeFilterPredicate("Price", 10.0, 50.0);

        var item = CreateNumericItem("1", "Price", 50.0);

        Assert.True(pred.Matches(item));
    }

    [Fact]
    public void NumericFilter_OutsideRangeReturnsFalse()
    {
        var pred = new NumericRangeFilterPredicate("Price", 10.0, 50.0);

        var belowItem = CreateNumericItem("1", "Price", 5.0);
        var aboveItem = CreateNumericItem("2", "Price", 100.0);

        Assert.False(pred.Matches(belowItem));
        Assert.False(pred.Matches(aboveItem));
    }

    [Fact]
    public void NumericFilter_MissingPropertyReturnsFalse()
    {
        var pred = new NumericRangeFilterPredicate("Price", 10.0, 50.0);

        var item = CreateNumericItem("1", "Weight", 25.0);

        Assert.False(pred.Matches(item));
    }

    [Fact]
    public void NumericFilter_MatchesIntValues()
    {
        var pred = new NumericRangeFilterPredicate("Count", 0.0, 100.0);

        var prop = new PivotViewerNumericProperty("Count") { DisplayName = "Count" };
        var item = new PivotViewerItem("1");
        item.Set(prop, new object[] { 42 });

        Assert.True(pred.Matches(item));
    }

    // --- DateTimeRangeFilterPredicate ---

    [Fact]
    public void DateFilter_MatchesWithinRange()
    {
        var min = new DateTime(2020, 1, 1);
        var max = new DateTime(2020, 12, 31);
        var pred = new DateTimeRangeFilterPredicate("Created", min, max);

        var item = CreateDateItem("1", "Created", new DateTime(2020, 6, 15));

        Assert.True(pred.Matches(item));
    }

    [Fact]
    public void DateFilter_MatchesAtLowerBoundary()
    {
        var min = new DateTime(2020, 1, 1);
        var max = new DateTime(2020, 12, 31);
        var pred = new DateTimeRangeFilterPredicate("Created", min, max);

        var item = CreateDateItem("1", "Created", min);

        Assert.True(pred.Matches(item));
    }

    [Fact]
    public void DateFilter_MatchesAtUpperBoundary()
    {
        var min = new DateTime(2020, 1, 1);
        var max = new DateTime(2020, 12, 31);
        var pred = new DateTimeRangeFilterPredicate("Created", min, max);

        var item = CreateDateItem("1", "Created", max);

        Assert.True(pred.Matches(item));
    }

    [Fact]
    public void DateFilter_OutsideRangeReturnsFalse()
    {
        var min = new DateTime(2020, 1, 1);
        var max = new DateTime(2020, 12, 31);
        var pred = new DateTimeRangeFilterPredicate("Created", min, max);

        var beforeItem = CreateDateItem("1", "Created", new DateTime(2019, 6, 15));
        var afterItem = CreateDateItem("2", "Created", new DateTime(2021, 6, 15));

        Assert.False(pred.Matches(beforeItem));
        Assert.False(pred.Matches(afterItem));
    }

    [Fact]
    public void DateFilter_MissingPropertyReturnsFalse()
    {
        var pred = new DateTimeRangeFilterPredicate("Created",
            new DateTime(2020, 1, 1), new DateTime(2020, 12, 31));

        var item = CreateDateItem("1", "Updated", new DateTime(2020, 6, 15));

        Assert.False(pred.Matches(item));
    }
}
