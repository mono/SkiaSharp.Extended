using System.Drawing;
using System.Drawing.Drawing2D;

namespace SkiaSharp.Drawing.Tests;

public class RegionTests
{
    // --- Constructors ---

    [Fact]
    public void Constructor_Default_IsInfinite()
    {
        using var bmp = new Bitmap(10, 10);
        using var g = Graphics.FromImage(bmp);
        using var region = new Region();
        Assert.True(region.IsInfinite(g));
    }

    [Fact]
    public void Constructor_FromRectangle()
    {
        using var region = new Region(new Rectangle(10, 20, 30, 40));
        using var bmp = new Bitmap(100, 100);
        using var g = Graphics.FromImage(bmp);
        Assert.False(region.IsInfinite(g));
        Assert.False(region.IsEmpty(g));
    }

    [Fact]
    public void Constructor_FromRectangleF()
    {
        using var region = new Region(new RectangleF(10, 20, 30, 40));
        using var bmp = new Bitmap(100, 100);
        using var g = Graphics.FromImage(bmp);
        Assert.False(region.IsInfinite(g));
    }

    [Fact]
    public void Constructor_FromGraphicsPath()
    {
        using var path = new GraphicsPath();
        path.AddRectangle(new RectangleF(0, 0, 50, 50));
        using var region = new Region(path);
        using var bmp = new Bitmap(100, 100);
        using var g = Graphics.FromImage(bmp);
        Assert.False(region.IsInfinite(g));
    }

    // --- IsVisible ---

    [Fact]
    public void IsVisible_PointInRegion_ReturnsTrue()
    {
        using var region = new Region(new RectangleF(0, 0, 100, 100));
        using var bmp = new Bitmap(200, 200);
        using var g = Graphics.FromImage(bmp);
        Assert.True(region.IsVisible(50, 50, g));
    }

    [Fact]
    public void IsVisible_PointOutside_ReturnsFalse()
    {
        using var region = new Region(new RectangleF(0, 0, 100, 100));
        using var bmp = new Bitmap(200, 200);
        using var g = Graphics.FromImage(bmp);
        Assert.False(region.IsVisible(150, 150, g));
    }

    [Fact]
    public void IsVisible_PointF_InRegion()
    {
        using var region = new Region(new RectangleF(10, 10, 50, 50));
        Assert.True(region.IsVisible(new PointF(30, 30)));
    }

    [Fact]
    public void IsVisible_RectangleF_Intersects()
    {
        using var region = new Region(new RectangleF(0, 0, 100, 100));
        Assert.True(region.IsVisible(new RectangleF(50, 50, 100, 100)));
    }

    // --- Union ---

    [Fact]
    public void Union_ExpandsRegion()
    {
        using var region = new Region(new RectangleF(0, 0, 50, 50));
        region.Union(new RectangleF(50, 50, 50, 50));
        // Point in second rect should be visible
        Assert.True(region.IsVisible(new PointF(75, 75)));
    }

    // --- Intersect ---

    [Fact]
    public void Intersect_ReducesRegion()
    {
        using var region = new Region(new RectangleF(0, 0, 100, 100));
        region.Intersect(new RectangleF(50, 50, 100, 100));
        // Point in intersection should be visible
        Assert.True(region.IsVisible(new PointF(75, 75)));
        // Point outside intersection should not be visible
        Assert.False(region.IsVisible(new PointF(25, 25)));
    }

    // --- Exclude ---

    [Fact]
    public void Exclude_RemovesOverlap()
    {
        using var region = new Region(new RectangleF(0, 0, 100, 100));
        region.Exclude(new RectangleF(50, 50, 100, 100));
        Assert.True(region.IsVisible(new PointF(25, 25)));
    }

    // --- Complement ---

    [Fact]
    public void Complement_DoesNotThrow()
    {
        using var region = new Region(new RectangleF(0, 0, 50, 50));
        region.Complement(new RectangleF(25, 25, 50, 50));
    }

    // --- Xor ---

    [Fact]
    public void Xor_DoesNotThrow()
    {
        using var region = new Region(new RectangleF(0, 0, 50, 50));
        region.Xor(new RectangleF(25, 25, 50, 50));
    }

    // --- MakeEmpty / MakeInfinite ---

    [Fact]
    public void MakeEmpty_ThenIsEmpty()
    {
        using var region = new Region(new RectangleF(0, 0, 100, 100));
        region.MakeEmpty();
        using var bmp = new Bitmap(200, 200);
        using var g = Graphics.FromImage(bmp);
        Assert.True(region.IsEmpty(g));
    }

    [Fact]
    public void MakeInfinite_ThenIsInfinite()
    {
        using var region = new Region(new RectangleF(0, 0, 100, 100));
        region.MakeInfinite();
        using var bmp = new Bitmap(200, 200);
        using var g = Graphics.FromImage(bmp);
        Assert.True(region.IsInfinite(g));
    }

    // --- GetBounds ---

    [Fact]
    public void GetBounds_ReturnsApproximateBounds()
    {
        using var region = new Region(new RectangleF(10, 20, 30, 40));
        using var bmp = new Bitmap(100, 100);
        using var g = Graphics.FromImage(bmp);
        var bounds = region.GetBounds(g);
        Assert.True(bounds.Width > 0);
        Assert.True(bounds.Height > 0);
    }

    // --- Clone ---

    [Fact]
    public void Clone_CreatesIndependentCopy()
    {
        using var region = new Region(new RectangleF(0, 0, 50, 50));
        using var clone = region.Clone();
        Assert.True(clone.IsVisible(new PointF(25, 25)));
    }

    // --- Translate ---

    [Fact]
    public void Translate_MovesRegion()
    {
        using var region = new Region(new RectangleF(0, 0, 10, 10));
        region.Translate(100, 100);
        Assert.True(region.IsVisible(new PointF(105, 105)));
        Assert.False(region.IsVisible(new PointF(5, 5)));
    }

    // --- Transform ---

    [Fact]
    public void Transform_ScalesRegion()
    {
        using var region = new Region(new RectangleF(0, 0, 10, 10));
        using var m = new Matrix();
        m.Scale(2, 2);
        region.Transform(m);
        Assert.True(region.IsVisible(new PointF(15, 15)));
    }

    // --- Dispose ---

    [Fact]
    public void Dispose_DoesNotThrow()
    {
        var region = new Region();
        region.Dispose();
    }
}
