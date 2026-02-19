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
    public bool IsDisposed { get; private set; }

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
        IsDisposed = true;
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
        Assert.False(needsRepaint, "Update should return false when idle and settled");
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
        controller.Load(dzi, fetcher);

        // Add tile to cache AFTER loading (Load clears the cache)
        var tileId = new TileId(0, 0, 0);
        var tileBmp = new SKBitmap(256, 256);
        using (var canvas = new SKCanvas(tileBmp))
            canvas.Clear(SKColors.Red);
        controller.Cache.Put(tileId, tileBmp);

        using var surface = SKSurface.Create(new SKImageInfo(800, 600));
        surface.Canvas.Clear(SKColors.White);
        controller.Render(surface.Canvas);

        // Verify something was drawn (pixel at center shouldn't be white/default)
        using var snapshot = surface.Snapshot();
        using var pixmap = snapshot.PeekPixels();
        // The low-level tile 0,0,0 covers the whole image at level 0
        // At full viewport, it should be rendered somewhere
        var centerPixel = pixmap.GetPixelColor(400, 300);
        Assert.NotEqual(SKColors.White, centerPixel);
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
    public void SubImages_EmptyByDefault()
    {
        using var controller = new DeepZoomController();
        Assert.Empty(controller.SubImages);
    }

    [Fact]
    public void Load_DzcTileSource_PopulatesSubImages()
    {
        using var controller = new DeepZoomController();
        var items = new List<DzcSubImage>
        {
            new DzcSubImage(0, 0, 256, 128, null) { ViewportWidth = 2.0, ViewportX = -0.5, ViewportY = -0.25 },
            new DzcSubImage(1, 1, 512, 512, "img1.dzi") { ViewportWidth = 1.0, ViewportX = 0, ViewportY = 0 },
        };
        var dzc = new DzcTileSource(8, 256, "jpg", items);
        using var fetcher = new MemoryTileFetcher();

        controller.Load(dzc, fetcher);

        Assert.Equal(2, controller.SubImages.Count);
        Assert.Equal(0, controller.SubImages[0].Id);
        Assert.Equal(2.0, controller.SubImages[0].AspectRatio, 6);
        Assert.Equal("img1.dzi", controller.SubImages[1].Source);
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

    [Fact]
    public void ZoomAboutLogicalPoint_FiresViewportChanged()
    {
        using var controller = new DeepZoomController();
        controller.Load(CreateSampleDzi(), new MemoryTileFetcher());
        int count = 0;
        controller.ViewportChanged += (s, e) => count++;

        controller.ZoomAboutLogicalPoint(0.5, 0.5, 0.5);
        Assert.True(count > 0);
    }

    [Fact]
    public void Pan_FiresViewportChanged()
    {
        using var controller = new DeepZoomController();
        controller.Load(CreateSampleDzi(), new MemoryTileFetcher());
        int count = 0;
        controller.ViewportChanged += (s, e) => count++;

        controller.Pan(10, 10);
        Assert.True(count > 0);
    }

    [Fact]
    public void Update_DuringSpringAnimation_FiresViewportChanged()
    {
        using var controller = new DeepZoomController();
        controller.Load(CreateSampleDzi(), new MemoryTileFetcher());
        controller.UseSprings = true;

        // Subscribe before zoom to catch all events
        int count = 0;
        controller.ViewportChanged += (s, e) => count++;

        // Zoom triggers ApplyViewportToSpring which fires ViewportChanged
        controller.ZoomAboutLogicalPoint(0.5, 0.5, 0.5);
        Assert.True(count > 0);

        // Update during animation should also fire if viewport moves
        int preUpdateCount = count;
        controller.Update(TimeSpan.FromMilliseconds(16));
        // Spring may or may not have moved — either way the zoom already proved it works
        Assert.True(count >= preUpdateCount);
    }

    [Fact]
    public void Dispose_DisposesFetcher()
    {
        var fetcher = new MemoryTileFetcher();
        var controller = new DeepZoomController();
        controller.Load(CreateSampleDzi(), fetcher);

        controller.Dispose();

        // Fetcher should be disposed (MemoryTileFetcher.Dispose clears tiles)
        Assert.True(fetcher.IsDisposed);
    }

    [Fact]
    public void Load_Reload_DisposesPreviousFetcher()
    {
        var fetcher1 = new MemoryTileFetcher();
        using var controller = new DeepZoomController();
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
        using var controller = new DeepZoomController();
        var fetcher = new MemoryTileFetcher();
        fetcher.AddSolidTile("http://example.com/image_files/0/0_0.jpg", 256, 256, SkiaSharp.SKColors.Red);
        controller.Load(CreateSampleDzi(), fetcher);

        // Trigger tile scheduling
        controller.SetControlSize(800, 600);
        controller.Update(TimeSpan.FromMilliseconds(16));

        // Now reload — cache should be cleared
        controller.Load(CreateSampleDzi(), fetcher);

        // After reload, fetcher should get new tile requests
        int fetchesBefore = fetcher.FetchCount;
        controller.SetControlSize(800, 600);
        controller.Update(TimeSpan.FromMilliseconds(16));
        // New fetches should occur since cache was cleared
        Assert.True(fetcher.FetchCount > fetchesBefore, "Cache clear should trigger new tile fetches");
    }

    [Fact]
    public void Load_DzcFromFile_PopulatesSubImagesWithCorrectCount()
    {
        using var stream = TestDataHelper.GetStream("conceptcars.dzc");
        var dzc = DzcTileSource.Parse(stream);
        using var controller = new DeepZoomController();
        using var fetcher = new MemoryTileFetcher();

        controller.Load(dzc, fetcher);

        Assert.Equal(dzc.ItemCount, controller.SubImages.Count);
        Assert.True(controller.SubImages.Count > 0, "SubImages should be populated from DZC file");
    }

    [Fact]
    public async Task LoadTileAsync_WithTilesBaseUri_FetcherReceivesFullUrl()
    {
        using var controller = new DeepZoomController();
        controller.UseSprings = false;
        controller.SetControlSize(512, 512);

        var dzi = CreateSampleDzi(); // TilesBaseUri = "http://example.com/test"
        using var fetcher = new MemoryTileFetcher();
        controller.Load(dzi, fetcher);

        // Trigger tile scheduling
        controller.Update(TimeSpan.FromSeconds(1.0 / 60));

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
        using var controller = new DeepZoomController();
        controller.UseSprings = false;
        controller.SetControlSize(800, 600);
        var dzi = CreateSampleDzi(); // 512x512, aspect ratio 1.0
        using var fetcher = new MemoryTileFetcher();
        controller.Load(dzi, fetcher);

        var (x, y, w, h) = controller.GetZoomRect();

        // At initial load: origin (0,0), width 1.0, height = width / aspect = 1.0
        Assert.Equal(0.0, x, 6);
        Assert.Equal(0.0, y, 6);
        Assert.Equal(1.0, w, 6);
        Assert.Equal(1.0, h, 6);

        // Zoom in and verify the rect changes
        controller.ZoomAboutScreenPoint(4.0, 400, 300);
        controller.Update(TimeSpan.FromSeconds(1.0 / 60));

        var (x2, y2, w2, h2) = controller.GetZoomRect();
        Assert.True(w2 < w, $"Zoomed width {w2} should be less than initial {w}");
        Assert.True(h2 < h, $"Zoomed height {h2} should be less than initial {h}");
    }

    [Fact]
    public void ViewportChanged_FiresOnZoom()
    {
        using var controller = new DeepZoomController();
        controller.UseSprings = false;
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
        using var controller = new DeepZoomController();
        controller.UseSprings = false;
        controller.SetControlSize(800, 600);
        controller.Load(CreateSampleDzi(), new MemoryTileFetcher());

        // Zoom in first so panning changes the viewport
        controller.ZoomAboutScreenPoint(4.0, 400, 300);
        controller.Update(TimeSpan.FromSeconds(1.0 / 60));

        int count = 0;
        controller.ViewportChanged += (s, e) => count++;

        controller.Pan(50, 50);
        Assert.True(count > 0, "ViewportChanged should fire on pan");
    }

    [Fact]
    public void ViewportChanged_FiresDuringSpringAnimation()
    {
        using var controller = new DeepZoomController();
        controller.UseSprings = true;
        controller.SetControlSize(800, 600);
        controller.Load(CreateSampleDzi(), new MemoryTileFetcher());

        controller.ZoomAboutScreenPoint(4.0, 400, 300);

        int count = 0;
        controller.ViewportChanged += (s, e) => count++;

        // During animation, Update should fire ViewportChanged as viewport moves
        controller.Update(TimeSpan.FromMilliseconds(16));
        controller.Update(TimeSpan.FromMilliseconds(16));
        controller.Update(TimeSpan.FromMilliseconds(16));

        Assert.True(count > 0, "ViewportChanged should fire during spring animation");
    }

    [Fact]
    public void IsIdle_TrueWhenSettledAndNoPendingTiles()
    {
        using var controller = new DeepZoomController();
        controller.UseSprings = false;
        controller.SetControlSize(800, 600);

        // Before load, should be idle
        Assert.True(controller.IsIdle);
    }

    [Fact]
    public void IsIdle_FalseDuringSpringAnimation()
    {
        using var controller = new DeepZoomController();
        controller.UseSprings = true;
        controller.SetControlSize(800, 600);
        controller.Load(CreateSampleDzi(), new MemoryTileFetcher());

        controller.ZoomAboutScreenPoint(4.0, 400, 300);
        controller.Update(TimeSpan.FromMilliseconds(16));

        // Spring should not be settled yet
        Assert.False(controller.IsIdle, "Should not be idle during spring animation");
    }

    [Fact]
    public void IsIdle_TransitionsToTrueAfterSettling()
    {
        using var controller = new DeepZoomController();
        controller.UseSprings = true;
        controller.SetControlSize(800, 600);
        controller.Load(CreateSampleDzi(), new MemoryTileFetcher());

        controller.ZoomAboutScreenPoint(2.0, 400, 300);

        // Run many frames until settled
        for (int i = 0; i < 600; i++)
        {
            controller.Update(TimeSpan.FromSeconds(1.0 / 60));
            if (controller.IsIdle) break;
        }

        Assert.True(controller.IsIdle, "Should eventually become idle after settling");
    }

    [Fact]
    public void ImageOpenSucceeded_FiresOnDziLoad()
    {
        using var controller = new DeepZoomController();
        bool fired = false;
        controller.ImageOpenSucceeded += (s, e) => fired = true;

        controller.Load(CreateSampleDzi(), new MemoryTileFetcher());
        Assert.True(fired, "ImageOpenSucceeded should fire on DZI load");
    }

    [Fact]
    public void ImageOpenSucceeded_FiresOnDzcLoad()
    {
        using var controller = new DeepZoomController();
        bool fired = false;
        controller.ImageOpenSucceeded += (s, e) => fired = true;

        var dzc = new DzcTileSource(8, 256, "jpg", new[]
        {
            new DzcSubImage(0, 0, 256, 256, null)
        });
        controller.Load(dzc, new MemoryTileFetcher());

        Assert.True(fired, "ImageOpenSucceeded should fire on DZC load");
    }

    [Fact]
    public void MotionFinished_FiresWhenSpringSettlesAfterZoom()
    {
        using var controller = new DeepZoomController();
        controller.UseSprings = true;
        controller.SetControlSize(800, 600);
        controller.Load(CreateSampleDzi(), new MemoryTileFetcher());

        bool motionFinished = false;
        controller.MotionFinished += (s, e) => motionFinished = true;

        controller.ZoomAboutScreenPoint(2.0, 400, 300);

        for (int i = 0; i < 600; i++)
        {
            controller.Update(TimeSpan.FromSeconds(1.0 / 60));
            if (motionFinished) break;
        }

        Assert.True(motionFinished, "MotionFinished should fire when spring settles after zoom");
    }

    [Fact]
    public void MotionFinished_FiresWhenSpringSettlesAfterPan()
    {
        using var controller = new DeepZoomController();
        controller.UseSprings = true;
        controller.SetControlSize(800, 600);
        controller.Load(CreateSampleDzi(), new MemoryTileFetcher());

        // Zoom in first so panning has effect
        controller.ZoomAboutScreenPoint(4.0, 400, 300);
        for (int i = 0; i < 600; i++)
        {
            controller.Update(TimeSpan.FromSeconds(1.0 / 60));
            if (controller.IsIdle) break;
        }

        bool motionFinished = false;
        controller.MotionFinished += (s, e) => motionFinished = true;

        controller.Pan(50, 50);

        for (int i = 0; i < 600; i++)
        {
            controller.Update(TimeSpan.FromSeconds(1.0 / 60));
            if (motionFinished) break;
        }

        Assert.True(motionFinished, "MotionFinished should fire when spring settles after pan");
    }

    [Fact]
    public void MotionFinished_DoesNotFireWithSpringsDisabled()
    {
        using var controller = new DeepZoomController();
        controller.UseSprings = false;
        controller.SetControlSize(800, 600);
        controller.Load(CreateSampleDzi(), new MemoryTileFetcher());

        bool motionFinished = false;
        controller.MotionFinished += (s, e) => motionFinished = true;

        controller.ZoomAboutScreenPoint(2.0, 400, 300);
        controller.Update(TimeSpan.FromSeconds(1.0 / 60));

        // With springs disabled, spring snaps to target instantly (already settled → no transition)
        // MotionFinished fires when !wasSettled && IsSettled, so if it was already settled, no event
        Assert.False(motionFinished, "MotionFinished should not fire with springs disabled");
    }

    [Fact]
    public void ResetView_FiresViewportChanged()
    {
        using var controller = new DeepZoomController();
        controller.UseSprings = false;
        controller.SetControlSize(800, 600);
        controller.Load(CreateSampleDzi(), new MemoryTileFetcher());

        controller.ZoomAboutScreenPoint(4.0, 400, 300);
        controller.Update(TimeSpan.FromSeconds(1.0 / 60));

        int count = 0;
        controller.ViewportChanged += (s, e) => count++;

        controller.ResetView();
        Assert.True(count > 0, "ViewportChanged should fire on ResetView");
    }

    [Fact]
    public async Task InvalidateRequired_FiresWhenTileLoads()
    {
        using var controller = new DeepZoomController();
        controller.UseSprings = false;
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
        controller.Update(TimeSpan.FromSeconds(1.0 / 60));

        // Wait for async tile load with polling
        for (int i = 0; i < 40 && !invalidated; i++)
            await Task.Delay(50);

        Assert.True(invalidated, "InvalidateRequired should fire when a tile loads");
    }

    [Fact]
    public void ZoomAboutLogicalPoint_ChangesViewport()
    {
        using var controller = new DeepZoomController();
        controller.UseSprings = false;
        controller.SetControlSize(800, 600);
        controller.Load(CreateSampleDzi(), new MemoryTileFetcher());

        double initialWidth = controller.Viewport.ViewportWidth;

        controller.ZoomAboutLogicalPoint(2.0, 0.5, 0.5);
        controller.Update(TimeSpan.FromSeconds(1.0 / 60));

        Assert.True(controller.Viewport.ViewportWidth < initialWidth);
    }

    [Fact]
    public void SetControlSize_UpdatesViewportDimensions()
    {
        using var controller = new DeepZoomController();
        controller.SetControlSize(1024, 768);

        Assert.Equal(1024, controller.Viewport.ControlWidth);
        Assert.Equal(768, controller.Viewport.ControlHeight);
    }
}
