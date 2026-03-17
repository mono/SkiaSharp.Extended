using SkiaSharp;
using SkiaSharp.Extended;
using System.Collections.Concurrent;
using Xunit;

namespace SkiaSharp.Extended.DeepZoom.Tests;

/// <summary>
/// In-memory tile fetcher for testing. Returns solid-colored tiles.
/// </summary>
internal class MemoryTileFetcher : ISKDeepZoomTileFetcher
{
    private readonly ConcurrentDictionary<string, SKImage> _tiles = new();
    public int FetchCount { get; private set; }
    public List<string> FetchedUrls { get; } = new();
    public bool IsDisposed { get; private set; }

    public void AddTile(string url, SKImage image) => _tiles[url] = image;

    public void AddSolidTile(string url, int width, int height, SKColor color)
    {
        using var surface = SKSurface.Create(new SKImageInfo(width, height));
        surface.Canvas.Clear(color);
        _tiles[url] = surface.Snapshot();
    }

    public Task<ISKDeepZoomTile?> FetchTileAsync(string url, CancellationToken ct = default)
    {
        FetchCount++;
        FetchedUrls.Add(url);

        if (_tiles.TryGetValue(url, out var image))
            return Task.FromResult<ISKDeepZoomTile?>(new SKDeepZoomImageTile(image));

        return Task.FromResult<ISKDeepZoomTile?>(null);
    }

    public void Dispose()
    {
        IsDisposed = true;
        foreach (var image in _tiles.Values)
            image.Dispose();
        _tiles.Clear();
    }
}

public class DeepZoomControllerTest
{
    private static SKDeepZoomImageSource CreateSampleDzi()
    {
        string xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Image xmlns=""http://schemas.microsoft.com/deepzoom/2008""
       Format=""jpg"" Overlap=""0"" TileSize=""256"">
  <Size Width=""512"" Height=""512""/>
</Image>";
        return SKDeepZoomImageSource.Parse(xml, "http://example.com/test");
    }

    [Fact]
    public void Constructor_CreatesAllComponents()
    {
        using var controller = new SKDeepZoomController();

        Assert.NotNull(controller.Viewport);
        Assert.NotNull(controller.Cache);
        Assert.NotNull(controller.TileLayout);
        Assert.Null(controller.TileSource);
        // Animation (spring, UseSprings) is NOT part of the controller; it lives in the view layer
    }

    [Fact]
    public void Load_SetsUpTileSourceAndResetsViewport()
    {
        using var controller = new SKDeepZoomController();
        var dzi = CreateSampleDzi();
        using var fetcher = new MemoryTileFetcher();

        bool openFired = false;
        controller.ImageOpenSucceeded += (s, e) => openFired = true;

        controller.SetControlSize(800, 600);
        controller.Load(dzi, fetcher);

        Assert.Equal(dzi, controller.TileSource);
        Assert.True(openFired);
        // 512x512 image in 800x600 control: fitWidth = 800/600 ≈ 1.333333, originX = (1-fitWidth)/2 ≈ -0.166667
        Assert.Equal(800.0 / 600.0, controller.Viewport.ViewportWidth, 6);
        Assert.Equal((1.0 - 800.0 / 600.0) / 2.0, controller.Viewport.ViewportOriginX, 6);
    }

    [Fact]
    public void ZoomAboutScreenPoint_ChangesViewportWidth()
    {
        using var controller = new SKDeepZoomController();
        controller.SetControlSize(800, 600);
        var dzi = CreateSampleDzi();
        using var fetcher = new MemoryTileFetcher();
        controller.Load(dzi, fetcher);

        double initialWidth = controller.Viewport.ViewportWidth;

        // Zoom in 2x at center — controller applies immediately (no spring)
        controller.ZoomAboutScreenPoint(2.0, 400, 300);

        Assert.True(controller.Viewport.ViewportWidth < initialWidth,
            $"Expected viewport width < {initialWidth} but got {controller.Viewport.ViewportWidth}");
    }

    [Fact]
    public void Pan_MovesViewportOrigin()
    {
        using var controller = new SKDeepZoomController();
        controller.SetControlSize(800, 600);
        var dzi = CreateSampleDzi();
        using var fetcher = new MemoryTileFetcher();
        controller.Load(dzi, fetcher);

        // First zoom in so there's room to pan
        controller.ZoomAboutScreenPoint(4.0, 400, 300);

        double originX = controller.Viewport.ViewportOriginX;

        controller.Pan(100, 0); // Pan right

        // Origin should have moved (panning right means origin moves in logical space)
        Assert.NotEqual(originX, controller.Viewport.ViewportOriginX);
    }

