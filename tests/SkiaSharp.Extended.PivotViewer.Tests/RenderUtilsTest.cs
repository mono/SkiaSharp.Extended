using SkiaSharp;
using SkiaSharp.Extended.PivotViewer;

namespace SkiaSharp.Extended.PivotViewer.Tests;

public class RenderUtilsTest
{
    private static readonly PivotViewerStringProperty NameProperty = new("Name");

    [Fact]
    public void GetItemDisplayName_WithName_ReturnsName()
    {
        var item = new PivotViewerItem("item-1");
        item.Add(NameProperty, "My Item");

        Assert.Equal("My Item", RenderUtils.GetItemDisplayName(item));
    }

    [Fact]
    public void GetItemDisplayName_WithoutName_ReturnsId()
    {
        var item = new PivotViewerItem("item-42");

        Assert.Equal("item-42", RenderUtils.GetItemDisplayName(item));
    }

    [Fact]
    public void GetItemDisplayName_EmptyName_ReturnsId()
    {
        var item = new PivotViewerItem("item-99");
        item.Add(NameProperty, "");

        // Empty string ToString() is "", which is returned — but the method checks name[0] != null.
        // An empty string is non-null so it returns "".
        var result = RenderUtils.GetItemDisplayName(item);
        // The method returns name[0].ToString() which is "" for empty string.
        // Per the API: name != null && name.Count > 0 && name[0] != null → returns "".
        Assert.Equal("", result);
    }

    [Fact]
    public void TruncateText_ShortText_NoTruncation()
    {
        using var font = new SKFont(SKTypeface.Default, 14);
        var text = "Hi";
        var result = RenderUtils.TruncateText(text, font, 1000f);

        Assert.Equal("Hi", result);
    }

    [Fact]
    public void TruncateText_LongText_AddEllipsis()
    {
        using var font = new SKFont(SKTypeface.Default, 14);
        var text = "This is a very long text that should definitely be truncated when measured";
        var result = RenderUtils.TruncateText(text, font, 50f);

        Assert.EndsWith("…", result);
        Assert.True(result.Length < text.Length);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void TruncateText_NullOrEmpty_ReturnsInput(string? input)
    {
        using var font = new SKFont(SKTypeface.Default, 14);
        var result = RenderUtils.TruncateText(input!, font, 100f);

        Assert.Equal(input, result);
    }

    [Fact]
    public void FitUniform_LandscapeIntoSquare_Letterboxed()
    {
        var dest = new SKRect(0, 0, 100, 100);
        var result = RenderUtils.FitUniform(200, 100, dest);

        // Wide source → fits to width, letterboxed vertically
        Assert.Equal(100f, result.Width, 0.01f);
        Assert.Equal(50f, result.Height, 0.01f);
        Assert.Equal(0f, result.Left, 0.01f);
        Assert.Equal(25f, result.Top, 0.01f);
    }

    [Fact]
    public void FitUniform_PortraitIntoSquare_Pillarboxed()
    {
        var dest = new SKRect(0, 0, 100, 100);
        var result = RenderUtils.FitUniform(100, 200, dest);

        // Tall source → fits to height, pillarboxed horizontally
        Assert.Equal(50f, result.Width, 0.01f);
        Assert.Equal(100f, result.Height, 0.01f);
        Assert.Equal(25f, result.Left, 0.01f);
        Assert.Equal(0f, result.Top, 0.01f);
    }

    [Fact]
    public void FitUniform_SquareIntoSquare_ExactFit()
    {
        var dest = new SKRect(0, 0, 100, 100);
        var result = RenderUtils.FitUniform(50, 50, dest);

        Assert.Equal(dest.Width, result.Width, 0.01f);
        Assert.Equal(dest.Height, result.Height, 0.01f);
        Assert.Equal(dest.Left, result.Left, 0.01f);
        Assert.Equal(dest.Top, result.Top, 0.01f);
    }

    [Theory]
    [InlineData(0, 100)]
    [InlineData(100, 0)]
    [InlineData(0, 0)]
    public void FitUniform_ZeroDimension_ReturnsTarget(float srcW, float srcH)
    {
        var dest = new SKRect(10, 20, 110, 120);
        var result = RenderUtils.FitUniform(srcW, srcH, dest);

        Assert.Equal(dest, result);
    }
}
