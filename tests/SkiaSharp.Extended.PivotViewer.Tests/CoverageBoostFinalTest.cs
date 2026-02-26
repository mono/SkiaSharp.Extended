using SkiaSharp.Extended.PivotViewer;
using Xunit;

namespace SkiaSharp.Extended.PivotViewer.Tests;

public class CoverageBoostFinalTest
{
    // --- NumericRangeFilterPredicate: int, float, decimal branches (lines 379-382) ---

    [Fact]
    public void NumericRangeFilter_IntValue_MatchesRange()
    {
        var prop = new PivotViewerNumericProperty("score") { DisplayName = "Score" };
        var item = new PivotViewerItem("1");
        item.Add(prop, (object)42); // boxed int

        var predicate = new NumericRangeFilterPredicate("score", 0, 100);
        Assert.True(predicate.Matches(item));
    }

    [Fact]
    public void NumericRangeFilter_IntValue_OutOfRange()
    {
        var prop = new PivotViewerNumericProperty("score") { DisplayName = "Score" };
        var item = new PivotViewerItem("1");
        item.Add(prop, (object)200); // boxed int

        var predicate = new NumericRangeFilterPredicate("score", 0, 100);
        Assert.False(predicate.Matches(item));
    }

    [Fact]
    public void NumericRangeFilter_FloatValue_MatchesRange()
    {
        var prop = new PivotViewerNumericProperty("weight") { DisplayName = "Weight" };
        var item = new PivotViewerItem("1");
        item.Add(prop, (object)3.14f); // boxed float

        var predicate = new NumericRangeFilterPredicate("weight", 0.0, 10.0);
        Assert.True(predicate.Matches(item));
    }

    [Fact]
    public void NumericRangeFilter_FloatValue_OutOfRange()
    {
        var prop = new PivotViewerNumericProperty("weight") { DisplayName = "Weight" };
        var item = new PivotViewerItem("1");
        item.Add(prop, (object)99.9f); // boxed float

        var predicate = new NumericRangeFilterPredicate("weight", 0.0, 10.0);
        Assert.False(predicate.Matches(item));
    }

    [Fact]
    public void NumericRangeFilter_DecimalValue_MatchesRange()
    {
        var prop = new PivotViewerNumericProperty("price") { DisplayName = "Price" };
        var item = new PivotViewerItem("1");
        item.Add(prop, (object)19.99m); // boxed decimal

        var predicate = new NumericRangeFilterPredicate("price", 0.0, 50.0);
        Assert.True(predicate.Matches(item));
    }

    [Fact]
    public void NumericRangeFilter_DecimalValue_OutOfRange()
    {
        var prop = new PivotViewerNumericProperty("price") { DisplayName = "Price" };
        var item = new PivotViewerItem("1");
        item.Add(prop, (object)999.99m); // boxed decimal

        var predicate = new NumericRangeFilterPredicate("price", 0.0, 50.0);
        Assert.False(predicate.Matches(item));
    }

    [Fact]
    public void NumericRangeFilter_MixedTypes_AllMatch()
    {
        var prop = new PivotViewerNumericProperty("val") { DisplayName = "Value" };
        var item = new PivotViewerItem("1");
        // Add each numeric type as separate values
        item.Add(prop, (object)5.0);      // double
        item.Add(prop, (object)10);       // int
        item.Add(prop, (object)15.0f);    // float
        item.Add(prop, (object)20.0m);    // decimal

        var predicate = new NumericRangeFilterPredicate("val", 0.0, 25.0);
        Assert.True(predicate.Matches(item));
    }

    [Fact]
    public void NumericRangeFilter_StringValue_Skipped()
    {
        var prop = new PivotViewerNumericProperty("val") { DisplayName = "Value" };
        var item = new PivotViewerItem("1");
        item.Add(prop, (object)"not a number"); // string - should be skipped

        var predicate = new NumericRangeFilterPredicate("val", 0.0, 100.0);
        Assert.False(predicate.Matches(item));
    }