    [Fact]
    public void ResetView_RestoresToInitialState()
    {
        using var controller = new SKDeepZoomController();
        controller.SetControlSize(800, 600);
        var dzi = CreateSampleDzi();
        using var fetcher = new MemoryTileFetcher();
        controller.Load(dzi, fetcher);

        // Zoom and pan, then reset
        controller.ZoomAboutScreenPoint(4.0, 400, 300);
        controller.ResetView();

        // After reset: should restore to fit mode (same as after load)
        Assert.Equal(800.0 / 600.0, controller.Viewport.ViewportWidth, 6);
    }

    [Fact]
    public void Update_ReturnsFalse_WhenIdleAndNoPendingTiles()
    {
        using var controller = new SKDeepZoomController();
        controller.SetControlSize(800, 600);
        var dzi = CreateSampleDzi();
        using var fetcher = new MemoryTileFetcher();
        controller.Load(dzi, fetcher);

        // First update may schedule tiles
        controller.Update();

        // Give tiles a moment to "load" (they'll return null from empty fetcher)
        Task.Delay(100).Wait();

        // After settling, update should return false (no pending tiles)
        bool needsRepaint = controller.Update();
        Assert.False(needsRepaint, "Update should return false when idle and no pending tiles");
    }

    [Fact]
    public void Render_DoesNotThrow_WhenNoTileSource()
    {
        using var controller = new SKDeepZoomController();
        using var surface = SKSurface.Create(new SKImageInfo(800, 600));
        using var renderer = new SKDeepZoomRenderer();
        renderer.Canvas = surface.Canvas;

        // Should not throw
        controller.Render(renderer);
    }

    [Fact]
    public void Render_DoesNotThrow_WhenLoaded()
    {
        using var controller = new SKDeepZoomController();
        controller.SetControlSize(800, 600);
        controller.Load(CreateSampleDzi(), new MemoryTileFetcher());

        using var surface = SKSurface.Create(new SKImageInfo(800, 600));
        using var renderer = new SKDeepZoomRenderer();
        renderer.Canvas = surface.Canvas;
        controller.Render(renderer);
    }

    [Fact]
    public void Render_DrawsCachedTiles()
    {
        using var controller = new SKDeepZoomController();
        controller.SetControlSize(800, 600);

        var dzi = CreateSampleDzi();
        using var fetcher = new MemoryTileFetcher();
        // Add tiles for level 9 (512x512 image) at all positions
        for (int col = 0; col < 3; col++)
            for (int row = 0; row < 3; row++)
            {
                fetcher.AddSolidTile($"http://example.com/test9/{col}_{row}.jpg", 256, 256, SKColors.Red);
                fetcher.AddSolidTile($"http://example.com/test8/{col}_{row}.jpg", 256, 256, SKColors.Red);
                fetcher.AddSolidTile($"http://example.com/test7/{col}_{row}.jpg", 256, 256, SKColors.Red);
            }

        controller.Load(dzi, fetcher);
        controller.Update();

        // Wait for tile loads
        Task.Delay(500).Wait();

        using var surface = SKSurface.Create(new SKImageInfo(800, 600));
        using var renderer = new SKDeepZoomRenderer();
        renderer.Canvas = surface.Canvas;
        controller.Render(renderer);

        // Verify some tiles were drawn by checking pixels
        using var pixmap = surface.PeekPixels();
        bool hasRed = false;
        for (int y = 0; y < pixmap.Height && !hasRed; y += 10)
            for (int x = 0; x < pixmap.Width && !hasRed; x += 10)
                if (pixmap.GetPixelColor(x, y).Red > 200) hasRed = true;

        Assert.True(hasRed, "Expected rendered tiles to include red pixels");
    }

    [Fact]
    public void EnableLodBlending_DefaultsTrue()
    {
        using var controller = new SKDeepZoomController();
        Assert.True(controller.EnableLodBlending);
    }

    [Fact]
    public void EnableLodBlending_Settable()
    {
        using var controller = new SKDeepZoomController();
        controller.EnableLodBlending = false;
        Assert.False(controller.EnableLodBlending);
    }

    [Fact]
    public void AspectRatio_ReturnsZero_WhenNoSource()
    {
        using var controller = new SKDeepZoomController();
        Assert.Equal(0.0, controller.AspectRatio);
    }

