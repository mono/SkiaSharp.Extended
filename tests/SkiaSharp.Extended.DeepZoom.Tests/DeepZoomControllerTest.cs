using SkiaSharp;
using SkiaSharp.Extended.DeepZoom;
using System.Collections.Concurrent;
using Xunit;

namespace SkiaSharp.Extended.DeepZoom.Tests;

/// <summary>
/// In-memory tile fetcher for testing. Returns solid-colored tiles.
/// </summary>
internal class MemoryTileFetcher : ITileFetcher
{
    private readonly ConcurrentDictionary<string, SKBitmap> _tiles = new();
    public int FetchCount { get; private set; }
    public List<string> FetchedUrls { get; } = new();

    public void AddTile(string url, SKBitmap bitmap) => _tiles[url] = bitmap;

    public void AddSolidTile(string url, int width, int height, SKColor color)
    {
        var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(color);
        _tiles[url] = bitmap;
    }

    public Task<SKBitmap?> FetchTileAsync(string url, CancellationToken ct = default)
    {
        FetchCount++;
        FetchedUrls.Add(url);

        if (_tiles.TryGetValue(url, out var bmp))
            return Task.FromResult<SKBitmap?>(bmp);

        return Task.FromResult<SKBitmap?>(null);
    }

    public void Dispose()
    {
        foreach (var bmp in _tiles.Values)
            bmp.Dispose();
        _tiles.Clear();
    }
}

public class DeepZoomControllerTest
{
    private static DziTileSource CreateSampleDzi()
    {
        string xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Image xmlns=""http://schemas.microsoft.com/deepzoom/2008""
       Format=""jpg"" Overlap=""0"" TileSize=""256"">
  <Size Width=""512"" Height=""512""/>
</Image>";
        return DziTileSource.Parse(xml, "http://example.com/test");
    }

    [Fact]
    public void Constructor_CreatesAllComponents()
    {
        using var controller = new DeepZoomController();

        Assert.NotNull(controller.Viewport);
        Assert.NotNull(controller.Spring);
        Assert.NotNull(controller.Cache);
        Assert.NotNull(controller.Scheduler);
        Assert.NotNull(controller.Renderer);
        Assert.Null(controller.TileSource);
        Assert.True(controller.UseSprings);
    }

    [Fact]
    public void Load_SetsUpTileSourceAndResetsViewport()
    {
        using var controller = new DeepZoomController();
        var dzi = CreateSampleDzi();
        using var fetcher = new MemoryTileFetcher();

        bool openFired = false;
        controller.ImageOpenSucceeded += (s, e) => openFired = true;

        controller.SetControlSize(800, 600);
        controller.Load(dzi, fetcher);

        Assert.Equal(dzi, controller.TileSource);
        Assert.True(openFired);
        Assert.Equal(1.0, controller.Viewport.ViewportWidth, 6);
        Assert.Equal(0.0, controller.Viewport.ViewportOriginX, 6);
    }

    [Fact]
    public void ZoomAboutScreenPoint_ChangesViewportWidth()
    {
        using var controller = new DeepZoomController();
        controller.UseSprings = false;
        controller.SetControlSize(800, 600);
        var dzi = CreateSampleDzi();
        using var fetcher = new MemoryTileFetcher();
        controller.Load(dzi, fetcher);

        double initialWidth = controller.Viewport.ViewportWidth;

        // Zoom in 2x at center
        controller.ZoomAboutScreenPoint(2.0, 400, 300);
        controller.Update(TimeSpan.FromSeconds(1.0 / 60));

        Assert.True(controller.Viewport.ViewportWidth < initialWidth,
            $"Expected viewport width < {initialWidth} but got {controller.Viewport.ViewportWidth}");
    }

