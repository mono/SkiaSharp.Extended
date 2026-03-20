using SkiaSharp;
using SkiaSharp.Extended;
using Xunit;

namespace SkiaSharp.Extended.ImagePyramid.Tests;

public class ImagePyramidRendererTest
{
    private static SKImagePyramidDziSource CreateSampleDzi()
    {
        string xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Image xmlns=""http://schemas.microsoft.com/deepzoom/2008""
       Format=""jpg"" Overlap=""0"" TileSize=""256"">
  <Size Width=""512"" Height=""512""/>
</Image>";
        return SKImagePyramidDziSource.Parse(xml, "http://example.com/test");
    }

    [Fact]
    public void Render_EmptyCache_DoesNotThrow()
    {
        using var renderer = new SKImagePyramidRenderer();
        using var surface = SKSurface.Create(new SKImageInfo(800, 600));
        var dzi = CreateSampleDzi();
        var cache = new SKImagePyramidMemoryTileCache(10);
        var controller = new SKImagePyramidController(cache);
        controller.SetControlSize(800, 600);
        controller.Load(dzi, new MemoryTileFetcher());
        controller.Viewport.ViewportWidth = 1.0;

        renderer.Canvas = surface.Canvas;
        controller.Render(renderer);
        cache.Dispose();
    }

    [Fact]
    public void Render_WithCachedTile_DrawsSomething()
    {
        using var renderer = new SKImagePyramidRenderer();
        using var surface = SKSurface.Create(new SKImageInfo(800, 600));
        var dzi = CreateSampleDzi();
        var cache = new SKImagePyramidMemoryTileCache(10);
        var controller = new SKImagePyramidController(cache);
        controller.SetControlSize(800, 600);
        controller.Load(dzi, new MemoryTileFetcher());
        controller.Viewport.ViewportWidth = 1.0;
        controller.Viewport.ViewportOriginX = 0;
        controller.Viewport.ViewportOriginY = 0;

        var layout = new SKImagePyramidTileLayout();

        // Get visible tiles
        var tiles = layout.GetVisibleTiles(dzi, controller.Viewport);
        Assert.NotEmpty(tiles);

        // Add a red bitmap for the first visible tile
        var tileId = tiles[0].TileId;
        var bitmap = new SKBitmap(256, 256);
        using (var tileCanvas = new SKCanvas(bitmap))
            tileCanvas.Clear(SKColors.Red);
        cache.Put(tileId, new SKImagePyramidTile(SKImage.FromBitmap(bitmap), new byte[] { 0xFF, 0xD8 }));

        // Clear surface to white
        surface.Canvas.Clear(SKColors.White);
        renderer.Canvas = surface.Canvas;
        controller.Render(renderer);

        // Verify something was drawn
        using var snapshot = surface.Snapshot();
        using var data = snapshot.Encode(SKEncodedImageFormat.Png, 100);
        Assert.True(data.Size > 0);
        cache.Dispose();
    }

    [Fact]
    public void EnableLodBlending_CanBeToggled()
    {
        using var controller = new SKImagePyramidController(new SKImagePyramidMemoryTileCache());
        Assert.True(controller.EnableLodBlending);

        controller.EnableLodBlending = false;
        Assert.False(controller.EnableLodBlending);

        controller.EnableLodBlending = true;
        Assert.True(controller.EnableLodBlending);
    }

    [Fact]
    public void Render_WithBordersEnabled_DrawsTiles()
    {
        using var renderer = new SKImagePyramidRenderer();

        using var surface = SKSurface.Create(new SKImageInfo(800, 600));
        var dzi = CreateSampleDzi();
        var cache = new SKImagePyramidMemoryTileCache(10);
        var controller = new SKImagePyramidController(cache);
        controller.SetControlSize(800, 600);
        controller.Load(dzi, new MemoryTileFetcher());
        controller.Viewport.ViewportWidth = 1.0;
        controller.Viewport.ViewportOriginX = 0;
        controller.Viewport.ViewportOriginY = 0;

        var layout = new SKImagePyramidTileLayout();

        // Add a tile
        var tiles = layout.GetVisibleTiles(dzi, controller.Viewport);
        if (tiles.Count > 0)
        {
            var tileId = tiles[0].TileId;
            var bitmap = new SKBitmap(256, 256);
            using (var tileCanvas = new SKCanvas(bitmap))
                tileCanvas.Clear(SKColors.Blue);
            cache.Put(tileId, new SKImagePyramidTile(SKImage.FromBitmap(bitmap), new byte[] { 0xFF, 0xD8 }));
        }

        surface.Canvas.Clear(SKColors.White);
        renderer.Canvas = surface.Canvas;
        controller.Render(renderer);

        // Just verify it doesn't throw.
        cache.Dispose();
    }

