using SkiaSharp;
using SkiaSharp.Extended.DeepZoom;
using Xunit;

namespace SkiaSharp.Extended.DeepZoom.Tests;

/// <summary>
/// Integration tests that exercise the full DeepZoom pipeline:
/// load → zoom → pan → schedule → render, simulating real user interactions.
/// </summary>
public class DeepZoomIntegrationTest
{
    private static DziTileSource CreateDzi(int width = 2048, int height = 2048)
    {
        string xml = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<Image xmlns=""http://schemas.microsoft.com/deepzoom/2008""
       Format=""jpg"" Overlap=""1"" TileSize=""256"">
  <Size Width=""{width}"" Height=""{height}""/>
</Image>";
        return DziTileSource.Parse(xml, "http://example.com/test");
    }

    [Fact]
    public void FullPipeline_LoadZoomPanRender()
    {
        using var controller = new DeepZoomController();
        using var fetcher = new MemoryTileFetcher();
        controller.UseSprings = false;
        controller.SetControlSize(800, 600);
        controller.Load(CreateDzi(), fetcher);

        // Zoom in
        controller.ZoomAboutScreenPoint(4.0, 400, 300);
        controller.Update(TimeSpan.FromSeconds(1.0 / 60));

        // Pan right
        controller.Pan(200, 0);
        controller.Update(TimeSpan.FromSeconds(1.0 / 60));

        // Render
        using var surface = SKSurface.Create(new SKImageInfo(800, 600));
        controller.Render(surface.Canvas);

        Assert.True(controller.Viewport.ViewportWidth < 1.0);
    }

    [Fact]
    public void ZoomSequence_ZoomInAndOut()
    {
        using var controller = new DeepZoomController();
        using var fetcher = new MemoryTileFetcher();
        controller.UseSprings = false;
        controller.SetControlSize(800, 600);
        controller.Load(CreateDzi(), fetcher);

        // Zoom in progressively
        for (int i = 0; i < 5; i++)
        {
            controller.ZoomAboutScreenPoint(2.0, 400, 300);
            controller.Update(TimeSpan.FromSeconds(1.0 / 60));
        }

        double zoomedWidth = controller.Viewport.ViewportWidth;
        Assert.True(zoomedWidth < 0.1, $"After 5x zoom in, width should be < 0.1, got {zoomedWidth}");

        // Zoom out
        for (int i = 0; i < 5; i++)
        {
            controller.ZoomAboutScreenPoint(0.5, 400, 300);
            controller.Update(TimeSpan.FromSeconds(1.0 / 60));
        }

        Assert.True(controller.Viewport.ViewportWidth > zoomedWidth);
    }

    [Fact]
    public void SpringAnimation_SettlesInReasonableTime()
    {
        using var controller = new DeepZoomController();
        using var fetcher = new MemoryTileFetcher();
        controller.UseSprings = true;
        controller.SetControlSize(800, 600);
        controller.Load(CreateDzi(), fetcher);

        controller.ZoomAboutScreenPoint(4.0, 400, 300);

        // Should settle within 10 seconds (600 frames at 60fps)
        bool settled = false;
        for (int i = 0; i < 600; i++)
        {
            controller.Update(TimeSpan.FromSeconds(1.0 / 60));
            if (controller.Spring.IsSettled)
            {
                settled = true;
                break;
            }
        }

        Assert.True(settled, "Spring should settle within 10 seconds");
    }

    [Fact]
    public void PanWhileZoomed_StaysInBounds()
    {
        using var controller = new DeepZoomController();
        using var fetcher = new MemoryTileFetcher();
        controller.UseSprings = false;
        controller.SetControlSize(800, 600);
        controller.Load(CreateDzi(), fetcher);

        // Zoom in
        controller.ZoomAboutScreenPoint(4.0, 400, 300);
        controller.Update(TimeSpan.FromSeconds(1.0 / 60));

        // Pan far to the right (beyond image bounds)
        for (int i = 0; i < 100; i++)
        {
            controller.Pan(-100, 0);
            controller.Update(TimeSpan.FromSeconds(1.0 / 60));
        }

        // Viewport origin should be constrained
        Assert.True(controller.Viewport.ViewportOriginX >= -0.1,
            $"Origin X should be constrained, got {controller.Viewport.ViewportOriginX}");
    }

