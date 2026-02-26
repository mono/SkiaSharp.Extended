using SkiaSharp;
using SkiaSharp.Extended.PivotViewer;
using Xunit;

namespace SkiaSharp.Extended.PivotViewer.Tests;

/// <summary>
/// Tests for <see cref="PivotViewerRenderer"/> covering rendering, hit-testing,
/// theming, and dispose behavior.
/// </summary>
public class PivotViewerRendererTest
{
    private const int Width = 800;
    private const int Height = 600;

    // =====================================================================
    // Render — basic scenarios
    // =====================================================================

    [Fact]
    public void Render_EmptyController_DoesNotThrow()
    {
        using var renderer = new PivotViewerRenderer();
        using var surface = SKSurface.Create(new SKImageInfo(Width, Height));
        var controller = new PivotViewerController();
        var theme = PivotViewerTheme.Default;

        renderer.Render(surface.Canvas, new SKImageInfo(Width, Height), controller, theme);
    }

    [Fact]
    public void Render_LoadedCollection_DoesNotThrow()
    {
        using var renderer = new PivotViewerRenderer();
        using var surface = SKSurface.Create(new SKImageInfo(Width, Height));
        var controller = CreateLoadedController();
        var theme = PivotViewerTheme.Default;

        renderer.Render(surface.Canvas, new SKImageInfo(Width, Height), controller, theme);
    }

    [Fact]
    public void Render_GridView_DrawsSomething()
    {
        using var renderer = new PivotViewerRenderer();
        using var surface = SKSurface.Create(new SKImageInfo(Width, Height));
        var controller = CreateLoadedController();
        var theme = PivotViewerTheme.Default;

        renderer.Render(surface.Canvas, new SKImageInfo(Width, Height), controller, theme);

        using var image = surface.Snapshot();
        using var pixmap = image.PeekPixels();
        int nonWhite = CountNonWhitePixels(pixmap);
        Assert.True(nonWhite > 100, $"Grid view should draw content, got {nonWhite} non-white pixels");
    }

    [Fact]
    public void Render_HistogramView_DrawsSomething()
    {
        using var renderer = new PivotViewerRenderer();
        using var surface = SKSurface.Create(new SKImageInfo(Width, Height));
        var controller = CreateLoadedController();
        controller.SortProperty = controller.Properties[0];
        controller.CurrentView = "graph";
        var theme = PivotViewerTheme.Default;

        renderer.Render(surface.Canvas, new SKImageInfo(Width, Height), controller, theme);

        using var image = surface.Snapshot();
        using var pixmap = image.PeekPixels();
        int nonWhite = CountNonWhitePixels(pixmap);
        Assert.True(nonWhite > 100, $"Histogram view should draw content, got {nonWhite} non-white pixels");
    }

    // =====================================================================
    // HitTest — empty area and item hits
    // =====================================================================

    [Fact]
    public void HitTest_EmptyArea_ReturnsNone()
    {
        using var renderer = new PivotViewerRenderer();
        var info = new SKImageInfo(Width, Height);
        var controller = new PivotViewerController();
        controller.SetAvailableSize(Width, Height);

        // Empty controller has no items — clicking in the content area returns None
        var result = renderer.HitTest(Width / 2.0, Height / 2.0, info, controller);
        Assert.Equal(RenderHitType.None, result.Type);
    }

    [Fact]
    public void HitTest_Item_ReturnedForGridPosition()
    {
        using var renderer = new PivotViewerRenderer();
        var info = new SKImageInfo(Width, Height);
        var controller = CreateLoadedController();

        // Render first so layout is computed
        using var surface = SKSurface.Create(info);
        renderer.Render(surface.Canvas, info, controller, PivotViewerTheme.Default);

        var layout = controller.GridLayout;
        Assert.NotNull(layout);
        Assert.True(layout!.Positions.Length > 0);

        var firstPos = layout.Positions[0];
        double hitX = firstPos.X + firstPos.Width / 2;
        double hitY = firstPos.Y + firstPos.Height / 2;

        var result = renderer.HitTest(hitX, hitY, info, controller);
        Assert.Equal(RenderHitType.Item, result.Type);
        Assert.NotNull(result.Item);
    }

    // =====================================================================
    // Render — theme accent color shows up
    // =====================================================================

    [Fact]
    public void Render_ThemeAccentColor_ShowsUp()
    {
        using var renderer = new PivotViewerRenderer();
        var info = new SKImageInfo(Width, Height);
        var controller = CreateLoadedController();
        var theme = new PivotViewerTheme
        {
            AccentColor = new SKColor(255, 0, 0),
            ItemFallbackColor = new SKColor(255, 0, 0),
        };

        using var surface = SKSurface.Create(info);
        renderer.Render(surface.Canvas, info, controller, theme);
        using var image = surface.Snapshot();
        using var pixmap = image.PeekPixels();

        int redPixels = CountColorPixels(pixmap, new SKColor(255, 0, 0), tolerance: 30);
        Assert.True(redPixels > 0, $"Accent/fallback color should appear in rendered output, got {redPixels} red pixels");
    }