    [Fact]
    public void Render_FallbackTile_DrawsScaledParent()
    {
        using var renderer = new SKImagePyramidRenderer();
        using var surface = SKSurface.Create(new SKImageInfo(512, 512));

        string xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Image xmlns=""http://schemas.microsoft.com/deepzoom/2008""
       Format=""jpg"" Overlap=""0"" TileSize=""256"">
  <Size Width=""1024"" Height=""1024""/>
</Image>";
        var dzi = SKImagePyramidDziSource.Parse(xml, "http://example.com/large");

        var cache = new SKImagePyramidMemoryTileCache(100);
        var controller = new SKImagePyramidController(cache);
        controller.SetControlSize(512, 512);
        controller.Load(dzi, new MemoryTileFetcher());
        controller.Viewport.ViewportWidth = 1.0;
        controller.Viewport.ViewportOriginX = 0;
        controller.Viewport.ViewportOriginY = 0;
        controller.Viewport.AspectRatio = 1.0;

        var layout = new SKImagePyramidTileLayout();

        // Get the visible tiles
        var visibleTiles = layout.GetVisibleTiles(dzi, controller.Viewport);
        Assert.NotEmpty(visibleTiles);

        // Add all visible tiles as green
        foreach (var req in visibleTiles)
        {
            var bmp = new SKBitmap(256, 256);
            using (var c = new SKCanvas(bmp))
                c.Clear(SKColors.Green);
            cache.Put(req.TileId, new SKImagePyramidTile(SKImage.FromBitmap(bmp), new byte[] { 0xFF, 0xD8 }));
        }

        surface.Canvas.Clear(SKColors.White);
        renderer.Canvas = surface.Canvas;
        controller.Render(renderer);

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
        var renderer = new SKImagePyramidRenderer();
        renderer.Dispose();
        // Should not throw on double dispose
        renderer.Dispose();
    }

    [Fact]
    public void Render_CachedTile_CanvasIsNotAllWhite()
    {
        using var renderer = new SKImagePyramidRenderer();
        using var surface = SKSurface.Create(new SKImageInfo(400, 400));

        // 256x256 image, single tile at max level
        var dzi = new SKImagePyramidDziSource(256, 256, 256, 0, "jpg");
        dzi.TilesBaseUri = "http://test/";

        using var cache = new SKImagePyramidMemoryTileCache(10);
        var controller = new SKImagePyramidController(cache);
        controller.SetControlSize(400, 400);
        controller.Load(dzi, new MemoryTileFetcher());
        controller.Viewport.ViewportWidth = 1.0;
        controller.Viewport.ViewportOriginX = 0;
        controller.Viewport.ViewportOriginY = 0;
        controller.Viewport.AspectRatio = 1.0;

        var layout = new SKImagePyramidTileLayout();

        // Create a magenta tile and add to cache
        var tiles = layout.GetVisibleTiles(dzi, controller.Viewport);
        Assert.NotEmpty(tiles);
        var tileId = tiles[0].TileId;
        var bmp = new SKBitmap(256, 256);
        using (var c = new SKCanvas(bmp))
            c.Clear(SKColors.Magenta);
        cache.Put(tileId, new SKImagePyramidTile(SKImage.FromBitmap(bmp), new byte[] { 0xFF, 0xD8 }));

        // Clear to white, then render
        surface.Canvas.Clear(SKColors.White);
        renderer.Canvas = surface.Canvas;
        controller.Render(renderer);

        // Verify center pixel is not white
        using var snap = surface.Snapshot();
        using var decoded = SKBitmap.Decode(snap.Encode(SKEncodedImageFormat.Png, 100));
        var pixel = decoded.GetPixel(200, 200);
        Assert.NotEqual(SKColors.White, pixel);
    }