    [Fact]
    public void Resize_UpdatesLayout()
    {
        using var controller = new DeepZoomController();
        using var fetcher = new MemoryTileFetcher();
        controller.UseSprings = false;
        controller.SetControlSize(800, 600);
        controller.Load(CreateDzi(), fetcher);

        var initialWidth = controller.Viewport.ViewportWidth;

        // Resize to larger
        controller.SetControlSize(1600, 1200);
        controller.Update(TimeSpan.FromSeconds(1.0 / 60));

        // Viewport width should remain the same (it's in logical units)
        Assert.Equal(initialWidth, controller.Viewport.ViewportWidth, 6);
    }

    [Fact]
    public void NonSquareImage_RespectAspectRatio()
    {
        using var controller = new DeepZoomController();
        using var fetcher = new MemoryTileFetcher();
        controller.UseSprings = false;
        controller.SetControlSize(800, 600);
        controller.Load(CreateDzi(1600, 900), fetcher);

        Assert.Equal(1600.0 / 900.0, controller.AspectRatio, 4);
    }

    [Fact]
    public void MultipleSources_CanReload()
    {
        using var controller = new DeepZoomController();
        using var fetcher = new MemoryTileFetcher();
        controller.UseSprings = false;
        controller.SetControlSize(800, 600);

        // Load first source
        controller.Load(CreateDzi(1024, 1024), fetcher);
        Assert.Equal(1.0, controller.AspectRatio, 4);

        // Load second source
        controller.Load(CreateDzi(1600, 900), fetcher);
        Assert.Equal(1600.0 / 900.0, controller.AspectRatio, 4);
    }

    [Fact]
    public async Task TileLoading_FetchesVisibleTiles()
    {
        using var controller = new DeepZoomController();
        using var fetcher = new MemoryTileFetcher();
        controller.UseSprings = false;
        controller.SetControlSize(512, 512);
        controller.Load(CreateDzi(512, 512), fetcher);

        controller.Update(TimeSpan.FromSeconds(1.0 / 60));

        // Wait for async tile loading
        await Task.Delay(300);

        Assert.True(fetcher.FetchCount > 0, "Should have fetched tiles");
    }

    [Fact]
    public void ControllerDispose_StopsTileLoading()
    {
        var controller = new DeepZoomController();
        var fetcher = new MemoryTileFetcher();
        controller.SetControlSize(800, 600);
        controller.Load(CreateDzi(), fetcher);
        controller.Update(TimeSpan.FromSeconds(1.0 / 60));

        controller.Dispose();
        fetcher.Dispose();
        // Should not throw
    }

    [Fact]
    public void ZoomAboutLogicalPoint_PointStaysFixed()
    {
        using var controller = new DeepZoomController();
        using var fetcher = new MemoryTileFetcher();
        controller.UseSprings = false;
        controller.SetControlSize(800, 600);
        controller.Load(CreateDzi(), fetcher);

        // Get screen position of logical point (0.5, 0.5) before zoom
        var (sx1, sy1) = controller.Viewport.LogicalToElementPoint(0.5, 0.5);

        // Zoom about that logical point
        controller.ZoomAboutLogicalPoint(2.0, 0.5, 0.5);
        controller.Update(TimeSpan.FromSeconds(1.0 / 60));

        // The same logical point should map to the same screen position
        var (sx2, sy2) = controller.Viewport.LogicalToElementPoint(0.5, 0.5);

        Assert.Equal(sx1, sx2, 1);
        Assert.Equal(sy1, sy2, 1);
    }
}