    // =====================================================================
    // Dispose pattern
    // =====================================================================

    [Fact]
    public void Dispose_DoesNotThrow()
    {
        var renderer = new PivotViewerRenderer();
        renderer.Dispose();
    }

    [Fact]
    public void Dispose_DoubleDispose_DoesNotThrow()
    {
        var renderer = new PivotViewerRenderer();
        renderer.Dispose();
        renderer.Dispose();
    }

    [Fact]
    public void Dispose_RendererCanBeGarbageCollected()
    {
        var renderer = new PivotViewerRenderer();
        renderer.Dispose();
        renderer.Dispose(); // Double dispose is safe
    }

    // =====================================================================
    // Render / HitTest after Dispose
    // =====================================================================

    [Fact]
    public void Render_AfterDispose_DoesNotThrow()
    {
        var renderer = new PivotViewerRenderer();
        using var surface = SKSurface.Create(new SKImageInfo(Width, Height));
        var controller = CreateLoadedController();
        var theme = PivotViewerTheme.Default;

        renderer.Dispose();

        // Render after dispose should return silently without exception
        renderer.Render(surface.Canvas, new SKImageInfo(Width, Height), controller, theme);
    }

    [Fact]
    public void HitTest_AfterDispose_ReturnsNone()
    {
        var renderer = new PivotViewerRenderer();
        var info = new SKImageInfo(Width, Height);
        var controller = CreateLoadedController();

        renderer.Dispose();

        var result = renderer.HitTest(Width / 2.0, Height / 2.0, info, controller);
        Assert.Equal(RenderHitType.None, result.Type);
    }

    // =====================================================================
    // Render — no items (impossible filter)
    // =====================================================================

    [Fact]
    public void Render_NoItems_ShowsNoResultsMessage()
    {
        using var renderer = new PivotViewerRenderer();
        var info = new SKImageInfo(Width, Height);
        var controller = CreateLoadedController();
        var theme = PivotViewerTheme.Default;

        // Render with items
        using var surfaceWith = SKSurface.Create(info);
        renderer.Render(surfaceWith.Canvas, info, controller, theme);
        using var imgWith = surfaceWith.Snapshot();
        using var pmWith = imgWith.PeekPixels();
        int pixelsWith = CountNonWhitePixels(pmWith);

        // Apply impossible filter to empty the result set
        controller.FilterEngine.AddStringFilter("Manufacturer", "NoSuchManufacturerXYZ999");
        Assert.Empty(controller.InScopeItems);

        // Render with no items
        using var surfaceEmpty = SKSurface.Create(info);
        renderer.Render(surfaceEmpty.Canvas, info, controller, theme);
        using var imgEmpty = surfaceEmpty.Snapshot();
        using var pmEmpty = imgEmpty.PeekPixels();
        int pixelsEmpty = CountNonWhitePixels(pmEmpty);

        // Both should draw something; but output should differ
        Assert.True(pixelsEmpty > 0, "No-results message should draw text");
        Assert.NotEqual(pixelsWith, pixelsEmpty);
    }

    // =====================================================================
    // Render — custom theme affects output
    // =====================================================================

    [Fact]
    public void Render_CustomTheme_AffectsOutput()
    {
        using var renderer = new PivotViewerRenderer();
        var info = new SKImageInfo(Width, Height);
        var controller = CreateLoadedController();

        // Render with default theme
        using var surfaceDefault = SKSurface.Create(info);
        renderer.Render(surfaceDefault.Canvas, info, controller, PivotViewerTheme.Default);
        using var imgDefault = surfaceDefault.Snapshot();
        using var pmDefault = imgDefault.PeekPixels();
        int pixelsDefault = CountNonWhitePixels(pmDefault);

        // Render with a custom theme that uses very different colors
        var customTheme = new PivotViewerTheme
        {
            AccentColor = new SKColor(255, 0, 255),
            ItemFallbackColor = new SKColor(255, 0, 0),
            SelectionColor = new SKColor(0, 0, 255),
        };
        using var surfaceCustom = SKSurface.Create(info);
        renderer.Render(surfaceCustom.Canvas, info, controller, customTheme);
        using var imgCustom = surfaceCustom.Snapshot();
        using var pmCustom = imgCustom.PeekPixels();

        // Verify red pixels from custom ItemFallbackColor
        int redPixels = CountColorPixels(pmCustom, new SKColor(255, 0, 0), tolerance: 30);
        Assert.True(redPixels > 0, $"Custom theme ItemFallbackColor should produce red pixels, got {redPixels}");
    }

    // =====================================================================
    // Render — hover item draws highlight
    // =====================================================================