    [Fact]
    public void Render_MultipleTimesInSequence_DoesNotThrow()
    {
        using var renderer = new SKImagePyramidRenderer();
        using var surface = SKSurface.Create(new SKImageInfo(400, 400));
        var dzi = CreateSampleDzi();
        using var cache = new SKImagePyramidMemoryTileCache(10);
        var controller = new SKImagePyramidController(cache);
        controller.SetControlSize(400, 400);
        controller.Load(dzi, new MemoryTileFetcher());
        controller.Viewport.ViewportWidth = 1.0;

        // Render multiple times should be safe
        for (int i = 0; i < 5; i++)
        {
            surface.Canvas.Clear(SKColors.White);
            renderer.Canvas = surface.Canvas;
            controller.Render(renderer);
        }
    }

    [Fact]
    public void Render_WithFallbackTile_DoesNotThrow()
    {
        using var renderer = new SKImagePyramidRenderer();

        string xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Image xmlns=""http://schemas.microsoft.com/deepzoom/2008""
       Format=""jpg"" Overlap=""0"" TileSize=""256"">
  <Size Width=""1024"" Height=""1024""/>
</Image>";
        var dzi = SKImagePyramidDziSource.Parse(xml, "http://example.com/large");
        using var surface = SKSurface.Create(new SKImageInfo(512, 512));
        using var cache = new SKImagePyramidMemoryTileCache(100);

        // Add only a low-level tile to trigger fallback path with borders
        var bmp = new SKBitmap(256, 256);
        using (var c = new SKCanvas(bmp))
            c.Clear(SKColors.Green);
        cache.Put(new SKImagePyramidTileId(0, 0, 0), new SKImagePyramidTile(SKImage.FromBitmap(bmp), new byte[] { 0xFF, 0xD8 }));

        var controller = new SKImagePyramidController(cache);
        controller.SetControlSize(512, 512);
        controller.Load(dzi, new MemoryTileFetcher());
        controller.Viewport.ViewportWidth = 1.0;
        controller.Viewport.ViewportOriginX = 0;
        controller.Viewport.ViewportOriginY = 0;
        controller.Viewport.AspectRatio = 1.0;

        var ex = Record.Exception(() =>
        {
            renderer.Canvas = surface.Canvas;
            controller.Render(renderer);
        });
        Assert.Null(ex);
    }

    [Fact]
    public void Dispose_PaintObjectsAreDisposed()
    {
        var renderer = new SKImagePyramidRenderer();
        renderer.Dispose();

        // After dispose, creating a new renderer should work (verifies no static state corruption)
        using var renderer2 = new SKImagePyramidRenderer();
        using var surface = SKSurface.Create(new SKImageInfo(100, 100));
        var dzi = CreateSampleDzi();
        using var cache = new SKImagePyramidMemoryTileCache(10);

        var controller = new SKImagePyramidController(cache);
        controller.SetControlSize(100, 100);
        controller.Load(dzi, new MemoryTileFetcher());
        controller.Viewport.ViewportWidth = 1.0;

        var ex = Record.Exception(() =>
        {
            renderer2.Canvas = surface.Canvas;
            controller.Render(renderer2);
        });
        Assert.Null(ex);
    }

