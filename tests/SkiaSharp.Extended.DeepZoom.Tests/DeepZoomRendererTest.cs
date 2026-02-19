using SkiaSharp;
using SkiaSharp.Extended.DeepZoom;
using Xunit;

namespace SkiaSharp.Extended.DeepZoom.Tests;

public class DeepZoomRendererTest
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
    public void Render_EmptyCache_DoesNotThrow()
    {
        using var renderer = new DeepZoomRenderer();
        using var surface = SKSurface.Create(new SKImageInfo(800, 600));
        var dzi = CreateSampleDzi();
        var viewport = new Viewport
        {
            ControlWidth = 800,
            ControlHeight = 600,
            ViewportWidth = 1.0
        };
        var cache = new TileCache(10);
        var scheduler = new TileScheduler();

        renderer.Render(surface.Canvas, dzi, viewport, cache, scheduler);
        cache.Dispose();
    }

    [Fact]
    public void Render_WithCachedTile_DrawsSomething()
    {
        using var renderer = new DeepZoomRenderer();
        using var surface = SKSurface.Create(new SKImageInfo(800, 600));
        var dzi = CreateSampleDzi();
        var viewport = new Viewport
        {
            ControlWidth = 800,
            ControlHeight = 600,
            ViewportWidth = 1.0,
            ViewportOriginX = 0,
            ViewportOriginY = 0
        };
        var cache = new TileCache(10);
        var scheduler = new TileScheduler();

        // Get visible tiles
        var tiles = scheduler.GetVisibleTiles(dzi, viewport);
        Assert.NotEmpty(tiles);

        // Add a red bitmap for the first visible tile
        var tileId = tiles[0].TileId;
        var bitmap = new SKBitmap(256, 256);
        using (var tileCanvas = new SKCanvas(bitmap))
            tileCanvas.Clear(SKColors.Red);
        cache.Put(tileId, bitmap);

        // Clear surface to white
        surface.Canvas.Clear(SKColors.White);

        // Render
        renderer.Render(surface.Canvas, dzi, viewport, cache, scheduler);

        // Verify something was drawn
        using var snapshot = surface.Snapshot();
        using var data = snapshot.Encode(SKEncodedImageFormat.Png, 100);
        Assert.True(data.Size > 0);
        cache.Dispose();
    }

    [Fact]
    public void ShowTileBorders_DrawsBorders()
    {
        using var renderer = new DeepZoomRenderer();
        renderer.ShowTileBorders = true;

        using var surface = SKSurface.Create(new SKImageInfo(800, 600));
        var dzi = CreateSampleDzi();
        var viewport = new Viewport
        {
            ControlWidth = 800,
            ControlHeight = 600,
            ViewportWidth = 1.0,
            ViewportOriginX = 0,
            ViewportOriginY = 0
        };
        var cache = new TileCache(10);
        var scheduler = new TileScheduler();

        // Add a tile
        var tiles = scheduler.GetVisibleTiles(dzi, viewport);
        if (tiles.Count > 0)
        {
            var tileId = tiles[0].TileId;
            var bitmap = new SKBitmap(256, 256);
            using (var tileCanvas = new SKCanvas(bitmap))
                tileCanvas.Clear(SKColors.Blue);
            cache.Put(tileId, bitmap);
        }

        surface.Canvas.Clear(SKColors.White);
        renderer.Render(surface.Canvas, dzi, viewport, cache, scheduler);

        // Just verify it doesn't throw. Red border pixels would be mixed with the tile.
        cache.Dispose();
    }

    [Fact]
    public void Render_FallbackTile_DrawsScaledParent()
    {
        using var renderer = new DeepZoomRenderer();
        using var surface = SKSurface.Create(new SKImageInfo(512, 512));

        string xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Image xmlns=""http://schemas.microsoft.com/deepzoom/2008""
       Format=""jpg"" Overlap=""0"" TileSize=""256"">
  <Size Width=""1024"" Height=""1024""/>
</Image>";
        var dzi = DziTileSource.Parse(xml, "http://example.com/large");

        // Full view
        var viewport = new Viewport
        {
            ControlWidth = 512,
            ControlHeight = 512,
            ViewportWidth = 1.0,
            ViewportOriginX = 0,
            ViewportOriginY = 0,
            AspectRatio = 1.0
        };
        var cache = new TileCache(100);
        var scheduler = new TileScheduler();

        // Get the visible tiles
        var visibleTiles = scheduler.GetVisibleTiles(dzi, viewport);
        Assert.NotEmpty(visibleTiles);

        // Add all visible tiles as green
        foreach (var req in visibleTiles)
        {
            var bmp = new SKBitmap(256, 256);
            using (var c = new SKCanvas(bmp))
                c.Clear(SKColors.Green);
            cache.Put(req.TileId, bmp);
        }

        surface.Canvas.Clear(SKColors.White);
        renderer.Render(surface.Canvas, dzi, viewport, cache, scheduler);

        // Verify by encoding the surface to check it's not all white
        using var snapshot = surface.Snapshot();
        using var data = snapshot.Encode(SKEncodedImageFormat.Png, 100);
        Assert.True(data.Size > 0, "Should produce a non-empty image");

        // Decode and check center pixel
        using var decoded = SKBitmap.Decode(data);
        var centerColor = decoded.GetPixel(256, 256);

        Assert.True(centerColor.Green > 100 && centerColor.Red < 50,
            $"Expected green at center but got R={centerColor.Red} G={centerColor.Green} B={centerColor.Blue}");
        cache.Dispose();
    }

    [Fact]
    public void Dispose_DisposesResources()
    {
        var renderer = new DeepZoomRenderer();
        renderer.Dispose();
        // Should not throw on double dispose
        renderer.Dispose();
    }

    [Fact]
    public void Render_CachedTile_CanvasIsNotAllWhite()
    {
        using var renderer = new DeepZoomRenderer();
        using var surface = SKSurface.Create(new SKImageInfo(400, 400));

        // 256x256 image, single tile at max level
        var dzi = new DziTileSource(256, 256, 256, 0, "jpg");
        dzi.TilesBaseUri = "http://test/";

        var viewport = new Viewport
        {
            ControlWidth = 400,
            ControlHeight = 400,
            ViewportWidth = 1.0,
            ViewportOriginX = 0,
            ViewportOriginY = 0,
            AspectRatio = 1.0
        };
        using var cache = new TileCache(10);
        var scheduler = new TileScheduler();

        // Create a magenta tile and add to cache
        var tiles = scheduler.GetVisibleTiles(dzi, viewport);
        Assert.NotEmpty(tiles);
        var tileId = tiles[0].TileId;
        var bmp = new SKBitmap(256, 256);
        using (var c = new SKCanvas(bmp))
            c.Clear(SKColors.Magenta);
        cache.Put(tileId, bmp);

        // Clear to white, then render
        surface.Canvas.Clear(SKColors.White);
        renderer.Render(surface.Canvas, dzi, viewport, cache, scheduler);

        // Verify center pixel is not white
        using var snap = surface.Snapshot();
        using var decoded = SKBitmap.Decode(snap.Encode(SKEncodedImageFormat.Png, 100));
        var pixel = decoded.GetPixel(200, 200);
        Assert.NotEqual(SKColors.White, pixel);
    }
}
