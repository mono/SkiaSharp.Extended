using SkiaSharp.Extended.DeepZoom;
using Xunit;

namespace SkiaSharp.Extended.DeepZoom.Tests;

public class DeepZoomSubImageTest
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        var sub = new DeepZoomSubImage(42, 7, 1.5, "http://example.com/42.dzi");

        Assert.Equal(42, sub.Id);
        Assert.Equal(7, sub.MortonIndex);
        Assert.Equal(1.5, sub.AspectRatio);
        Assert.Equal("http://example.com/42.dzi", sub.Source);
        Assert.Equal(1.0, sub.Opacity);
        Assert.Equal(0, sub.ZIndex);
    }

    [Fact]
    public void Opacity_ClampedTo01()
    {
        var sub = new DeepZoomSubImage(0, 0, 1.0, null);

        sub.Opacity = 1.5;
        Assert.Equal(1.0, sub.Opacity);

        sub.Opacity = -0.5;
        Assert.Equal(0.0, sub.Opacity);

        sub.Opacity = 0.75;
        Assert.Equal(0.75, sub.Opacity);
    }

    [Fact]
    public void GetMosaicBounds_InvertsViewportCoords()
    {
        var sub = new DeepZoomSubImage(0, 0, 1.0, null);

        // In SL inverted coords:
        // ViewportWidth = 10 means the sub-image occupies 1/10 of the mosaic width
        // ViewportOriginX = -5 means the sub-image is at position 0.5 in mosaic space
        sub.ViewportWidth = 10;
        sub.ViewportOriginX = -5;
        sub.ViewportOriginY = -3;

        var (x, y, w, h) = sub.GetMosaicBounds();

        Assert.Equal(0.5, x, 6);
        Assert.Equal(0.3, y, 6);
        Assert.Equal(0.1, w, 6);
        Assert.Equal(0.1, h, 6); // Square (aspect ratio = 1.0)
    }

    [Fact]
    public void GetMosaicBounds_RespectsAspectRatio()
    {
        var sub = new DeepZoomSubImage(0, 0, 2.0, null); // Width is 2x height

        sub.ViewportWidth = 10;
        sub.ViewportOriginX = 0;
        sub.ViewportOriginY = 0;

        var (x, y, w, h) = sub.GetMosaicBounds();

        Assert.Equal(0.0, x, 6);
        Assert.Equal(0.0, y, 6);
        Assert.Equal(0.1, w, 6);
        Assert.Equal(0.05, h, 6); // Half height due to 2:1 aspect ratio
    }

    [Fact]
    public void ParentToLocal_RoundTrip()
    {
        var sub = new DeepZoomSubImage(0, 0, 1.5, null);
        sub.ViewportWidth = 5;
        sub.ViewportOriginX = -2;
        sub.ViewportOriginY = -1;

        double parentX = 0.6;
        double parentY = 0.35;

        var (localX, localY) = sub.ParentToLocal(parentX, parentY);
        var (backX, backY) = sub.LocalToParent(localX, localY);

        Assert.Equal(parentX, backX, 10);
        Assert.Equal(parentY, backY, 10);
    }

    [Fact]
    public void LocalToParent_AtOrigin_ReturnsMosaicPosition()
    {
        var sub = new DeepZoomSubImage(0, 0, 1.0, null);
        sub.ViewportWidth = 10;
        sub.ViewportOriginX = -5;
        sub.ViewportOriginY = -3;

        var (px, py) = sub.LocalToParent(0, 0);

        // Should be at the mosaic position of the sub-image
        Assert.Equal(0.5, px, 6);
        Assert.Equal(0.3, py, 6);
    }

    [Fact]
    public void ZIndex_Settable()
    {
        var sub = new DeepZoomSubImage(0, 0, 1.0, null);

        sub.ZIndex = 5;
        Assert.Equal(5, sub.ZIndex);

        sub.ZIndex = -1;
        Assert.Equal(-1, sub.ZIndex);
    }

    [Fact]
    public void ViewportOrigin_DefaultsToZero()
    {
        var sub = new DeepZoomSubImage(0, 0, 1.0, null);

        Assert.Equal(0, sub.ViewportOriginX);
        Assert.Equal(0, sub.ViewportOriginY);
        Assert.Equal(0, sub.ViewportWidth);
    }
}