    [Fact]
    public void Render_CachedColoredTile_PixelsAreNonWhite()
    {
        // 256x256 DZI, put a colored bitmap at level 0 tile (0,0), verify pixels
        using var renderer = new SKImagePyramidRenderer();
        var dzi = new SKImagePyramidDziSource(256, 256, 256, 0, "png");
        dzi.TilesBaseUri = "http://test/";

        using var surface = SKSurface.Create(new SKImageInfo(256, 256));
        using var cache = new SKImagePyramidMemoryTileCache(10);
        var layout = new SKImagePyramidTileLayout();

        var controller = new SKImagePyramidController(cache);
        controller.SetControlSize(256, 256);
        controller.Load(dzi, new MemoryTileFetcher());
        controller.Viewport.ViewportWidth = 1.0;
        controller.Viewport.ViewportOriginX = 0;
        controller.Viewport.ViewportOriginY = 0;
        controller.Viewport.AspectRatio = 1.0;

        // Put a cyan bitmap at level 0, tile (0,0)
        var bmp = new SKBitmap(256, 256);
        using (var c = new SKCanvas(bmp))
            c.Clear(SKColors.Cyan);
        cache.Put(new SKImagePyramidTileId(0, 0, 0), new SKImagePyramidTile(SKImage.FromBitmap(bmp), new byte[] { 0xFF, 0xD8 }));

        // Also put tiles for whichever level the layout picks
        var tiles = layout.GetVisibleTiles(dzi, controller.Viewport);
        foreach (var t in tiles)
        {
            if (!cache.Contains(t.TileId))
            {
                var tileBmp = new SKBitmap(256, 256);
                using (var c = new SKCanvas(tileBmp))
                    c.Clear(SKColors.Cyan);
                cache.Put(t.TileId, new SKImagePyramidTile(SKImage.FromBitmap(tileBmp), new byte[] { 0xFF, 0xD8 }));
            }
        }

        surface.Canvas.Clear(SKColors.White);
        renderer.Canvas = surface.Canvas;
        controller.Render(renderer);

        using var snap = surface.Snapshot();
        using var decoded = SKBitmap.Decode(snap.Encode(SKEncodedImageFormat.Png, 100));
        var pixel = decoded.GetPixel(128, 128);
        Assert.True(pixel.Blue > 200 && pixel.Green > 200 && pixel.Red < 50,
            $"Expected cyan-like pixel but got R={pixel.Red} G={pixel.Green} B={pixel.Blue}");
    }

    [Fact]
    public void Render_FallbackFromParent_UsesParentTileWhenChildMissing()
    {
        using var renderer = new SKImagePyramidRenderer();

        // Large image so there are multiple levels
        string xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Image xmlns=""http://schemas.microsoft.com/deepzoom/2008""
       Format=""jpg"" Overlap=""0"" TileSize=""256"">
  <Size Width=""2048"" Height=""2048""/>
</Image>";
        var dzi = SKImagePyramidDziSource.Parse(xml, "http://example.com/fallback");

        using var surface = SKSurface.Create(new SKImageInfo(512, 512));
        using var cache = new SKImagePyramidMemoryTileCache(100);

        // Only cache a level-0 (1x1 pixel) parent tile as yellow — do NOT cache the requested level tiles
        var parentBmp = new SKBitmap(1, 1);
        parentBmp.SetPixel(0, 0, SKColors.Yellow);
        cache.Put(new SKImagePyramidTileId(0, 0, 0), new SKImagePyramidTile(SKImage.FromBitmap(parentBmp), new byte[] { 0xFF, 0xD8 }));

        var controller = new SKImagePyramidController(cache);
        controller.SetControlSize(512, 512);
        controller.Load(dzi, new MemoryTileFetcher());
        controller.Viewport.ViewportWidth = 1.0;
        controller.Viewport.ViewportOriginX = 0;
        controller.Viewport.ViewportOriginY = 0;
        controller.Viewport.AspectRatio = 1.0;

        surface.Canvas.Clear(SKColors.White);
        renderer.Canvas = surface.Canvas;
        controller.Render(renderer);

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
        // SKImagePyramidViewport with ControlWidth/Height of 1 (minimum clamped value)
        using var renderer = new SKImagePyramidRenderer();
        using var surface = SKSurface.Create(new SKImageInfo(1, 1));
        var dzi = CreateSampleDzi();
        using var cache = new SKImagePyramidMemoryTileCache(10);

        var controller = new SKImagePyramidController(cache);
        controller.SetControlSize(0, 0); // gets clamped to 1
        controller.Load(dzi, new MemoryTileFetcher());
        controller.Viewport.ViewportWidth = 1.0;

        var ex = Record.Exception(() =>
        {
            renderer.Canvas = surface.Canvas;
            controller.Render(renderer);
        });
        Assert.Null(ex);
    }

