using SkiaSharp.Extended.PivotViewer;

namespace SkiaSharp.Extended.PivotViewer.Tests;

public class PivotViewerViewStateTest
{
    [Fact]
    public void Default_IsFilterPaneVisible_True()
    {
        var state = new PivotViewerViewState();
        Assert.True(state.IsFilterPaneVisible);
    }

    [Fact]
    public void Default_FilterScrollOffset_Zero()
    {
        var state = new PivotViewerViewState();
        Assert.Equal(0.0, state.FilterScrollOffset);
    }

    [Fact]
    public void Default_FilterContentHeight_Zero()
    {
        var state = new PivotViewerViewState();
        Assert.Equal(0.0, state.FilterContentHeight);
    }

    [Fact]
    public void Default_ExpandedFilterCategories_Empty()
    {
        var state = new PivotViewerViewState();
        Assert.Empty(state.ExpandedFilterCategories);
    }

    [Fact]
    public void Default_IsSortDropdownVisible_False()
    {
        var state = new PivotViewerViewState();
        Assert.False(state.IsSortDropdownVisible);
    }

    [Fact]
    public void Default_HoverItem_Null()
    {
        var state = new PivotViewerViewState();
        Assert.Null(state.HoverItem);
    }

    [Fact]
    public void ClampFilterScroll_NegativeOffset_ClampsToZero()
    {
        var state = new PivotViewerViewState
        {
            FilterScrollOffset = -50,
            FilterContentHeight = 500,
        };

        state.ClampFilterScroll(200);
        Assert.Equal(0.0, state.FilterScrollOffset);
    }

    [Fact]
    public void ClampFilterScroll_ExceedsMax_ClampsToMax()
    {
        var state = new PivotViewerViewState
        {
            FilterScrollOffset = 400,
            FilterContentHeight = 500,
        };

        state.ClampFilterScroll(200);
        Assert.Equal(300.0, state.FilterScrollOffset);
    }

    [Fact]
    public void ClampFilterScroll_WithinBounds_Unchanged()
    {
        var state = new PivotViewerViewState
        {
            FilterScrollOffset = 100,
            FilterContentHeight = 500,
        };

        state.ClampFilterScroll(200);
        Assert.Equal(100.0, state.FilterScrollOffset);
    }

    [Fact]
    public void ClampFilterScroll_ContentSmallerThanViewport_ClampsToZero()
    {
        var state = new PivotViewerViewState
        {
            FilterScrollOffset = 50,
            FilterContentHeight = 100,
        };

        state.ClampFilterScroll(200);
        Assert.Equal(0.0, state.FilterScrollOffset);
    }

    [Fact]
    public void ClampFilterScroll_ZeroContentHeight_ClampsToZero()
    {
        var state = new PivotViewerViewState
        {
            FilterScrollOffset = 50,
            FilterContentHeight = 0,
        };

        state.ClampFilterScroll(200);
        Assert.Equal(0.0, state.FilterScrollOffset);
    }

    [Fact]
    public void ExpandedFilterCategories_CanAddAndRemove()
    {
        var state = new PivotViewerViewState();

        state.ExpandedFilterCategories.Add("Color");
        state.ExpandedFilterCategories.Add("Size");
        Assert.Equal(2, state.ExpandedFilterCategories.Count);
        Assert.Contains("Color", state.ExpandedFilterCategories);

        state.ExpandedFilterCategories.Remove("Color");
        Assert.Single(state.ExpandedFilterCategories);
        Assert.DoesNotContain("Color", state.ExpandedFilterCategories);
    }

    [Fact]
    public void HoverItem_CanSetAndClear()
    {
        var state = new PivotViewerViewState();
        var item = new PivotViewerItem("test");

        state.HoverItem = item;
        Assert.Same(item, state.HoverItem);

        state.HoverItem = null;
        Assert.Null(state.HoverItem);
    }

    [Fact]
    public void DetailScrollOffset_Default_Zero()
    {
        var state = new PivotViewerViewState();
        Assert.Equal(0.0, state.DetailScrollOffset);
    }

    [Fact]
    public void DetailContentHeight_Default_Zero()
    {
        var state = new PivotViewerViewState();
        Assert.Equal(0.0, state.DetailContentHeight);
    }

    [Fact]
    public void ClampDetailScroll_NegativeOffset_ClampsToZero()
    {
        var state = new PivotViewerViewState
        {
            DetailScrollOffset = -10,
            DetailContentHeight = 500,
        };

        state.ClampDetailScroll(600);
        Assert.Equal(0.0, state.DetailScrollOffset);
    }

    [Fact]
    public void ClampDetailScroll_ExceedsMax_ClampsToMax()
    {
        var state = new PivotViewerViewState
        {
            DetailScrollOffset = 1000,
            DetailContentHeight = 800,
        };

        state.ClampDetailScroll(500);
        Assert.Equal(300.0, state.DetailScrollOffset);
    }

    [Fact]
    public void ClampDetailScroll_ContentSmallerThanViewport_ClampsToZero()
    {
        var state = new PivotViewerViewState
        {
            DetailScrollOffset = 50,
            DetailContentHeight = 100,
        };

        state.ClampDetailScroll(600);
        Assert.Equal(0.0, state.DetailScrollOffset);
    }

    [Fact]
    public void ClampDetailScroll_WithinBounds_Unchanged()
    {
        var state = new PivotViewerViewState
        {
            DetailScrollOffset = 100,
            DetailContentHeight = 800,
        };

        state.ClampDetailScroll(400);
        Assert.Equal(100.0, state.DetailScrollOffset);
    }
}