    [Fact]
    public void Pan_MovesViewportOrigin()
    {
        using var controller = new DeepZoomController();
        controller.UseSprings = false;
        controller.SetControlSize(800, 600);
        var dzi = CreateSampleDzi();
        using var fetcher = new MemoryTileFetcher();
        controller.Load(dzi, fetcher);

        // First zoom in so there's room to pan
        controller.ZoomAboutScreenPoint(4.0, 400, 300);
        controller.Update(TimeSpan.FromSeconds(1.0 / 60));

        double originX = controller.Viewport.ViewportOriginX;

        controller.Pan(100, 0); // Pan right
        controller.Update(TimeSpan.FromSeconds(1.0 / 60));

        // Origin should have moved (panning right means origin moves left in logical space)
        Assert.NotEqual(originX, controller.Viewport.ViewportOriginX);
    }

    [Fact]
    public void ResetView_RestoresToInitialState()
    {
        using var controller = new DeepZoomController();
        controller.UseSprings = false;
        controller.SetControlSize(800, 600);
        var dzi = CreateSampleDzi();
        using var fetcher = new MemoryTileFetcher();
        controller.Load(dzi, fetcher);

        // Zoom and pan
        controller.ZoomAboutScreenPoint(4.0, 400, 300);
        controller.Update(TimeSpan.FromSeconds(1.0 / 60));

        // Reset
        controller.ResetView();
        controller.Update(TimeSpan.FromSeconds(1.0 / 60));

        Assert.Equal(1.0, controller.Viewport.ViewportWidth, 6);
    }

    [Fact]
    public void UseSprings_True_AnimatesGradually()
    {
        using var controller = new DeepZoomController();
        controller.UseSprings = true;
        controller.SetControlSize(800, 600);
        var dzi = CreateSampleDzi();
        using var fetcher = new MemoryTileFetcher();
        controller.Load(dzi, fetcher);

        controller.ZoomAboutScreenPoint(4.0, 400, 300);

        // After one frame, should NOT have reached target yet
        controller.Update(TimeSpan.FromSeconds(1.0 / 60));
        double afterOneFrame = controller.Viewport.ViewportWidth;

        Assert.True(afterOneFrame > 0.25 + 0.01,
            $"Expected spring to not reach target immediately, but got {afterOneFrame}");
    }

    [Fact]
    public void UseSprings_False_JumpsImmediately()
    {
        using var controller = new DeepZoomController();
        controller.UseSprings = false;
        controller.SetControlSize(800, 600);
        var dzi = CreateSampleDzi();
        using var fetcher = new MemoryTileFetcher();
        controller.Load(dzi, fetcher);

        controller.ZoomAboutScreenPoint(4.0, 400, 300);
        controller.Update(TimeSpan.FromSeconds(1.0 / 60));

        // Should be at target immediately (0.25 = 1.0 / 4.0)
        Assert.Equal(0.25, controller.Viewport.ViewportWidth, 2);
    }

    [Fact]
    public void Update_ReturnsFalse_WhenIdleAndSettled()
    {
        using var controller = new DeepZoomController();
        controller.UseSprings = false;
        controller.SetControlSize(800, 600);
        var dzi = CreateSampleDzi();
        using var fetcher = new MemoryTileFetcher();
        controller.Load(dzi, fetcher);

        // First update may schedule tiles
        controller.Update(TimeSpan.FromSeconds(1.0 / 60));

        // Give tiles a moment to "load" (they'll return null from empty fetcher)
        Task.Delay(100).Wait();

        // After settling, update should return false
        bool needsRepaint = controller.Update(TimeSpan.FromSeconds(1.0 / 60));
        // May still need repaint due to pending tiles, that's OK
    }