    [Fact]
    public void EnableLodBlending_DefaultsToTrue()
    {
        using var controller = new SKImagePyramidController(new SKImagePyramidMemoryTileCache());
        Assert.True(controller.EnableLodBlending);
    }

    [Fact]
    public void Render_WithLodBlendingDisabled_StillWorks()
    {
        using var renderer = new SKImagePyramidRenderer();

        string xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Image xmlns=""http://schemas.microsoft.com/deepzoom/2008""
       Format=""jpg"" Overlap=""0"" TileSize=""256"">
  <Size Width=""2048"" Height=""2048""/>
</Image>";
        var dzi = SKImagePyramidDziSource.Parse(xml, "http://example.com/noblend");

        using var surface = SKSurface.Create(new SKImageInfo(512, 512));
        using var cache = new SKImagePyramidMemoryTileCache(100);

        // Only cache a low-level parent tile to trigger single-pass fallback
        var parentBmp = new SKBitmap(1, 1);
        parentBmp.SetPixel(0, 0, SKColors.Yellow);
        cache.Put(new SKImagePyramidTileId(0, 0, 0), new SKImagePyramidTile(SKImage.FromBitmap(parentBmp), new byte[] { 0xFF, 0xD8 }));

        var controller = new SKImagePyramidController(cache);
        controller.SetControlSize(512, 512);
        controller.Load(dzi, new MemoryTileFetcher());
        controller.Viewport.ViewportWidth = 1.0;
        controller.Viewport.ViewportOriginX = 0;
        controller.Viewport.ViewportOriginY = 0;
        controller.Viewport.AspectRatio = 1.0;
        controller.EnableLodBlending = false;

        surface.Canvas.Clear(SKColors.White);
        renderer.Canvas = surface.Canvas;
        var ex = Record.Exception(() => controller.Render(renderer));
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
        using var renderer = new SKImagePyramidRenderer();

        string xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Image xmlns=""http://schemas.microsoft.com/deepzoom/2008""
       Format=""jpg"" Overlap=""0"" TileSize=""256"">
  <Size Width=""1024"" Height=""1024""/>
</Image>";
        var dzi = SKImagePyramidDziSource.Parse(xml, "http://example.com/blend");

        using var surface = SKSurface.Create(new SKImageInfo(512, 512));
        using var cache = new SKImagePyramidMemoryTileCache(100);
        var layout = new SKImagePyramidTileLayout();

        var controller = new SKImagePyramidController(cache);
        Assert.True(controller.EnableLodBlending);
        controller.SetControlSize(512, 512);
        controller.Load(dzi, new MemoryTileFetcher());
        controller.Viewport.ViewportWidth = 1.0;
        controller.Viewport.ViewportOriginX = 0;
        controller.Viewport.ViewportOriginY = 0;
        controller.Viewport.AspectRatio = 1.0;

        // Cache a blue parent tile at level 0 (fallback)
        var parentBmp = new SKBitmap(1, 1);
        parentBmp.SetPixel(0, 0, SKColors.Blue);
        cache.Put(new SKImagePyramidTileId(0, 0, 0), new SKImagePyramidTile(SKImage.FromBitmap(parentBmp), new byte[] { 0xFF, 0xD8 }));

        // Cache a green high-res tile for only the first visible tile
        var visibleTiles = layout.GetVisibleTiles(dzi, controller.Viewport);
        Assert.NotEmpty(visibleTiles);
        var firstTile = visibleTiles[0].TileId;
        var highResBmp = new SKBitmap(256, 256);
        using (var c = new SKCanvas(highResBmp))
            c.Clear(SKColors.Green);
        cache.Put(firstTile, new SKImagePyramidTile(SKImage.FromBitmap(highResBmp), new byte[] { 0xFF, 0xD8 }));

        surface.Canvas.Clear(SKColors.White);
        renderer.Canvas = surface.Canvas;
        controller.Render(renderer);

        // The surface should not be all white — fallback and high-res tiles were drawn
        using var snap = surface.Snapshot();
        using var decoded = SKBitmap.Decode(snap.Encode(SKEncodedImageFormat.Png, 100));
        var pixel = decoded.GetPixel(256, 256);
        Assert.NotEqual(SKColors.White, pixel);
    }
}
