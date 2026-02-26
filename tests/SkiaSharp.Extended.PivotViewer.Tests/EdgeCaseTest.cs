using SkiaSharp;
using SkiaSharp.Extended.DeepZoom;
using SkiaSharp.Extended.PivotViewer;
using Xunit;

namespace SkiaSharp.Extended.PivotViewer.Tests;

/// <summary>
/// Edge case and corner case tests for robustness.
/// </summary>
public class EdgeCaseTest
{
    // --- Empty collection ---

    [Fact]
    public void EmptyCollection_NoExceptions()
    {
        var controller = new PivotViewerController();
        controller.SetAvailableSize(800, 600);

        Assert.Empty(controller.Items);
        Assert.Empty(controller.InScopeItems);
        Assert.Null(controller.GridLayout);
        Assert.Null(controller.SelectedItem);
    }

    [Fact]
    public void EmptyCollection_SearchReturnsEmpty()
    {
        var controller = new PivotViewerController();
        var results = controller.Search("anything");
        Assert.Empty(results);
    }

    [Fact]
    public void EmptyCollection_FilterCountsEmpty()
    {
        var controller = new PivotViewerController();
        var counts = controller.GetFilterCounts("nonexistent");
        Assert.Empty(counts);
    }

    [Fact]
    public void EmptyCollection_SerializeDeserializeState()
    {
        var controller = new PivotViewerController();
        var state = controller.SerializeViewerState();
        Assert.NotNull(state);

        controller.SetViewerState(state);
        // Should not throw
    }

    // --- Single item ---

    [Fact]
    public void SingleItem_GridLayoutWorks()
    {
        var controller = new PivotViewerController();
        var prop = new PivotViewerStringProperty("Name") { DisplayName = "Name" };
        var item = new PivotViewerItem("only");
        item.Set(prop, new object[] { "Only Item" });

        controller.LoadItems(new[] { item }, new[] { prop });
        controller.SetAvailableSize(800, 600);

        Assert.NotNull(controller.GridLayout);
        Assert.Single(controller.GridLayout.Positions);
    }

    [Fact]
    public void SingleItem_SelectAndDeselect()
    {
        var controller = new PivotViewerController();
        var prop = new PivotViewerStringProperty("Name") { DisplayName = "Name" };
        var item = new PivotViewerItem("only");
        item.Set(prop, new object[] { "Only Item" });

        controller.LoadItems(new[] { item }, new[] { prop });
        controller.SetAvailableSize(800, 600);

        controller.SelectedItem = item;
        Assert.True(controller.DetailPane.IsShowing);

        controller.SelectedItem = null;
        Assert.False(controller.DetailPane.IsShowing);
    }

    // --- Max zoom behavior ---

    [Fact]
    public void MaxZoom_SingleColumn()
    {
        var controller = CreateSmallController(10);

        controller.ZoomLevel = 1.0;
        var layout = controller.GridLayout;
        Assert.NotNull(layout);
        Assert.Equal(1, layout.Columns);
    }

    // --- Zero-size view ---

    [Fact]
    public void ZeroSize_DoesNotCrash()
    {
        var controller = CreateSmallController(10);
        controller.SetAvailableSize(0, 0);
        // Should handle gracefully
    }

    [Fact]
    public void NegativeSize_DoesNotCrash()
    {
        var controller = CreateSmallController(10);
        controller.SetAvailableSize(-100, -100);
        // Should handle gracefully
    }

    // --- Rapid filter changes ---

    [Fact]
    public void RapidFilterToggle_DoesNotCrash()
    {
        var controller = new PivotViewerController();
        var source = TestDataHelper.LoadCxml("conceptcars.cxml");
        controller.LoadCollection(source);
        controller.SetAvailableSize(800, 600);

        for (int i = 0; i < 20; i++)
        {
            controller.FilterEngine.AddStringFilter("Manufacturer", "BMW");
            controller.FilterEngine.RemoveStringFilter("Manufacturer", "BMW");
        }

        Assert.Equal(controller.Items.Count, controller.InScopeItems.Count);
    }

    // --- Rapid zoom changes ---

    [Fact]
    public void RapidZoom_DoesNotCrash()
    {
        var controller = CreateSmallController(20);

        for (int i = 0; i < 50; i++)
        {
            controller.ZoomLevel = i % 2 == 0 ? 0.8 : 0.0;
        }

        // Should stabilize
        Assert.Equal(0.0, controller.ZoomLevel);
    }

    // --- Multiple sorts ---

    [Fact]
    public void ChangeSort_MultipleTimesDoesNotCrash()
    {
        var controller = new PivotViewerController();
        var source = TestDataHelper.LoadCxml("conceptcars.cxml");
        controller.LoadCollection(source);
        controller.SetAvailableSize(800, 600);

        foreach (var prop in controller.Properties)
        {
            controller.SortProperty = prop;
        }

        controller.SortProperty = null;
    }

    // --- HitTest edge cases ---

    [Fact]
    public void HitTest_OutOfBounds_ReturnsNull()
    {
        var controller = CreateSmallController(10);

        var hit = controller.HitTest(-100, -100);
        Assert.Null(hit);

        hit = controller.HitTest(10000, 10000);
        Assert.Null(hit);
    }

    [Fact]
    public void HitTest_ExactlyOnItem_ReturnsItem()
    {
        var controller = CreateSmallController(5);

        var pos = controller.GridLayout!.Positions[0];
        var hit = controller.HitTest(pos.X + 1, pos.Y + 1);
        Assert.NotNull(hit);
    }

    // --- View switching ---

