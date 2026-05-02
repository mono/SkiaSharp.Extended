using System.Drawing;
using System.Drawing.Drawing2D;

namespace SkiaSharp.Drawing.Tests;

public class MatrixTests
{
    // --- Constructors ---

    [Fact]
    public void Constructor_Default_IsIdentity()
    {
        using var m = new Matrix();
        Assert.True(m.IsIdentity);
    }

    [Fact]
    public void Constructor_WithElements()
    {
        using var m = new Matrix(1, 0, 0, 1, 10, 20);
        var e = m.Elements;
        Assert.Equal(1f, e[0], 0.001f);
        Assert.Equal(0f, e[1], 0.001f);
        Assert.Equal(0f, e[2], 0.001f);
        Assert.Equal(1f, e[3], 0.001f);
        Assert.Equal(10f, e[4], 0.001f);
        Assert.Equal(20f, e[5], 0.001f);
    }

    // --- Properties ---

    [Fact]
    public void OffsetX_ReturnsTranslationX()
    {
        using var m = new Matrix(1, 0, 0, 1, 42, 0);
        Assert.Equal(42f, m.OffsetX, 0.001f);
    }

    [Fact]
    public void OffsetY_ReturnsTranslationY()
    {
        using var m = new Matrix(1, 0, 0, 1, 0, 99);
        Assert.Equal(99f, m.OffsetY, 0.001f);
    }

    [Fact]
    public void IsInvertible_IdentityMatrix()
    {
        using var m = new Matrix();
        Assert.True(m.IsInvertible);
    }

    [Fact]
    public void IsInvertible_ZeroMatrix_ReturnsFalse()
    {
        using var m = new Matrix(0, 0, 0, 0, 0, 0);
        Assert.False(m.IsInvertible);
    }

    // --- Translate ---

    [Fact]
    public void Translate_ModifiesOffset()
    {
        using var m = new Matrix();
        m.Translate(10, 20);
        Assert.Equal(10f, m.OffsetX, 0.001f);
        Assert.Equal(20f, m.OffsetY, 0.001f);
    }

    // --- Scale ---

    [Fact]
    public void Scale_ModifiesElements()
    {
        using var m = new Matrix();
        m.Scale(2, 3);
        var e = m.Elements;
        Assert.Equal(2f, e[0], 0.001f);
        Assert.Equal(3f, e[3], 0.001f);
    }

    // --- Rotate ---

    [Fact]
    public void Rotate_90Degrees()
    {
        using var m = new Matrix();
        m.Rotate(90);
        Assert.False(m.IsIdentity);
        // After 90-degree rotation, (1,0) maps to (0,1)
        var pts = new PointF[] { new PointF(1, 0) };
        m.TransformPoints(pts);
        Assert.True(Math.Abs(pts[0].X) < 0.01f);
        Assert.True(Math.Abs(pts[0].Y - 1f) < 0.01f);
    }

    // --- RotateAt ---

    [Fact]
    public void RotateAt_PivotPoint()
    {
        using var m = new Matrix();
        m.RotateAt(180, new PointF(5, 5));
        var pts = new PointF[] { new PointF(10, 5) };
        m.TransformPoints(pts);
        Assert.True(Math.Abs(pts[0].X) < 0.1f);
        Assert.True(Math.Abs(pts[0].Y - 5f) < 0.1f);
    }

    // --- Multiply ---

    [Fact]
    public void Multiply_CombinesMatrices()
    {
        using var m1 = new Matrix();
        m1.Translate(10, 0);
        using var m2 = new Matrix();
        m2.Translate(0, 20);
        m1.Multiply(m2);
        Assert.True(Math.Abs(m1.OffsetX - 10) < 0.1f);
        Assert.True(Math.Abs(m1.OffsetY - 20) < 0.1f);
    }

    // --- Invert ---

    [Fact]
    public void Invert_ThenMultiply_GivesIdentity()
    {
        using var m = new Matrix();
        m.Translate(10, 20);
        m.Scale(2, 3);
        using var copy = m.Clone();
        m.Invert();
        m.Multiply(copy);
        Assert.True(m.IsIdentity || (
            Math.Abs(m.Elements[0] - 1) < 0.001f &&
            Math.Abs(m.Elements[3] - 1) < 0.001f &&
            Math.Abs(m.OffsetX) < 0.1f &&
            Math.Abs(m.OffsetY) < 0.1f));
    }

    // --- Reset ---

    [Fact]
    public void Reset_RestoresToIdentity()
    {
        using var m = new Matrix();
        m.Translate(10, 20);
        m.Reset();
        Assert.True(m.IsIdentity);
    }

    // --- TransformPoints ---

    [Fact]
    public void TransformPoints_AppliesTranslation()
    {
        using var m = new Matrix();
        m.Translate(100, 200);
        var pts = new PointF[] { new PointF(0, 0), new PointF(1, 1) };
        m.TransformPoints(pts);
        Assert.Equal(100f, pts[0].X, 0.001f);
        Assert.Equal(200f, pts[0].Y, 0.001f);
    }

    [Fact]
    public void TransformVectors_DoesNotApplyTranslation()
    {
        using var m = new Matrix();
        m.Translate(100, 200);
        var pts = new PointF[] { new PointF(1, 0) };
        m.TransformVectors(pts);
        Assert.Equal(1f, pts[0].X, 0.001f);
        Assert.Equal(0f, pts[0].Y, 0.001f);
    }

    // --- Clone ---

    [Fact]
    public void Clone_CreatesIndependentCopy()
    {
        using var m = new Matrix(1, 2, 3, 4, 5, 6);
        using var clone = m.Clone();
        var e1 = m.Elements;
        var e2 = clone.Elements;
        for (int i = 0; i < 6; i++)
            Assert.Equal(e1[i], e2[i], 0.001f);
    }

    // --- Shear ---

    [Fact]
    public void Shear_ModifiesMatrix()
    {
        using var m = new Matrix();
        m.Shear(1, 0);
        Assert.False(m.IsIdentity);
    }

    // --- Equals ---

    [Fact]
    public void Equals_SameMatrix_ReturnsTrue()
    {
        using var m1 = new Matrix(1, 0, 0, 1, 10, 20);
        using var m2 = new Matrix(1, 0, 0, 1, 10, 20);
        Assert.True(m1.Equals(m2));
    }

    [Fact]
    public void Equals_DifferentMatrix_ReturnsFalse()
    {
        using var m1 = new Matrix(1, 0, 0, 1, 10, 20);
        using var m2 = new Matrix(1, 0, 0, 1, 30, 40);
        Assert.False(m1.Equals(m2));
    }
}
