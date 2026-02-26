using SkiaSharp.Extended.PivotViewer;
using Xunit;

namespace SkiaSharp.Extended.PivotViewer.Tests;

/// <summary>
/// Additional tests targeting low-coverage areas: HistogramLayout, Hyperlink,
/// PropertyChangedEventArgs, CxmlCollectionStateChangedEventArgs, CharBucket.
/// </summary>
public class CoverageGapTest
{
    // --- CxmlCollectionStateChangedEventArgs ---

    [Fact]
    public void CxmlCollectionStateChangedEventArgs_AllProperties()
    {
        var ex = new InvalidOperationException("test error");
        var args = new CxmlCollectionStateChangedEventArgs(
            CxmlCollectionState.Loading, CxmlCollectionState.Failed, "Failed to load", ex);

        Assert.Equal(CxmlCollectionState.Loading, args.OldState);
        Assert.Equal(CxmlCollectionState.Failed, args.NewState);
        Assert.Equal("Failed to load", args.Message);
        Assert.Same(ex, args.Exception);
    }

    [Fact]
    public void CxmlCollectionStateChangedEventArgs_NullMessageAndException()
    {
        var args = new CxmlCollectionStateChangedEventArgs(
            CxmlCollectionState.Initialized, CxmlCollectionState.Loading);

        Assert.Null(args.Message);
        Assert.Null(args.Exception);
    }

    // --- HistogramLayout ---

    [Fact]
    public void HistogramLayout_HitTest_ReturnsCorrectItem()
    {
        var item1 = new PivotViewerItem("1");
        var item2 = new PivotViewerItem("2");
        var positions = new[]
        {
            new ItemPosition(item1, 0, 0, 50, 50),
            new ItemPosition(item2, 60, 0, 50, 50)
        };
        var columns = new[]
        {
            new HistogramColumn("A", 0, 50, new[] { positions[0] }, 1),
            new HistogramColumn("B", 60, 50, new[] { positions[1] }, 1)
        };
        var layout = new HistogramLayout(columns, positions);

        Assert.Same(item1, layout.HitTest(25, 25));
        Assert.Same(item2, layout.HitTest(70, 25));
        Assert.Null(layout.HitTest(55, 25)); // Between items
    }

    [Fact]
    public void HistogramLayout_HitTest_EdgeCases()
    {
        var item = new PivotViewerItem("1");
        var pos = new ItemPosition(item, 10, 10, 30, 30);
        var layout = new HistogramLayout(
            new[] { new HistogramColumn("A", 10, 30, new[] { pos }, 1) },
            new[] { pos });

        // Exact top-left corner = hit
        Assert.Same(item, layout.HitTest(10, 10));
        // Just inside bottom-right
        Assert.Same(item, layout.HitTest(39.9, 39.9));
        // Exact right edge = miss (exclusive)
        Assert.Null(layout.HitTest(40, 10));
        // Exact bottom edge = miss (exclusive)
        Assert.Null(layout.HitTest(10, 40));
        // Outside
        Assert.Null(layout.HitTest(0, 0));
    }

    [Fact]
    public void HistogramColumn_AllProperties()
    {
        var items = new[] { new ItemPosition(new PivotViewerItem("1"), 0, 0, 10, 10) };
        var col = new HistogramColumn("Category", 100.5, 50.0, items, 42);

        Assert.Equal("Category", col.Label);
        Assert.Equal(100.5, col.X);
        Assert.Equal(50.0, col.Width);
        Assert.Single(col.Items);
        Assert.Equal(42, col.Count);
    }

    // --- PivotViewerHyperlink ---

    [Fact]
    public void PivotViewerHyperlink_Comparison()
    {
        var link1 = new PivotViewerHyperlink("A Link", new Uri("https://a.com"));
        var link2 = new PivotViewerHyperlink("B Link", new Uri("https://b.com"));

        Assert.True(link1.CompareTo(link2) < 0);
        Assert.True(link2.CompareTo(link1) > 0);
    }

    [Fact]
    public void PivotViewerHyperlink_Equality()
    {
        var link1 = new PivotViewerHyperlink("Same", new Uri("https://test.com"));
        var link2 = new PivotViewerHyperlink("Same", new Uri("https://test.com"));
        var link3 = new PivotViewerHyperlink("Diff", new Uri("https://other.com"));

        Assert.Equal(link1, link2);
        Assert.NotEqual(link1, link3);
        Assert.Equal(link1.GetHashCode(), link2.GetHashCode());
    }

    [Fact]
    public void PivotViewerHyperlink_NullComparison()
    {
        var link = new PivotViewerHyperlink("Test", new Uri("https://test.com"));
        Assert.True(link.CompareTo(null) > 0);
    }

    // --- PivotViewerPropertyChangedEventArgs ---

    [Fact]
    public void PivotViewerPropertyChangedEventArgs_Properties()
    {
        var prop = new PivotViewerStringProperty("name") { DisplayName = "Name" };
        var args = new PivotViewerPropertyChangedEventArgs(prop, "old", "new");

        Assert.Same(prop, args.PivotProperty);
        Assert.Equal("old", args.OldValue);
        Assert.Equal("new", args.NewValue);
    }

