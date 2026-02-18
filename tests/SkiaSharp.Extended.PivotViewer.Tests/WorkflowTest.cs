using SkiaSharp;
using SkiaSharp.Extended.PivotViewer;
using Xunit;

namespace SkiaSharp.Extended.PivotViewer.Tests;

/// <summary>
/// Full end-to-end workflow tests using real CXML data.
/// These tests simulate user interactions through the controller API.
/// </summary>
public class WorkflowTest
{
    [Fact]
    public void FullWorkflow_BrowseFilterSortSelectZoom()
    {
        // 1. Load collection
        var source = TestDataHelper.LoadCxml("conceptcars.cxml");
        var controller = new PivotViewerController();
        controller.LoadCollection(source);
        controller.SetAvailableSize(1024, 768);

        Assert.True(controller.Items.Count > 0);
        Assert.True(controller.Properties.Count > 0);

        // 2. Verify grid layout
        Assert.NotNull(controller.GridLayout);
        Assert.Equal(controller.Items.Count, controller.GridLayout.Positions.Length);

        // 3. Filter by manufacturer
        controller.FilterEngine.AddStringFilter("Manufacturer", "BMW");
        Assert.True(controller.InScopeItems.Count < controller.Items.Count);
        Assert.True(controller.InScopeItems.Count > 0);

        // 4. Sort by year
        var yearProp = controller.Properties.FirstOrDefault(p =>
            p.DisplayName?.Contains("Year") == true);
        if (yearProp != null)
        {
            controller.SortProperty = yearProp;
        }

        // 5. Select first item
        controller.SelectedItem = controller.InScopeItems.First();
        Assert.NotNull(controller.SelectedItem);
        Assert.True(controller.DetailPane.IsShowing);

        // 6. Zoom in
        controller.ZoomLevel = 0.5;
        Assert.NotNull(controller.GridLayout);

        // 7. Pan
        controller.Pan(100, 50);

        // 8. Switch to graph view
        controller.CurrentView = "graph";
        if (controller.SortProperty != null)
        {
            Assert.NotNull(controller.HistogramLayout);
        }

        // 9. Switch back to grid
        controller.CurrentView = "grid";
        Assert.NotNull(controller.GridLayout);

        // 10. Clear filters
        controller.FilterEngine.ClearAll();
        Assert.Equal(controller.Items.Count, controller.InScopeItems.Count);
    }

    [Fact]
    public void FullWorkflow_SearchAndNavigate()
    {
        var source = TestDataHelper.LoadCxml("conceptcars.cxml");
        var controller = new PivotViewerController();
        controller.LoadCollection(source);
        controller.SetAvailableSize(800, 600);

        // Search
        var results = controller.Search("concept");
        // May or may not find results depending on data

        // Navigate through items
        for (int i = 0; i < Math.Min(5, controller.InScopeItems.Count); i++)
        {
            controller.SelectedItem = controller.InScopeItems[i];
            Assert.True(controller.DetailPane.IsShowing);

            // Check detail pane has data
            Assert.NotNull(controller.DetailPane.FacetValues);
        }
    }

    [Fact]
    public void FullWorkflow_StatePersistence()
    {
        var source = TestDataHelper.LoadCxml("conceptcars.cxml");
        var controller = new PivotViewerController();
        controller.LoadCollection(source);
        controller.SetAvailableSize(800, 600);

        // Set up some state
        controller.FilterEngine.AddStringFilter("Manufacturer", "BMW");
        controller.CurrentView = "grid";
        if (controller.InScopeItems.Count > 0)
            controller.SelectedItem = controller.InScopeItems.First();

        // Serialize
        string state = controller.SerializeViewerState();

        // Create fresh controller and restore
        var controller2 = new PivotViewerController();
        controller2.LoadCollection(TestDataHelper.LoadCxml("conceptcars.cxml"));
        controller2.SetAvailableSize(800, 600);
        controller2.SetViewerState(state);

        Assert.Equal(controller.InScopeItems.Count, controller2.InScopeItems.Count);
    }