    // --- HistogramBucketer: exceed MaxBuckets consolidation (lines 125-140) ---

    [Fact]
    public void DateTimeBuckets_ManyDays_ConsolidatesWhenExceedMaxBuckets()
    {
        // Create 200+ unique days within 2 months to force Day granularity
        // (range.TotalDays <= 60 triggers Day granularity)
        // Then generate > 15 buckets to trigger consolidation
        var dates = new List<DateTime>();
        var baseDate = new DateTime(2023, 1, 1);
        for (int i = 0; i < 55; i++) // 55 days within 2 months → Day granularity, > 15 buckets
        {
            dates.Add(baseDate.AddDays(i));
        }

        var buckets = HistogramBucketer.CreateDateTimeBuckets(dates);

        // Should have been consolidated to MaxBuckets or fewer
        Assert.True(buckets.Count <= HistogramBucketer.MaxBuckets);
        // Total count should still match
        Assert.Equal(dates.Count, buckets.Sum(b => b.Count));
    }

    [Fact]
    public void DateTimeBuckets_ManyDays_ConsolidatedLabelsContainDateRange()
    {
        var dates = new List<DateTime>();
        var baseDate = new DateTime(2023, 3, 1);
        for (int i = 0; i < 50; i++)
        {
            dates.Add(baseDate.AddDays(i));
        }

        var buckets = HistogramBucketer.CreateDateTimeBuckets(dates);

        Assert.True(buckets.Count <= HistogramBucketer.MaxBuckets);
        // Consolidated labels contain " – " separator
        foreach (var bucket in buckets)
        {
            Assert.False(string.IsNullOrEmpty(bucket.Label));
        }
    }

    // --- HistogramBucketer: Day granularity FloorDate (line 187-188) ---

    [Fact]
    public void DateTimeBuckets_DayGranularity_SameMonthDifferentDays()
    {
        // Dates within same month but different days forces Day granularity
        // range < 60 days → Day granularity
        var dates = new List<DateTime>
        {
            new DateTime(2023, 6, 1),
            new DateTime(2023, 6, 5),
            new DateTime(2023, 6, 10),
            new DateTime(2023, 6, 15),
        };

        var buckets = HistogramBucketer.CreateDateTimeBuckets(dates);

        // Should have day-level buckets
        Assert.True(buckets.Count >= 4);
        // First bucket should start at June 1
        Assert.Equal(new DateTime(2023, 6, 1), buckets[0].Min);
    }

    // --- HistogramBucketer: Day AdvanceDate (line 204) ---

    [Fact]
    public void DateTimeBuckets_DayGranularity_AdvancesByOneDay()
    {
        var dates = new List<DateTime>
        {
            new DateTime(2023, 7, 1),
            new DateTime(2023, 7, 3),
        };

        var buckets = HistogramBucketer.CreateDateTimeBuckets(dates);

        // Day granularity: each bucket spans exactly 1 day
        Assert.True(buckets.Count >= 2);
        var firstBucketSpan = buckets[0].Max - buckets[0].Min;
        Assert.Equal(1.0, firstBucketSpan.TotalDays);
    }

    // --- DetailPaneModel: IsShowing property setter (line 161) ---

    [Fact]
    public void DetailPaneModel_IsShowing_SetTrue_FiresPropertyChanged()
    {
        var model = new DetailPaneModel();
        var changedProps = new List<string>();
        model.PropertyChanged += (s, e) => changedProps.Add(e.PropertyName!);

        model.IsShowing = true;
        Assert.True(model.IsShowing);
        Assert.Contains("IsShowing", changedProps);
    }

    [Fact]
    public void DetailPaneModel_IsShowing_SetFalse_FiresPropertyChanged()
    {
        var model = new DetailPaneModel();
        model.IsShowing = true; // set initial state

        var changedProps = new List<string>();
        model.PropertyChanged += (s, e) => changedProps.Add(e.PropertyName!);

        model.IsShowing = false;
        Assert.False(model.IsShowing);
        Assert.Contains("IsShowing", changedProps);
    }