    [Fact]
    public void ViewSwitch_GridToGraph_ClearsGridLayout()
    {
        var controller = new PivotViewerController();
        var source = TestDataHelper.LoadCxml("conceptcars.cxml");
        controller.LoadCollection(source);
        controller.SetAvailableSize(800, 600);

        Assert.NotNull(controller.GridLayout);

        // Switch to graph — need a sort property for histogram
        controller.SortProperty = controller.Properties.First(p =>
            p.PropertyType == PivotViewerPropertyType.Text);
        controller.CurrentView = "graph";

        Assert.NotNull(controller.HistogramLayout);
        Assert.Null(controller.GridLayout);
    }

    [Fact]
    public void ViewSwitch_GraphToGrid_ClearsHistogramLayout()
    {
        var controller = new PivotViewerController();
        var source = TestDataHelper.LoadCxml("conceptcars.cxml");
        controller.LoadCollection(source);
        controller.SetAvailableSize(800, 600);

        controller.SortProperty = controller.Properties.First(p =>
            p.PropertyType == PivotViewerPropertyType.Text);
        controller.CurrentView = "graph";

        controller.CurrentView = "grid";
        Assert.NotNull(controller.GridLayout);
        Assert.Null(controller.HistogramLayout);
    }

    // --- Item with no properties ---

    [Fact]
    public void ItemWithNoProperties_HandledGracefully()
    {
        var controller = new PivotViewerController();
        var item = new PivotViewerItem("bare");
        controller.LoadItems(new[] { item }, Array.Empty<PivotViewerProperty>());
        controller.SetAvailableSize(800, 600);

        Assert.Single(controller.InScopeItems);
        Assert.NotNull(controller.GridLayout);

        controller.SelectedItem = item;
        Assert.Empty(controller.DetailPane.FacetValues);
    }

    // --- Multi-valued facets ---

    [Fact]
    public void MultiValuedFacet_ShowsAllValues()
    {
        var controller = new PivotViewerController();
        var tagProp = new PivotViewerStringProperty("Tags") { DisplayName = "Tags" };
        var item = new PivotViewerItem("multi");
        item.Set(tagProp, new object[] { "fast", "red", "italian" });

        controller.LoadItems(new[] { item }, new[] { tagProp });
        controller.SelectedItem = item;

        var facets = controller.DetailPane.FacetValues;
        Assert.Single(facets);
        Assert.Equal(3, facets[0].Values.Count);
    }

    // --- Concurrent UpdateLayout during animation ---

    [Fact]
    public void UpdateDuringAnimation_Stable()
    {
        var controller = CreateSmallController(20);

        // Trigger animation via resize
        controller.SetAvailableSize(500, 400);

        // Simulate animation frames while changing zoom
        for (int i = 0; i < 10; i++)
        {
            controller.Update(TimeSpan.FromMilliseconds(16));
            if (i == 5) controller.ZoomLevel = 0.3;
        }

        // Should stabilize
        controller.Update(TimeSpan.FromMilliseconds(500));
    }

    // --- ItemPosition equality ---

    [Fact]
    public void ItemPosition_HasCorrectBounds()
    {
        var item = new PivotViewerItem("1");
        var pos = new ItemPosition(item, 10, 20, 100, 50);

        Assert.Equal(10, pos.X);
        Assert.Equal(20, pos.Y);
        Assert.Equal(100, pos.Width);
        Assert.Equal(50, pos.Height);
        Assert.Same(item, pos.Item);
    }

    // --- Dispose safety ---

    [Fact]
    public void DoubleDispose_NoException()
    {
        var controller = CreateSmallController(5);
        controller.Dispose();
        controller.Dispose(); // Should not throw
    }

    // --- Headless render after filter ---

    [Fact]
    public void RenderAfterFilter_ReducedItemCount()
    {
        var controller = new PivotViewerController();
        var source = TestDataHelper.LoadCxml("conceptcars.cxml");
        controller.LoadCollection(source);
        controller.SetAvailableSize(1024, 768);

        int beforeCount = controller.GridLayout!.Positions.Length;

        controller.FilterEngine.AddStringFilter("Manufacturer", "BMW");
        int afterCount = controller.GridLayout!.Positions.Length;

        Assert.True(afterCount < beforeCount, "Filtering should reduce positions");

        // Verify rendering
        using var surface = SKSurface.Create(new SKImageInfo(1024, 768));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.White);

        using var paint = new SKPaint { Color = SKColors.Blue };
        foreach (var pos in controller.GridLayout.Positions)
        {
            canvas.DrawRect((float)pos.X, (float)pos.Y, (float)pos.Width, (float)pos.Height, paint);
        }

        // Verify pixels were drawn
        using var pixmap = surface.PeekPixels();
        bool hasBlue = false;
        for (int y = 0; y < pixmap.Height && !hasBlue; y += 10)
        {
            for (int x = 0; x < pixmap.Width && !hasBlue; x += 10)
            {
                var color = pixmap.GetPixelColor(x, y);
                if (color.Blue > 200 && color.Red < 50) hasBlue = true;
            }
        }
        Assert.True(hasBlue, "Should have blue pixels from rendered items");
    }

    private static PivotViewerController CreateSmallController(int count)
    {
        var controller = new PivotViewerController();
        var nameProp = new PivotViewerStringProperty("Name") { DisplayName = "Name" };
        var items = new List<PivotViewerItem>();
        for (int i = 0; i < count; i++)
        {
            var item = new PivotViewerItem(i.ToString());
            item.Set(nameProp, new object[] { $"Item {i}" });
            items.Add(item);
        }
        controller.LoadItems(items, new[] { nameProp });
        controller.SetAvailableSize(800, 600);
        return controller;
    }
}
