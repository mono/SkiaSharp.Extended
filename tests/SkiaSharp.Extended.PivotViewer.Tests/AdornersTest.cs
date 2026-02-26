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

    [Fact]
    public void DefaultAdorner_RequestCommands_EventArgsHasEmptyCommandsList()
    {
        var adorner = new PivotViewerDefaultItemAdorner();
        var item = new PivotViewerItem("test-1");
        PivotViewerCommandsRequestedEventArgs? received = null;

        adorner.CommandsRequested += (s, e) => received = e;
        adorner.RequestCommands(item, false);

        Assert.NotNull(received);
        Assert.Empty(received!.Commands);
    }

    [Fact]
    public void DefaultAdorner_RequestCommands_IsItemSelectedFalse()
    {
        var adorner = new PivotViewerDefaultItemAdorner();
        var item = new PivotViewerItem("test-1");
        PivotViewerCommandsRequestedEventArgs? received = null;

        adorner.CommandsRequested += (s, e) => received = e;
        adorner.RequestCommands(item, false);

        Assert.NotNull(received);
        Assert.False(received!.IsItemSelected);
    }

    [Fact]
    public void DefaultAdorner_RequestCommands_SenderIsAdorner()
    {
        var adorner = new PivotViewerDefaultItemAdorner();
        var item = new PivotViewerItem("test-1");
        object? sender = null;

        adorner.CommandsRequested += (s, e) => sender = s;
        adorner.RequestCommands(item, true);

        Assert.Same(adorner, sender);
    }

    [Fact]
    public void DefaultAdorner_CommandsCanBeAddedDuringEvent()
    {
        var adorner = new PivotViewerDefaultItemAdorner();
        var item = new PivotViewerItem("test-1");
        PivotViewerCommandsRequestedEventArgs? received = null;

        adorner.CommandsRequested += (s, e) =>
        {
            e.Commands.Add(new TestCommand("Open"));
            received = e;
        };
        adorner.RequestCommands(item, true);

        Assert.NotNull(received);
        Assert.Single(received!.Commands);
        Assert.Equal("Open", received.Commands[0].DisplayName);
    }

    [Fact]
    public void PivotViewerItemAdorner_IsMouseOverToggle()
    {
        var adorner = new PivotViewerDefaultItemAdorner();

        adorner.IsMouseOver = true;
        Assert.True(adorner.IsMouseOver);

        adorner.IsMouseOver = false;
        Assert.False(adorner.IsMouseOver);
    }

    [Fact]
    public void PivotViewerItemAdorner_IsItemSelectedToggle()
    {
        var adorner = new PivotViewerDefaultItemAdorner();

        adorner.IsItemSelected = true;
        Assert.True(adorner.IsItemSelected);

        adorner.IsItemSelected = false;
        Assert.False(adorner.IsItemSelected);
    }

    private class TestCommand : IPivotViewerUICommand
    {
        public TestCommand(string name) => DisplayName = name;
        public string DisplayName { get; }
        public Uri? Icon => null;
        public object? ToolTip => null;
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) { }
    }
}