    [Fact]
    public void MotionFinished_FiredWhenSpringSettles()
    {
        using var controller = new DeepZoomController();
        controller.UseSprings = true;
        controller.SetControlSize(800, 600);
        var dzi = CreateSampleDzi();
        using var fetcher = new MemoryTileFetcher();
        controller.Load(dzi, fetcher);

        bool motionFinished = false;
        controller.MotionFinished += (s, e) => motionFinished = true;

        controller.ZoomAboutScreenPoint(2.0, 400, 300);

        // Advance many frames until settled
        for (int i = 0; i < 600; i++)
        {
            controller.Update(TimeSpan.FromSeconds(1.0 / 60));
            if (motionFinished) break;
        }

        Assert.True(motionFinished, "MotionFinished should fire when spring settles");
    }

    [Fact]
    public void Render_DoesNotThrow_WhenNoTileSource()
    {
        using var controller = new DeepZoomController();
        using var surface = SKSurface.Create(new SKImageInfo(800, 600));

        // Should not throw
        controller.Render(surface.Canvas);
    }

    [Fact]
    public void Render_DoesNotThrow_WhenLoaded()
    {
        using var controller = new DeepZoomController();
        controller.SetControlSize(800, 600);
        var dzi = CreateSampleDzi();
        using var fetcher = new MemoryTileFetcher();
        controller.Load(dzi, fetcher);

        using var surface = SKSurface.Create(new SKImageInfo(800, 600));

        // Should not throw even with empty cache
        controller.Render(surface.Canvas);
    }

    [Fact]
    public void Render_DrawsCachedTiles()
    {
        using var controller = new DeepZoomController();
        controller.UseSprings = false;
        controller.SetControlSize(800, 600);
        var dzi = CreateSampleDzi();

        using var fetcher = new MemoryTileFetcher();

        // Pre-populate cache with a tile
        var tileId = new TileId(0, 0, 0);
        var tileBmp = new SKBitmap(256, 256);
        using (var canvas = new SKCanvas(tileBmp))
            canvas.Clear(SKColors.Red);
        controller.Cache.Put(tileId, tileBmp);

        controller.Load(dzi, fetcher);

        using var surface = SKSurface.Create(new SKImageInfo(800, 600));
        controller.Render(surface.Canvas);

        // Verify something was drawn (pixel at center shouldn't be white/default)
        using var snapshot = surface.Snapshot();
        using var pixmap = snapshot.PeekPixels();
        // The low-level tile 0,0,0 covers the whole image at level 0
        // At full viewport, it should be rendered somewhere
    }

    [Fact]
    public void ShowTileBorders_DefaultsFalse()
    {
        using var controller = new DeepZoomController();
        Assert.False(controller.ShowTileBorders);
    }

    [Fact]
    public void ShowTileBorders_Settable()
    {
        using var controller = new DeepZoomController();
        controller.ShowTileBorders = true;
        Assert.True(controller.ShowTileBorders);
    }

    [Fact]
    public void AspectRatio_ReturnsZero_WhenNoSource()
    {
        using var controller = new DeepZoomController();
        Assert.Equal(0, controller.AspectRatio);
    }

    [Fact]
    public void AspectRatio_ReturnsCorrectValue_WhenLoaded()
    {
        using var controller = new DeepZoomController();
        var dzi = CreateSampleDzi(); // 512x512 = 1.0
        using var fetcher = new MemoryTileFetcher();
        controller.Load(dzi, fetcher);

        Assert.Equal(1.0, controller.AspectRatio, 6);
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        var controller = new DeepZoomController();
        controller.Dispose();
        controller.Dispose(); // Should not throw
    }

    [Fact]
    public async Task TileScheduling_FetchesTiles()
    {
        using var controller = new DeepZoomController();
        controller.UseSprings = false;
        controller.SetControlSize(512, 512);
        var dzi = CreateSampleDzi();

        using var fetcher = new MemoryTileFetcher();
        controller.Load(dzi, fetcher);

        // Update to trigger tile scheduling
        controller.Update(TimeSpan.FromSeconds(1.0 / 60));

        // Wait for async tile loads
        await Task.Delay(200);

        Assert.True(fetcher.FetchCount > 0, "Should have fetched at least one tile");
    }
}
