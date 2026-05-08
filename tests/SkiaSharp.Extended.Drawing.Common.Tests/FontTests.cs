using System.Drawing;
using System.Drawing.Text;

namespace SkiaSharp.Extended.Drawing.Common.Tests;

public class FontTests
{
    // --- Constructors ---

    [Fact]
    public void Constructor_FamilyName_DefaultStyle()
    {
        using var font = new Font("Arial", 12);
        Assert.Equal(12f, font.Size);
        Assert.Equal(FontStyle.Regular, font.Style);
        Assert.Equal(GraphicsUnit.Point, font.Unit);
    }

    [Fact]
    public void Constructor_FamilyName_Bold()
    {
        using var font = new Font("Arial", 14, FontStyle.Bold);
        Assert.True(font.Bold);
        Assert.False(font.Italic);
    }

    [Fact]
    public void Constructor_FamilyName_BoldItalic()
    {
        using var font = new Font("Arial", 10, FontStyle.Bold | FontStyle.Italic);
        Assert.True(font.Bold);
        Assert.True(font.Italic);
    }

    [Fact]
    public void Constructor_FamilyName_WithUnit()
    {
        using var font = new Font("Arial", 16, FontStyle.Regular, GraphicsUnit.Pixel);
        Assert.Equal(GraphicsUnit.Pixel, font.Unit);
        Assert.Equal(16f, font.Size);
    }

    [Fact]
    public void Constructor_FontFamily_Object()
    {
        using var family = new FontFamily("Arial");
        using var font = new Font(family, 12);
        Assert.NotNull(font.FontFamily);
        Assert.Equal(12f, font.Size);
    }

    [Fact]
    public void Constructor_Prototype()
    {
        using var original = new Font("Arial", 12, FontStyle.Regular);
        using var bold = new Font(original, FontStyle.Bold);
        Assert.True(bold.Bold);
        Assert.Equal(original.Size, bold.Size);
    }

    [Fact]
    public void Constructor_NullFamily_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new Font((FontFamily)null!, 12));
    }

    [Fact]
    public void Constructor_ZeroSize_Throws()
    {
        Assert.Throws<ArgumentException>(() => new Font("Arial", 0));
    }

    // --- Properties ---

    [Fact]
    public void Name_ReturnsNonEmpty()
    {
        using var font = new Font("Arial", 12);
        Assert.False(string.IsNullOrEmpty(font.Name));
    }

    [Fact]
    public void OriginalFontName_MatchesConstructor()
    {
        using var font = new Font("Arial", 12);
        Assert.Equal("Arial", font.OriginalFontName);
    }

    [Fact]
    public void SizeInPoints_MatchesForPointUnit()
    {
        using var font = new Font("Arial", 12, GraphicsUnit.Point);
        Assert.Equal(12f, font.SizeInPoints, 0.1f);
    }

    [Fact]
    public void Height_ReturnsPositive()
    {
        using var font = new Font("Arial", 12);
        Assert.True(font.Height > 0);
    }

    [Fact]
    public void Strikeout_Property()
    {
        using var font = new Font("Arial", 12, FontStyle.Strikeout);
        Assert.True(font.Strikeout);
    }

    [Fact]
    public void Underline_Property()
    {
        using var font = new Font("Arial", 12, FontStyle.Underline);
        Assert.True(font.Underline);
    }

    [Fact]
    public void GdiCharSet_Default()
    {
        using var font = new Font("Arial", 12);
        Assert.Equal(1, font.GdiCharSet);
    }

    [Fact]
    public void IsSystemFont_ReturnsFalse()
    {
        using var font = new Font("Arial", 12);
        Assert.False(font.IsSystemFont);
    }

    // --- GetHeight ---

    [Fact]
    public void GetHeight_ReturnsPositive()
    {
        using var font = new Font("Arial", 12);
        Assert.True(font.GetHeight() > 0);
    }

    [Fact]
    public void GetHeight_WithDpi_ScalesAppropriately()
    {
        using var font = new Font("Arial", 12);
        float h96 = font.GetHeight(96f);
        float h192 = font.GetHeight(192f);
        // Double DPI should roughly double the height
        Assert.True(h192 > h96);
    }

    [Fact]
    public void GetHeight_WithGraphics()
    {
        using var bmp = new Bitmap(10, 10);
        using var g = Graphics.FromImage(bmp);
        using var font = new Font("Arial", 12);
        float h = font.GetHeight(g);
        Assert.True(h > 0);
    }

    // --- Clone ---

    [Fact]
    public void Clone_ReturnsEqualFont()
    {
        using var font = new Font("Arial", 14, FontStyle.Bold);
        using var clone = (Font)font.Clone();
        Assert.Equal(font.Name, clone.Name);
        Assert.Equal(font.Size, clone.Size);
        Assert.Equal(font.Style, clone.Style);
        Assert.Equal(font.Unit, clone.Unit);
    }

    // --- Equals and GetHashCode ---

    [Fact]
    public void Equals_SameFonts_ReturnsTrue()
    {
        using var f1 = new Font("Arial", 12, FontStyle.Bold);
        using var f2 = new Font("Arial", 12, FontStyle.Bold);
        Assert.Equal(f1, f2);
    }

    [Fact]
    public void Equals_DifferentSize_ReturnsFalse()
    {
        using var f1 = new Font("Arial", 12);
        using var f2 = new Font("Arial", 14);
        Assert.NotEqual(f1, f2);
    }

    [Fact]
    public void GetHashCode_SameFonts_SameHash()
    {
        using var f1 = new Font("Arial", 12, FontStyle.Bold);
        using var f2 = new Font("Arial", 12, FontStyle.Bold);
        Assert.Equal(f1.GetHashCode(), f2.GetHashCode());
    }

    // --- ToString ---

    [Fact]
    public void ToString_ContainsName()
    {
        using var font = new Font("Arial", 12);
        Assert.Contains("Arial", font.ToString());
    }

    // --- Dispose ---

    [Fact]
    public void Dispose_DoesNotThrow()
    {
        var font = new Font("Arial", 12);
        font.Dispose();
        // Double dispose shouldn't throw
        font.Dispose();
    }

    // --- DrawString renders without crash ---

    [Fact]
    public void DrawString_WithFont_DoesNotThrow()
    {
        using var bmp = new Bitmap(100, 50);
        using var g = Graphics.FromImage(bmp);
        using var font = new Font("Arial", 12);
        g.DrawString("Hello", font, Brushes.Black, 0, 0);
    }
}
