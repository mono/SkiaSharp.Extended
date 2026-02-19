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
    public void ZoomAboutLogicalPoint_ExtremeZoom_ClampsToMinViewportWidth()
    {
        var vp = new Viewport { ViewportWidth = 0.001 };
        // Zoom in by 1 billion — should clamp to MinViewportWidth
        vp.ZoomAboutLogicalPoint(1_000_000_000, 0.5, 0.5);
        Assert.True(vp.ViewportWidth >= Viewport.MinViewportWidth);
        Assert.True(double.IsFinite(vp.Zoom));
        Assert.True(double.IsFinite(vp.Scale));
    }

    [Fact]
    public void ZoomAboutLogicalPoint_AtMinWidth_NoPan()
    {
        var vp = new Viewport { ViewportWidth = Viewport.MinViewportWidth };
        double origOriginX = vp.ViewportOriginX;
        double origOriginY = vp.ViewportOriginY;
        // Trying to zoom further should not shift the viewport
        vp.ZoomAboutLogicalPoint(2.0, 0.5, 0.5);
        Assert.Equal(Viewport.MinViewportWidth, vp.ViewportWidth);
        Assert.Equal(origOriginX, vp.ViewportOriginX);
        Assert.Equal(origOriginY, vp.ViewportOriginY);
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
    public void Constrain_NegativeOrigin_ClampsToZero()
    {
        var vp = new Viewport
        {
            ControlWidth = 800,
            ControlHeight = 600,
            AspectRatio = 1.0,
            ViewportWidth = 0.5,
            ViewportOriginX = -0.3,
            ViewportOriginY = -0.2
        };

        vp.Constrain();
        Assert.Equal(0.0, vp.ViewportOriginX);
        Assert.Equal(0.0, vp.ViewportOriginY);
    }

    [Fact]
    public void GetState_SetState_IndependentCopies()
    {
        var vp = new Viewport
        {
            ViewportWidth = 0.5,
            ViewportOriginX = 0.1,
            ViewportOriginY = 0.2
        };

        var state = vp.GetState();

        // Modify original viewport
        vp.ViewportWidth = 1.0;
        vp.ViewportOriginX = 0.0;
        vp.ViewportOriginY = 0.0;

        // State snapshot retains original values
        Assert.Equal(0.5, state.ViewportWidth);
        Assert.Equal(0.1, state.OriginX);
        Assert.Equal(0.2, state.OriginY);

        // Restore from state
        vp.SetState(state);
        Assert.Equal(0.5, vp.ViewportWidth);
        Assert.Equal(0.1, vp.ViewportOriginX);
        Assert.Equal(0.2, vp.ViewportOriginY);
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

    [Fact]
    public void Constrain_ExtremeZoom_ClampsToValid()
    {
        var vp = new Viewport { AspectRatio = 1.5 };
        vp.ControlWidth = 800;
        vp.ControlHeight = 600;
        vp.ViewportWidth = 0.0001;
        vp.Constrain();
        Assert.True(vp.ViewportWidth > 0);
    }

    [Fact]
    public void ZeroSize_DoesNotThrow()
    {
        var vp = new Viewport { AspectRatio = 1.5 };
        vp.ControlWidth = 0;
        vp.ControlHeight = 0;
        var state = vp.GetState();
        Assert.NotNull(state);
    }

    [Fact]
    public void Constrain_LargePositiveOrigin_ClampsToMaximum()
    {
        var vp = new Viewport
        {
            ControlWidth = 800,
            ControlHeight = 600,
            AspectRatio = 1.0,
            ViewportWidth = 0.5,
            ViewportOriginX = 10.0,
            ViewportOriginY = 10.0
        };
        vp.Constrain();
        // Origin + ViewportWidth should not exceed 1.0
        Assert.True(vp.ViewportOriginX + vp.ViewportWidth <= 1.001);
        Assert.True(vp.ViewportOriginX >= 0);
        Assert.True(vp.ViewportOriginY >= 0);
    }

    [Fact]
    public void Constrain_VeryWideAspectRatio_StaysWithinBounds()
    {
        var vp = new Viewport
        {
            ControlWidth = 1920,
            ControlHeight = 200,
            AspectRatio = 10.0, // Very wide: logical height = 1/10 = 0.1
            ViewportWidth = 0.5,
            ViewportOriginX = 0.8,
            ViewportOriginY = 0.5
        };
        vp.Constrain();

        double imageLogicalHeight = 1.0 / 10.0;
        Assert.True(vp.ViewportOriginX >= 0);
        Assert.True(vp.ViewportOriginX + vp.ViewportWidth <= 1.001);
        Assert.True(vp.ViewportOriginY >= 0);
        Assert.True(vp.ViewportOriginY + vp.ViewportHeight <= imageLogicalHeight + 0.001);
    }

    [Fact]
    public void Constrain_VeryTallAspectRatio_StaysWithinBounds()
    {
        var vp = new Viewport
        {
            ControlWidth = 200,
            ControlHeight = 1920,
            AspectRatio = 0.1, // Very tall: logical height = 1/0.1 = 10
            ViewportWidth = 0.5,
            ViewportOriginX = 0.8,
            ViewportOriginY = 15.0
        };
        vp.Constrain();

        double imageLogicalHeight = 1.0 / 0.1;
        Assert.True(vp.ViewportOriginX >= 0);
        Assert.True(vp.ViewportOriginX + vp.ViewportWidth <= 1.001);
        Assert.True(vp.ViewportOriginY >= 0);
        Assert.True(vp.ViewportOriginY + vp.ViewportHeight <= imageLogicalHeight + 0.001);
    }

    [Fact]
    public void ViewportWidth_Zero_ClampsToMinViewportWidth()
    {
        var vp = new Viewport();
        vp.ViewportWidth = 0;
        Assert.Equal(Viewport.MinViewportWidth, vp.ViewportWidth);
    }

    [Fact]
    public void ViewportWidth_Negative_ClampsToMinViewportWidth()
    {
        var vp = new Viewport();
        vp.ViewportWidth = -1.0;
        Assert.Equal(Viewport.MinViewportWidth, vp.ViewportWidth);
    }

    [Fact]
    public void Zoom_WithMinViewportWidth_ReturnsFiniteValue()
    {
        var vp = new Viewport();
        vp.ViewportWidth = 0;
        Assert.True(double.IsFinite(vp.Zoom));
        Assert.True(vp.Zoom > 0);
    }

    [Fact]
    public void GetLogicalBounds_AfterZoomAndPan_ReturnsCorrectBounds()
    {
        var vp = new Viewport
        {
            ControlWidth = 800,
            ControlHeight = 600,
            ViewportWidth = 0.5,
            ViewportOriginX = 0.25,
            ViewportOriginY = 0.1
        };

        var (left, top, right, bottom) = vp.GetLogicalBounds();
        Assert.Equal(0.25, left, 6);
        Assert.Equal(0.1, top, 6);
        Assert.Equal(0.75, right, 6); // 0.25 + 0.5
        double expectedBottom = 0.1 + vp.ViewportHeight;
        Assert.Equal(expectedBottom, bottom, 6);
    }

    [Fact]
    public void Constrain_ZoomedOut_UsesPostClampHeightForOriginY()
    {
        // Regression: Constrain must compute vpHeight AFTER clamping ViewportWidth,
        // not before, otherwise OriginY is clamped using a stale (too-large) height.
        var vp = new Viewport
        {
            ControlWidth = 1000,
            ControlHeight = 200,
            AspectRatio = 2.0,       // landscape: imageLogicalHeight = 0.5
            ViewportWidth = 2.0,     // zoomed out beyond image
            ViewportOriginX = 0.0,
            ViewportOriginY = 0.35
        };

        vp.Constrain();

        // ViewportWidth must be clamped to 1.0
        Assert.Equal(1.0, vp.ViewportWidth, 6);

        // Post-clamp vpHeight = 1.0 * 200/1000 = 0.2
        // Correct: originY = 0.5 - 0.2 = 0.3
        // Stale bug: vpHeight = 2.0 * 200/1000 = 0.4, originY = 0.5 - 0.4 = 0.1
        double imageLogicalHeight = 1.0 / 2.0; // 0.5
        Assert.Equal(0.3, vp.ViewportOriginY, 6);
        Assert.True(vp.ViewportOriginY + vp.ViewportHeight <= imageLogicalHeight + 1e-9);
    }

    // --- Additional Constrain tests ---

    [Fact]
    public void Constrain_SquareAspect_ClampsOriginXAndY()
    {
        var vp = new Viewport
        {
            ControlWidth = 500,
            ControlHeight = 500,
            AspectRatio = 1.0,
            ViewportWidth = 0.25,
            ViewportOriginX = 0.9,
            ViewportOriginY = 0.9
        };
        vp.Constrain();
        Assert.Equal(0.75, vp.ViewportOriginX, 6);
        Assert.Equal(0.75, vp.ViewportOriginY, 6);
    }

    [Fact]
    public void Constrain_ViewportExactlyFitsImage_OriginIsZero()
    {
        var vp = new Viewport
        {
            ControlWidth = 800,
            ControlHeight = 800,
            AspectRatio = 1.0,
            ViewportWidth = 1.0,
            ViewportOriginX = 0.0,
            ViewportOriginY = 0.0
        };
        vp.Constrain();
        Assert.Equal(0.0, vp.ViewportOriginX);
        Assert.Equal(0.0, vp.ViewportOriginY);
    }

    // --- Additional GetLogicalBounds tests ---

    [Fact]
    public void GetLogicalBounds_ZoomedIn4x_ReturnsQuarterWidth()
    {
        var vp = new Viewport
        {
            ControlWidth = 1000,
            ControlHeight = 1000,
            ViewportWidth = 0.25,
            ViewportOriginX = 0.1,
            ViewportOriginY = 0.2
        };
        var (left, top, right, bottom) = vp.GetLogicalBounds();
        Assert.Equal(0.1, left, 6);
        Assert.Equal(0.2, top, 6);
        Assert.Equal(0.35, right, 6);
        Assert.Equal(0.45, bottom, 6);
    }

    [Fact]
    public void GetLogicalBounds_WidthEqualsHeight_SquareViewport()
    {
        var vp = new Viewport
        {
            ControlWidth = 400,
            ControlHeight = 400,
            ViewportWidth = 1.0,
            ViewportOriginX = 0.0,
            ViewportOriginY = 0.0
        };
        var (left, top, right, bottom) = vp.GetLogicalBounds();
        Assert.Equal(0.0, left);
        Assert.Equal(0.0, top);
        Assert.Equal(1.0, right);
        Assert.Equal(1.0, bottom, 6);
    }

    // --- Additional ElementToLogicalPoint tests ---

    [Fact]
    public void ElementToLogicalPoint_WithOffset_ConvertsCorrectly()
    {
        var vp = new Viewport
        {
            ControlWidth = 800,
            ControlHeight = 600,
            ViewportWidth = 0.5,
            ViewportOriginX = 0.25,
            ViewportOriginY = 0.1
        };
        // Scale = 800/0.5 = 1600
        var (lx, ly) = vp.ElementToLogicalPoint(160, 80);
        Assert.Equal(0.35, lx, 6); // 0.25 + 160/1600
        Assert.Equal(0.15, ly, 6); // 0.1 + 80/1600
    }

    [Fact]
    public void ElementToLogicalPoint_BottomRightCorner()
    {
        var vp = new Viewport
        {
            ControlWidth = 800,
            ControlHeight = 600,
            ViewportWidth = 1.0,
            ViewportOriginX = 0.0,
            ViewportOriginY = 0.0
        };
        var (lx, ly) = vp.ElementToLogicalPoint(800, 600);
        Assert.Equal(1.0, lx, 6);
        Assert.Equal(0.75, ly, 6);
    }

    // --- Additional GetZoomRect tests ---

    [Fact]
    public void GetZoomRect_HalfWidth_ReturnsHalfSizeRect()
    {
        var vp = new Viewport
        {
            ControlWidth = 800,
            ControlHeight = 600,
            AspectRatio = 2.0,
            ViewportWidth = 0.5,
            ViewportOriginX = 0.1,
            ViewportOriginY = 0.05
        };
        var (x, y, w, h) = vp.GetZoomRect(0.5);
        Assert.Equal(0.1, x, 6);
        Assert.Equal(0.05, y, 6);
        Assert.Equal(0.5, w, 6);
        Assert.Equal(0.25, h, 6); // 0.5 / 2.0
    }

    [Fact]
    public void GetZoomRect_DifferentViewportWidthParam_UsesParam()
    {
        var vp = new Viewport
        {
            ControlWidth = 800,
            ControlHeight = 600,
            AspectRatio = 1.0,
            ViewportWidth = 0.5,
            ViewportOriginX = 0.0,
            ViewportOriginY = 0.0
        };
        // Pass a different viewportWidth than what the viewport currently has
        var (x, y, w, h) = vp.GetZoomRect(0.25);
        Assert.Equal(0.25, w, 6);
        Assert.Equal(0.25, h, 6);
    }
}
