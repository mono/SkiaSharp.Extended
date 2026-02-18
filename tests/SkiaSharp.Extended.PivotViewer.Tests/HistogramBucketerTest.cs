using SkiaSharp.Extended.PivotViewer;
using Xunit;

namespace SkiaSharp.Extended.PivotViewer.Tests;

public class HistogramBucketerTest
{
    [Fact]
    public void NumericBuckets_Empty_ReturnsEmpty()
    {
        var buckets = HistogramBucketer.CreateNumericBuckets(Array.Empty<double>());
        Assert.Empty(buckets);
    }

    [Fact]
    public void NumericBuckets_SingleValue_OneBucket()
    {
        var buckets = HistogramBucketer.CreateNumericBuckets(new[] { 5.0 });
        Assert.Single(buckets);
        Assert.Equal(1, buckets[0].Count);
    }

    [Fact]
    public void NumericBuckets_EqualValues_OneBucket()
    {
        var buckets = HistogramBucketer.CreateNumericBuckets(new[] { 3.0, 3.0, 3.0 });
        Assert.Single(buckets);
        Assert.Equal(3, buckets[0].Count);
    }

    [Fact]
    public void NumericBuckets_Range_MultipleBuckets()
    {
        var values = Enumerable.Range(1, 100).Select(i => (double)i).ToList();
        var buckets = HistogramBucketer.CreateNumericBuckets(values);

        Assert.True(buckets.Count > 1);
        Assert.True(buckets.Count <= HistogramBucketer.MaxBuckets);
        Assert.Equal(100, buckets.Sum(b => b.Count));
    }

    [Fact]
    public void NumericBuckets_NeverExceedsMaxBuckets()
    {
        var values = Enumerable.Range(0, 1000).Select(i => (double)i).ToList();
        var buckets = HistogramBucketer.CreateNumericBuckets(values);
        Assert.True(buckets.Count <= HistogramBucketer.MaxBuckets);
    }

    [Fact]
    public void NumericBuckets_NegativeValues()
    {
        var values = new[] { -10.0, -5.0, 0.0, 5.0, 10.0 };
        var buckets = HistogramBucketer.CreateNumericBuckets(values);
        Assert.True(buckets.Count > 0);
        Assert.Equal(5, buckets.Sum(b => b.Count));
    }

    [Fact]
    public void DateTimeBuckets_Empty_ReturnsEmpty()
    {
        var buckets = HistogramBucketer.CreateDateTimeBuckets(Array.Empty<DateTime>());
        Assert.Empty(buckets);
    }

    [Fact]
    public void DateTimeBuckets_SingleDate_OneBucket()
    {
        var dt = new DateTime(2020, 6, 15);
        var buckets = HistogramBucketer.CreateDateTimeBuckets(new[] { dt });
        Assert.Single(buckets);
        Assert.Equal(1, buckets[0].Count);
    }

    [Fact]
    public void DateTimeBuckets_SameDate_OneBucket()
    {
        var dt = new DateTime(2020, 6, 15);
        var buckets = HistogramBucketer.CreateDateTimeBuckets(new[] { dt, dt, dt });
        Assert.Single(buckets);
        Assert.Equal(3, buckets[0].Count);
    }

    [Fact]
    public void DateTimeBuckets_MonthRange_MonthGranularity()
    {
        var dates = Enumerable.Range(0, 180).Select(i => new DateTime(2020, 1, 1).AddDays(i)).ToList();
        var buckets = HistogramBucketer.CreateDateTimeBuckets(dates);
        Assert.True(buckets.Count > 1);
        Assert.True(buckets.Count <= HistogramBucketer.MaxBuckets);
        Assert.Equal(180, buckets.Sum(b => b.Count));
    }

    [Fact]
    public void DateTimeBuckets_YearRange_YearGranularity()
    {
        var dates = Enumerable.Range(2010, 5).Select(y => new DateTime(y, 6, 15)).ToList();
        var buckets = HistogramBucketer.CreateDateTimeBuckets(dates);
        Assert.True(buckets.Count > 1);
        Assert.Equal(5, buckets.Sum(b => b.Count));
    }

    [Fact]
    public void DateTimeBuckets_DecadeRange()
    {
        var dates = Enumerable.Range(1950, 50).Select(y => new DateTime(y, 1, 1)).ToList();
        var buckets = HistogramBucketer.CreateDateTimeBuckets(dates);
        Assert.True(buckets.Count > 1);
        Assert.True(buckets.Count <= HistogramBucketer.MaxBuckets);
    }

    [Fact]
    public void StringBuckets_Empty_ReturnsEmpty()
    {
        var buckets = HistogramBucketer.CreateStringBuckets(Array.Empty<string>());
        Assert.Empty(buckets);
    }

    [Fact]
    public void StringBuckets_CountsCorrectly()
    {
        var values = new[] { "Red", "Blue", "Red", "Green", "Red", "Blue" };
        var buckets = HistogramBucketer.CreateStringBuckets(values);

        Assert.Equal(3, buckets.Count);
        Assert.Equal("Red", buckets[0].Min); // Most common first
        Assert.Equal(3, buckets[0].Count);
        Assert.Equal("Blue", buckets[1].Min);
        Assert.Equal(2, buckets[1].Count);
        Assert.Equal("Green", buckets[2].Min);
        Assert.Equal(1, buckets[2].Count);
    }

    [Fact]
    public void StringBuckets_CaseInsensitive()
    {
        var values = new[] { "red", "RED", "Red" };
        var buckets = HistogramBucketer.CreateStringBuckets(values);

        Assert.Single(buckets);
        Assert.Equal(3, buckets[0].Count);
    }

    [Fact]
    public void StringBuckets_TiedCounts_Alphabetical()
    {
        var values = new[] { "Banana", "Apple" };
        var buckets = HistogramBucketer.CreateStringBuckets(values);

        Assert.Equal(2, buckets.Count);
        Assert.Equal("Apple", buckets[0].Min);
        Assert.Equal("Banana", buckets[1].Min);
    }

    [Fact]
    public void HistogramBucket_ToString()
    {
        var bucket = new HistogramBucket<double>(0.0, 10.0, 5);
        Assert.Contains("0", bucket.ToString());
        Assert.Contains("10", bucket.ToString());
        Assert.Contains("5", bucket.ToString());
    }

    [Fact]
    public void HistogramBucket_Label()
    {
        var bucket = new HistogramBucket<string>("A", "A", 3);
        Assert.Equal("", bucket.Label);
        bucket.Label = "Category A";
        Assert.Equal("Category A", bucket.Label);
    }
}
