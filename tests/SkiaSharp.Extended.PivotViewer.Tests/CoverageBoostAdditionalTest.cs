using System;
using System.Linq;
using SkiaSharp.Extended.PivotViewer;

namespace SkiaSharp.Extended.PivotViewer.Tests;

/// <summary>
/// Additional coverage tests targeting specific low-coverage areas:
/// - PivotViewerCollectionBuilder.ItemBuilder (Set with unknown property, Set by property object)
/// - HistogramBucket (Label, ToString, Count setter)
/// - HistogramBucketer edge cases (single value, same values, decade granularity)
/// </summary>
public class CoverageBoostAdditionalTest
{
    // === ItemBuilder Coverage ===

    [Fact]
    public void ItemBuilder_SetByPropertyId_UnknownId_DoesNotThrow()
    {
        var builder = new PivotViewerCollectionBuilder()
            .AddStringProperty("color", "Color");

        builder.AddItem("item-1", item =>
        {
            // Setting a property that doesn't exist should not throw
            item.Set("nonexistent", "value");
        });

        var (items, _) = builder.Build();
        Assert.Single(items);
    }

    [Fact]
    public void ItemBuilder_SetByPropertyObject_Works()
    {
        var prop = new PivotViewerStringProperty("name") { DisplayName = "Name" };

        var builder = new PivotViewerCollectionBuilder()
            .AddProperty(prop);

        builder.AddItem("item-1", item =>
        {
            item.Set(prop, "Direct Set");
        });

        var (items, _) = builder.Build();
        Assert.Equal("Direct Set", items[0][prop]![0]!.ToString());
    }

    [Fact]
    public void ItemBuilder_SetMultipleValues_Works()
    {
        var builder = new PivotViewerCollectionBuilder()
            .AddStringProperty("tags", "Tags");

        builder.AddItem("item-1", item =>
        {
            item.Set("tags", "red", "green", "blue");
        });

        var (items, _) = builder.Build();
        var values = items[0]["tags"];
        Assert.NotNull(values);
        Assert.Equal(3, values.Count);
    }

    [Fact]
    public void ItemBuilder_FluentChaining_Works()
    {
        var builder = new PivotViewerCollectionBuilder()
            .AddStringProperty("a", "A")
            .AddNumericProperty("b", "B")
            .AddDateTimeProperty("c", "C")
            .AddLinkProperty("d", "D");

        builder.AddItem("item-1", item =>
        {
            item.Set("a", "Hello")
                .Set("b", 42.0)
                .Set("c", DateTime.Now)
                .Set("d", new PivotViewerHyperlink("Test", new Uri("http://test.com")));
        });

        var (items, properties) = builder.Build();
        Assert.Equal(4, properties.Count);
        Assert.Single(items);
    }

    [Fact]
    public void Builder_WithCopyright_SetsFields()
    {
        var builder = new PivotViewerCollectionBuilder()
            .WithName("Licensed Collection")
            .WithCopyright("ACME Corp", "https://acme.com/license");

        var (_, _) = builder.Build();
        // Should not throw
    }

    // === HistogramBucket Coverage ===

    [Fact]
    public void HistogramBucket_LabelProperty_DefaultEmpty()
    {
        var bucket = new HistogramBucket<double>(0, 10, 5);
        Assert.Equal("", bucket.Label);
    }

    [Fact]
    public void HistogramBucket_LabelProperty_Settable()
    {
        var bucket = new HistogramBucket<double>(0, 10, 5);
        bucket.Label = "0-10";
        Assert.Equal("0-10", bucket.Label);
    }

    [Fact]
    public void HistogramBucket_CountProperty_Settable()
    {
        var bucket = new HistogramBucket<double>(0, 10, 5);
        bucket.Count = 15;
        Assert.Equal(15, bucket.Count);
    }

    [Fact]
    public void HistogramBucket_ToString_IncludesRange()
    {
        var bucket = new HistogramBucket<double>(0, 10, 5);
        var str = bucket.ToString();
        Assert.Contains("0", str);
        Assert.Contains("10", str);
        Assert.Contains("5", str);
    }

    [Fact]
    public void HistogramBucket_StringType_Works()
    {
        var bucket = new HistogramBucket<string>("Apple", "Apple", 3);
        Assert.Equal("Apple", bucket.Min);
        Assert.Equal("Apple", bucket.Max);
        Assert.Equal(3, bucket.Count);
    }

    [Fact]
    public void HistogramBucket_DateTimeType_Works()
    {
        var min = new DateTime(2020, 1, 1);
        var max = new DateTime(2020, 12, 31);
        var bucket = new HistogramBucket<DateTime>(min, max, 12);
        Assert.Equal(min, bucket.Min);
        Assert.Equal(max, bucket.Max);
    }

    // === HistogramBucketer Edge Cases ===

    [Fact]
    public void NumericBuckets_SingleValue_ReturnsSingleBucket()
    {
        var buckets = HistogramBucketer.CreateNumericBuckets(new[] { 42.0 });

        Assert.Single(buckets);
        Assert.Equal(1, buckets[0].Count);
    }

    [Fact]
    public void NumericBuckets_AllSameValue_ReturnsSingleBucket()
    {
        var buckets = HistogramBucketer.CreateNumericBuckets(
            Enumerable.Repeat(7.0, 100));

        Assert.Single(buckets);
        Assert.Equal(100, buckets[0].Count);
    }

