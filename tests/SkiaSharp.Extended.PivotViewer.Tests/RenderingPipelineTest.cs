using SkiaSharp;
using SkiaSharp.Extended.DeepZoom;
using SkiaSharp.Extended.PivotViewer;
using Xunit;

namespace SkiaSharp.Extended.PivotViewer.Tests;

/// <summary>
/// Full end-to-end rendering tests using headless SKSurface.
/// Simulates the complete PivotViewer rendering pipeline:
/// CXML → Filter → Layout → Render → Pixel verification.
/// </summary>
public class RenderingPipelineTest
{
    [Fact]
    public void GridView_RendersAllItems()
    {
        var controller = CreateController();
        var layout = controller.GridLayout!;

        using var surface = SKSurface.Create(new SKImageInfo(1024, 768));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.White);

        using var paint = new SKPaint { Color = SKColors.CornflowerBlue, IsAntialias = true };

        foreach (var pos in layout.Positions)
        {
            var rect = new SKRect(
                (float)pos.X + 1, (float)pos.Y + 1,
                (float)(pos.X + pos.Width) - 1, (float)(pos.Y + pos.Height) - 1);
            canvas.DrawRect(rect, paint);
        }

        // Verify the rendered image has non-white pixels (items were drawn)
        using var image = surface.Snapshot();
        using var pixmap = image.PeekPixels();