    [Fact]
    public void AspectRatio_ReturnsCorrectValue_WhenLoaded()
    {
        using var controller = new SKDeepZoomController();
        var dzi = CreateSampleDzi(); // 512x512 → 1.0
        controller.Load(dzi, new MemoryTileFetcher());

        Assert.Equal(1.0, controller.AspectRatio, 6);
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        var controller = new SKDeepZoomController();
        controller.Dispose();
        controller.Dispose(); // Should not throw
    }

    [Fact]
    public void SubImages_EmptyByDefault()
    {
        using var controller = new SKDeepZoomController();
        Assert.Empty(controller.SubImages);
    }

    [Fact]
    public void Load_DzcTileSource_PopulatesSubImages()
    {
        using var controller = new SKDeepZoomController();
        var dzc = new SKDeepZoomCollectionSource(8, 256, "jpg", new[]
        {
            new SKDeepZoomCollectionSubImage(0, 0, 512, 256, "img0.dzi"),
            new SKDeepZoomCollectionSubImage(512, 0, 256, 256, "img1.dzi"),
        });

        using var fetcher = new MemoryTileFetcher();
        controller.Load(dzc, fetcher);

        Assert.Equal(2, controller.SubImages.Count);
        Assert.Equal(2.0, controller.SubImages[0].AspectRatio, 6);
        Assert.Equal("img1.dzi", controller.SubImages[1].Source);
    }

    [Fact]
    public async Task TileScheduling_FetchesTiles()
    {
        using var controller = new SKDeepZoomController();
        controller.SetControlSize(512, 512);
        var dzi = CreateSampleDzi();

        using var fetcher = new MemoryTileFetcher();
        controller.Load(dzi, fetcher);

        // Update to trigger tile scheduling
        controller.Update();

        // Wait for async tile loads
        await Task.Delay(200);

        Assert.True(fetcher.FetchCount > 0, "Should have fetched at least one tile");
    }

    [Fact]
    public void ZoomAboutLogicalPoint_FiresViewportChanged()
    {
        using var controller = new SKDeepZoomController();
        controller.Load(CreateSampleDzi(), new MemoryTileFetcher());
        int count = 0;
        controller.ViewportChanged += (s, e) => count++;

        controller.ZoomAboutLogicalPoint(0.5, 0.5, 0.5);
        Assert.True(count > 0);
    }

    [Fact]
    public void Pan_FiresViewportChanged()
    {
        using var controller = new SKDeepZoomController();
        controller.Load(CreateSampleDzi(), new MemoryTileFetcher());
        int count = 0;
        controller.ViewportChanged += (s, e) => count++;

        controller.Pan(10, 10);
        Assert.True(count > 0);
    }

    [Fact]
    public void Dispose_DisposesFetcher()
    {
        var fetcher = new MemoryTileFetcher();
        var controller = new SKDeepZoomController();
        controller.Load(CreateSampleDzi(), fetcher);

        controller.Dispose();

        // Fetcher should be disposed (MemoryTileFetcher.Dispose clears tiles)
        Assert.True(fetcher.IsDisposed);
    }

    [Fact]
    public void Load_Reload_DisposesPreviousFetcher()
    {
        var fetcher1 = new MemoryTileFetcher();
        using var controller = new SKDeepZoomController();
        controller.Load(CreateSampleDzi(), fetcher1);

        var fetcher2 = new MemoryTileFetcher();
        controller.Load(CreateSampleDzi(), fetcher2);

        // First fetcher should be disposed on reload
        Assert.True(fetcher1.IsDisposed);
        Assert.False(fetcher2.IsDisposed);
    }

    [Fact]
    public void Load_ClearsCacheFromPreviousImage()
    {
        using var controller = new SKDeepZoomController();
        var fetcher = new MemoryTileFetcher();
        fetcher.AddSolidTile("http://example.com/image_files/0/0_0.jpg", 256, 256, SkiaSharp.SKColors.Red);
        controller.Load(CreateSampleDzi(), fetcher);

        // Trigger tile scheduling
        controller.SetControlSize(800, 600);
        controller.Update();

        // Now reload — cache should be cleared
        controller.Load(CreateSampleDzi(), fetcher);

        // After reload, fetcher should get new tile requests
        int fetchesBefore = fetcher.FetchCount;
        controller.SetControlSize(800, 600);
        controller.Update();
        // New fetches should occur since cache was cleared
        Assert.True(fetcher.FetchCount > fetchesBefore, "Cache clear should trigger new tile fetches");
    }