    [Fact]
    public void PivotViewerPropertyChangedEventArgs_NullValues()
    {
        var prop = new PivotViewerNumericProperty("num") { DisplayName = "Number" };
        var args = new PivotViewerPropertyChangedEventArgs(prop, null, 42.0);

        Assert.Null(args.OldValue);
        Assert.Equal(42.0, args.NewValue);
    }

    // --- GridLayoutEngine.ComputeHistogramLayout ---

    [Fact]
    public void ComputeHistogramLayout_GroupsItemsByProperty()
    {
        var prop = new PivotViewerStringProperty("color") { DisplayName = "Color" };
        var items = new List<PivotViewerItem>();
        for (int i = 0; i < 6; i++)
        {
            var item = new PivotViewerItem($"item-{i}");
            item.Add(prop, new object[] { i < 3 ? "Red" : "Blue" });
            items.Add(item);
        }

        var engine = new GridLayoutEngine();
        var layout = engine.ComputeHistogramLayout(items, "color", 400, 300);

        Assert.NotNull(layout);
        Assert.True(layout.Columns.Length > 0);
        Assert.Equal(6, layout.AllPositions.Length);
    }

    [Fact]
    public void ComputeHistogramLayout_EmptyItems()
    {
        var engine = new GridLayoutEngine();
        var layout = engine.ComputeHistogramLayout(
            new List<PivotViewerItem>(), "test", 400, 300);

        Assert.Empty(layout.Columns);
        Assert.Empty(layout.AllPositions);
    }

    // --- WordWheelIndex CharBucket coverage ---

    [Fact]
    public void WordWheelIndex_SearchWithShortQuery()
    {
        var index = new WordWheelIndex();
        var prop = new PivotViewerStringProperty("name")
        {
            DisplayName = "Name",
            Options = PivotViewerPropertyOptions.CanSearchText
        };
        var items = new List<PivotViewerItem>();
        var item1 = new PivotViewerItem("1");
        item1.Add(prop, new object[] { "abc" });
        var item2 = new PivotViewerItem("2");
        item2.Add(prop, new object[] { "abd" });
        var item3 = new PivotViewerItem("3");
        item3.Add(prop, new object[] { "xyz" });
        items.AddRange(new[] { item1, item2, item3 });

        index.Build(items, new[] { prop });
        var results = index.Search("a");
        Assert.True(results.Count >= 2);
    }

    [Fact]
    public void WordWheelIndex_SearchWithFullMatch()
    {
        var index = new WordWheelIndex();
        var prop = new PivotViewerStringProperty("name")
        {
            DisplayName = "Name",
            Options = PivotViewerPropertyOptions.CanSearchText
        };
        var item = new PivotViewerItem("1");
        item.Add(prop, new object[] { "hello world" });
        index.Build(new[] { item }, new[] { prop });

        var results = index.Search("hello");
        Assert.True(results.Count >= 1);
    }

    [Fact]
    public void WordWheelIndex_GetMatchingItems()
    {
        var index = new WordWheelIndex();
        var prop = new PivotViewerStringProperty("name")
        {
            DisplayName = "Name",
            Options = PivotViewerPropertyOptions.CanSearchText
        };
        var item1 = new PivotViewerItem("1");
        item1.Add(prop, new object[] { "red car" });
        var item2 = new PivotViewerItem("2");
        item2.Add(prop, new object[] { "blue car" });
        var item3 = new PivotViewerItem("3");
        item3.Add(prop, new object[] { "red bike" });

        index.Build(new[] { item1, item2, item3 }, new[] { prop });

        var carItems = index.GetMatchingItems("car");
        Assert.Equal(2, carItems.Count);

        var redItems = index.GetMatchingItems("red");
        Assert.Equal(2, redItems.Count);
    }

    [Fact]
    public void WordWheelIndex_GetCharBuckets()
    {
        var index = new WordWheelIndex();
        var prop = new PivotViewerStringProperty("name")
        {
            DisplayName = "Name",
            Options = PivotViewerPropertyOptions.CanSearchText
        };
        var items = new List<PivotViewerItem>();
        foreach (var name in new[] { "apple", "banana", "avocado", "cherry" })
        {
            var item = new PivotViewerItem(name);
            item.Add(prop, new object[] { name });
            items.Add(item);
        }

        index.Build(items, new[] { prop });

        var buckets = index.GetCharBuckets();
        Assert.True(buckets.Count > 0);
        // 'a' bucket should have at least 2 entries (apple, avocado)
        var aBucket = buckets.FirstOrDefault(b => b.Character == 'a');
        Assert.True(aBucket.EntryCount >= 2);
    }

    // --- ViewerStateSerializer edge cases ---

    [Fact]
    public void ViewerStateSerializer_EmptyState()
    {
        var state = new ViewerState();
        var serialized = ViewerStateSerializer.Serialize(state);
        Assert.NotNull(serialized);

        var restored = ViewerStateSerializer.Deserialize(serialized);
        Assert.NotNull(restored);
    }

    [Fact]
    public void ViewerStateSerializer_NullInput()
    {
        var result = ViewerStateSerializer.Deserialize(null!);
        Assert.NotNull(result);
    }

    [Fact]
    public void ViewerStateSerializer_EmptyString()
    {
        var result = ViewerStateSerializer.Deserialize("");
        Assert.NotNull(result);
    }
}
