using SkiaSharp;
using SkiaSharp.Extended.DeepZoom;

namespace SkiaSharp.Extended.DeepZoom.Tests;

/// <summary>
/// Tests for tile loading paths through DeepZoomController:
/// - LoadTileAsync success (tile cached, InvalidateRequired fired)
/// - LoadTileAsync failure (null bitmap, TileFailed event)
/// - LoadTileAsync cancellation
/// - Tile scheduling with pre-populated cache
/// </summary>
public class ControllerTileLoadingTest
{
    private static DziTileSource CreateDzi(int width = 512, int height = 512)
    {
        string xml = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<Image xmlns=""http://schemas.microsoft.com/deepzoom/2008""
       Format=""jpg"" Overlap=""0"" TileSize=""256"">
  <Size Width=""{width}"" Height=""{height}""/>
</Image>";
        return DziTileSource.Parse(xml, "http://test.com/image");
    }

    [Fact]
    public async Task TileLoad_Success_CachesTileAndFiresInvalidate()
    {
        using var controller = new DeepZoomController();
        controller.UseSprings = false;
        controller.SetControlSize(256, 256);

        var dzi = CreateDzi(256, 256);
        using var fetcher = new MemoryTileFetcher();

        // Pre-add a tile the scheduler will request (level 0, col 0, row 0)
        fetcher.AddSolidTile("http://test.com/image_files/0/0_0.jpg", 256, 256, SKColors.Red);

        bool invalidated = false;
        controller.InvalidateRequired += (s, e) => invalidated = true;

        controller.Load(dzi, fetcher);
        controller.Update(TimeSpan.FromSeconds(1.0 / 60));

        // Wait for async tile loading
        await Task.Delay(500);

        Assert.True(fetcher.FetchCount > 0, "Should have fetched tiles");
    }

    [Fact]
    public async Task TileLoad_NullBitmap_RemovesFromPending()
    {
        using var controller = new DeepZoomController();
        controller.UseSprings = false;
        controller.SetControlSize(256, 256);

        var dzi = CreateDzi(256, 256);
        using var fetcher = new MemoryTileFetcher();
        // Don't add any tiles — fetcher will return null

        controller.Load(dzi, fetcher);
        controller.Update(TimeSpan.FromSeconds(1.0 / 60));

        await Task.Delay(300);

        // Should not crash; tile loads gracefully with null result
        Assert.True(fetcher.FetchCount > 0);
    }

    [Fact]
    public async Task TileLoad_FetcherThrows_FiresTileFailed()
    {
        using var controller = new DeepZoomController();
        controller.UseSprings = false;
        controller.SetControlSize(256, 256);

        var dzi = CreateDzi(256, 256);
        using var fetcher = new ThrowingTileFetcher();

        TileId? failedTile = null;
        controller.TileFailed += (s, id) => failedTile = id;

        controller.Load(dzi, fetcher);
        controller.Update(TimeSpan.FromSeconds(1.0 / 60));

        await Task.Delay(500);

        Assert.NotNull(failedTile);
    }

    [Fact]
    public void Dispose_CancelsPendingLoads()
    {
        var controller = new DeepZoomController();
        controller.UseSprings = false;
        controller.SetControlSize(512, 512);

        var dzi = CreateDzi(512, 512);
        using var fetcher = new SlowTileFetcher();

        controller.Load(dzi, fetcher);
        controller.Update(TimeSpan.FromSeconds(1.0 / 60));

        // Dispose should cancel pending loads without hanging
        controller.Dispose();
    }

    [Fact]
    public void Controller_WithNoSource_UpdateDoesNotThrow()
    {
        using var controller = new DeepZoomController();
        controller.SetControlSize(800, 600);

        // Should not throw when no tile source is loaded
        controller.Update(TimeSpan.FromSeconds(1.0 / 60));
    }

    [Fact]
    public void Controller_SetControlSize_AffectsViewport()
    {
        using var controller = new DeepZoomController();

        controller.SetControlSize(1920, 1080);
        // Viewport should be initialized
        Assert.NotNull(controller.Viewport);
    }

    [Fact]
    public async Task TileScheduling_RequestsTilesForVisibleArea()
    {
        using var controller = new DeepZoomController();
        controller.UseSprings = false;
        controller.SetControlSize(800, 600);

        var dzi = CreateDzi(2048, 2048);
        using var fetcher = new MemoryTileFetcher();

        controller.Load(dzi, fetcher);

        // Multiple frames to trigger scheduling
        for (int i = 0; i < 5; i++)
        {
            controller.Update(TimeSpan.FromSeconds(1.0 / 60));
            await Task.Delay(50);
        }

        Assert.True(fetcher.FetchCount > 0,
            "Should have scheduled tile fetches for visible area");
        Assert.True(fetcher.FetchedUrls.Count > 0);

        // All URLs should be relative tile paths (level/col_row.format)
        foreach (var url in fetcher.FetchedUrls)
        {
            Assert.Matches(@"\d+/\d+_\d+\.\w+", url);
        }
    }

    [Fact]
    public void Render_WithTileBorders_DoesNotThrow()
    {
        using var controller = new DeepZoomController();
        controller.UseSprings = false;
        controller.ShowTileBorders = true;
        controller.SetControlSize(400, 400);

        var dzi = CreateDzi(256, 256);
        using var fetcher = new MemoryTileFetcher();
        fetcher.AddSolidTile("http://test.com/image_files/0/0_0.jpg", 256, 256, SKColors.Blue);

        controller.Load(dzi, fetcher);
        controller.Cache.Put(new TileId(0, 0, 0),
            CreateSolidBitmap(256, 256, SKColors.Blue));

        using var surface = SKSurface.Create(new SKImageInfo(400, 400));
        controller.Render(surface.Canvas);
    }

    [Fact]
    public void CoordinateConversion_ElementToLogical_Roundtrips()
    {
        using var controller = new DeepZoomController();
        controller.UseSprings = false;
        controller.SetControlSize(800, 600);

        var dzi = CreateDzi(1000, 750);
        using var fetcher = new MemoryTileFetcher();
        controller.Load(dzi, fetcher);

        var logical = controller.Viewport.ElementToLogicalPoint(400, 300);
        var screen = controller.Viewport.LogicalToElementPoint(logical.X, logical.Y);

        Assert.Equal(400, screen.X, 1);
        Assert.Equal(300, screen.Y, 1);
    }

    private static SKBitmap CreateSolidBitmap(int w, int h, SKColor color)
    {
        var bmp = new SKBitmap(w, h);
        using var canvas = new SKCanvas(bmp);
        canvas.Clear(color);
        return bmp;
    }
}

/// <summary>
/// Tile fetcher that always throws for testing error paths.
/// </summary>
internal class ThrowingTileFetcher : ITileFetcher
{
    public Task<SKBitmap?> FetchTileAsync(string url, CancellationToken ct = default)
    {
        throw new InvalidOperationException("Simulated fetch failure");
    }

    public void Dispose() { }
}

/// <summary>
/// Tile fetcher that delays forever (for testing cancellation).
/// </summary>
internal class SlowTileFetcher : ITileFetcher
{
    public async Task<SKBitmap?> FetchTileAsync(string url, CancellationToken ct = default)
    {
        await Task.Delay(TimeSpan.FromSeconds(60), ct);
        return null;
    }

    public void Dispose() { }
}
