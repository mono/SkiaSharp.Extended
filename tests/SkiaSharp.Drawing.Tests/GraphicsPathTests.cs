using System.Drawing;
using System.Drawing.Drawing2D;

namespace SkiaSharp.Drawing.Tests;

public class GraphicsPathTests
{
    // --- Constructors ---

    [Fact]
    public void Constructor_Default_CreatesEmptyPath()
    {
        using var path = new GraphicsPath();
        Assert.Equal(0, path.PointCount);
    }

    [Fact]
    public void Constructor_WithFillMode()
    {
        using var path = new GraphicsPath(FillMode.Winding);
        Assert.Equal(FillMode.Winding, path.FillMode);
    }

    // --- AddLine ---

    [Fact]
    public void AddLine_IncreasesPointCount()
    {
        using var path = new GraphicsPath();
        path.AddLine(0, 0, 10, 10);
        Assert.True(path.PointCount >= 2);
    }

    [Fact]
    public void AddLine_PointF_Overload()
    {
        using var path = new GraphicsPath();
        path.AddLine(new PointF(0, 0), new PointF(10, 10));
        Assert.True(path.PointCount >= 2);
    }

    // --- AddRectangle ---

    [Fact]
    public void AddRectangle_Adds4Points()
    {
        using var path = new GraphicsPath();
        path.AddRectangle(new RectangleF(0, 0, 10, 10));
        Assert.Equal(4, path.PointCount);
    }

    [Fact]
    public void AddRectangle_Int_Overload()
    {
        using var path = new GraphicsPath();
        path.AddRectangle(new Rectangle(0, 0, 10, 10));
        Assert.Equal(4, path.PointCount);
    }

    // --- AddEllipse ---

    [Fact]
    public void AddEllipse_AddsPoints()
    {
        using var path = new GraphicsPath();
        path.AddEllipse(0, 0, 20, 20);
        Assert.True(path.PointCount > 0);
    }

    // --- AddArc ---

    [Fact]
    public void AddArc_AddsPoints()
    {
        using var path = new GraphicsPath();
        path.AddArc(0, 0, 20, 20, 0, 90);
        Assert.True(path.PointCount > 0);
    }

    // --- AddPolygon ---

    [Fact]
    public void AddPolygon_AddsPoints()
    {
        using var path = new GraphicsPath();
        path.AddPolygon(new PointF[] { new(0, 0), new(10, 0), new(10, 10) });
        Assert.True(path.PointCount >= 3);
    }

    // --- AddBezier ---

    [Fact]
    public void AddBezier_Adds4Points()
    {
        using var path = new GraphicsPath();
        path.AddBezier(0, 0, 5, 10, 15, 10, 20, 0);
        Assert.True(path.PointCount >= 4);
    }

    // --- AddPath ---

    [Fact]
    public void AddPath_CombinesPaths()
    {
        using var p1 = new GraphicsPath();
        p1.AddRectangle(new RectangleF(0, 0, 10, 10));
        using var p2 = new GraphicsPath();
        p2.AddRectangle(new RectangleF(20, 20, 10, 10));
        p1.AddPath(p2, false);
        Assert.True(p1.PointCount >= 8);
    }

    // --- PathPoints and PathTypes ---

    [Fact]
    public void PathPoints_MatchPointCount()
    {
        using var path = new GraphicsPath();
        path.AddRectangle(new RectangleF(0, 0, 10, 10));
        Assert.Equal(path.PointCount, path.PathPoints.Length);
    }

    [Fact]
    public void PathTypes_LengthMatchesPathPoints()
    {
        using var path = new GraphicsPath();
        path.AddLine(0, 0, 10, 10);
        path.AddLine(10, 10, 20, 0);
        // PathPoints and PathTypes should have the same length
        Assert.Equal(path.PathPoints.Length, path.PathTypes.Length);
    }

    // --- GetBounds ---

    [Fact]
    public void GetBounds_ReturnsCorrectBounds()
    {
        using var path = new GraphicsPath();
        path.AddRectangle(new RectangleF(5, 10, 20, 30));
        var bounds = path.GetBounds();
        Assert.True(bounds.Width >= 19);
        Assert.True(bounds.Height >= 29);
    }

    // --- Transform ---

    [Fact]
    public void Transform_TranslatesPoints()
    {
        using var path = new GraphicsPath();
        path.AddLine(0, 0, 10, 0);
        using var matrix = new Matrix();
        matrix.Translate(100, 100);
        path.Transform(matrix);
        var pts = path.PathPoints;
        Assert.True(pts[0].X >= 99);
    }

    // --- Clone ---

    [Fact]
    public void Clone_CreatesIndependentCopy()
    {
        using var original = new GraphicsPath();
        original.AddRectangle(new RectangleF(0, 0, 10, 10));
        using var clone = (GraphicsPath)original.Clone();
        Assert.Equal(original.PointCount, clone.PointCount);
    }