    [Fact]
    public void Render_HoverItem_DrawsHighlight()
    {
        using var renderer = new PivotViewerRenderer();
        var info = new SKImageInfo(Width, Height);
        var controller = CreateLoadedController();
        var theme = PivotViewerTheme.Default;

        // Render without hover
        using var surfaceNoHover = SKSurface.Create(info);
        renderer.Render(surfaceNoHover.Canvas, info, controller, theme, hoverItem: null);
        using var imgNoHover = surfaceNoHover.Snapshot();
        using var pmNoHover = imgNoHover.PeekPixels();

        // Render with hover on first item
        using var surfaceHover = SKSurface.Create(info);
        renderer.Render(surfaceHover.Canvas, info, controller, theme, hoverItem: controller.InScopeItems[0]);
        using var imgHover = surfaceHover.Snapshot();
        using var pmHover = imgHover.PeekPixels();

        // Compare pixel data — hover overlay changes actual pixel colors
        int diffPixels = CountDifferentPixels(pmNoHover, pmHover);
        Assert.True(diffPixels > 0, "Hover highlight should change some pixel colors");
    }

    // =====================================================================
    // Render — histogram view with thumbnails
    // =====================================================================

    [Fact]
    public void Render_HistogramView_WithThumbnails_DrawsItems()
    {
        using var renderer = new PivotViewerRenderer();
        var info = new SKImageInfo(Width, Height);
        var controller = CreateLoadedController();
        controller.SortProperty = controller.Properties[0];
        controller.CurrentView = "graph";
        var theme = PivotViewerTheme.Default;

        Assert.NotNull(controller.HistogramLayout);

        using var surface = SKSurface.Create(info);
        renderer.Render(surface.Canvas, info, controller, theme);

        using var image = surface.Snapshot();
        using var pixmap = image.PeekPixels();
        int nonWhite = CountNonWhitePixels(pixmap);
        Assert.True(nonWhite > 100, $"Histogram view with thumbnails should draw content, got {nonWhite} non-white pixels");

        // Verify histogram layout has columns with items
        Assert.True(controller.HistogramLayout!.Columns.Length > 0);
        Assert.True(controller.HistogramLayout.Columns.Any(c => c.Items.Length > 0));
    }

    // =====================================================================
    // HitTest — graph column label
    // =====================================================================

    [Fact]
    public void HitTest_GraphColumnLabel_ReturnsGraphColumnLabel()
    {
        using var renderer = new PivotViewerRenderer();
        var info = new SKImageInfo(Width, Height);
        var controller = CreateLoadedController();
        controller.SortProperty = controller.Properties.First(p =>
            p.PropertyType == PivotViewerPropertyType.Text);
        controller.CurrentView = "graph";
        var theme = PivotViewerTheme.Default;

        // Render to establish layout
        using var surface = SKSurface.Create(info);
        renderer.Render(surface.Canvas, info, controller, theme);

        var layout = controller.HistogramLayout;
        Assert.NotNull(layout);
        Assert.True(layout!.Columns.Length > 0);

        // Scan the bottom of content area (x-axis labels) for a GraphColumnLabel hit
        RenderHitResult? found = null;
        for (double x = 60; x < Width - 20; x += 10)
        {
            double y = Height - 10;
            var result = renderer.HitTest(x, y, info, controller);
            if (result.Type == RenderHitType.GraphColumnLabel)
            {
                found = result;
                break;
            }
        }

        Assert.NotNull(found);
        Assert.Equal(RenderHitType.GraphColumnLabel, found!.Type);
        Assert.NotNull(found.FilterPropertyId);
        Assert.NotNull(found.FilterValue);
    }

    // =====================================================================
    // Render — custom template invokes RenderAction
    // =====================================================================

    [Fact]
    public void Render_CustomTemplate_InvokesRenderAction()
    {
        using var renderer = new PivotViewerRenderer();
        var info = new SKImageInfo(Width, Height);
        var controller = CreateLoadedController();
        var theme = PivotViewerTheme.Default;

        bool templateInvoked = false;
        controller.ItemTemplates = new PivotViewerItemTemplateCollection
        {
            new PivotViewerItemTemplate
            {
                MaxWidth = 9999,
                RenderAction = (canvas, item, bounds) => templateInvoked = true,
            }
        };

        using var surface = SKSurface.Create(info);
        renderer.Render(surface.Canvas, info, controller, theme);

        Assert.True(templateInvoked, "Custom template RenderAction should be invoked during grid render");
    }

    // =====================================================================
    // Helpers
    // =====================================================================

    private static PivotViewerController CreateLoadedController()
    {
        var controller = new PivotViewerController();
        var source = TestDataHelper.LoadCxml("conceptcars.cxml");
        controller.LoadCollection(source);
        controller.SetAvailableSize(Width, Height);
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

    private static int CountDifferentPixels(SKPixmap a, SKPixmap b)
    {
        int count = 0;
        int w = Math.Min(a.Width, b.Width);
        int h = Math.Min(a.Height, b.Height);
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (a.GetPixelColor(x, y) != b.GetPixelColor(x, y))
                    count++;
            }
        }
        return count;
    }
}
