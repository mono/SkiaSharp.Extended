using System.Drawing;
using System.Drawing.Drawing2D;

namespace SkiaSharp.Drawing.Tests;

public class BrushTests
{
    // --- SolidBrush ---

    [Fact]
    public void SolidBrush_Constructor_StoresColor()
    {
        using var brush = new SolidBrush(Color.Red);
        Assert.Equal(Color.Red.ToArgb(), brush.Color.ToArgb());
    }

    [Fact]
    public void SolidBrush_Color_GetSet()
    {
        using var brush = new SolidBrush(Color.Red);
        brush.Color = Color.Green;
        Assert.Equal(Color.Green.ToArgb(), brush.Color.ToArgb());
    }

    [Fact]
    public void SolidBrush_Clone_CreatesIndependentCopy()
    {
        using var original = new SolidBrush(Color.Blue);
        var clone = (SolidBrush)original.Clone();
        Assert.Equal(Color.Blue.ToArgb(), clone.Color.ToArgb());
        clone.Color = Color.Red;
        Assert.Equal(Color.Blue.ToArgb(), original.Color.ToArgb());
        clone.Dispose();
    }

    [Fact]
    public void SolidBrush_Dispose_AccessColor_ThrowsObjectDisposedException()
    {
        var brush = new SolidBrush(Color.Red);
        brush.Dispose();
        Assert.Throws<ObjectDisposedException>(() => brush.Color);
    }

    [Fact]
    public void SolidBrush_Dispose_SetColor_ThrowsObjectDisposedException()
    {
        var brush = new SolidBrush(Color.Red);
        brush.Dispose();
        Assert.Throws<ObjectDisposedException>(() => brush.Color = Color.Blue);
    }

    [Fact]
    public void SolidBrush_DoubleDispose_DoesNotThrow()
    {
        var brush = new SolidBrush(Color.Red);
        brush.Dispose();
        brush.Dispose(); // should not throw
    }

    [Fact]
    public void SolidBrush_WithTransparentColor()
    {
        using var brush = new SolidBrush(Color.FromArgb(0, 0, 0, 0));
        Assert.Equal(0, brush.Color.A);
    }

    [Fact]
    public void SolidBrush_WithSemiTransparentColor()
    {
        using var brush = new SolidBrush(Color.FromArgb(128, 255, 0, 0));
        Assert.Equal(128, brush.Color.A);
        Assert.Equal(255, brush.Color.R);
    }

    // --- TextureBrush ---

    [Fact]
    public void TextureBrush_Constructor_Image_StoresImage()
    {
        using var bmp = new System.Drawing.Bitmap(8, 8);
        using var brush = new TextureBrush(bmp);
        Assert.Same(bmp, brush.Image);
    }

    [Fact]
    public void TextureBrush_Constructor_NullImage_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new TextureBrush(null!));
    }

    [Fact]
    public void TextureBrush_WrapMode_Default_IsTile()
    {
        using var bmp = new System.Drawing.Bitmap(4, 4);
        using var brush = new TextureBrush(bmp);
        Assert.Equal(WrapMode.Tile, brush.WrapMode);
    }

    [Fact]
    public void TextureBrush_WrapMode_GetSet()
    {
        using var bmp = new System.Drawing.Bitmap(4, 4);
        using var brush = new TextureBrush(bmp);
        brush.WrapMode = WrapMode.Clamp;
        Assert.Equal(WrapMode.Clamp, brush.WrapMode);
    }

    [Theory]
    [InlineData(WrapMode.Tile)]
    [InlineData(WrapMode.TileFlipX)]
    [InlineData(WrapMode.TileFlipY)]
    [InlineData(WrapMode.TileFlipXY)]
    [InlineData(WrapMode.Clamp)]
    public void TextureBrush_Constructor_WrapMode_AllValues(WrapMode mode)
    {
        using var bmp = new System.Drawing.Bitmap(4, 4);
        using var brush = new TextureBrush(bmp, mode);
        Assert.Equal(mode, brush.WrapMode);
    }

    [Fact]
    public void TextureBrush_Clone_CreatesIndependentCopy()
    {
        using var bmp = new System.Drawing.Bitmap(4, 4);
        using var original = new TextureBrush(bmp, WrapMode.Clamp);
        var clone = (TextureBrush)original.Clone();
        Assert.Equal(WrapMode.Clamp, clone.WrapMode);
        clone.WrapMode = WrapMode.TileFlipXY;
        Assert.Equal(WrapMode.Clamp, original.WrapMode);
        clone.Dispose();
    }

    [Fact]
    public void TextureBrush_Dispose_AccessImage_ThrowsObjectDisposedException()
    {
        using var bmp = new System.Drawing.Bitmap(4, 4);
        var brush = new TextureBrush(bmp);
        brush.Dispose();
        Assert.Throws<ObjectDisposedException>(() => brush.Image);
    }

    [Fact]
    public void TextureBrush_WithRectangle_Constructor()
    {
        using var bmp = new System.Drawing.Bitmap(8, 8);
        using var brush = new TextureBrush(bmp, new Rectangle(0, 0, 4, 4));
        Assert.Same(bmp, brush.Image);
    }

    [Fact]
    public void TextureBrush_WithRectangleF_Constructor()
    {
        using var bmp = new System.Drawing.Bitmap(8, 8);
        using var brush = new TextureBrush(bmp, new RectangleF(0, 0, 4f, 4f));
        Assert.Same(bmp, brush.Image);
    }

    [Fact]
    public void TextureBrush_ResetTransform_DoesNotThrow()
    {
        using var bmp = new System.Drawing.Bitmap(4, 4);
        using var brush = new TextureBrush(bmp);
        brush.ResetTransform();
    }
}
