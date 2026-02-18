using SkiaSharp.Extended.PivotViewer;
using Xunit;

namespace SkiaSharp.Extended.PivotViewer.Tests;

public class ComparerTest
{
    [Fact]
    public void CardinalityComparer_SortsByFrequency()
    {
        var values = new[] { "Red", "Blue", "Red", "Green", "Red", "Blue" };
        var comparer = new PivotViewerPropertyCardinalityComparer(values);

        var sorted = values.Distinct().OrderBy(v => v, comparer).ToList();
        Assert.Equal("Red", sorted[0]); // 3 occurrences
        Assert.Equal("Blue", sorted[1]); // 2 occurrences
        Assert.Equal("Green", sorted[2]); // 1 occurrence
    }

    [Fact]
    public void CardinalityComparer_TiesAlphabetical()
    {
        var values = new[] { "Banana", "Apple", "Cherry" };
        var comparer = new PivotViewerPropertyCardinalityComparer(values);

        var sorted = values.OrderBy(v => v, comparer).ToList();
        Assert.Equal("Apple", sorted[0]);
        Assert.Equal("Banana", sorted[1]);
        Assert.Equal("Cherry", sorted[2]);
    }

    [Fact]
    public void CardinalityComparer_NullsLast()
    {
        var values = new[] { "A", "B" };
        var comparer = new PivotViewerPropertyCardinalityComparer(values);

        Assert.True(comparer.Compare("A", null) < 0);
        Assert.True(comparer.Compare(null, "A") > 0);
        Assert.Equal(0, comparer.Compare(null, null));
    }

    [Fact]
    public void CardinalityComparer_CaseInsensitive()
    {
        var values = new[] { "red", "RED", "Red", "blue" };
        var comparer = new PivotViewerPropertyCardinalityComparer(values);

        // "red" has 3 occurrences, "blue" has 1
        Assert.True(comparer.Compare("red", "blue") < 0); // red sorts first (higher count)
    }

    [Fact]
    public void CardinalityComparer_FromPrecomputedCounts()
    {
        var counts = new Dictionary<string, int>
        {
            ["Rare"] = 1,
            ["Common"] = 100,
            ["Medium"] = 10
        };
        var comparer = new PivotViewerPropertyCardinalityComparer(counts);

        var sorted = counts.Keys.OrderBy(k => k, comparer).ToList();
        Assert.Equal("Common", sorted[0]);
        Assert.Equal("Medium", sorted[1]);
        Assert.Equal("Rare", sorted[2]);
    }

    [Fact]
    public void CardinalityComparer_UnknownValues_TreatedAsZero()
    {
        var values = new[] { "Known" };
        var comparer = new PivotViewerPropertyCardinalityComparer(values);

        Assert.True(comparer.Compare("Known", "Unknown") < 0); // Known=1, Unknown=0
    }
}
