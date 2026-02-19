using SkiaSharp.Extended.PivotViewer;

namespace SkiaSharp.Extended.PivotViewer.Tests;

public class AdornersTest
{
    [Fact]
    public void PivotViewerDefaultItemAdorner_InheritsBase()
    {
        var adorner = new PivotViewerDefaultItemAdorner();
        Assert.IsAssignableFrom<PivotViewerItemAdorner>(adorner);
    }

    [Fact]
    public void PivotViewerItemAdorner_DefaultProperties()
    {
        var adorner = new PivotViewerDefaultItemAdorner();
        Assert.False(adorner.IsMouseOver);
        Assert.False(adorner.IsItemSelected);
    }

    [Fact]
    public void PivotViewerItemAdorner_SetProperties()
    {
        var adorner = new PivotViewerDefaultItemAdorner();
        adorner.IsMouseOver = true;
        adorner.IsItemSelected = true;

        Assert.True(adorner.IsMouseOver);
        Assert.True(adorner.IsItemSelected);
    }

    [Fact]
    public void DefaultAdorner_RequestCommands_FiresEvent()
    {
        var adorner = new PivotViewerDefaultItemAdorner();
        var item = new PivotViewerItem("test-1");
        PivotViewerCommandsRequestedEventArgs? received = null;

        adorner.CommandsRequested += (s, e) => received = e;
        adorner.RequestCommands(item, true);

        Assert.NotNull(received);
        Assert.Same(item, received!.Item);
        Assert.True(received.IsItemSelected);
    }

    [Fact]
    public void DefaultAdorner_RequestCommands_NoSubscriber_NoException()
    {
        var adorner = new PivotViewerDefaultItemAdorner();
        var item = new PivotViewerItem("test-1");

        // Should not throw even with no subscribers
        adorner.RequestCommands(item, false);
    }
}
