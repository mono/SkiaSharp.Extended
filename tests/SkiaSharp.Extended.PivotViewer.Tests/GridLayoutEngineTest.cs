using System.Globalization;
using System.Threading;
using SkiaSharp.Extended.PivotViewer;
using Xunit;

namespace SkiaSharp.Extended.PivotViewer.Tests;

public class GridLayoutEngineTest
{
    private static List<PivotViewerItem> CreateItems(int count)
    {
        var items = new List<PivotViewerItem>();
        for (int i = 0; i < count; i++)
            items.Add(new PivotViewerItem(i.ToString()));
        return items;
    }

    private static List<PivotViewerItem> CreateItemsWithProperty(int count, string propId, string value)
    {
        var prop = new PivotViewerStringProperty(propId) { DisplayName = propId };
        var items = new List<PivotViewerItem>();
        for (int i = 0; i < count; i++)
        {
            var item = new PivotViewerItem(i.ToString());
            item.Add(prop, value);
            items.Add(item);
        }
        return items;
    }

    [Fact]
    public void ComputeLayout_EmptyItems_ReturnsEmptyLayout()
    {
        var engine = new GridLayoutEngine();
        var layout = engine.ComputeLayout(Array.Empty<PivotViewerItem>(), 800, 600);

        Assert.Empty(layout.Positions);
        Assert.Equal(0, layout.Columns);
        Assert.Equal(0, layout.Rows);
    }

    [Fact]
    public void ComputeLayout_SingleItem_ReturnsOnePosition()
    {
        var engine = new GridLayoutEngine();
        var items = CreateItems(1);
        var layout = engine.ComputeLayout(items, 800, 600);

        Assert.Single(layout.Positions);
        Assert.Equal(1, layout.Columns);
        Assert.Equal(1, layout.Rows);
    }

    [Fact]
    public void ComputeLayout_MultipleItems_HasCorrectCount()
    {
        var engine = new GridLayoutEngine();
        var items = CreateItems(12);
        var layout = engine.ComputeLayout(items, 800, 600);

        Assert.Equal(12, layout.Positions.Length);
    }

    [Fact]
    public void ComputeLayout_ItemsFitWithinBounds()
    {
        var engine = new GridLayoutEngine();
        var items = CreateItems(20);
        var layout = engine.ComputeLayout(items, 800, 600);

        foreach (var pos in layout.Positions)
        {
            Assert.True(pos.X >= 0, $"Item X={pos.X} is negative");
            Assert.True(pos.Y >= 0, $"Item Y={pos.Y} is negative");
            Assert.True(pos.X + pos.Width <= 800 + 1, $"Item exceeds width");
            Assert.True(pos.Y + pos.Height <= 600 + 1, $"Item exceeds height");
        }
    }

    [Fact]
    public void ComputeLayout_AllPositionsAreUnique()
    {
        var engine = new GridLayoutEngine();
        var items = CreateItems(16);
        var layout = engine.ComputeLayout(items, 800, 600);

        var positions = new HashSet<(double, double)>();
        foreach (var pos in layout.Positions)
        {
            bool added = positions.Add((Math.Round(pos.X, 2), Math.Round(pos.Y, 2)));
            Assert.True(added, $"Duplicate position at ({pos.X}, {pos.Y})");
        }
    }

    [Fact]
    public void ComputeLayout_ItemsHavePositiveSize()
    {
        var engine = new GridLayoutEngine();
        var items = CreateItems(8);
        var layout = engine.ComputeLayout(items, 800, 600);

        Assert.True(layout.ItemWidth > 0);
        Assert.True(layout.ItemHeight > 0);
    }

    [Fact]
    public void ComputeLayout_RespectsAspectRatio()
    {
        var engine = new GridLayoutEngine();
        var items = CreateItems(4);
        var layout = engine.ComputeLayout(items, 800, 600, itemAspectRatio: 2.0);

        // Width should be approximately 2x height
        Assert.Equal(layout.ItemWidth / layout.ItemHeight, 2.0, 2);
    }

    [Fact]
    public void HitTest_FindsCorrectItem()
    {
        var engine = new GridLayoutEngine();
        var items = CreateItems(4);
        var layout = engine.ComputeLayout(items, 800, 600, itemAspectRatio: 1.0);

        // Hit test the center of the first item
        var first = layout.Positions[0];
        var hit = layout.HitTest(first.X + first.Width / 2, first.Y + first.Height / 2);

        Assert.Equal(first.Item, hit);
    }