    [Fact]
    public void Load_DzcFromFile_PopulatesSubImagesWithCorrectCount()
    {
        using var stream = TestDataHelper.GetStream("conceptcars.dzc");
        var dzc = SKDeepZoomCollectionSource.Parse(stream);
        using var controller = new SKDeepZoomController();
        using var fetcher = new MemoryTileFetcher();

        controller.Load(dzc, fetcher);

        Assert.Equal(dzc.ItemCount, controller.SubImages.Count);
        Assert.True(controller.SubImages.Count > 0, "SubImages should be populated from DZC file");
    }

    [Fact]
    public async Task LoadTileAsync_WithTilesBaseUri_FetcherReceivesFullUrl()
    {
        using var controller = new SKDeepZoomController();
        controller.SetControlSize(512, 512);

        var dzi = CreateSampleDzi(); // TilesBaseUri = "http://example.com/test"
        using var fetcher = new MemoryTileFetcher();
        controller.Load(dzi, fetcher);

        // Trigger tile scheduling
        controller.Update();

        // Wait for async tile loads
        await Task.Delay(300);

        Assert.True(fetcher.FetchedUrls.Count > 0, "Should have fetched at least one tile");

        // All fetched URLs should be full URLs starting with TilesBaseUri
        foreach (var url in fetcher.FetchedUrls)
        {
            Assert.StartsWith("http://example.com/test", url,
                StringComparison.Ordinal);
        }
    }

    [Fact]
    public void GetZoomRect_ReturnsCurrentViewportBounds()
    {
        using var controller = new SKDeepZoomController();
        controller.SetControlSize(800, 600);
        var dzi = CreateSampleDzi(); // 512x512, aspect ratio 1.0
        using var fetcher = new MemoryTileFetcher();
        controller.Load(dzi, fetcher);

        var (x, y, w, h) = controller.GetZoomRect();

        // At initial load with fit mode: 512x512 in 800x600 → fitWidth = 800/600 ≈ 1.333333
        double fitWidth = 800.0 / 600.0;
        double fitOriginX = (1.0 - fitWidth) / 2.0;
        Assert.Equal(fitOriginX, x, 6);
        Assert.Equal(0.0, y, 6);
        Assert.Equal(fitWidth, w, 6);
        Assert.Equal(fitWidth / 1.0 /*aspectRatio*/, h, 6);

        // Zoom in and verify the rect changes (immediate — no spring)
        controller.ZoomAboutScreenPoint(4.0, 400, 300);

        var (x2, y2, w2, h2) = controller.GetZoomRect();
        Assert.True(w2 < w, $"Zoomed width {w2} should be less than initial {w}");
        Assert.True(h2 < h, $"Zoomed height {h2} should be less than initial {h}");
    }

    [Fact]
    public void ViewportChanged_FiresOnZoom()
    {
        using var controller = new SKDeepZoomController();
        controller.SetControlSize(800, 600);
        controller.Load(CreateSampleDzi(), new MemoryTileFetcher());

        int count = 0;
        controller.ViewportChanged += (s, e) => count++;

        controller.ZoomAboutScreenPoint(2.0, 400, 300);
        Assert.True(count > 0, "ViewportChanged should fire on zoom");
    }

    [Fact]
    public void ViewportChanged_FiresOnPan()
    {
        using var controller = new SKDeepZoomController();
        controller.SetControlSize(800, 600);
        controller.Load(CreateSampleDzi(), new MemoryTileFetcher());

        // Zoom in first so panning changes the viewport
        controller.ZoomAboutScreenPoint(4.0, 400, 300);

        int count = 0;
        controller.ViewportChanged += (s, e) => count++;

        controller.Pan(50, 50);
        Assert.True(count > 0, "ViewportChanged should fire on pan");
    }

    [Fact]
    public void IsIdle_TrueWhenNoPendingTiles()
    {
        using var controller = new SKDeepZoomController();
        controller.SetControlSize(800, 600);

        // Before any Update, no tiles are pending
        Assert.True(controller.IsIdle);
    }

    [Fact]
    public void ImageOpenSucceeded_FiresOnDziLoad()
    {
        using var controller = new SKDeepZoomController();
        bool fired = false;
        controller.ImageOpenSucceeded += (s, e) => fired = true;

        controller.Load(CreateSampleDzi(), new MemoryTileFetcher());
        Assert.True(fired, "ImageOpenSucceeded should fire on DZI load");
    }

