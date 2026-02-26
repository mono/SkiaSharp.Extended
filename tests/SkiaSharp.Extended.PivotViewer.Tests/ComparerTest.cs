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

    [Fact]
    public void CardinalityComparer_IDictionary_Constructor()
    {
        IDictionary<string, int> counts = new Dictionary<string, int>
        {
            ["Alpha"] = 5,
            ["Beta"] = 10,
            ["Gamma"] = 5
        };
        var comparer = new PivotViewerPropertyCardinalityComparer(counts);

        // Beta (10) sorts first, then Alpha/Gamma alphabetically
        Assert.True(comparer.Compare("Beta", "Alpha") < 0);
        Assert.True(comparer.Compare("Alpha", "Gamma") < 0); // equal count, alphabetical
    }

    [Fact]
    public void CardinalityComparer_EqualCardinalities_SortAlphabetically()
    {
        var counts = new Dictionary<string, int>
        {
            ["Zebra"] = 3,
            ["Apple"] = 3,
            ["Mango"] = 3
        };
        var comparer = new PivotViewerPropertyCardinalityComparer(counts);

        var sorted = counts.Keys.OrderBy(k => k, comparer).ToList();
        Assert.Equal("Apple", sorted[0]);
        Assert.Equal("Mango", sorted[1]);
        Assert.Equal("Zebra", sorted[2]);
    }

    [Fact]
    public void CardinalityComparer_Compare_BothNull_ReturnsZero()
    {
        var comparer = new PivotViewerPropertyCardinalityComparer(new[] { "A" });
        Assert.Equal(0, comparer.Compare(null, null));
    }

    [Fact]
    public void CustomSortOrderComparer_EmptyOrderedValues()
    {
        var comparer = new CustomSortOrderComparer(new List<string>());

        // Both unknown, alphabetical
        Assert.True(comparer.Compare("Apple", "Banana") < 0);
        Assert.True(comparer.Compare("Banana", "Apple") > 0);
        Assert.Equal(0, comparer.Compare("Same", "Same"));
    }

    [Fact]
    public void CustomSortOrderComparer_NullHandling()
    {
        var comparer = new CustomSortOrderComparer(new[] { "A", "B" });

        Assert.Equal(0, comparer.Compare(null, null));
        Assert.True(comparer.Compare(null, "A") > 0);  // null sorts after
        Assert.True(comparer.Compare("A", null) < 0);   // non-null sorts before
    }

    [Fact]
    public void CustomSortOrderComparer_DuplicatesInOrder_FirstOccurrenceWins()
    {
        var comparer = new CustomSortOrderComparer(new[] { "X", "Y", "X", "Z" });

        // X is at index 0, Y at 1, Z at 3 (second X ignored)
        Assert.True(comparer.Compare("X", "Y") < 0);
        Assert.True(comparer.Compare("Y", "Z") < 0);
    }

    [Fact]
    public void CustomSortOrderComparer_KnownBeforeUnknown()
    {
        var comparer = new CustomSortOrderComparer(new[] { "Known" });

        Assert.True(comparer.Compare("Known", "Unknown") < 0);
        Assert.True(comparer.Compare("Unknown", "Known") > 0);
    }

    [Fact]
    public void CustomSortOrderComparer_BothUnknown_Alphabetical()
    {
        var comparer = new CustomSortOrderComparer(new[] { "X" });

        Assert.True(comparer.Compare("Alpha", "Beta") < 0);
        Assert.True(comparer.Compare("Beta", "Alpha") > 0);
        Assert.Equal(0, comparer.Compare("Same", "SAME")); // case insensitive
    }

    [Fact]
    public void CustomSortOrderComparer_FullSort_Ascending()
    {
        var comparer = new CustomSortOrderComparer(new[] { "High", "Medium", "Low" });
        var values = new[] { "Low", "High", "Medium" };

        var sorted = values.OrderBy(v => v, comparer).ToList();
        Assert.Equal("High", sorted[0]);
        Assert.Equal("Medium", sorted[1]);
        Assert.Equal("Low", sorted[2]);
    }

    [Fact]
    public void CustomSortOrderComparer_FullSort_Descending()
    {
        var comparer = new CustomSortOrderComparer(new[] { "High", "Medium", "Low" });
        var values = new[] { "Low", "High", "Medium" };

        var sorted = values.OrderByDescending(v => v, comparer).ToList();
        Assert.Equal("Low", sorted[0]);
        Assert.Equal("Medium", sorted[1]);
        Assert.Equal("High", sorted[2]);
    }

    [Fact]
    public void CustomSortOrderComparer_EmptyStrings()
    {
        var comparer = new CustomSortOrderComparer(new[] { "", "A", "B" });

        Assert.True(comparer.Compare("", "A") < 0); // "" at index 0, "A" at index 1
        Assert.True(comparer.Compare("A", "B") < 0);
    }

    [Fact]
    public void CustomSortOrderComparer_CaseInsensitiveLookup()
    {
        var comparer = new CustomSortOrderComparer(new[] { "apple", "Banana" });

        // "APPLE" should match "apple" at index 0
        Assert.True(comparer.Compare("APPLE", "Banana") < 0);
        Assert.True(comparer.Compare("banana", "APPLE") > 0);
    }

    [Fact]
    public void CardinalityComparer_EmptyInput_TreatsAllAsZero()
    {
        var comparer = new PivotViewerPropertyCardinalityComparer(Array.Empty<string>());
        // Both unknown = zero cardinality, falls back to alphabetical
        Assert.True(comparer.Compare("Apple", "Banana") < 0);
    }

    [Fact]
    public void CardinalityComparer_NullAndEmptyString()
    {
        var values = new[] { "", "A" };
        var comparer = new PivotViewerPropertyCardinalityComparer(values);

        // null sorts after everything
        Assert.True(comparer.Compare(null, "") > 0);
        Assert.True(comparer.Compare("", null) < 0);
    }
}
