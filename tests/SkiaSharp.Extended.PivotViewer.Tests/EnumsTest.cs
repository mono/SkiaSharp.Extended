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

    [Fact]
    public void PivotViewerStretch_Values()
    {
        Assert.Equal(0, (int)PivotViewerStretch.None);
        Assert.Equal(1, (int)PivotViewerStretch.Fill);
        Assert.Equal(2, (int)PivotViewerStretch.Uniform);
        Assert.Equal(3, (int)PivotViewerStretch.UniformToFill);
    }

    [Fact]
    public void PivotViewerStretch_HasFourValues()
    {
        var values = Enum.GetValues<PivotViewerStretch>();
        Assert.Equal(4, values.Length);
    }

    [Fact]
    public void PivotViewerPropertyOptions_FlagsCombinations()
    {
        var filterAndSearch = PivotViewerPropertyOptions.CanFilter | PivotViewerPropertyOptions.CanSearchText;
        Assert.Equal(6, (int)filterAndSearch);
        Assert.True(filterAndSearch.HasFlag(PivotViewerPropertyOptions.CanFilter));
        Assert.True(filterAndSearch.HasFlag(PivotViewerPropertyOptions.CanSearchText));
        Assert.False(filterAndSearch.HasFlag(PivotViewerPropertyOptions.Private));
        Assert.False(filterAndSearch.HasFlag(PivotViewerPropertyOptions.WrappingText));

        var all = PivotViewerPropertyOptions.Private
            | PivotViewerPropertyOptions.CanFilter
            | PivotViewerPropertyOptions.CanSearchText
            | PivotViewerPropertyOptions.WrappingText;
        Assert.Equal(15, (int)all);
        Assert.True(all.HasFlag(PivotViewerPropertyOptions.Private));
        Assert.True(all.HasFlag(PivotViewerPropertyOptions.WrappingText));
    }

    [Fact]
    public void PivotViewerPropertyOptions_None_IsDefault()
    {
        PivotViewerPropertyOptions opts = default;
        Assert.Equal(PivotViewerPropertyOptions.None, opts);
    }

    [Fact]
    public void RenderHitType_AllValuesPresent()
    {
        var values = Enum.GetValues<RenderHitType>();
        Assert.Equal(15, values.Length);
    }

    [Fact]
    public void RenderHitType_NoneIsDefault()
    {
        Assert.Equal(RenderHitType.None, default(RenderHitType));
    }
}