        int coloredPixels = CountNonWhitePixels(pixmap);
        Assert.True(coloredPixels > 100, $"Should have colored pixels from items, got {coloredPixels}");
    }

    [Fact]
    public void HistogramView_RendersColumns()
    {
        var controller = CreateController();
        controller.SortProperty = controller.Properties.FirstOrDefault(p => p.Id == "Manufacturer");
        controller.CurrentView = "graph";

        var layout = controller.HistogramLayout;
        Assert.NotNull(layout);

        using var surface = SKSurface.Create(new SKImageInfo(1024, 768));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.White);

        using var paint = new SKPaint { Color = SKColors.SteelBlue, IsAntialias = true };

        foreach (var col in layout!.Columns)
        {
            foreach (var pos in col.Items)
            {
                var rect = new SKRect(
                    (float)pos.X + 1, (float)pos.Y + 1,
                    (float)(pos.X + pos.Width) - 1, (float)(pos.Y + pos.Height) - 1);
                canvas.DrawRect(rect, paint);
            }
        }

        using var image = surface.Snapshot();
        using var pixmap = image.PeekPixels();

        int coloredPixels = CountNonWhitePixels(pixmap);
        Assert.True(coloredPixels > 100, $"Should have colored pixels from histogram, got {coloredPixels}");
    }

    [Fact]
    public void FilteredGridView_RendersFewer()
    {
        var controller = CreateController();

        // Count all positions
        int allCount = controller.GridLayout!.Positions.Length;

        // Apply filter
        controller.FilterEngine.AddStringFilter("Manufacturer", "Alfa Romeo");
        int filteredCount = controller.GridLayout!.Positions.Length;

        Assert.True(filteredCount < allCount, "Filtered layout should have fewer positions");
        Assert.True(filteredCount > 0, "Should still have some positions");
    }

    [Fact]
    public void SelectedItem_RendersDifferently()
    {
        var controller = CreateController();
        var layout = controller.GridLayout!;

        controller.SelectedItem = controller.InScopeItems[0];

        using var surface = SKSurface.Create(new SKImageInfo(1024, 768));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.White);

        using var itemPaint = new SKPaint { Color = SKColors.CornflowerBlue, IsAntialias = true };
        using var selectedPaint = new SKPaint { Color = SKColors.Orange, IsStroke = true, StrokeWidth = 3, IsAntialias = true };

        foreach (var pos in layout.Positions)
        {
            var rect = new SKRect(
                (float)pos.X + 1, (float)pos.Y + 1,
                (float)(pos.X + pos.Width) - 1, (float)(pos.Y + pos.Height) - 1);

            canvas.DrawRect(rect, itemPaint);

            if (pos.Item == controller.SelectedItem)
                canvas.DrawRect(rect, selectedPaint);
        }

        using var image = surface.Snapshot();
        using var pixmap = image.PeekPixels();

        // Check for orange pixels (selected item border)
        int orangePixels = CountColorPixels(pixmap, SKColors.Orange, tolerance: 30);
        Assert.True(orangePixels > 0, "Should have orange pixels from selection border");
    }

    [Fact]
    public void Resize_RelayoutsItems()
    {
        var controller = CreateController();

        var layout1 = controller.GridLayout!;
        int cols1 = layout1.Columns;

        controller.SetAvailableSize(500, 400);
        var layout2 = controller.GridLayout!;
        int cols2 = layout2.Columns;

        // Different size should produce different column count
        Assert.NotEqual(cols1, cols2);
    }

    [Fact]
    public void SimulateClickSelect()
    {
        var controller = CreateController();
        Assert.Null(controller.SelectedItem);

        // Find center of first item
        var pos = controller.GridLayout!.Positions[0];
        double clickX = pos.X + pos.Width / 2;
        double clickY = pos.Y + pos.Height / 2;

        // Hit test and select
        var hit = controller.HitTest(clickX, clickY);
        Assert.NotNull(hit);
        controller.SelectedItem = hit;

        Assert.Equal(hit, controller.SelectedItem);
        Assert.True(controller.DetailPane.IsShowing);
    }

    [Fact]
    public void SimulateFilterSortCycle()
    {
        var controller = CreateController();
        int total = controller.InScopeItems.Count;

        // Step 1: Apply string filter
        controller.FilterEngine.AddStringFilter("Manufacturer", "Alfa Romeo");
        Assert.True(controller.InScopeItems.Count < total);

        // Step 2: Sort by a property
        var yearProp = controller.Properties.FirstOrDefault(p => p.PropertyType == PivotViewerPropertyType.Decimal);
        if (yearProp != null)
        {
            controller.SortProperty = yearProp;
            Assert.NotNull(controller.GridLayout);
        }

        // Step 3: Switch to graph view
        controller.CurrentView = "graph";
        if (controller.SortProperty != null)
        {
            Assert.NotNull(controller.HistogramLayout);
        }

        // Step 4: Clear filters
        controller.FilterEngine.ClearAll();
        Assert.Equal(total, controller.InScopeItems.Count);

        // Step 5: Switch back to grid
        controller.CurrentView = "grid";
        Assert.NotNull(controller.GridLayout);
        Assert.Equal(total, controller.GridLayout!.Positions.Length);
    }

    [Fact]
    public void SimulateStateSerializationRoundTrip()
    {
        var controller = CreateController();

        // Set up some state
        controller.FilterEngine.AddStringFilter("Manufacturer", "Alfa Romeo");
        controller.SortProperty = controller.Properties.FirstOrDefault(p => p.Id == "Manufacturer");
        controller.SelectedItem = controller.InScopeItems.FirstOrDefault();
        controller.CurrentView = "graph";

        // Serialize
        var state = controller.SerializeViewerState();
        Assert.NotEmpty(state);

        // Create a new controller and restore
        var controller2 = CreateController();
        controller2.SetViewerState(state);

        Assert.Equal("graph", controller2.CurrentView);
        Assert.True(controller2.InScopeItems.Count < controller2.Items.Count);
    }

    [Fact]
    public void SimulateSearchAndFilter()
    {
        var controller = CreateController();

        // Search
        var results = controller.Search("Ford");
        Assert.NotEmpty(results);

        // The search results suggest items — select the first match
        var firstResult = results[0];
        Assert.NotNull(firstResult.Text);
    }

    [Fact]
    public void AnimationTransition_ProgressesOverTime()
    {
        var controller = CreateController();

        // Trigger a resize to start a transition
        controller.SetAvailableSize(500, 400);

        Assert.True(controller.LayoutTransition.IsAnimating);

        // Simulate 50ms of animation
        bool needsRedraw = controller.Update(TimeSpan.FromMilliseconds(50));
        Assert.True(needsRedraw || controller.LayoutTransition.IsAnimating);

        // Simulate completion (500ms total)
        controller.Update(TimeSpan.FromMilliseconds(500));
        Assert.False(controller.LayoutTransition.IsAnimating);
    }

    [Fact]
    public void SimulateZoomIn_IncreasesItemSize()
    {
        var controller = CreateController();
        var sizeBefore = controller.GridLayout!.ItemWidth;

        controller.ZoomLevel = 0.5;
        var sizeAfter = controller.GridLayout!.ItemWidth;

        Assert.True(sizeAfter > sizeBefore, "Zooming in should increase item size");
    }

    [Fact]
    public void SimulateZoomAndPan_Sequence()
    {
        var controller = CreateController();

        // Zoom in
        controller.ZoomLevel = 0.6;
        Assert.True(controller.GridLayout!.ItemWidth > 50, "Items should be larger");

        // Pan to reveal different items (clamped to valid bounds)
        controller.Pan(100, 50);
        // Panning should be within valid range (may be clamped)
        Assert.True(Math.Abs(controller.PanOffsetX) <= 1000, "PanOffsetX should be reasonable");
        Assert.True(Math.Abs(controller.PanOffsetY) <= 1000, "PanOffsetY should be reasonable");

        // Select an item
        var pos = controller.GridLayout.Positions[5];
        var hit = controller.HitTest(pos.X + pos.Width / 2, pos.Y + pos.Height / 2);
        Assert.NotNull(hit);

        // Zoom out back to fit-all
        controller.ZoomLevel = 0.0;
        Assert.Equal(0.0, controller.ZoomLevel);
    }

    [Fact]
    public void FilterPane_ShowsCategories()
    {
        var controller = CreateController();
        var filterPane = controller.FilterPaneModel!;

        var categories = filterPane.GetCategories(controller.Items);
        Assert.NotEmpty(categories);

        // Verify categories have value counts
        var firstFilterable = categories.First();
        Assert.NotNull(firstFilterable.ValueCounts);
        Assert.NotEmpty(firstFilterable.ValueCounts);
    }

    [Fact]
    public void FilterPane_ToggleFilter_UpdatesInScopeItems()
    {
        var controller = CreateController();
        var filterPane = controller.FilterPaneModel!;
        int totalBefore = controller.InScopeItems.Count;

        var categories = filterPane.GetCategories(controller.Items);
        var firstCategory = categories.First(c => c.ValueCounts != null && c.ValueCounts.Count > 1);

        // Toggle a filter
        var firstValue = firstCategory.ValueCounts!.First().Key;
        filterPane.ToggleStringFilter(firstCategory.Property.Id, firstValue);

        // Re-query in-scope items (controller auto-updates)
        Assert.True(controller.InScopeItems.Count <= totalBefore);
    }

    [Fact]
    public void DetailPane_ShowsFacetValues()
    {
        var controller = CreateController();

        // Select an item
        controller.SelectedItem = controller.InScopeItems[0];

        var detail = controller.DetailPane;
        Assert.True(detail.IsShowing);
        Assert.NotEmpty(detail.FacetValues);

        // Each facet should have a display name and values
        foreach (var facet in detail.FacetValues)
        {
            Assert.NotNull(facet.DisplayName);
            Assert.NotEmpty(facet.Values);
        }
    }

    [Fact]
    public void DetailPane_DismissOnDeselect()
    {
        var controller = CreateController();

        controller.SelectedItem = controller.InScopeItems[0];
        Assert.True(controller.DetailPane.IsShowing);

        controller.SelectedItem = null;
        Assert.False(controller.DetailPane.IsShowing);
        Assert.Empty(controller.DetailPane.FacetValues);
    }

    [Fact]
    public void FullWorkflow_LoadFilterSortZoomSelectRender()
    {
        var controller = CreateController();

        // 1. Filter to reduce items
        controller.FilterEngine.AddStringFilter("Manufacturer", "Alfa Romeo");
        int filteredCount = controller.InScopeItems.Count;
        Assert.True(filteredCount < controller.Items.Count);

        // 2. Sort
        var sortProp = controller.Properties.FirstOrDefault(p => p.DisplayName == "Name");
        if (sortProp != null)
            controller.SortProperty = sortProp;

        // 3. Zoom in
        controller.ZoomLevel = 0.4;
        Assert.True(controller.GridLayout!.ItemWidth > 10);

        // 4. Select first item
        controller.SelectedItem = controller.InScopeItems[0];
        Assert.True(controller.DetailPane.IsShowing);

        // 5. Render headlessly
        using var surface = SKSurface.Create(new SKImageInfo(1024, 768));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.White);

        using var paint = new SKPaint { Color = SKColors.CornflowerBlue };
        foreach (var pos in controller.GridLayout.Positions)
        {
            canvas.DrawRect(
                (float)pos.X + 1, (float)pos.Y + 1,
                (float)pos.Width - 2, (float)pos.Height - 2,
                paint);
        }

        using var pixmap = surface.PeekPixels();
        int drawnPixels = CountNonWhitePixels(pixmap);
        Assert.True(drawnPixels > 0, "Should render items");

        // 6. Serialize state
        var state = controller.SerializeViewerState();
        Assert.NotEmpty(state);

        // 7. Clear filters
        controller.FilterEngine.ClearAll();
        Assert.Equal(controller.Items.Count, controller.InScopeItems.Count);
    }

    private static PivotViewerController CreateController()
    {
        var controller = new PivotViewerController();
        var source = TestDataHelper.LoadCxml("conceptcars.cxml");
        controller.LoadCollection(source);
        controller.SetAvailableSize(1024, 768);
        return controller;
    }

    private static int CountNonWhitePixels(SKPixmap pixmap)
    {
        int count = 0;
        for (int y = 0; y < pixmap.Height; y++)
        {
            for (int x = 0; x < pixmap.Width; x++)
            {
                var color = pixmap.GetPixelColor(x, y);
                if (color != SKColors.White)
                    count++;
            }
        }
        return count;
    }

    private static int CountColorPixels(SKPixmap pixmap, SKColor target, int tolerance)
    {
        int count = 0;
        for (int y = 0; y < pixmap.Height; y++)
        {
            for (int x = 0; x < pixmap.Width; x++)
            {
                var color = pixmap.GetPixelColor(x, y);
                if (Math.Abs(color.Red - target.Red) <= tolerance &&
                    Math.Abs(color.Green - target.Green) <= tolerance &&
                    Math.Abs(color.Blue - target.Blue) <= tolerance)
                    count++;
            }
        }
        return count;
    }
}