    [Fact]
    public void ImageOpenSucceeded_FiresOnDzcLoad()
    {
        using var controller = new SKDeepZoomController();
        bool fired = false;
        controller.ImageOpenSucceeded += (s, e) => fired = true;

        var dzc = new SKDeepZoomCollectionSource(8, 256, "jpg", new[]
        {
            new SKDeepZoomCollectionSubImage(0, 0, 256, 256, null)
        });
        controller.Load(dzc, new MemoryTileFetcher());

        Assert.True(fired, "ImageOpenSucceeded should fire on DZC load");
    }

    [Fact]
    public void ResetView_FiresViewportChanged()
    {
        using var controller = new SKDeepZoomController();
        controller.SetControlSize(800, 600);
        controller.Load(CreateSampleDzi(), new MemoryTileFetcher());

        controller.ZoomAboutScreenPoint(4.0, 400, 300);

        int count = 0;
        controller.ViewportChanged += (s, e) => count++;

        controller.ResetView();
        Assert.True(count > 0, "ViewportChanged should fire on ResetView");
    }

    [Fact]
    public async Task InvalidateRequired_FiresWhenTileLoads()
    {
        using var controller = new SKDeepZoomController();
        controller.SetControlSize(512, 512);
        var dzi = CreateSampleDzi();

        using var fetcher = new MemoryTileFetcher();
        // Pre-add tiles at the optimal level (level 9 for 512x512 image at 1:1 zoom)
        for (int col = 0; col < 2; col++)
            for (int row = 0; row < 2; row++)
                fetcher.AddSolidTile($"http://example.com/test9/{col}_{row}.jpg", 256, 256, SKColors.Blue);

        bool invalidated = false;
        controller.InvalidateRequired += (s, e) => invalidated = true;

        controller.Load(dzi, fetcher);
        controller.Update();

        // Wait for async tile load with polling
        for (int i = 0; i < 40 && !invalidated; i++)
            await Task.Delay(50);

        Assert.True(invalidated, "InvalidateRequired should fire when a tile loads");
    }

    [Fact]
    public void ZoomAboutLogicalPoint_ChangesViewport()
    {
        using var controller = new SKDeepZoomController();
        controller.SetControlSize(800, 600);
        controller.Load(CreateSampleDzi(), new MemoryTileFetcher());

        double initialWidth = controller.Viewport.ViewportWidth;

        controller.ZoomAboutLogicalPoint(2.0, 0.5, 0.5);

        Assert.True(controller.Viewport.ViewportWidth < initialWidth);
    }

    [Fact]
    public void SetControlSize_UpdatesViewportDimensions()
    {
        using var controller = new SKDeepZoomController();
        controller.SetControlSize(1024, 768);

        Assert.Equal(1024, controller.Viewport.ControlWidth);
        Assert.Equal(768, controller.Viewport.ControlHeight);
    }

    [Fact]
    public void SetViewport_UpdatesViewport()
    {
        using var controller = new SKDeepZoomController();
        controller.Load(CreateSampleDzi(), new MemoryTileFetcher());
        controller.SetControlSize(800, 600);

        controller.SetViewport(0.5, 0.1, 0.2);

        // Controller applies viewport changes immediately (no spring in controller)
        Assert.Equal(0.5, controller.Viewport.ViewportWidth, 4);
        Assert.Equal(0.1, controller.Viewport.ViewportOriginX, 4);
        Assert.Equal(0.2, controller.Viewport.ViewportOriginY, 4);
    }

    [Fact]
    public void SetViewport_FiresViewportChanged()
    {
        using var controller = new SKDeepZoomController();
        controller.Load(CreateSampleDzi(), new MemoryTileFetcher());
        controller.SetControlSize(800, 600);

        int count = 0;
        controller.ViewportChanged += (s, e) => count++;

        controller.SetViewport(0.5, 0.0, 0.0);

        Assert.True(count > 0, "ViewportChanged should fire on SetViewport");
    }

    [Fact]
    public void TileFailedEventArgs_Properties()
    {
        var tileId = new SKDeepZoomTileId(3, 1, 2);
        var ex = new InvalidOperationException("test error");
        var args = new SKDeepZoomTileFailedEventArgs(tileId, ex);

        Assert.Equal(tileId, args.TileId);
        Assert.Same(ex, args.Exception);
    }
}