    [Fact]
    public void HitTest_ReturnsNull_OutsideItems()
    {
        var engine = new GridLayoutEngine();
        var items = CreateItems(4);
        var layout = engine.ComputeLayout(items, 800, 600);

        var hit = layout.HitTest(-10, -10);
        Assert.Null(hit);
    }

    [Fact]
    public void GetItemAt_ValidCoords_ReturnsItem()
    {
        var engine = new GridLayoutEngine();
        var items = CreateItems(6);
        var layout = engine.ComputeLayout(items, 800, 600);

        var item = layout.GetItemAt(0, 0);
        Assert.NotNull(item);
        Assert.Equal(items[0], item);
    }

    [Fact]
    public void GetItemAt_InvalidCoords_ReturnsNull()
    {
        var engine = new GridLayoutEngine();
        var items = CreateItems(4);
        var layout = engine.ComputeLayout(items, 800, 600);

        var item = layout.GetItemAt(100, 100);
        Assert.Null(item);
    }

    [Fact]
    public void GetItemAt_SecondRow_ReturnsCorrectItem()
    {
        var engine = new GridLayoutEngine();
        var items = CreateItems(9);
        var layout = engine.ComputeLayout(items, 900, 900, itemAspectRatio: 1.0);

        // 9 items in 900x900 with 1:1 aspect → 3 cols, 3 rows
        // Item at (col=1, row=1) → index = 1*3+1 = 4
        var item = layout.GetItemAt(1, 1);
        Assert.NotNull(item);
        Assert.Equal("4", item!.Id);
    }

    [Fact]
    public void GetItemAt_LastItem_ReturnsCorrectItem()
    {
        var engine = new GridLayoutEngine();
        var items = CreateItems(6);
        var layout = engine.ComputeLayout(items, 800, 600);

        int lastIndex = items.Count - 1;
        int lastCol = lastIndex % layout.Columns;
        int lastRow = lastIndex / layout.Columns;
        var item = layout.GetItemAt(lastCol, lastRow);
        Assert.NotNull(item);
        Assert.Equal("5", item!.Id);
    }

    [Fact]
    public void ComputeHistogramLayout_GroupsByProperty()
    {
        var engine = new GridLayoutEngine();
        var catP = new PivotViewerStringProperty("Category") { DisplayName = "Category" };

        var items = new List<PivotViewerItem>();

        var i1 = new PivotViewerItem("1");
        i1.Set(catP, new object[] { "A" });
        items.Add(i1);

        var i2 = new PivotViewerItem("2");
        i2.Set(catP, new object[] { "B" });
        items.Add(i2);

        var i3 = new PivotViewerItem("3");
        i3.Set(catP, new object[] { "A" });
        items.Add(i3);

        var layout = engine.ComputeHistogramLayout(items, "Category", 800, 600);

        Assert.Equal(2, layout.Columns.Length);
        Assert.Equal(3, layout.AllPositions.Length);

        // Column A should have 2 items
        var colA = layout.Columns.First(c => c.Label == "A");
        Assert.Equal(2, colA.Count);

        // Column B should have 1 item
        var colB = layout.Columns.First(c => c.Label == "B");
        Assert.Equal(1, colB.Count);
    }

    [Fact]
    public void ComputeHistogramLayout_EmptyItems_ReturnsEmpty()
    {
        var engine = new GridLayoutEngine();
        var layout = engine.ComputeHistogramLayout(
            Array.Empty<PivotViewerItem>(), "Category", 800, 600);

        Assert.Empty(layout.Columns);
        Assert.Empty(layout.AllPositions);
    }

    [Fact]
    public void ComputeHistogramLayout_ItemsWithNoValue_GroupedAsSuch()
    {
        var engine = new GridLayoutEngine();
        var catP = new PivotViewerStringProperty("Category") { DisplayName = "Category" };

        var items = new List<PivotViewerItem>();

        var i1 = new PivotViewerItem("1");
        i1.Set(catP, new object[] { "A" });
        items.Add(i1);

        var i2 = new PivotViewerItem("2"); // No Category value
        items.Add(i2);

        var layout = engine.ComputeHistogramLayout(items, "Category", 800, 600);

        Assert.Equal(2, layout.Columns.Length);
        Assert.True(layout.Columns.Any(c => c.Label == "(No value)"));
    }

