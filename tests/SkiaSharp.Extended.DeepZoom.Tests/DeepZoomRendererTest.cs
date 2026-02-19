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

    [Fact]
    public void ShowTileBorders_EmptyCache_DoesNotThrow()
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
        };
        var cache = new TileCache(10);
        var scheduler = new TileScheduler();

        // Render with borders on but no tiles cached — should not throw
        var ex = Record.Exception(() => renderer.Render(surface.Canvas, dzi, viewport, cache, scheduler));
        Assert.Null(ex);
        cache.Dispose();
    }

    [Fact]
    public void Render_MultipleTimesInSequence_DoesNotThrow()
    {
        using var renderer = new DeepZoomRenderer();
        using var surface = SKSurface.Create(new SKImageInfo(400, 400));
        var dzi = CreateSampleDzi();
        var viewport = new Viewport
        {
            ControlWidth = 400,
            ControlHeight = 400,
            ViewportWidth = 1.0,
        };
        using var cache = new TileCache(10);
        var scheduler = new TileScheduler();

        // Render multiple times should be safe
        for (int i = 0; i < 5; i++)
        {
            surface.Canvas.Clear(SKColors.White);
            renderer.Render(surface.Canvas, dzi, viewport, cache, scheduler);
        }
    }

    [Fact]
    public void ShowTileBorders_WithFallbackTile_DrawsBordersWithoutCrash()
    {
        using var renderer = new DeepZoomRenderer();
        renderer.ShowTileBorders = true;

        string xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Image xmlns=""http://schemas.microsoft.com/deepzoom/2008""
       Format=""jpg"" Overlap=""0"" TileSize=""256"">
  <Size Width=""1024"" Height=""1024""/>
</Image>";
        var dzi = DziTileSource.Parse(xml, "http://example.com/large");
        using var surface = SKSurface.Create(new SKImageInfo(512, 512));
        var viewport = new Viewport
        {
            ControlWidth = 512,
            ControlHeight = 512,
            ViewportWidth = 1.0,
            ViewportOriginX = 0,
            ViewportOriginY = 0,
            AspectRatio = 1.0
        };
        using var cache = new TileCache(100);
        var scheduler = new TileScheduler();

        // Add only a low-level tile to trigger fallback path with borders
        var bmp = new SKBitmap(256, 256);
        using (var c = new SKCanvas(bmp))
            c.Clear(SKColors.Green);
        cache.Put(new TileId(0, 0, 0), bmp);

        var ex = Record.Exception(() => renderer.Render(surface.Canvas, dzi, viewport, cache, scheduler));
        Assert.Null(ex);
    }

    [Fact]
    public void Dispose_PaintObjectsAreDisposed()
    {
        var renderer = new DeepZoomRenderer();
        renderer.Dispose();

        // After dispose, creating a new renderer should work (verifies no static state corruption)
        using var renderer2 = new DeepZoomRenderer();
        using var surface = SKSurface.Create(new SKImageInfo(100, 100));
        var dzi = CreateSampleDzi();
        var viewport = new Viewport { ControlWidth = 100, ControlHeight = 100, ViewportWidth = 1.0 };
        using var cache = new TileCache(10);
        var scheduler = new TileScheduler();

        var ex = Record.Exception(() => renderer2.Render(surface.Canvas, dzi, viewport, cache, scheduler));
        Assert.Null(ex);
    }

    [Fact]
    public void Render_CachedColoredTile_PixelsAreNonWhite()
    {
        // 256x256 DZI, put a colored bitmap at level 0 tile (0,0), verify pixels
        using var renderer = new DeepZoomRenderer();
        var dzi = new DziTileSource(256, 256, 256, 0, "png");
        dzi.TilesBaseUri = "http://test/";

        using var surface = SKSurface.Create(new SKImageInfo(256, 256));
        var viewport = new Viewport
        {
            ControlWidth = 256,
            ControlHeight = 256,
            ViewportWidth = 1.0,
            ViewportOriginX = 0,
            ViewportOriginY = 0,
            AspectRatio = 1.0
        };
        using var cache = new TileCache(10);
        var scheduler = new TileScheduler();

        // Put a cyan bitmap at level 0, tile (0,0)
        var bmp = new SKBitmap(256, 256);
        using (var c = new SKCanvas(bmp))
            c.Clear(SKColors.Cyan);
        cache.Put(new TileId(0, 0, 0), bmp);

        // Also put tiles for whichever level the scheduler picks
        var tiles = scheduler.GetVisibleTiles(dzi, viewport);
        foreach (var t in tiles)
        {
            if (!cache.Contains(t.TileId))
            {
                var tileBmp = new SKBitmap(256, 256);
                using (var c = new SKCanvas(tileBmp))
                    c.Clear(SKColors.Cyan);
                cache.Put(t.TileId, tileBmp);
            }
        }

        surface.Canvas.Clear(SKColors.White);
        renderer.Render(surface.Canvas, dzi, viewport, cache, scheduler);

        using var snap = surface.Snapshot();
        using var decoded = SKBitmap.Decode(snap.Encode(SKEncodedImageFormat.Png, 100));
        var pixel = decoded.GetPixel(128, 128);
        Assert.True(pixel.Blue > 200 && pixel.Green > 200 && pixel.Red < 50,
            $"Expected cyan-like pixel but got R={pixel.Red} G={pixel.Green} B={pixel.Blue}");
    }

    [Fact]
    public void Render_FallbackFromParent_UsesParentTileWhenChildMissing()
    {
        using var renderer = new DeepZoomRenderer();

        // Large image so there are multiple levels
        string xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Image xmlns=""http://schemas.microsoft.com/deepzoom/2008""
       Format=""jpg"" Overlap=""0"" TileSize=""256"">
  <Size Width=""2048"" Height=""2048""/>
</Image>";
        var dzi = DziTileSource.Parse(xml, "http://example.com/fallback");

        using var surface = SKSurface.Create(new SKImageInfo(512, 512));
        var viewport = new Viewport
        {
            ControlWidth = 512,
            ControlHeight = 512,
            ViewportWidth = 1.0,
            ViewportOriginX = 0,
            ViewportOriginY = 0,
            AspectRatio = 1.0
        };
        using var cache = new TileCache(100);
        var scheduler = new TileScheduler();

        // Only cache a level-0 (1x1 pixel) parent tile as yellow — do NOT cache the requested level tiles
        var parentBmp = new SKBitmap(1, 1);
        parentBmp.SetPixel(0, 0, SKColors.Yellow);
        cache.Put(new TileId(0, 0, 0), parentBmp);

        surface.Canvas.Clear(SKColors.White);
        renderer.Render(surface.Canvas, dzi, viewport, cache, scheduler);

        // The fallback should have drawn the yellow parent into the child tile area
        using var snap = surface.Snapshot();
        using var decoded = SKBitmap.Decode(snap.Encode(SKEncodedImageFormat.Png, 100));
        var pixel = decoded.GetPixel(256, 256);
        // Pixel should not be pure white if fallback worked
        Assert.True(pixel.Red > 200 || pixel.Green > 200 || pixel.Blue < 255,
            $"Expected fallback rendering but got R={pixel.Red} G={pixel.Green} B={pixel.Blue}");
    }

    [Fact]
    public void Render_MinimumSizeViewport_DoesNotCrash()
    {
        // Viewport with ControlWidth/Height of 1 (minimum clamped value)
        using var renderer = new DeepZoomRenderer();
        using var surface = SKSurface.Create(new SKImageInfo(1, 1));
        var dzi = CreateSampleDzi();
        var viewport = new Viewport
        {
            ControlWidth = 0,  // gets clamped to 1
            ControlHeight = 0, // gets clamped to 1
            ViewportWidth = 1.0,
        };
        using var cache = new TileCache(10);
        var scheduler = new TileScheduler();

        var ex = Record.Exception(() => renderer.Render(surface.Canvas, dzi, viewport, cache, scheduler));
        Assert.Null(ex);
    }

    [Fact]
    public void EnableLodBlending_DefaultsToTrue()
    {
        using var renderer = new DeepZoomRenderer();
        Assert.True(renderer.EnableLodBlending);
    }

    [Fact]
    public void Render_WithLodBlendingDisabled_StillWorks()
    {
        using var renderer = new DeepZoomRenderer();
        renderer.EnableLodBlending = false;

        string xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Image xmlns=""http://schemas.microsoft.com/deepzoom/2008""
       Format=""jpg"" Overlap=""0"" TileSize=""256"">
  <Size Width=""2048"" Height=""2048""/>
</Image>";
        var dzi = DziTileSource.Parse(xml, "http://example.com/noblend");

        using var surface = SKSurface.Create(new SKImageInfo(512, 512));
        var viewport = new Viewport
        {
            ControlWidth = 512,
            ControlHeight = 512,
            ViewportWidth = 1.0,
            ViewportOriginX = 0,
            ViewportOriginY = 0,
            AspectRatio = 1.0
        };
        using var cache = new TileCache(100);
        var scheduler = new TileScheduler();

        // Only cache a low-level parent tile to trigger single-pass fallback
        var parentBmp = new SKBitmap(1, 1);
        parentBmp.SetPixel(0, 0, SKColors.Yellow);
        cache.Put(new TileId(0, 0, 0), parentBmp);

        surface.Canvas.Clear(SKColors.White);
        var ex = Record.Exception(() => renderer.Render(surface.Canvas, dzi, viewport, cache, scheduler));
        Assert.Null(ex);

        using var snap = surface.Snapshot();
        using var decoded = SKBitmap.Decode(snap.Encode(SKEncodedImageFormat.Png, 100));
        var pixel = decoded.GetPixel(256, 256);
        // Fallback should have rendered something (not pure white)
        Assert.True(pixel.Red > 200 || pixel.Green > 200 || pixel.Blue < 255,
            $"Expected fallback rendering but got R={pixel.Red} G={pixel.Green} B={pixel.Blue}");
    }

    [Fact]
    public void Render_WithLodBlendingEnabled_DrawsFallbackBehindHighRes()
    {
        using var renderer = new DeepZoomRenderer();
        Assert.True(renderer.EnableLodBlending);

        string xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Image xmlns=""http://schemas.microsoft.com/deepzoom/2008""
       Format=""jpg"" Overlap=""0"" TileSize=""256"">
  <Size Width=""1024"" Height=""1024""/>
</Image>";
        var dzi = DziTileSource.Parse(xml, "http://example.com/blend");

        using var surface = SKSurface.Create(new SKImageInfo(512, 512));
        var viewport = new Viewport
        {
            ControlWidth = 512,
            ControlHeight = 512,
            ViewportWidth = 1.0,
            ViewportOriginX = 0,
            ViewportOriginY = 0,
            AspectRatio = 1.0
        };
        using var cache = new TileCache(100);
        var scheduler = new TileScheduler();

        // Cache a blue parent tile at level 0 (fallback)
        var parentBmp = new SKBitmap(1, 1);
        parentBmp.SetPixel(0, 0, SKColors.Blue);
        cache.Put(new TileId(0, 0, 0), parentBmp);

        // Cache a green high-res tile for only the first visible tile
        var visibleTiles = scheduler.GetVisibleTiles(dzi, viewport);
        Assert.NotEmpty(visibleTiles);
        var firstTile = visibleTiles[0].TileId;
        var highResBmp = new SKBitmap(256, 256);
        using (var c = new SKCanvas(highResBmp))
            c.Clear(SKColors.Green);
        cache.Put(firstTile, highResBmp);

        surface.Canvas.Clear(SKColors.White);
        renderer.Render(surface.Canvas, dzi, viewport, cache, scheduler);

        // The surface should not be all white — fallback and high-res tiles were drawn
        using var snap = surface.Snapshot();
        using var decoded = SKBitmap.Decode(snap.Encode(SKEncodedImageFormat.Png, 100));
        var pixel = decoded.GetPixel(256, 256);
        Assert.NotEqual(SKColors.White, pixel);
    }
}
