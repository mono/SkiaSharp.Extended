using SkiaSharp;
using SkiaSharp.Extended;
using SkiaSharp.Extended;
using Xunit;

namespace SkiaSharp.Extended.ImagePyramid.Tests;

/// <summary>
/// Integration tests for the ImagePyramid rendering pipeline.
/// Tests the full flow: DZI parse → viewport → scheduler → renderer.
/// </summary>
public class ImagePyramidRenderPipelineTest
{
    [Fact]
    public void FullPipeline_DziToCanvas()
    {
        // Parse DZI
        var dzi = CreateTestDzi(2048, 1536);

        // Set up controller and viewport
        var cache = new SKImagePyramidMemoryTileCache(100);
        var renderer = new SKImagePyramidRenderer();
        var controller = new SKImagePyramidController(cache);
        controller.SetControlSize(800, 600);
        controller.Load(dzi, new MemoryTileFetcher());
        controller.Viewport.AspectRatio = dzi.AspectRatio;

        // Get visible tiles using the controller's configured viewport
        var layout = new SKImagePyramidTileLayout();
        var tiles = layout.GetVisibleTiles(dzi, controller.Viewport);

        Assert.True(tiles.Count > 0);

        // Create cache with some test tiles
        foreach (var request in tiles)
        {
            var bmp = new SKBitmap(dzi.TileSize, dzi.TileSize);
            using var canvas2 = new SKCanvas(bmp);
            canvas2.Clear(SKColors.CornflowerBlue);
            cache.Put(request.TileId, new SKImagePyramidTile(SKImage.FromBitmap(bmp), new byte[] { 0xFF, 0xD8 }));
        }

        // Render
        using var surface = SKSurface.Create(new SKImageInfo(800, 600));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.White);

        renderer.Canvas = canvas;

        controller.Render(renderer);

        // Verify pixels were drawn
        using var pixmap = surface.PeekPixels();
        bool hasColor = false;
        for (int y = 0; y < pixmap.Height && !hasColor; y += 10)
        {
            for (int x = 0; x < pixmap.Width && !hasColor; x += 10)
            {
                var c = pixmap.GetPixelColor(x, y);
                if (c != SKColors.White) hasColor = true;
            }
        }
        Assert.True(hasColor, "Renderer should draw tiles to canvas");
    }

    [Fact]
    public void Controller_FullLifecycle()
    {
        using var controller = new SKImagePyramidController(new SKImagePyramidMemoryTileCache());
        controller.SetControlSize(800, 600);

        var dzi = CreateTestDzi(2048, 1536);
        var fetcher = new MemoryTileFetcher();
        controller.Load(dzi, fetcher);

        Assert.Equal(dzi, controller.TileSource);

        // Zoom in — directly mutates viewport (no spring)
        controller.ZoomAboutScreenPoint(2.0, 400, 300);
        Assert.True(controller.Viewport.ViewportWidth < 1.0);

        // Pan
        controller.Pan(100, 50);

        // Update tile scheduling (no delta time — no animation in controller)
        controller.Update();

        // Render
        using var surface = SKSurface.Create(new SKImageInfo(800, 600));
        using var renderer = new SKImagePyramidRenderer();
        renderer.Canvas = surface.Canvas;
        controller.Render(renderer);

        // Reset
        controller.ResetView();
        Assert.Equal(1.0, controller.Viewport.ViewportWidth, 0.01);
    }

    [Fact]
    public void Controller_SetViewport_AppliesImmediately()
    {
        using var controller = new SKImagePyramidController(new SKImagePyramidMemoryTileCache());
        controller.SetControlSize(800, 600);
        controller.Load(CreateTestDzi(1024, 768), new MemoryTileFetcher());

        controller.SetViewport(0.5, 0.1, 0.05);

        // SKImagePyramidViewport changes are immediate — no spring in the controller
        Assert.Equal(0.5, controller.Viewport.ViewportWidth, 3);
    }

    [Fact]
    public void Controller_EventsFire()
    {
        using var controller = new SKImagePyramidController(new SKImagePyramidMemoryTileCache());
        controller.SetControlSize(800, 600);

        bool openSucceeded = false;
        controller.ImageOpenSucceeded += (s, e) => openSucceeded = true;

        controller.Load(CreateTestDzi(512, 512), new MemoryTileFetcher());
        Assert.True(openSucceeded);
    }

    [Fact]
    public void Controller_ViewportChanged_FiresOnNavigation()
    {
        using var controller = new SKImagePyramidController(new SKImagePyramidMemoryTileCache());
        controller.SetControlSize(800, 600);
        controller.Load(CreateTestDzi(2048, 1536), new MemoryTileFetcher());

        int changeCount = 0;
        controller.ViewportChanged += (s, e) => changeCount++;

        controller.ZoomAboutScreenPoint(2.0, 400, 300);
        controller.Pan(50, 50);
        controller.ResetView();

        Assert.Equal(3, changeCount);
    }

    [Fact]
    public void Controller_IsIdle_WhenNoPendingTiles()
    {
        using var controller = new SKImagePyramidController(new SKImagePyramidMemoryTileCache());
        controller.SetControlSize(800, 600);
        controller.Load(CreateTestDzi(512, 512), new MemoryTileFetcher());

        // IsIdle is based only on pending tiles (no spring)
        // Right after load, no tiles are fetching yet (fetching starts on Update())
        Assert.True(controller.IsIdle);
    }

    // --- SKImagePyramidDziSource tests ---

    [Fact]
    public void DziTileSource_GetOptimalLevel()
    {
        var dzi = CreateTestDzi(4096, 4096);

        int overviewLevel = dzi.GetOptimalLevel(1.0, 800);
        int zoomLevel = dzi.GetOptimalLevel(0.01, 800);

        Assert.True(zoomLevel >= overviewLevel,
            $"Zoom level {zoomLevel} should be >= overview level {overviewLevel}");
    }

    [Fact]
    public void DziTileSource_LevelDimensions()
    {
        var dzi = CreateTestDzi(2048, 1536);

        Assert.Equal(1, dzi.GetLevelWidth(0));
        Assert.Equal(1, dzi.GetLevelHeight(0));
        Assert.Equal(2048, dzi.GetLevelWidth(dzi.MaxLevel));
        Assert.Equal(1536, dzi.GetLevelHeight(dzi.MaxLevel));
    }

    private static SKImagePyramidDziSource CreateTestDzi(int width, int height)
    {
        var xml = $@"<Image xmlns='http://schemas.microsoft.com/deepzoom/2008'
                     Format='jpg' TileSize='256' Overlap='1'>
                     <Size Width='{width}' Height='{height}'/></Image>";
        return SKImagePyramidDziSource.Parse(xml, "http://test.com/img");
    }
}
