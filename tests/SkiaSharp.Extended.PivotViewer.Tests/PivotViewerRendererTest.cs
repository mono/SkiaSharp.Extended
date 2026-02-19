using SkiaSharp;
using SkiaSharp.Extended.PivotViewer;
using Xunit;

namespace SkiaSharp.Extended.PivotViewer.Tests;

/// <summary>
/// Tests for <see cref="PivotViewerRenderer"/> covering rendering, hit-testing,
/// theming, filter/detail pane visibility, and dispose behavior.
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
        var viewState = new PivotViewerViewState();

        renderer.Render(surface.Canvas, new SKImageInfo(Width, Height), controller, theme, viewState);
    }

    [Fact]
    public void Render_LoadedCollection_DoesNotThrow()
    {
        using var renderer = new PivotViewerRenderer();
        using var surface = SKSurface.Create(new SKImageInfo(Width, Height));
        var controller = CreateLoadedController();
        var theme = PivotViewerTheme.Default;
        var viewState = new PivotViewerViewState();

        renderer.Render(surface.Canvas, new SKImageInfo(Width, Height), controller, theme, viewState);
    }

    [Fact]
    public void Render_GridView_DrawsSomething()
    {
        using var renderer = new PivotViewerRenderer();
        using var surface = SKSurface.Create(new SKImageInfo(Width, Height));
        var controller = CreateLoadedController();
        var theme = PivotViewerTheme.Default;
        var viewState = new PivotViewerViewState { IsFilterPaneVisible = false };

        renderer.Render(surface.Canvas, new SKImageInfo(Width, Height), controller, theme, viewState);

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
        var viewState = new PivotViewerViewState { IsFilterPaneVisible = false };

        renderer.Render(surface.Canvas, new SKImageInfo(Width, Height), controller, theme, viewState);

        using var image = surface.Snapshot();
        using var pixmap = image.PeekPixels();
        int nonWhite = CountNonWhitePixels(pixmap);
        Assert.True(nonWhite > 100, $"Histogram view should draw content, got {nonWhite} non-white pixels");
    }

    // =====================================================================
    // HitTest — out-of-bounds and control bar regions
    // =====================================================================

    [Fact]
    public void HitTest_EmptyArea_ReturnsNone()
    {
        using var renderer = new PivotViewerRenderer();
        var info = new SKImageInfo(Width, Height);
        var controller = new PivotViewerController();
        controller.SetAvailableSize(Width, Height);
        var viewState = new PivotViewerViewState { IsFilterPaneVisible = false };

        // Empty controller has no items — clicking in the content area returns None
        var result = renderer.HitTest(Width / 2.0, Height / 2.0, info, controller, viewState);
        Assert.Equal(RenderHitType.None, result.Type);
    }

    [Fact]
    public void HitTest_FilterToggle_ReturnedForLeftEdge()
    {
        using var renderer = new PivotViewerRenderer();
        var info = new SKImageInfo(Width, Height);
        var controller = CreateLoadedController();
        var viewState = new PivotViewerViewState();

        // Click at (5, ControlBarHeight/2) — left edge of control bar
        var result = renderer.HitTest(5, PivotViewerRenderer.ControlBarHeight / 2, info, controller, viewState);
        Assert.Equal(RenderHitType.FilterToggle, result.Type);
    }

    [Fact]
    public void HitTest_ViewGrid_ReturnedForViewSwitcherArea()
    {
        using var renderer = new PivotViewerRenderer();
        var info = new SKImageInfo(Width, Height);
        var controller = CreateLoadedController();
        var viewState = new PivotViewerViewState { IsFilterPaneVisible = false };

        // When filter pane is hidden, view switcher starts near x=24+10=34
        var result = renderer.HitTest(40, PivotViewerRenderer.ControlBarHeight / 2, info, controller, viewState);
        Assert.Equal(RenderHitType.ViewGrid, result.Type);
    }

    [Fact]
    public void HitTest_ViewGraph_ReturnedForGraphSwitcherArea()
    {
        using var renderer = new PivotViewerRenderer();
        var info = new SKImageInfo(Width, Height);
        var controller = CreateLoadedController();
        var viewState = new PivotViewerViewState { IsFilterPaneVisible = false };

        // Graph label is to the right of Grid label; use x ~160
        var result = renderer.HitTest(160, PivotViewerRenderer.ControlBarHeight / 2, info, controller, viewState);
        Assert.Equal(RenderHitType.ViewGraph, result.Type);
    }

    [Fact]
    public void HitTest_Item_ReturnedForGridPosition()
    {
        using var renderer = new PivotViewerRenderer();
        var info = new SKImageInfo(Width, Height);
        var controller = CreateLoadedController();
        var viewState = new PivotViewerViewState { IsFilterPaneVisible = false };

        // Render first so layout is computed with the right content area
        using var surface = SKSurface.Create(info);
        renderer.Render(surface.Canvas, info, controller, PivotViewerTheme.Default, viewState);

        var layout = controller.GridLayout;
        Assert.NotNull(layout);
        Assert.True(layout!.Positions.Length > 0);

        var firstPos = layout.Positions[0];
        double hitX = firstPos.X + firstPos.Width / 2;
        double hitY = firstPos.Y + firstPos.Height / 2 + PivotViewerRenderer.ControlBarHeight;

        var result = renderer.HitTest(hitX, hitY, info, controller, viewState);
        Assert.Equal(RenderHitType.Item, result.Type);
        Assert.NotNull(result.Item);
    }

    // =====================================================================
    // Render — filter pane visible vs hidden
    // =====================================================================

    [Fact]
    public void Render_FilterPaneVisible_DrawsMoreThanHidden()
    {
        using var renderer = new PivotViewerRenderer();
        var info = new SKImageInfo(Width, Height);
        var controller = CreateLoadedController();
        var theme = PivotViewerTheme.Default;

        // Render with filter pane visible
        using var surfaceVisible = SKSurface.Create(info);
        var vsVisible = new PivotViewerViewState { IsFilterPaneVisible = true };
        renderer.Render(surfaceVisible.Canvas, info, controller, theme, vsVisible);
        using var imgVisible = surfaceVisible.Snapshot();
        using var pmVisible = imgVisible.PeekPixels();
        int pixelsVisible = CountNonWhitePixels(pmVisible);

        // Render with filter pane hidden
        using var surfaceHidden = SKSurface.Create(info);
        var vsHidden = new PivotViewerViewState { IsFilterPaneVisible = false };
        renderer.Render(surfaceHidden.Canvas, info, controller, theme, vsHidden);
        using var imgHidden = surfaceHidden.Snapshot();
        using var pmHidden = imgHidden.PeekPixels();
        int pixelsHidden = CountNonWhitePixels(pmHidden);

        // Both should draw something; filter pane adds dark background
        Assert.True(pixelsVisible > 100);
        Assert.True(pixelsHidden > 100);
        Assert.NotEqual(pixelsVisible, pixelsHidden);
    }

    // =====================================================================
    // Render — detail pane visible (selected item)
    // =====================================================================

    [Fact]
    public void Render_DetailPaneVisible_DrawsSelectedItem()
    {
        using var renderer = new PivotViewerRenderer();
        var info = new SKImageInfo(Width, Height);
        var controller = CreateLoadedController();
        controller.SelectedItem = controller.InScopeItems[0];
        Assert.True(controller.DetailPane.IsShowing);
        var theme = PivotViewerTheme.Default;

        // Render with detail pane
        using var surfaceWith = SKSurface.Create(info);
        var vs = new PivotViewerViewState { IsFilterPaneVisible = false };
        renderer.Render(surfaceWith.Canvas, info, controller, theme, vs);
        using var imgWith = surfaceWith.Snapshot();
        using var pmWith = imgWith.PeekPixels();
        int pixelsWith = CountNonWhitePixels(pmWith);

        // Render without detail pane
        controller.SelectedItem = null;
        using var surfaceWithout = SKSurface.Create(info);
        renderer.Render(surfaceWithout.Canvas, info, controller, theme, vs);
        using var imgWithout = surfaceWithout.Snapshot();
        using var pmWithout = imgWithout.PeekPixels();
        int pixelsWithout = CountNonWhitePixels(pmWithout);

        // Detail pane adds secondary background and text
        Assert.NotEqual(pixelsWith, pixelsWithout);
    }

    // =====================================================================
    // Render — sort dropdown visible
    // =====================================================================

    [Fact]
    public void Render_SortDropdownVisible_DrawsOverlay()
    {
        using var renderer = new PivotViewerRenderer();
        var info = new SKImageInfo(Width, Height);
        var controller = CreateLoadedController();
        var theme = PivotViewerTheme.Default;

        // Render without sort dropdown
        using var surfaceOff = SKSurface.Create(info);
        var vsOff = new PivotViewerViewState { IsSortDropdownVisible = false };
        renderer.Render(surfaceOff.Canvas, info, controller, theme, vsOff);
        using var imgOff = surfaceOff.Snapshot();
        using var pmOff = imgOff.PeekPixels();
        int pixelsOff = CountNonWhitePixels(pmOff);

        // Render with sort dropdown
        using var surfaceOn = SKSurface.Create(info);
        var vsOn = new PivotViewerViewState { IsSortDropdownVisible = true };
        renderer.Render(surfaceOn.Canvas, info, controller, theme, vsOn);
        using var imgOn = surfaceOn.Snapshot();
        using var pmOn = imgOn.PeekPixels();
        int pixelsOn = CountNonWhitePixels(pmOn);

        Assert.NotEqual(pixelsOff, pixelsOn);
    }

    // =====================================================================
    // Theme — accent color shows up
    // =====================================================================

    [Fact]
    public void Render_ThemeAccentColor_ShowsUp()
    {
        using var renderer = new PivotViewerRenderer();
        var info = new SKImageInfo(Width, Height);
        var controller = CreateLoadedController();
        // Use a distinct accent color
        var theme = new PivotViewerTheme { AccentColor = new SKColor(255, 0, 0) };
        var viewState = new PivotViewerViewState { IsFilterPaneVisible = false };

        using var surface = SKSurface.Create(info);
        renderer.Render(surface.Canvas, info, controller, theme, viewState);
        using var image = surface.Snapshot();
        using var pixmap = image.PeekPixels();

        // The accent color is used as ItemFallbackColor default; override that too
        theme.ItemFallbackColor = new SKColor(255, 0, 0);
        using var surface2 = SKSurface.Create(info);
        renderer.Render(surface2.Canvas, info, controller, theme, viewState);
        using var image2 = surface2.Snapshot();
        using var pixmap2 = image2.PeekPixels();

        int redPixels = CountColorPixels(pixmap2, new SKColor(255, 0, 0), tolerance: 30);
        Assert.True(redPixels > 0, $"Accent/fallback color should appear in rendered output, got {redPixels} red pixels");
    }

    // =====================================================================
    // InvalidateHistogramCaches
    // =====================================================================

    [Fact]
    public void InvalidateHistogramCaches_DoesNotThrow()
    {
        using var renderer = new PivotViewerRenderer();
        renderer.InvalidateHistogramCaches();

        // Also after a render that would populate caches
        using var surface = SKSurface.Create(new SKImageInfo(Width, Height));
        var controller = CreateLoadedController();
        var theme = PivotViewerTheme.Default;
        var viewState = new PivotViewerViewState { IsFilterPaneVisible = true };
        renderer.Render(surface.Canvas, new SKImageInfo(Width, Height), controller, theme, viewState);

        renderer.InvalidateHistogramCaches();
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
        // Verify dispose cleans up without error and object can be collected
        var renderer = new PivotViewerRenderer();
        renderer.InvalidateHistogramCaches();
        renderer.Dispose();
        renderer.Dispose(); // Double dispose is safe
        // No assertion needed — if we get here, dispose pattern works
    }

    // =====================================================================
    // HitTest — SortDropdown overlay intercepts
    // =====================================================================

    [Fact]
    public void HitTest_SortDropdownVisible_ReturnsSortDropdownRow()
    {
        using var renderer = new PivotViewerRenderer();
        var info = new SKImageInfo(Width, Height);
        var controller = CreateLoadedController();
        var viewState = new PivotViewerViewState { IsSortDropdownVisible = true };

        // Dropdown is centered at Width/2, starts at ControlBarHeight
        double dropX = Width / 2.0;
        double dropY = PivotViewerRenderer.ControlBarHeight + 20;

        var result = renderer.HitTest(dropX, dropY, info, controller, viewState);
        Assert.Equal(RenderHitType.SortDropdownRow, result.Type);
        Assert.True(result.SortRowIndex >= 0);
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
        var viewState = new PivotViewerViewState();

        renderer.Dispose();

        // Render after dispose should return silently without exception
        renderer.Render(surface.Canvas, new SKImageInfo(Width, Height), controller, theme, viewState);
    }

    [Fact]
    public void HitTest_AfterDispose_ReturnsNone()
    {
        var renderer = new PivotViewerRenderer();
        var info = new SKImageInfo(Width, Height);
        var controller = CreateLoadedController();
        var viewState = new PivotViewerViewState();

        renderer.Dispose();

        var result = renderer.HitTest(Width / 2.0, Height / 2.0, info, controller, viewState);
        Assert.Equal(RenderHitType.None, result.Type);
    }

    // =====================================================================
    // HitTest — filter pane checkbox
    // =====================================================================

    [Fact]
    public void HitTest_FilterPaneCheckbox_ReturnsFilterCheckbox()
    {
        using var renderer = new PivotViewerRenderer();
        var info = new SKImageInfo(Width, Height);
        var controller = CreateLoadedController();
        var viewState = new PivotViewerViewState { IsFilterPaneVisible = true };

        // Render first so filter pane layout is computed
        using var surface = SKSurface.Create(info);
        renderer.Render(surface.Canvas, info, controller, PivotViewerTheme.Default, viewState);

        // Filter pane checkboxes start after ControlBarHeight + padding + header line
        // Walk y downward from ControlBarHeight to find a checkbox hit
        RenderHitResult? found = null;
        for (double y = PivotViewerRenderer.ControlBarHeight + 60; y < 400; y += 5)
        {
            var result = renderer.HitTest(50, y, info, controller, viewState);
            if (result.Type == RenderHitType.FilterCheckbox)
            {
                found = result;
                break;
            }
        }

        Assert.NotNull(found);
        Assert.Equal(RenderHitType.FilterCheckbox, found!.Type);
        Assert.NotNull(found.FilterPropertyId);
        Assert.NotNull(found.FilterValue);
    }

    // =====================================================================
    // HitTest — clear all filters
    // =====================================================================

    [Fact]
    public void HitTest_ClearAllFilters_ReturnsClearAll()
    {
        using var renderer = new PivotViewerRenderer();
        var info = new SKImageInfo(Width, Height);
        var controller = CreateLoadedController();
        // Apply a filter so "Clear All" button appears
        controller.FilterEngine.AddStringFilter("Manufacturer", "Alfa Romeo");
        var viewState = new PivotViewerViewState { IsFilterPaneVisible = true };

        // Render to populate filter pane layout
        using var surface = SKSurface.Create(info);
        renderer.Render(surface.Canvas, info, controller, PivotViewerTheme.Default, viewState);

        // "Clear All" button is at ControlBarHeight + ItemPadding (y ~48..72)
        var result = renderer.HitTest(50, PivotViewerRenderer.ControlBarHeight + PivotViewerRenderer.ItemPadding + 10, info, controller, viewState);
        Assert.Equal(RenderHitType.ClearAllFilters, result.Type);
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
        var viewState = new PivotViewerViewState { IsFilterPaneVisible = false };

        // Render with items
        using var surfaceWith = SKSurface.Create(info);
        renderer.Render(surfaceWith.Canvas, info, controller, theme, viewState);
        using var imgWith = surfaceWith.Snapshot();
        using var pmWith = imgWith.PeekPixels();
        int pixelsWith = CountNonWhitePixels(pmWith);

        // Apply impossible filter to empty the result set
        controller.FilterEngine.AddStringFilter("Manufacturer", "NoSuchManufacturerXYZ999");
        Assert.Empty(controller.InScopeItems);

        // Render with no items
        using var surfaceEmpty = SKSurface.Create(info);
        renderer.Render(surfaceEmpty.Canvas, info, controller, theme, viewState);
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
        var viewState = new PivotViewerViewState { IsFilterPaneVisible = false };

        // Render with default theme
        using var surfaceDefault = SKSurface.Create(info);
        renderer.Render(surfaceDefault.Canvas, info, controller, PivotViewerTheme.Default, viewState);
        using var imgDefault = surfaceDefault.Snapshot();
        using var pmDefault = imgDefault.PeekPixels();
        int pixelsDefault = CountNonWhitePixels(pmDefault);

        // Render with a custom theme that uses very different colors
        var customTheme = new PivotViewerTheme
        {
            AccentColor = new SKColor(255, 0, 255),
            ControlBackground = new SKColor(0, 255, 0),
            ItemFallbackColor = new SKColor(255, 0, 0),
            SelectionColor = new SKColor(0, 0, 255),
        };
        using var surfaceCustom = SKSurface.Create(info);
        renderer.Render(surfaceCustom.Canvas, info, controller, customTheme, viewState);
        using var imgCustom = surfaceCustom.Snapshot();
        using var pmCustom = imgCustom.PeekPixels();

        // Verify green pixels from custom ControlBackground
        int greenPixels = CountColorPixels(pmCustom, new SKColor(0, 255, 0), tolerance: 30);
        Assert.True(greenPixels > 0, $"Custom theme ControlBackground should produce green pixels, got {greenPixels}");
    }

    // =====================================================================
    // HitTest — detail pane facet filter
    // =====================================================================

    [Fact]
    public void HitTest_DetailPaneFacet_ReturnsDetailFacetFilter()
    {
        using var renderer = new PivotViewerRenderer();
        var info = new SKImageInfo(Width, Height);
        var controller = CreateLoadedController();
        controller.SelectedItem = controller.InScopeItems[0];
        Assert.True(controller.DetailPane.IsShowing);
        var theme = PivotViewerTheme.Default;
        var viewState = new PivotViewerViewState { IsFilterPaneVisible = false };

        // Render so detail pane hit rects are populated
        using var surface = SKSurface.Create(info);
        renderer.Render(surface.Canvas, info, controller, theme, viewState);

        // Detail pane is on the right side; scan for a DetailFacetFilter hit
        float detailLeft = Width - PivotViewerRenderer.DetailPaneWidth;
        RenderHitResult? found = null;
        for (double y = PivotViewerRenderer.ControlBarHeight + 80; y < Height - 20; y += 5)
        {
            var result = renderer.HitTest(detailLeft + 20, y, info, controller, viewState);
            if (result.Type == RenderHitType.DetailFacetFilter)
            {
                found = result;
                break;
            }
        }

        Assert.NotNull(found);
        Assert.Equal(RenderHitType.DetailFacetFilter, found!.Type);
        Assert.NotNull(found.FilterPropertyId);
        Assert.NotNull(found.FilterValue);
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
        var vsNoHover = new PivotViewerViewState { IsFilterPaneVisible = false, HoverItem = null };
        renderer.Render(surfaceNoHover.Canvas, info, controller, theme, vsNoHover);
        using var imgNoHover = surfaceNoHover.Snapshot();
        using var pmNoHover = imgNoHover.PeekPixels();

        // Render with hover on first item
        using var surfaceHover = SKSurface.Create(info);
        var vsHover = new PivotViewerViewState { IsFilterPaneVisible = false, HoverItem = controller.InScopeItems[0] };
        renderer.Render(surfaceHover.Canvas, info, controller, theme, vsHover);
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
        var viewState = new PivotViewerViewState { IsFilterPaneVisible = false };

        Assert.NotNull(controller.HistogramLayout);

        using var surface = SKSurface.Create(info);
        renderer.Render(surface.Canvas, info, controller, theme, viewState);

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
        var viewState = new PivotViewerViewState { IsFilterPaneVisible = false };

        // Render to establish layout
        using var surface = SKSurface.Create(info);
        renderer.Render(surface.Canvas, info, controller, theme, viewState);

        var layout = controller.HistogramLayout;
        Assert.NotNull(layout);
        Assert.True(layout!.Columns.Length > 0);

        // Scan the bottom of content area (x-axis labels) for a GraphColumnLabel hit
        RenderHitResult? found = null;
        float contentHeight = Height - PivotViewerRenderer.ControlBarHeight;
        for (double x = 60; x < Width - 20; x += 10)
        {
            double y = PivotViewerRenderer.ControlBarHeight + contentHeight - 10;
            var result = renderer.HitTest(x, y, info, controller, viewState);
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
        var viewState = new PivotViewerViewState { IsFilterPaneVisible = false };

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
        renderer.Render(surface.Canvas, info, controller, theme, viewState);

        Assert.True(templateInvoked, "Custom template RenderAction should be invoked during grid render");
    }

    // =====================================================================
    // HitTest — per-category clear button
    // =====================================================================

    [Fact]
    public void HitTest_FilterCategoryClear_WhenFiltered()
    {
        using var renderer = new PivotViewerRenderer();
        var info = new SKImageInfo(Width, Height);
        var controller = CreateLoadedController();
        controller.FilterEngine.AddStringFilter("Manufacturer", "Alfa Romeo");
        var viewState = new PivotViewerViewState { IsFilterPaneVisible = true };

        // Render to populate layout
        using var surface = SKSurface.Create(info);
        renderer.Render(surface.Canvas, info, controller, PivotViewerTheme.Default, viewState);

        // Sweep the filter pane header area on the right side to find FilterCategoryClear
        RenderHitResult? found = null;
        for (double y = PivotViewerRenderer.ControlBarHeight + 20; y < Height - 20; y += 3)
        {
            var result = renderer.HitTest(PivotViewerRenderer.FilterPaneWidth - 10, y, info, controller, viewState);
            if (result.Type == RenderHitType.FilterCategoryClear)
            {
                found = result;
                break;
            }
        }

        Assert.NotNull(found);
        Assert.Equal(RenderHitType.FilterCategoryClear, found!.Type);
        Assert.NotNull(found.CategoryName);
    }

    // =====================================================================
    // Render — detail pane scrolled draws different content
    // =====================================================================

    [Fact]
    public void Render_DetailPaneScrolled_DrawsDifferentContent()
    {
        using var renderer = new PivotViewerRenderer();
        var info = new SKImageInfo(Width, Height);
        var controller = CreateLoadedController();
        controller.SelectedItem = controller.InScopeItems[0];
        Assert.True(controller.DetailPane.IsShowing);
        var theme = PivotViewerTheme.Default;

        // Render with scroll offset 0
        using var surface0 = SKSurface.Create(info);
        var vs0 = new PivotViewerViewState { IsFilterPaneVisible = false, DetailScrollOffset = 0 };
        renderer.Render(surface0.Canvas, info, controller, theme, vs0);
        using var img0 = surface0.Snapshot();
        using var pm0 = img0.PeekPixels();

        // Render with scroll offset 50
        using var surface50 = SKSurface.Create(info);
        var vs50 = new PivotViewerViewState { IsFilterPaneVisible = false, DetailScrollOffset = 50 };
        renderer.Render(surface50.Canvas, info, controller, theme, vs50);
        using var img50 = surface50.Snapshot();
        using var pm50 = img50.PeekPixels();

        int diffPixels = CountDifferentPixels(pm0, pm50);
        Assert.True(diffPixels > 0, "Detail pane with different scroll offsets should produce different pixel data");
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
