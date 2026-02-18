using SkiaSharp.Extended.DeepZoom;

namespace SkiaSharp.Extended.DeepZoom.Tests;

public class ViewportTest
{
    [Fact]
    public void DefaultViewport_IsFullyZoomedOut()
    {
        var vp = new Viewport();
        Assert.Equal(1.0, vp.ViewportWidth);
        Assert.Equal(0.0, vp.ViewportOriginX);
        Assert.Equal(0.0, vp.ViewportOriginY);
    }

    [Fact]
    public void Scale_CalculatedCorrectly()
    {
        var vp = new Viewport { ControlWidth = 800, ViewportWidth = 1.0 };
        Assert.Equal(800.0, vp.Scale);

        vp.ViewportWidth = 0.5;
        Assert.Equal(1600.0, vp.Scale);
    }

    [Fact]
    public void ViewportHeight_DerivedFromWidthAndControlAspect()
    {
        var vp = new Viewport
        {
            ControlWidth = 800,
            ControlHeight = 600,
            ViewportWidth = 1.0
        };
        Assert.Equal(0.75, vp.ViewportHeight, 6); // 1.0 * 600/800
    }

    [Fact]
    public void ElementToLogicalPoint_Origin()
    {
        var vp = new Viewport
        {
            ControlWidth = 800,
            ControlHeight = 600,
            ViewportWidth = 1.0,
            ViewportOriginX = 0.0,
            ViewportOriginY = 0.0
        };

        var (lx, ly) = vp.ElementToLogicalPoint(0, 0);
        Assert.Equal(0.0, lx, 6);
        Assert.Equal(0.0, ly, 6);
    }

    [Fact]
    public void ElementToLogicalPoint_Center()
    {
        var vp = new Viewport
        {
            ControlWidth = 800,
            ControlHeight = 600,
            ViewportWidth = 1.0,
            ViewportOriginX = 0.0,
            ViewportOriginY = 0.0
        };

        var (lx, ly) = vp.ElementToLogicalPoint(400, 300);
        Assert.Equal(0.5, lx, 6);
        Assert.Equal(0.375, ly, 6); // 300/800 = 0.375
    }

    [Fact]
    public void LogicalToElementPoint_RoundTrips()
    {
        var vp = new Viewport
        {
            ControlWidth = 800,
            ControlHeight = 600,
            ViewportWidth = 0.5,
            ViewportOriginX = 0.25,
            ViewportOriginY = 0.1
        };

        var (lx, ly) = vp.ElementToLogicalPoint(200, 150);
        var (sx, sy) = vp.LogicalToElementPoint(lx, ly);

        Assert.Equal(200, sx, 4);
        Assert.Equal(150, sy, 4);
    }

    [Fact]
    public void ZoomAboutLogicalPoint_DoubleZoom_PointStaysFixed()
    {
        var vp = new Viewport
        {
            ControlWidth = 800,
            ControlHeight = 600,
            ViewportWidth = 1.0,
            ViewportOriginX = 0.0,
            ViewportOriginY = 0.0
        };

        // Get screen position of logical point (0.5, 0.3) before zoom
        var (screenXBefore, screenYBefore) = vp.LogicalToElementPoint(0.5, 0.3);

        // Zoom 2x about (0.5, 0.3)
        vp.ZoomAboutLogicalPoint(2.0, 0.5, 0.3);

        // Get screen position after zoom — should be the same
        var (screenXAfter, screenYAfter) = vp.LogicalToElementPoint(0.5, 0.3);

        Assert.Equal(screenXBefore, screenXAfter, 6);
        Assert.Equal(screenYBefore, screenYAfter, 6);
        Assert.Equal(0.5, vp.ViewportWidth, 6); // 1.0 / 2.0
    }

    [Fact]
    public void ZoomAboutLogicalPoint_ZoomOut_PointStaysFixed()
    {
        var vp = new Viewport
        {
            ControlWidth = 800,
            ControlHeight = 600,
            ViewportWidth = 0.5,
            ViewportOriginX = 0.25,
            ViewportOriginY = 0.1
        };

        var (sx1, sy1) = vp.LogicalToElementPoint(0.4, 0.2);
        vp.ZoomAboutLogicalPoint(0.5, 0.4, 0.2); // zoom out
        var (sx2, sy2) = vp.LogicalToElementPoint(0.4, 0.2);

        Assert.Equal(sx1, sx2, 4);
        Assert.Equal(sy1, sy2, 4);
        Assert.Equal(1.0, vp.ViewportWidth, 6); // 0.5 / 0.5
    }

    [Fact]
    public void ZoomAboutLogicalPoint_InvalidFactor_Throws()
    {
        var vp = new Viewport();
        Assert.Throws<ArgumentOutOfRangeException>(() => vp.ZoomAboutLogicalPoint(0, 0.5, 0.5));
        Assert.Throws<ArgumentOutOfRangeException>(() => vp.ZoomAboutLogicalPoint(-1, 0.5, 0.5));
    }

