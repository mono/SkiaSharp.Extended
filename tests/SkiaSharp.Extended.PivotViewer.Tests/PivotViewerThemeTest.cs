using SkiaSharp;
using SkiaSharp.Extended.PivotViewer;

namespace SkiaSharp.Extended.PivotViewer.Tests;

public class PivotViewerThemeTest
{
    [Fact]
    public void Default_HasReasonableColors()
    {
        var theme = PivotViewerTheme.Default;

        Assert.NotEqual(0, theme.AccentColor.Alpha);
        Assert.NotEqual(0, theme.ControlBackground.Alpha);
        Assert.NotEqual(0, theme.SecondaryBackground.Alpha);
        Assert.NotEqual(0, theme.SecondaryForeground.Alpha);
        Assert.NotEqual(0, theme.ForegroundColor.Alpha);
        Assert.NotEqual(0, theme.LightForegroundColor.Alpha);
        Assert.NotEqual(0, theme.ItemFallbackColor.Alpha);
        Assert.NotEqual(0, theme.SelectionColor.Alpha);
    }

    [Fact]
    public void Default_AccentColor_IsNotBlack()
    {
        var theme = PivotViewerTheme.Default;

        Assert.NotEqual(SKColors.Black, theme.AccentColor);
    }

    [Fact]
    public void CustomTheme_PreservesValues()
    {
        var theme = new PivotViewerTheme
        {
            AccentColor = SKColors.Red,
            ControlBackground = SKColors.Green,
            ForegroundColor = SKColors.Blue,
            SelectionColor = SKColors.Yellow,
        };

        Assert.Equal(SKColors.Red, theme.AccentColor);
        Assert.Equal(SKColors.Green, theme.ControlBackground);
        Assert.Equal(SKColors.Blue, theme.ForegroundColor);
        Assert.Equal(SKColors.Yellow, theme.SelectionColor);
    }

    [Fact]
    public void ForegroundColor_ContrastsWithBackground()
    {
        var theme = PivotViewerTheme.Default;

        Assert.NotEqual(theme.ForegroundColor, theme.SecondaryBackground);
    }

    [Fact]
    public void Default_AllColorsHaveValues()
    {
        var theme = PivotViewerTheme.Default;

        Assert.NotEqual(SKColors.Transparent, theme.AccentColor);
        Assert.NotEqual(SKColors.Transparent, theme.ControlBackground);
        Assert.NotEqual(SKColors.Transparent, theme.SecondaryBackground);
        Assert.NotEqual(SKColors.Transparent, theme.SecondaryForeground);
        Assert.NotEqual(SKColors.Transparent, theme.ForegroundColor);
        Assert.NotEqual(SKColors.Transparent, theme.LightForegroundColor);
        Assert.NotEqual(SKColors.Transparent, theme.ItemFallbackColor);
        Assert.NotEqual(SKColors.Transparent, theme.SelectionColor);
        Assert.NotEqual(SKColors.Transparent, theme.HoverColor);
    }

    [Fact]
    public void Default_ForegroundColor_IsBlack()
    {
        var theme = PivotViewerTheme.Default;
        Assert.Equal(SKColors.Black, theme.ForegroundColor);
    }

    [Fact]
    public void Default_LightForegroundColor_IsWhite()
    {
        var theme = PivotViewerTheme.Default;
        Assert.Equal(SKColors.White, theme.LightForegroundColor);
    }

    [Fact]
    public void Default_ItemFallbackColor_IsCornflowerBlue()
    {
        var theme = PivotViewerTheme.Default;
        Assert.Equal(new SKColor(100, 149, 237), theme.ItemFallbackColor);
    }

    [Fact]
    public void Default_SelectionColor_IsOrange()
    {
        var theme = PivotViewerTheme.Default;
        Assert.Equal(SKColors.Orange, theme.SelectionColor);
    }

    [Fact]
    public void Default_HoverColor_HasAlpha()
    {
        var theme = PivotViewerTheme.Default;
        Assert.True(theme.HoverColor.Alpha < 255);
        Assert.True(theme.HoverColor.Alpha > 0);
    }

    [Fact]
    public void Custom_CanOverrideAllColors()
    {
        var theme = new PivotViewerTheme
        {
            AccentColor = SKColors.Red,
            ControlBackground = SKColors.Green,
            SecondaryBackground = SKColors.Blue,
            SecondaryForeground = SKColors.Yellow,
            ForegroundColor = SKColors.Cyan,
            LightForegroundColor = SKColors.Magenta,
            ItemFallbackColor = SKColors.Gray,
            SelectionColor = SKColors.White,
            HoverColor = SKColors.Black,
        };

        Assert.Equal(SKColors.Red, theme.AccentColor);
        Assert.Equal(SKColors.Green, theme.ControlBackground);
        Assert.Equal(SKColors.Blue, theme.SecondaryBackground);
        Assert.Equal(SKColors.Yellow, theme.SecondaryForeground);
        Assert.Equal(SKColors.Cyan, theme.ForegroundColor);
        Assert.Equal(SKColors.Magenta, theme.LightForegroundColor);
        Assert.Equal(SKColors.Gray, theme.ItemFallbackColor);
        Assert.Equal(SKColors.White, theme.SelectionColor);
        Assert.Equal(SKColors.Black, theme.HoverColor);
    }

    [Fact]
    public void Default_StaticProperty_ReturnsNewInstance()
    {
        var a = PivotViewerTheme.Default;
        var b = PivotViewerTheme.Default;
        Assert.NotSame(a, b);
    }
}