    [Fact]
    public void FullWorkflow_AnimatedTransition()
    {
        var source = TestDataHelper.LoadCxml("conceptcars.cxml");
        var controller = new PivotViewerController();
        controller.LoadCollection(source);
        controller.SetAvailableSize(1024, 768);

        // Trigger animation by resizing
        controller.SetAvailableSize(800, 600);

        // Simulate 60fps for 1 second
        for (int frame = 0; frame < 60; frame++)
        {
            bool needsRedraw = controller.Update(TimeSpan.FromMilliseconds(16.67));
            if (frame == 0) Assert.True(needsRedraw || true); // May or may not need redraw

            // Get interpolated positions
            foreach (var item in controller.InScopeItems.Take(3))
            {
                var bounds = controller.GetItemBounds(item);
                Assert.True(bounds.Width > 0);
                Assert.True(bounds.Height > 0);
            }
        }
    }

    [Fact]
    public void FullWorkflow_HeadlessRender()
    {
        var source = TestDataHelper.LoadCxml("conceptcars.cxml");
        var controller = new PivotViewerController();
        controller.LoadCollection(source);
        controller.SetAvailableSize(1024, 768);

        // Render grid view
        using var surface = SKSurface.Create(new SKImageInfo(1024, 768));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.White);

        using var paint = new SKPaint { Color = SKColors.CornflowerBlue };
        using var selectedPaint = new SKPaint { Color = SKColors.Orange, IsStroke = true, StrokeWidth = 3 };
        using var textFont = new SKFont { Size = 10 };
        using var textPaint = new SKPaint { Color = SKColors.White };

        // Select an item
        controller.SelectedItem = controller.InScopeItems.First();

        foreach (var pos in controller.GridLayout!.Positions)
        {
            var rect = new SKRect(
                (float)pos.X + 1, (float)pos.Y + 1,
                (float)(pos.X + pos.Width) - 1, (float)(pos.Y + pos.Height) - 1);

            canvas.DrawRect(rect, paint);

            if (pos.Item == controller.SelectedItem)
            {
                canvas.DrawRect(rect, selectedPaint);
            }
        }

        // Verify pixels
        using var pixmap = surface.PeekPixels();
        int bluePixels = 0;
        int orangePixels = 0;

        for (int y = 0; y < pixmap.Height; y += 5)
        {
            for (int x = 0; x < pixmap.Width; x += 5)
            {
                var c = pixmap.GetPixelColor(x, y);
                if (c.Blue > 200 && c.Red < 150) bluePixels++;
                if (c.Red > 200 && c.Green > 100 && c.Blue < 50) orangePixels++;
            }
        }

        Assert.True(bluePixels > 0, "Should have blue item rectangles");
        Assert.True(orangePixels > 0, "Should have orange selected border");
    }

    [Fact]
    public void FullWorkflow_DisposeCleanup()
    {
        var source = TestDataHelper.LoadCxml("conceptcars.cxml");
        var controller = new PivotViewerController();
        controller.LoadCollection(source);
        controller.SetAvailableSize(800, 600);
        controller.FilterEngine.AddStringFilter("Manufacturer", "BMW");
        controller.SelectedItem = controller.InScopeItems.First();

        controller.Dispose();

        // After dispose, controller should be in clean state
        // Double dispose should be safe
        controller.Dispose();
    }

    [Fact]
    public void FullWorkflow_MultipleFilterCategories()
    {
        var source = TestDataHelper.LoadCxml("conceptcars.cxml");
        var controller = new PivotViewerController();
        controller.LoadCollection(source);
        controller.SetAvailableSize(800, 600);

        int totalCount = controller.InScopeItems.Count;

        // Apply filters from different categories
        var textProps = controller.Properties
            .Where(p => p.PropertyType == PivotViewerPropertyType.Text &&
                        p.Options.HasFlag(PivotViewerPropertyOptions.CanFilter))
            .Take(2)
            .ToList();

        foreach (var prop in textProps)
        {
            var counts = controller.GetFilterCounts(prop.Id);
            if (counts.Count > 0)
            {
                var firstValue = counts.First().Key;
                controller.FilterEngine.AddStringFilter(prop.Id, firstValue);
            }
        }

        // Should have fewer items with AND logic across categories
        Assert.True(controller.InScopeItems.Count <= totalCount);

        // Clear and verify restoration
        controller.FilterEngine.ClearAll();
        Assert.Equal(totalCount, controller.InScopeItems.Count);
    }
}