    [Fact]
    public void NumericBuckets_TwoValues_ReturnsBuckets()
    {
        var buckets = HistogramBucketer.CreateNumericBuckets(new[] { 0.0, 100.0 });

        Assert.True(buckets.Count >= 1);
        Assert.Equal(2, buckets.Sum(b => b.Count));
    }

    [Fact]
    public void NumericBuckets_LargeRange_CapsAtMaxBuckets()
    {
        var values = Enumerable.Range(0, 1000).Select(i => (double)i).ToArray();
        var buckets = HistogramBucketer.CreateNumericBuckets(values);

        Assert.True(buckets.Count <= HistogramBucketer.MaxBuckets,
            $"Expected <= {HistogramBucketer.MaxBuckets} buckets, got {buckets.Count}");
    }

    [Fact]
    public void DateTimeBuckets_Empty_ReturnsEmpty()
    {
        var buckets = HistogramBucketer.CreateDateTimeBuckets(Array.Empty<DateTime>());
        Assert.Empty(buckets);
    }

    [Fact]
    public void DateTimeBuckets_SameDay_ReturnsSingleBucket()
    {
        var date = new DateTime(2020, 6, 15, 10, 30, 0);
        var buckets = HistogramBucketer.CreateDateTimeBuckets(new[] { date, date, date });

        Assert.Single(buckets);
        Assert.Equal(3, buckets[0].Count);
    }

    [Fact]
    public void DateTimeBuckets_YearRange_UsesYearGranularity()
    {
        var dates = Enumerable.Range(2010, 8)
            .Select(y => new DateTime(y, 6, 15))
            .ToArray();

        var buckets = HistogramBucketer.CreateDateTimeBuckets(dates);

        Assert.True(buckets.Count >= 2, "Should create multiple year buckets");
        Assert.Equal(8, buckets.Sum(b => b.Count));
    }

    [Fact]
    public void DateTimeBuckets_DecadeRange_UsesDecadeGranularity()
    {
        var dates = Enumerable.Range(1950, 70)
            .Select(y => new DateTime(y, 1, 1))
            .ToArray();

        var buckets = HistogramBucketer.CreateDateTimeBuckets(dates);

        Assert.True(buckets.Count >= 2, "Should create decade-level buckets");
        Assert.True(buckets.Count <= HistogramBucketer.MaxBuckets);
    }

    [Fact]
    public void DateTimeBuckets_MonthRange_UsesMonthGranularity()
    {
        var dates = Enumerable.Range(0, 6)
            .Select(m => new DateTime(2020, 1, 1).AddMonths(m))
            .ToArray();

        var buckets = HistogramBucketer.CreateDateTimeBuckets(dates);

        Assert.True(buckets.Count >= 2, "Should create month-level buckets");
    }

    [Fact]
    public void DateTimeBuckets_DayRange_UsesDayGranularity()
    {
        var dates = Enumerable.Range(0, 10)
            .Select(d => new DateTime(2020, 6, 1).AddDays(d))
            .ToArray();

        var buckets = HistogramBucketer.CreateDateTimeBuckets(dates);

        Assert.True(buckets.Count >= 2, "Should create day-level buckets");
    }

    [Fact]
    public void DateTimeBuckets_ManyDays_ConsolidatesToMaxBuckets()
    {
        // 30 days at day granularity = 30 buckets, needs consolidation to <= 15
        var dates = Enumerable.Range(0, 30)
            .Select(d => new DateTime(2020, 6, 1).AddDays(d))
            .ToArray();

        var buckets = HistogramBucketer.CreateDateTimeBuckets(dates);

        Assert.True(buckets.Count <= HistogramBucketer.MaxBuckets,
            $"Expected <= {HistogramBucketer.MaxBuckets} buckets after consolidation, got {buckets.Count}");
        Assert.Equal(30, buckets.Sum(b => b.Count));
    }

    [Fact]
    public void StringBuckets_Empty_ReturnsEmpty()
    {
        var buckets = HistogramBucketer.CreateStringBuckets(Array.Empty<string>());
        Assert.Empty(buckets);
    }

    [Fact]
    public void StringBuckets_CaseInsensitive_MergesCounts()
    {
        var buckets = HistogramBucketer.CreateStringBuckets(
            new[] { "Red", "red", "RED", "Blue" });

        Assert.Equal(2, buckets.Count);
        var redBucket = buckets.First(b =>
            b.Min.Equals("Red", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(3, redBucket.Count);
    }

    [Fact]
    public void StringBuckets_SortedByCountDescending()
    {
        var buckets = HistogramBucketer.CreateStringBuckets(
            new[] { "A", "B", "B", "C", "C", "C" });

        Assert.Equal("C", buckets[0].Min);
        Assert.Equal(3, buckets[0].Count);
        Assert.Equal("B", buckets[1].Min);
        Assert.Equal(2, buckets[1].Count);
        Assert.Equal("A", buckets[2].Min);
        Assert.Equal(1, buckets[2].Count);
    }

    [Fact]
    public void NumericBuckets_Empty_ReturnsEmpty()
    {
        var buckets = HistogramBucketer.CreateNumericBuckets(Array.Empty<double>());
        Assert.Empty(buckets);
    }

    [Fact]
    public void NumericBuckets_NegativeValues_Works()
    {
        var buckets = HistogramBucketer.CreateNumericBuckets(
            new[] { -100.0, -50.0, 0.0, 50.0, 100.0 });

        Assert.True(buckets.Count >= 1);
        Assert.Equal(5, buckets.Sum(b => b.Count));
    }
}