    [Fact]
    public void ComputeLayout_LargeItemCount_Handles()
    {
        var engine = new GridLayoutEngine();
        var items = CreateItems(500);
        var layout = engine.ComputeLayout(items, 1920, 1080);

        Assert.Equal(500, layout.Positions.Length);
        Assert.True(layout.Columns > 1);
        Assert.True(layout.Rows > 1);
    }

    [Fact]
    public void ComputeLayout_WithZoom_ZeroZoomMatchesFitAll()
    {
        var engine = new GridLayoutEngine();
        var items = CreateItems(20);

        var fitAll = engine.ComputeLayout(items, 800, 600);
        var zoomZero = engine.ComputeZoomedLayout(items, 800, 600, 0.0);

        // Zero zoom should be equivalent to fit-all
        Assert.Equal(fitAll.Columns, zoomZero.Columns);
        Assert.Equal(fitAll.Rows, zoomZero.Rows);
    }

    [Fact]
    public void ComputeLayout_WithZoom_FullZoomReducesToSingleColumn()
    {
        var engine = new GridLayoutEngine();
        var items = CreateItems(20);

        var layout = engine.ComputeZoomedLayout(items, 800, 600, 1.0);

        Assert.Equal(1, layout.Columns);
        Assert.Equal(20, layout.Rows);
    }

    [Fact]
    public void ComputeLayout_WithZoom_HalfZoomReducesColumns()
    {
        var engine = new GridLayoutEngine();
        var items = CreateItems(50);

        var fitAll = engine.ComputeLayout(items, 1000, 800);
        var halfZoom = engine.ComputeZoomedLayout(items, 1000, 800, 0.5);

        Assert.True(halfZoom.Columns <= fitAll.Columns, "Half zoom should have fewer or equal columns");
        Assert.True(halfZoom.ItemWidth >= fitAll.ItemWidth, "Half zoom items should be at least as wide");
    }

    [Fact]
    public void ComputeLayout_WithZoom_ItemsSpanBeyondAvailableHeight()
    {
        var engine = new GridLayoutEngine();
        var items = CreateItems(100);

        var layout = engine.ComputeZoomedLayout(items, 800, 600, 0.9);

        // At high zoom, total grid height should exceed available height (scrollable)
        double totalHeight = layout.Rows * layout.ItemHeight;
        Assert.True(totalHeight > 600, "High zoom should produce scrollable content");
    }

    [Fact]
    public void ComputeLayout_WithZoom_AllItemsPlaced()
    {
        var engine = new GridLayoutEngine();
        var items = CreateItems(30);

        var layout = engine.ComputeZoomedLayout(items, 800, 600, 0.5);
        Assert.Equal(30, layout.Positions.Length);

        foreach (var pos in layout.Positions)
        {
            Assert.True(pos.Width > 0);
            Assert.True(pos.Height > 0);
        }
    }

    [Fact]
    public void ComputeLayout_VeryWideAspectRatio_NoDegeneratePositions()
    {
        var engine = new GridLayoutEngine();
        var items = CreateItems(12);
        var layout = engine.ComputeLayout(items, 800, 600, itemAspectRatio: 10.0);

        Assert.Equal(12, layout.Positions.Length);
        Assert.True(layout.ItemWidth > 0);
        Assert.True(layout.ItemHeight > 0);
        foreach (var pos in layout.Positions)
        {
            Assert.True(pos.X >= 0, $"Negative X: {pos.X}");
            Assert.True(pos.Y >= 0, $"Negative Y: {pos.Y}");
            Assert.True(pos.Width > 0);
            Assert.True(pos.Height > 0);
        }
    }

    [Fact]
    public void ComputeLayout_VeryTallAspectRatio_NoDegeneratePositions()
    {
        var engine = new GridLayoutEngine();
        var items = CreateItems(12);
        var layout = engine.ComputeLayout(items, 800, 600, itemAspectRatio: 0.1);

        Assert.Equal(12, layout.Positions.Length);
        Assert.True(layout.ItemWidth > 0);
        Assert.True(layout.ItemHeight > 0);
        foreach (var pos in layout.Positions)
        {
            Assert.True(pos.X >= 0, $"Negative X: {pos.X}");
            Assert.True(pos.Y >= 0, $"Negative Y: {pos.Y}");
            Assert.True(pos.Width > 0);
            Assert.True(pos.Height > 0);
        }
    }

