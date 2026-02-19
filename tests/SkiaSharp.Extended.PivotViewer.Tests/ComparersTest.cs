using SkiaSharp.Extended.PivotViewer;

namespace SkiaSharp.Extended.PivotViewer.Tests;

public class ComparersTest
{
    // --- PivotViewerPropertyCardinalityComparer ---

    [Fact]
    public void CardinalityComparer_HigherCountSortsFirst()
    {
        var values = new[] { "A", "B", "B", "B", "A", "C" };
        var comparer = new PivotViewerPropertyCardinalityComparer(values);

        // B has 3 occurrences, A has 2, C has 1
        Assert.True(comparer.Compare("B", "A") < 0); // B first (higher count)
        Assert.True(comparer.Compare("A", "C") < 0); // A first (higher count)
        Assert.True(comparer.Compare("B", "C") < 0); // B first
    }

    [Fact]
    public void CardinalityComparer_TieBreaksAlphabetically()
    {
        var values = new[] { "Banana", "Apple" };
        var comparer = new PivotViewerPropertyCardinalityComparer(values);

        // Both have count 1, so alphabetical
        Assert.True(comparer.Compare("Apple", "Banana") < 0);
    }

    [Fact]
    public void CardinalityComparer_CaseInsensitive()
    {
        var values = new[] { "apple", "APPLE", "Apple" };
        var comparer = new PivotViewerPropertyCardinalityComparer(values);

        // All map to same key (case-insensitive), so count is 3
        Assert.Equal(0, comparer.Compare("apple", "Apple"));
    }

    [Fact]
    public void CardinalityComparer_UnknownValuesSortLast()
    {
        var values = new[] { "Known" };
        var comparer = new PivotViewerPropertyCardinalityComparer(values);

        // Known has count 1, Unknown has count 0
        Assert.True(comparer.Compare("Known", "Unknown") < 0);
    }

    [Fact]
    public void CardinalityComparer_NullHandling()
    {
        var comparer = new PivotViewerPropertyCardinalityComparer(new[] { "A" });

        Assert.Equal(0, comparer.Compare(null, null));
        Assert.True(comparer.Compare(null, "A") > 0); // null sorts after
        Assert.True(comparer.Compare("A", null) < 0); // non-null sorts before
    }

    [Fact]
    public void CardinalityComparer_FromDictionary()
    {
        var counts = new Dictionary<string, int> { { "X", 10 }, { "Y", 5 } };
        var comparer = new PivotViewerPropertyCardinalityComparer(counts);

        Assert.True(comparer.Compare("X", "Y") < 0); // X has higher count
    }

    // --- CustomSortOrderComparer ---

    [Fact]
    public void CustomSortOrder_FollowsPredefinedOrder()
    {
        var order = new[] { "Third", "First", "Second" };
        var comparer = new CustomSortOrderComparer(order);

        // "Third" is at index 0, "First" at 1, "Second" at 2
        Assert.True(comparer.Compare("Third", "First") < 0);
        Assert.True(comparer.Compare("First", "Second") < 0);
        Assert.True(comparer.Compare("Third", "Second") < 0);
    }

    [Fact]
    public void CustomSortOrder_KnownBeforeUnknown()
    {
        var order = new[] { "Known" };
        var comparer = new CustomSortOrderComparer(order);

        Assert.True(comparer.Compare("Known", "Unknown") < 0);
        Assert.True(comparer.Compare("Unknown", "Known") > 0);
    }

    [Fact]
    public void CustomSortOrder_UnknownAlphabetical()
    {
        var order = new[] { "Z" };
        var comparer = new CustomSortOrderComparer(order);

        // Both unknown → alphabetical
        Assert.True(comparer.Compare("Apple", "Banana") < 0);
    }

    [Fact]
    public void CustomSortOrder_CaseInsensitive()
    {
        var order = new[] { "apple", "banana" };
        var comparer = new CustomSortOrderComparer(order);

        Assert.True(comparer.Compare("APPLE", "BANANA") < 0);
        Assert.Equal(0, comparer.Compare("apple", "APPLE"));
    }

    [Fact]
    public void CustomSortOrder_NullHandling()
    {
        var comparer = new CustomSortOrderComparer(new[] { "A" });

        Assert.Equal(0, comparer.Compare(null, null));
        Assert.True(comparer.Compare(null, "A") > 0);
        Assert.True(comparer.Compare("A", null) < 0);
    }

    [Fact]
    public void CustomSortOrder_DuplicatesIgnored()
    {
        var order = new[] { "A", "B", "A" }; // duplicate "A"
        var comparer = new CustomSortOrderComparer(order);

        // "A" should be at index 0 (first occurrence), "B" at index 1
        Assert.True(comparer.Compare("A", "B") < 0);
    }
}