    [Fact]
    public void PanByScreenDelta_MovesOrigin()
    {
        var vp = new Viewport
        {
            ControlWidth = 800,
            ViewportWidth = 1.0,
            ViewportOriginX = 0.0,
            ViewportOriginY = 0.0
        };

        vp.PanByScreenDelta(80, 60); // drag right and down
        // Screen delta of 80 at scale 800 = logical delta of 0.1
        Assert.Equal(-0.1, vp.ViewportOriginX, 6);
        Assert.Equal(-0.075, vp.ViewportOriginY, 6); // 60/800
    }

    [Fact]
    public void GetLogicalBounds_FullView()
    {
        var vp = new Viewport
        {
            ControlWidth = 800,
            ControlHeight = 600,
            ViewportWidth = 1.0,
            ViewportOriginX = 0.0,
            ViewportOriginY = 0.0
        };

        var (left, top, right, bottom) = vp.GetLogicalBounds();
        Assert.Equal(0.0, left);
        Assert.Equal(0.0, top);
        Assert.Equal(1.0, right);
        Assert.Equal(0.75, bottom, 6);
    }

    [Fact]
    public void Constrain_ClampsToImageBounds()
    {
        var vp = new Viewport
        {
            ControlWidth = 800,
            ControlHeight = 600,
            AspectRatio = 1.5, // 1.5:1 → logical height = 1/1.5 = 0.667
            ViewportWidth = 0.5,
            ViewportOriginX = 0.8, // too far right — 0.8 + 0.5 = 1.3 > 1.0
            ViewportOriginY = 0.0
        };

        vp.Constrain();
        Assert.Equal(0.5, vp.ViewportOriginX, 6); // clamped to 1.0 - 0.5
    }

    [Fact]
    public void Constrain_PreventsZoomOutBeyondFull()
    {
        var vp = new Viewport
        {
            ViewportWidth = 2.0, // zoomed out beyond image
            AspectRatio = 1.0
        };

        vp.Constrain();
        Assert.Equal(1.0, vp.ViewportWidth, 6);
    }

    [Fact]
    public void GetState_SetState_RoundTrips()
    {
        var vp = new Viewport
        {
            ViewportWidth = 0.3,
            ViewportOriginX = 0.25,
            ViewportOriginY = 0.15
        };

        var state = vp.GetState();
        var vp2 = new Viewport();
        vp2.SetState(state);

        Assert.Equal(0.3, vp2.ViewportWidth, 10);
        Assert.Equal(0.25, vp2.ViewportOriginX, 10);
        Assert.Equal(0.15, vp2.ViewportOriginY, 10);
    }

    [Fact]
    public void Zoom_Property_ReflectsViewportWidth()
    {
        var vp = new Viewport { ViewportWidth = 0.5 };
        Assert.Equal(2.0, vp.Zoom);

        vp.ViewportWidth = 0.25;
        Assert.Equal(4.0, vp.Zoom);
    }

    [Fact]
    public void ViewportState_Equality()
    {
        var a = new ViewportState(1.0, 0.5, 0.25);
        var b = new ViewportState(1.0, 0.5, 0.25);
        var c = new ViewportState(0.5, 0.5, 0.25);

        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.NotEqual(a, c);
        Assert.True(a != c);
    }

    [Fact]
    public void GetZoomRect_InitialViewport_ReturnsFullWidth()
    {
        var vp = new Viewport
        {
            ControlWidth = 800,
            ControlHeight = 600,
            AspectRatio = 2.0,
            ViewportWidth = 1.0,
            ViewportOriginX = 0.0,
            ViewportOriginY = 0.0
        };

        var (x, y, w, h) = vp.GetZoomRect(vp.ViewportWidth);
        Assert.Equal(0.0, x);
        Assert.Equal(0.0, y);
        Assert.Equal(1.0, w);
        Assert.Equal(0.5, h, 6); // 1.0 / 2.0
    }

    [Fact]
    public void GetZoomRect_AfterZoom_ReturnsSmallerRect()
    {
        var vp = new Viewport
        {
            ControlWidth = 800,
            ControlHeight = 600,
            AspectRatio = 1.0,
            ViewportWidth = 1.0,
            ViewportOriginX = 0.0,
            ViewportOriginY = 0.0
        };

        // Zoom in 2x
        vp.ZoomAboutLogicalPoint(2.0, 0.5, 0.5);

        var (x, y, w, h) = vp.GetZoomRect(vp.ViewportWidth);
        Assert.Equal(0.5, w, 6); // 1.0 / 2.0
        Assert.Equal(0.5, h, 6); // 0.5 / 1.0
        Assert.True(w < 1.0);
    }
}