    [Fact]
    public void ComputeLayout_SingleItem_HasValidPosition()
    {
        var engine = new GridLayoutEngine();
        var items = CreateItems(1);
        var layout = engine.ComputeLayout(items, 800, 600, itemAspectRatio: 1.5);

        Assert.Single(layout.Positions);
        var pos = layout.Positions[0];
        Assert.True(pos.X >= 0);
        Assert.True(pos.Y >= 0);
        Assert.True(pos.Width > 0);
        Assert.True(pos.Height > 0);
    }

    [Fact]
    public void ComputeHistogramLayout_PropertyName_MatchesGroupByPropertyId()
    {
        var engine = new GridLayoutEngine();
        var catP = new PivotViewerStringProperty("Category") { DisplayName = "Category" };

        var items = new List<PivotViewerItem>();

        var i1 = new PivotViewerItem("1");
        i1.Set(catP, new object[] { "A" });
        items.Add(i1);

        var i2 = new PivotViewerItem("2");
        i2.Set(catP, new object[] { "B" });
        items.Add(i2);

        var layout = engine.ComputeHistogramLayout(items, "Category", 800, 600);

        Assert.Equal("Category", layout.PropertyName);
    }

    [Fact]
    public void ComputeLayout_VeryLargeCollection_AllItemsPlacedWithValidPositions()
    {
        var engine = new GridLayoutEngine();
        var items = CreateItems(1000);
        var layout = engine.ComputeLayout(items, 1920, 1080);

        Assert.Equal(1000, layout.Positions.Length);
        foreach (var pos in layout.Positions)
        {
            Assert.True(pos.X >= 0, $"Negative X: {pos.X}");
            Assert.True(pos.Y >= 0, $"Negative Y: {pos.Y}");
            Assert.True(pos.Width > 0);
            Assert.True(pos.Height > 0);
        }
    }

    [Fact]
    public void ComputeHistogramLayout_NumericValues_UseInvariantCultureLabels()
    {
        // Regression: numeric group keys must use InvariantCulture ("1.5")
        // not the current thread culture (e.g., "1,5" in de-DE).
        var engine = new GridLayoutEngine();
        var numProp = new PivotViewerNumericProperty("Price") { DisplayName = "Price" };

        var items = new List<PivotViewerItem>();

        var i1 = new PivotViewerItem("1");
        i1.Set(numProp, new object[] { 1.5 });
        items.Add(i1);

        var i2 = new PivotViewerItem("2");
        i2.Set(numProp, new object[] { 2.5 });
        items.Add(i2);

        var savedCulture = Thread.CurrentThread.CurrentCulture;
        try
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");

            var layout = engine.ComputeHistogramLayout(items, "Price", 800, 600);

            Assert.Equal(2, layout.Columns.Length);
            Assert.Contains(layout.Columns, c => c.Label == "1.5");
            Assert.Contains(layout.Columns, c => c.Label == "2.5");
            // Must NOT use locale-specific decimal separator
            Assert.DoesNotContain(layout.Columns, c => c.Label == "1,5");
            Assert.DoesNotContain(layout.Columns, c => c.Label == "2,5");
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = savedCulture;
        }
    }

    [Fact]
    public void ComputeZoomedLayout_ZeroHeight_DoesNotThrow()
    {
        var engine = new GridLayoutEngine();
        var items = CreateItems(10);

        // Zero height should not cause division by zero or overflow
        var layout = engine.ComputeZoomedLayout(items, 800, 0, 0.5);
        Assert.Equal(10, layout.Positions.Length);
    }

    // --- Edge case: itemAspectRatio guard ---

    [Fact]
    public void ComputeHistogramLayout_ZeroAspectRatioDoesNotCrash()
    {
        var engine = new GridLayoutEngine();
        var items = CreateItemsWithProperty(5, "Color", "Red");

        var layout = engine.ComputeHistogramLayout(items, "Color", 800, 600, itemAspectRatio: 0);
        Assert.True(layout.Columns.Length > 0);
    }

    [Fact]
    public void ComputeHistogramLayout_NegativeAspectRatioDoesNotCrash()
    {
        var engine = new GridLayoutEngine();
        var items = CreateItemsWithProperty(5, "Color", "Red");

        var layout = engine.ComputeHistogramLayout(items, "Color", 800, 600, itemAspectRatio: -1.5);
        Assert.True(layout.Columns.Length > 0);
    }
}