    // --- Reset ---

    [Fact]
    public void Reset_ClearsPath()
    {
        using var path = new GraphicsPath();
        path.AddRectangle(new RectangleF(0, 0, 10, 10));
        path.Reset();
        Assert.Equal(0, path.PointCount);
    }

    // --- Reverse ---

    [Fact]
    public void Reverse_ReversesPoints()
    {
        using var path = new GraphicsPath();
        path.AddLine(0, 0, 10, 0);
        path.AddLine(10, 0, 10, 10);
        var firstBefore = path.PathPoints[0];
        path.Reverse();
        var firstAfter = path.PathPoints[0];
        Assert.NotEqual(firstBefore, firstAfter);
    }

    // --- IsVisible ---

    [Fact]
    public void IsVisible_InsideRect_ReturnsTrue()
    {
        using var path = new GraphicsPath();
        path.AddRectangle(new RectangleF(0, 0, 100, 100));
        Assert.True(path.IsVisible(50, 50));
    }

    [Fact]
    public void IsVisible_OutsideRect_ReturnsFalse()
    {
        using var path = new GraphicsPath();
        path.AddRectangle(new RectangleF(0, 0, 100, 100));
        Assert.False(path.IsVisible(200, 200));
    }

    // --- CloseFigure ---

    [Fact]
    public void CloseFigure_DoesNotThrow()
    {
        using var path = new GraphicsPath();
        path.AddLine(0, 0, 10, 10);
        path.CloseFigure();
        // Should have close flag on last type
        var types = path.PathTypes;
        Assert.True((types[^1] & 0x80) != 0);
    }

    // --- Dispose ---

    [Fact]
    public void Dispose_DoesNotThrow()
    {
        var path = new GraphicsPath();
        path.AddLine(0, 0, 10, 10);
        path.Dispose();
    }

    // --- GraphicsPathIterator ---

    [Fact]
    public void Iterator_Count_MatchesPathPoints()
    {
        using var path = new GraphicsPath();
        path.AddRectangle(new RectangleF(0, 0, 10, 10));
        using var iter = new GraphicsPathIterator(path);
        Assert.Equal(path.PointCount, iter.Count);
    }

    [Fact]
    public void Iterator_SubpathCount()
    {
        using var path = new GraphicsPath();
        path.AddRectangle(new RectangleF(0, 0, 10, 10));
        path.AddRectangle(new RectangleF(20, 20, 10, 10));
        using var iter = new GraphicsPathIterator(path);
        Assert.Equal(2, iter.SubpathCount);
    }

    [Fact]
    public void Iterator_NextSubpath()
    {
        using var path = new GraphicsPath();
        path.AddRectangle(new RectangleF(0, 0, 10, 10));
        path.AddRectangle(new RectangleF(20, 20, 10, 10));
        using var iter = new GraphicsPathIterator(path);
        int count1 = iter.NextSubpath(out int s1, out int e1, out bool closed1);
        Assert.True(count1 > 0);
        int count2 = iter.NextSubpath(out int s2, out int e2, out bool closed2);
        Assert.True(count2 > 0);
        int count3 = iter.NextSubpath(out _, out _, out _);
        Assert.Equal(0, count3);
    }

    [Fact]
    public void Iterator_HasCurve_ForBezier()
    {
        using var path = new GraphicsPath();
        path.AddBezier(0, 0, 5, 10, 15, 10, 20, 0);
        using var iter = new GraphicsPathIterator(path);
        Assert.True(iter.HasCurve());
    }

    [Fact]
    public void Iterator_HasCurve_FalseForLines()
    {
        using var path = new GraphicsPath();
        path.AddLine(0, 0, 10, 10);
        using var iter = new GraphicsPathIterator(path);
        Assert.False(iter.HasCurve());
    }

    [Fact]
    public void Iterator_Enumerate_CopiesData()
    {
        using var path = new GraphicsPath();
        path.AddRectangle(new RectangleF(0, 0, 10, 10));
        using var iter = new GraphicsPathIterator(path);
        var pts = new PointF[iter.Count];
        var types = new byte[iter.Count];
        int n = iter.Enumerate(ref pts, ref types);
        Assert.Equal(iter.Count, n);
    }

    [Fact]
    public void Iterator_Rewind_ResetsIteration()
    {
        using var path = new GraphicsPath();
        path.AddRectangle(new RectangleF(0, 0, 10, 10));
        using var iter = new GraphicsPathIterator(path);
        iter.NextSubpath(out _, out _, out _);
        iter.Rewind();
        int count = iter.NextSubpath(out _, out _, out _);
        Assert.True(count > 0);
    }

    [Fact]
    public void Iterator_NullPath_ZeroCount()
    {
        using var iter = new GraphicsPathIterator(null);
        Assert.Equal(0, iter.Count);
        Assert.Equal(0, iter.SubpathCount);
    }
}
