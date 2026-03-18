using SkiaSharp.Extended;
using Xunit;

namespace SkiaSharp.Extended.ImagePyramid.Tests;

/// <summary>Tests for the generic <see cref="Rect{T}"/> and <see cref="Point{T}"/> value types.</summary>
public class GeometricTypesTest
{
    // ---- Rect<T> construction ----

    [Fact]
    public void Rect_Int_StoresComponents()
    {
        var r = new Rect<int>(10, 20, 100, 50);
        Assert.Equal(10, r.X);
        Assert.Equal(20, r.Y);
        Assert.Equal(100, r.Width);
        Assert.Equal(50, r.Height);
    }

    [Fact]
    public void Rect_Float_StoresComponents()
    {
        var r = new Rect<float>(1.5f, 2.5f, 10.0f, 5.0f);
        Assert.Equal(1.5f, r.X);
        Assert.Equal(2.5f, r.Y);
        Assert.Equal(10.0f, r.Width);
        Assert.Equal(5.0f, r.Height);
    }

    [Fact]
    public void Rect_Double_StoresComponents()
    {
        var r = new Rect<double>(0.1, 0.2, 0.5, 0.3);
        Assert.Equal(0.1, r.X);
        Assert.Equal(0.2, r.Y);
        Assert.Equal(0.5, r.Width);
        Assert.Equal(0.3, r.Height);
    }

    // ---- Rect<T> equality ----

    [Fact]
    public void Rect_Equality_SameValues_Equal()
    {
        var a = new Rect<int>(1, 2, 3, 4);
        var b = new Rect<int>(1, 2, 3, 4);
        Assert.Equal(a, b);
        Assert.True(a == b);
    }

    [Fact]
    public void Rect_Equality_DifferentValues_NotEqual()
    {
        var a = new Rect<int>(1, 2, 3, 4);
        var b = new Rect<int>(1, 2, 3, 5);
        Assert.NotEqual(a, b);
        Assert.True(a != b);
    }

    // ---- Rect<T> deconstruct ----

    [Fact]
    public void Rect_Deconstruct_Int()
    {
        var r = new Rect<int>(5, 10, 20, 30);
        var (x, y, w, h) = r;
        Assert.Equal(5, x);
        Assert.Equal(10, y);
        Assert.Equal(20, w);
        Assert.Equal(30, h);
    }

    [Fact]
    public void Rect_Deconstruct_Float()
    {
        var r = new Rect<float>(1f, 2f, 3f, 4f);
        var (x, y, w, h) = r;
        Assert.Equal(1f, x);
        Assert.Equal(2f, y);
        Assert.Equal(3f, w);
        Assert.Equal(4f, h);
    }

    // ---- Rect<T> right/bottom via inline arithmetic ----

    [Fact]
    public void Rect_Right_IsXPlusWidth()
    {
        var r = new Rect<int>(3, 0, 10, 0);
        Assert.Equal(13, r.X + r.Width);
    }

    [Fact]
    public void Rect_Bottom_IsYPlusHeight()
    {
        var r = new Rect<float>(0f, 5f, 0f, 12f);
        Assert.Equal(17f, r.Y + r.Height);
    }

    // ---- Point<T> construction ----

    [Fact]
    public void Point_Double_StoresComponents()
    {
        var p = new Point<double>(0.25, 0.75);
        Assert.Equal(0.25, p.X);
        Assert.Equal(0.75, p.Y);
    }

    [Fact]
    public void Point_Int_StoresComponents()
    {
        var p = new Point<int>(42, -7);
        Assert.Equal(42, p.X);
        Assert.Equal(-7, p.Y);
    }

    // ---- Point<T> equality ----

    [Fact]
    public void Point_Equality_SameValues_Equal()
    {
        var a = new Point<double>(1.0, 2.0);
        var b = new Point<double>(1.0, 2.0);
        Assert.Equal(a, b);
    }

    [Fact]
    public void Point_Equality_DifferentValues_NotEqual()
    {
        var a = new Point<float>(1f, 2f);
        var b = new Point<float>(1f, 3f);
        Assert.NotEqual(a, b);
    }

    // ---- Point<T> deconstruct ----

    [Fact]
    public void Point_Deconstruct_Double()
    {
        var p = new Point<double>(3.14, 2.71);
        var (x, y) = p;
        Assert.Equal(3.14, x);
        Assert.Equal(2.71, y);
    }

    // ---- Zero / default ----

    [Fact]
    public void Rect_Default_IsAllZero()
    {
        var r = default(Rect<int>);
        Assert.Equal(0, r.X);
        Assert.Equal(0, r.Y);
        Assert.Equal(0, r.Width);
        Assert.Equal(0, r.Height);
    }

    [Fact]
    public void Point_Default_IsAllZero()
    {
        var p = default(Point<double>);
        Assert.Equal(0.0, p.X);
        Assert.Equal(0.0, p.Y);
    }
}