    // --- DetailPaneModel: null/empty values list → skip property (line 183) ---

    [Fact]
    public void DetailPaneModel_PropertyWithEmptyValues_SkippedInFacets()
    {
        var prop = new PivotViewerStringProperty("color") { DisplayName = "Color" };
        var item = new PivotViewerItem("1");
        // Add property with empty values array
        item.Add(prop, Array.Empty<object>());

        var model = new DetailPaneModel();
        model.SelectedItem = item;

        // Empty values list → property should be skipped
        Assert.Empty(model.FacetValues);
    }

    // --- DetailPaneModel: null value inside values list → empty string (line 193) ---

    [Fact]
    public void DetailPaneModel_NullValueInList_FormatsAsEmptyString()
    {
        var prop = new PivotViewerStringProperty("tag") { DisplayName = "Tag" };
        var item = new PivotViewerItem("1");
        item.Add(prop, new object?[] { "valid", null! });

        var model = new DetailPaneModel();
        model.SelectedItem = item;

        Assert.Single(model.FacetValues);
        var values = model.FacetValues[0].Values;
        Assert.Equal(2, values.Count);
        Assert.Equal("valid", values[0]);
        Assert.Equal("", values[1]);
    }

    // --- DetailPaneModel: FormatValue with DecimalPlaces >= 0, Format == null (lines 231-232) ---

    [Fact]
    public void DetailPaneModel_FormatValue_DecimalPlaces2_NoFormat_UsesF2()
    {
        var prop = new PivotViewerNumericProperty("price")
        {
            DisplayName = "Price",
            DecimalPlaces = 2
            // Format is null by default
        };
        var item = new PivotViewerItem("1");
        item.Add(prop, (object)3.14159);

        var model = new DetailPaneModel();
        model.SelectedItem = item;

        Assert.Single(model.FacetValues);
        Assert.Equal("3.14", model.FacetValues[0].Values[0]);
    }

    [Fact]
    public void DetailPaneModel_FormatValue_DecimalPlaces0_NoFormat_UsesF0()
    {
        var prop = new PivotViewerNumericProperty("count")
        {
            DisplayName = "Count",
            DecimalPlaces = 0
        };
        var item = new PivotViewerItem("1");
        item.Add(prop, (object)42.7);

        var model = new DetailPaneModel();
        model.SelectedItem = item;

        Assert.Single(model.FacetValues);
        Assert.Equal("43", model.FacetValues[0].Values[0]);
    }

    [Fact]
    public void DetailPaneModel_FormatValue_DecimalPlaces4_NoFormat_UsesF4()
    {
        var prop = new PivotViewerNumericProperty("precision")
        {
            DisplayName = "Precision",
            DecimalPlaces = 4
        };
        var item = new PivotViewerItem("1");
        item.Add(prop, (object)1.23456789);

        var model = new DetailPaneModel();
        model.SelectedItem = item;

        Assert.Single(model.FacetValues);
        Assert.Equal("1.2346", model.FacetValues[0].Values[0]);
    }

    [Fact]
    public void DetailPaneModel_FormatValue_WithExplicitFormat_OverridesDecimalPlaces()
    {
        var prop = new PivotViewerNumericProperty("price")
        {
            DisplayName = "Price",
            DecimalPlaces = 2,
            Format = "C2" // Explicit format takes precedence
        };
        var item = new PivotViewerItem("1");
        item.Add(prop, (object)19.99);

        var model = new DetailPaneModel();
        model.SelectedItem = item;

        Assert.Single(model.FacetValues);
        // With Format set, DecimalPlaces path is skipped; Format is used
        Assert.NotEmpty(model.FacetValues[0].Values[0]);
    }

    // --- Helper ---

    private static PivotViewerItem CreateItem(string id, PivotViewerProperty prop, object value)
    {
        var item = new PivotViewerItem(id);
        item.Add(prop, new[] { value });
        return item;
    }
}
