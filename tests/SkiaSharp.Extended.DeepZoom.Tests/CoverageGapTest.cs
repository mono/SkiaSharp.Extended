using SkiaSharp;
using SkiaSharp.Extended;
using Xunit;
using System.Threading;

namespace SkiaSharp.Extended.DeepZoom.Tests;

/// <summary>
/// Tests for low-coverage areas: SKDeepZoomViewportState, SKDeepZoomTileRequest, viewport, and controller.
/// </summary>
public class CoverageGapTest
{
    // --- SKDeepZoomViewportState ---

    [Fact]
    public void ViewportState_Construction()
    {
        var state = new SKDeepZoomViewportState(0.5, 0.1, 0.2);
        Assert.Equal(0.5, state.ViewportWidth);
        Assert.Equal(0.1, state.OriginX);
        Assert.Equal(0.2, state.OriginY);
    }

    [Fact]
    public void ViewportState_Equality()
    {
        var s1 = new SKDeepZoomViewportState(0.5, 0.1, 0.2);
        var s2 = new SKDeepZoomViewportState(0.5, 0.1, 0.2);
        var s3 = new SKDeepZoomViewportState(0.3, 0.1, 0.2);

        Assert.Equal(s1, s2);
        Assert.True(s1 == s2);
        Assert.NotEqual(s1, s3);
        Assert.True(s1 != s3);
    }

    [Fact]
    public void ViewportState_GetHashCode_SameForEqual()
    {
        var s1 = new SKDeepZoomViewportState(0.5, 0.1, 0.2);
        var s2 = new SKDeepZoomViewportState(0.5, 0.1, 0.2);
        Assert.Equal(s1.GetHashCode(), s2.GetHashCode());
    }

    [Fact]
    public void ViewportState_Equals_Object()
    {
        var s1 = new SKDeepZoomViewportState(0.5, 0.1, 0.2);
        Assert.False(s1.Equals((object)"not a state"));
        Assert.True(s1.Equals((object)new SKDeepZoomViewportState(0.5, 0.1, 0.2)));
    }

    // --- SKDeepZoomViewport ---

    [Fact]
    public void Viewport_ElementToLogical_FullImage()
    {
        var vp = new SKDeepZoomViewport();
        vp.ControlWidth = 800;
        vp.ControlHeight = 600;
        vp.ViewportWidth = 1.0;
        vp.AspectRatio = 800.0 / 600.0;

        var (lx, ly) = vp.ElementToLogicalPoint(0, 0);
        Assert.Equal(0.0, lx, 5);
        Assert.Equal(0.0, ly, 5);
    }

    [Fact]
    public void Viewport_LogicalToElement_Roundtrip()
    {
        var vp = new SKDeepZoomViewport();
        vp.ControlWidth = 1024;
        vp.ControlHeight = 768;
        vp.ViewportWidth = 0.5;
        vp.ViewportOriginX = 0.2;
        vp.ViewportOriginY = 0.1;
        vp.AspectRatio = 1024.0 / 768.0;

        var (lx, ly) = vp.ElementToLogicalPoint(512, 384);
        var (sx, sy) = vp.LogicalToElementPoint(lx, ly);

        Assert.InRange(sx, 511, 513);
        Assert.InRange(sy, 383, 385);
    }

    [Fact]
    public void Viewport_ZoomAboutLogicalPoint()
    {
        var vp = new SKDeepZoomViewport();
        vp.ControlWidth = 800;
        vp.ControlHeight = 600;
        vp.ViewportWidth = 1.0;
        vp.AspectRatio = 800.0 / 600.0;

        vp.ZoomAboutLogicalPoint(2.0, 0.5, 0.375);
        Assert.Equal(0.5, vp.ViewportWidth, 5);
    }

    [Fact]
    public void Viewport_PanByScreenDelta()
    {
        var vp = new SKDeepZoomViewport();
        vp.ControlWidth = 800;
        vp.ControlHeight = 600;
        vp.ViewportWidth = 0.5;
        vp.AspectRatio = 800.0 / 600.0;

        double origX = vp.ViewportOriginX;
        vp.PanByScreenDelta(100, 0);
        Assert.True(vp.ViewportOriginX != origX);
    }

    [Fact]
    public void Viewport_Scale()
    {
        var vp = new SKDeepZoomViewport();
        vp.ControlWidth = 800;
        vp.ViewportWidth = 0.5;
        Assert.Equal(1600.0, vp.Scale, 5);
    }

    [Fact]
    public void Viewport_GetState_SetState()
    {
        var vp = new SKDeepZoomViewport();
        vp.ControlWidth = 800;
        vp.ControlHeight = 600;
        vp.ViewportWidth = 0.5;
        vp.ViewportOriginX = 0.1;
        vp.ViewportOriginY = 0.2;

        var state = vp.GetState();
        Assert.Equal(0.5, state.ViewportWidth);
        Assert.Equal(0.1, state.OriginX);
        Assert.Equal(0.2, state.OriginY);

        vp.ViewportWidth = 1.0;
        vp.SetState(state);
        Assert.Equal(0.5, vp.ViewportWidth);
    }

    // --- SKDeepZoomController ---

    [Fact]
    public void DeepZoomController_LoadAndDispose()
    {
        var xml = TestDataHelper.GetString("sample.dzi");
        var dzi = SKDeepZoomImageSource.Parse(xml);
        var controller = new SKDeepZoomController();
        controller.Load(dzi, new MemoryTileFetcher());
        controller.Dispose();
    }

    [Fact]
    public void DeepZoomController_Dispose_MultipleTimes()
    {
        var xml = TestDataHelper.GetString("sample.dzi");
        var dzi = SKDeepZoomImageSource.Parse(xml);
        var controller = new SKDeepZoomController();
        controller.Load(dzi, new MemoryTileFetcher());
        controller.Dispose();
        controller.Dispose(); // Should not throw
    }

    [Fact]
    public void DeepZoomController_Update_BeforeLoad()
    {
        var controller = new SKDeepZoomController();
        controller.Update();
        controller.Dispose();
    }

    [Fact]
    public void DeepZoomController_Render_BeforeLoad()
    {
        var controller = new SKDeepZoomController();
        using var surface = SKSurface.Create(new SKImageInfo(100, 100));
        using var renderer = new SKDeepZoomRenderer();
        renderer.Canvas = surface.Canvas;
        controller.Render(renderer);
        controller.Dispose();
    }

    [Fact]
    public void DeepZoomController_NullFetcher_GracefulDegradation()
    {
        var xml = TestDataHelper.GetString("sample.dzi");        var dzi = SKDeepZoomImageSource.Parse(xml);
        var controller = new SKDeepZoomController();
        controller.Load(dzi, new NullTileFetcher());
        controller.Update();
        using var surface = SKSurface.Create(new SKImageInfo(400, 300));
        using var renderer = new SKDeepZoomRenderer();
        renderer.Canvas = surface.Canvas;
        controller.Render(renderer);
        controller.Dispose();
    }

    /// <summary>A tile fetcher that always returns null (simulates 404s).</summary>
    private class NullTileFetcher : ISKDeepZoomTileFetcher
    {
        public Task<ISKDeepZoomTile?> FetchTileAsync(string url, CancellationToken ct = default)
            => Task.FromResult<ISKDeepZoomTile?>(null);
        public void Dispose() { }
    }
}
