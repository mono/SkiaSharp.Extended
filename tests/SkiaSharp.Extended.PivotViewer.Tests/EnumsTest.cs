using SkiaSharp.Extended.PivotViewer;

namespace SkiaSharp.Extended.PivotViewer.Tests;

public class EnumsTest
{
    [Fact]
    public void PivotViewerPropertyOptions_Values_MatchSilverlight()
    {
        Assert.Equal(0, (int)PivotViewerPropertyOptions.None);
        Assert.Equal(1, (int)PivotViewerPropertyOptions.Private);
        Assert.Equal(2, (int)PivotViewerPropertyOptions.CanFilter);
        Assert.Equal(4, (int)PivotViewerPropertyOptions.CanSearchText);
        Assert.Equal(8, (int)PivotViewerPropertyOptions.WrappingText);
    }

    [Fact]
    public void PivotViewerPropertyOptions_IsFlags()
    {
        var combined = PivotViewerPropertyOptions.CanFilter | PivotViewerPropertyOptions.CanSearchText;
        Assert.Equal(6, (int)combined);
        Assert.True(combined.HasFlag(PivotViewerPropertyOptions.CanFilter));
        Assert.True(combined.HasFlag(PivotViewerPropertyOptions.CanSearchText));
        Assert.False(combined.HasFlag(PivotViewerPropertyOptions.Private));
    }

    [Fact]
    public void PivotViewerPropertyType_Values_MatchSilverlight()
    {
        // Silverlight order: DateTime, Decimal, Text, Link
        Assert.Equal(0, (int)PivotViewerPropertyType.DateTime);
        Assert.Equal(1, (int)PivotViewerPropertyType.Decimal);
        Assert.Equal(2, (int)PivotViewerPropertyType.Text);
        Assert.Equal(3, (int)PivotViewerPropertyType.Link);
    }

    [Fact]
    public void CxmlCollectionState_Values_MatchSilverlight()
    {
        Assert.Equal(0, (int)CxmlCollectionState.Initialized);
        Assert.Equal(1, (int)CxmlCollectionState.Loading);
        Assert.Equal(2, (int)CxmlCollectionState.Loaded);
        Assert.Equal(3, (int)CxmlCollectionState.Failed);
    }
}
