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

    [Fact]
    public void NumericBuckets_LabelsAreNonEmpty()
    {
        var values = Enumerable.Range(1, 100).Select(i => (double)i).ToList();
        var buckets = HistogramBucketer.CreateNumericBuckets(values);

        Assert.True(buckets.Count > 1);
        foreach (var bucket in buckets)
        {
            Assert.NotNull(bucket.Label);
            Assert.NotEqual("", bucket.Label);
        }
    }

    [Fact]
    public void NumericBuckets_LabelContainsMinAndMax()
    {
        var values = new[] { 0.0, 50.0, 100.0 };
        var buckets = HistogramBucketer.CreateNumericBuckets(values);

        Assert.True(buckets.Count > 1);
        foreach (var bucket in buckets)
        {
            // Label should contain the "–" separator between min and max
            Assert.Contains("–", bucket.Label);
        }
    }

    [Fact]
    public void NumericBuckets_SingleValue_LabelIsValue()
    {
        var buckets = HistogramBucketer.CreateNumericBuckets(new[] { 42.0 });
        Assert.Single(buckets);
        Assert.Contains("42", buckets[0].Label);
    }

    [Fact]
    public void DateTimeBuckets_LabelsAreNonEmpty()
    {
        var dates = Enumerable.Range(2010, 5).Select(y => new DateTime(y, 6, 15)).ToList();
        var buckets = HistogramBucketer.CreateDateTimeBuckets(dates);

        Assert.True(buckets.Count > 1);
        foreach (var bucket in buckets)
        {
            Assert.NotNull(bucket.Label);
            Assert.NotEqual("", bucket.Label);
        }
    }

    [Fact]
    public void DateTimeBuckets_SingleDate_LabelIsNonEmpty()
    {
        var dt = new DateTime(2020, 6, 15);
        var buckets = HistogramBucketer.CreateDateTimeBuckets(new[] { dt });
        Assert.Single(buckets);
        Assert.NotNull(buckets[0].Label);
        Assert.NotEqual("", buckets[0].Label);
    }

    [Fact]
    public void StringBuckets_ManyUniqueValues_AllRepresented()
    {
        var values = Enumerable.Range(0, 100).Select(i => $"Value_{i:D3}").ToList();
        var buckets = HistogramBucketer.CreateStringBuckets(values);
        // All unique values produce individual buckets for strings
        Assert.Equal(100, buckets.Count);
        Assert.Equal(100, buckets.Sum(b => b.Count));
    }

    [Fact]
    public void StringBuckets_SingleValue_OneBucket()
    {
        var buckets = HistogramBucketer.CreateStringBuckets(new[] { "Only" });
        Assert.Single(buckets);
        Assert.Equal(1, buckets[0].Count);
        Assert.Equal("Only", buckets[0].Min);
    }

    [Fact]
    public void NumericBuckets_TwoValues_LabelsContainRange()
    {
        var buckets = HistogramBucketer.CreateNumericBuckets(new[] { 10.0, 20.0 });
        Assert.True(buckets.Count >= 1);
        Assert.Equal(2, buckets.Sum(b => b.Count));
    }

    [Fact]
    public void DateTimeBuckets_DayRange_HasMultipleBuckets()
    {
        // Dates within a single month but different days
        var dates = Enumerable.Range(1, 28).Select(d => new DateTime(2020, 3, d)).ToList();
        var buckets = HistogramBucketer.CreateDateTimeBuckets(dates);
        Assert.True(buckets.Count >= 1);
        Assert.Equal(28, buckets.Sum(b => b.Count));
    }

    [Fact]
    public void DateTimeBuckets_WideRange_UsesAppropriateGranularity()
    {
        // Dates spanning 100 years
        var dates = Enumerable.Range(1900, 100).Select(y => new DateTime(y, 1, 1)).ToList();
        var buckets = HistogramBucketer.CreateDateTimeBuckets(dates);
        Assert.True(buckets.Count > 1);
        Assert.True(buckets.Count <= HistogramBucketer.MaxBuckets);
        Assert.Equal(100, buckets.Sum(b => b.Count));
    }

    [Fact]
    public void NumericBuckets_VeryCloseValues_HandlesPrecision()
    {
        var values = new[] { 1.001, 1.002, 1.003 };
        var buckets = HistogramBucketer.CreateNumericBuckets(values);
        Assert.True(buckets.Count >= 1);
        Assert.Equal(3, buckets.Sum(b => b.Count));
    }

    [Fact]
    public void HistogramBucket_MinAndMax_Properties()
    {
        var bucket = new HistogramBucket<double>(5.0, 15.0, 10);
        Assert.Equal(5.0, bucket.Min);
        Assert.Equal(15.0, bucket.Max);
        Assert.Equal(10, bucket.Count);
    }

    [Fact]
    public void StringBuckets_NullOrEmpty_Handled()
    {
        // Empty strings should be grouped together
        var values = new[] { "", "", "A" };
        var buckets = HistogramBucketer.CreateStringBuckets(values);
        Assert.True(buckets.Count >= 1);
        Assert.Equal(3, buckets.Sum(b => b.Count));
    }

    [Fact]
    public void NumericBuckets_NaN_FilteredOut()
    {
        var values = new[] { 1.0, 2.0, double.NaN, 3.0 };
        var buckets = HistogramBucketer.CreateNumericBuckets(values);
        Assert.True(buckets.Count > 0);
        Assert.Equal(3, buckets.Sum(b => b.Count)); // NaN excluded
    }

    [Fact]
    public void NumericBuckets_Infinity_FilteredOut()
    {
        var values = new[] { 1.0, double.PositiveInfinity, double.NegativeInfinity, 2.0 };
        var buckets = HistogramBucketer.CreateNumericBuckets(values);
        Assert.True(buckets.Count > 0);
        Assert.Equal(2, buckets.Sum(b => b.Count)); // Infinities excluded
    }

    [Fact]
    public void NumericBuckets_AllNaN_ReturnsEmpty()
    {
        var values = new[] { double.NaN, double.NaN };
        var buckets = HistogramBucketer.CreateNumericBuckets(values);
        Assert.Empty(buckets);
    }
}
