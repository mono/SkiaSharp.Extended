using SkiaSharp.Extended.PivotViewer;
using Xunit;

namespace SkiaSharp.Extended.PivotViewer.Tests;

public class AdornerTest
{
    [Fact]
    public void DefaultAdorner_InitialState()
    {
        var adorner = new PivotViewerDefaultItemAdorner();
        Assert.False(adorner.IsMouseOver);
        Assert.False(adorner.IsItemSelected);
    }

    [Fact]
    public void DefaultAdorner_PropertiesCanBeSet()
    {
        var adorner = new PivotViewerDefaultItemAdorner();
        adorner.IsMouseOver = true;
        adorner.IsItemSelected = true;
        Assert.True(adorner.IsMouseOver);
        Assert.True(adorner.IsItemSelected);
    }

    [Fact]
    public void DefaultAdorner_CommandsRequested_Fires()
    {
        var adorner = new PivotViewerDefaultItemAdorner();
        var item = new PivotViewerItem("test");
        PivotViewerCommandsRequestedEventArgs? received = null;

        adorner.CommandsRequested += (s, e) => received = e;
        adorner.RequestCommands(item, true);

        Assert.NotNull(received);
        Assert.Same(item, received!.Item);
        Assert.True(received.IsItemSelected);
    }

    [Fact]
    public void DefaultAdorner_CommandsRequested_CanAddCommands()
    {
        var adorner = new PivotViewerDefaultItemAdorner();
        var item = new PivotViewerItem("test");

        adorner.CommandsRequested += (s, e) =>
        {
            e.Commands.Add(new TestCommand("Open"));
            e.Commands.Add(new TestCommand("Share"));
        };
        adorner.RequestCommands(item, false);

        // Verify the commands were added by subscribing again
        PivotViewerCommandsRequestedEventArgs? args = null;
        adorner.CommandsRequested += (s, e) => args = e;
        adorner.RequestCommands(item, false);
        Assert.NotNull(args);
        Assert.Equal(2, args!.Commands.Count);
    }

    [Fact]
    public void DefaultAdorner_NoSubscribers_DoesNotThrow()
    {
        var adorner = new PivotViewerDefaultItemAdorner();
        var item = new PivotViewerItem("test");
        // Should not throw even with no subscribers
        adorner.RequestCommands(item, true);
    }

    private class TestCommand : IPivotViewerUICommand
    {
        public TestCommand(string name) { DisplayName = name; }
        public string DisplayName { get; }
        public Uri? Icon => null;
        public object? ToolTip => null;
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) { }
    }
}
