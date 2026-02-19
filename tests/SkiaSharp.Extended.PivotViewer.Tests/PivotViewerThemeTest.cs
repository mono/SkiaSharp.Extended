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
}
