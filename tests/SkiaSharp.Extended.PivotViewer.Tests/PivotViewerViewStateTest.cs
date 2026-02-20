using SkiaSharp.Extended.PivotViewer;

namespace SkiaSharp.Extended.PivotViewer.Tests;

public class PivotViewerViewStateTest
{
    [Fact]
    public void Default_HoverItem_Null()
    {
        var state = new PivotViewerViewState();
        Assert.Null(state.HoverItem);
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
}
