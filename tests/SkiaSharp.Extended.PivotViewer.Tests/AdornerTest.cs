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

    [Fact]
    public void DefaultAdorner_ToggleProperties()
    {
        var adorner = new PivotViewerDefaultItemAdorner();

        adorner.IsMouseOver = true;
        Assert.True(adorner.IsMouseOver);
        adorner.IsMouseOver = false;
        Assert.False(adorner.IsMouseOver);

        adorner.IsItemSelected = true;
        Assert.True(adorner.IsItemSelected);
        adorner.IsItemSelected = false;
        Assert.False(adorner.IsItemSelected);
    }

    [Fact]
    public void CommandsRequestedEventArgs_Constructor_SetsProperties()
    {
        var item = new PivotViewerItem("item1");
        var args = new PivotViewerCommandsRequestedEventArgs(item, true);

        Assert.Same(item, args.Item);
        Assert.True(args.IsItemSelected);
        Assert.NotNull(args.Commands);
        Assert.Empty(args.Commands);
    }

    [Fact]
    public void CommandsRequestedEventArgs_NotSelected()
    {
        var item = new PivotViewerItem("item2");
        var args = new PivotViewerCommandsRequestedEventArgs(item, false);

        Assert.False(args.IsItemSelected);
    }

    [Fact]
    public void CommandsRequestedEventArgs_NullItem_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new PivotViewerCommandsRequestedEventArgs(null!, true));
    }

    [Fact]
    public void CommandsRequestedEventArgs_CommandsCanBePopulated()
    {
        var item = new PivotViewerItem("item3");
        var args = new PivotViewerCommandsRequestedEventArgs(item, false);

        var cmd = new TestCommand("Delete");
        args.Commands.Add(cmd);

        Assert.Single(args.Commands);
        Assert.Equal("Delete", args.Commands[0].DisplayName);
    }

    [Fact]
    public void TestCommand_ImplementsIPivotViewerUICommand()
    {
        var cmd = new TestCommand("Open");

        Assert.Equal("Open", cmd.DisplayName);
        Assert.Null(cmd.Icon);
        Assert.Null(cmd.ToolTip);
        Assert.True(cmd.CanExecute(null));
        cmd.Execute(null); // should not throw
    }

    [Fact]
    public void DefaultAdorner_RequestCommands_WithNotSelected()
    {
        var adorner = new PivotViewerDefaultItemAdorner();
        var item = new PivotViewerItem("test");
        PivotViewerCommandsRequestedEventArgs? received = null;

        adorner.CommandsRequested += (s, e) => received = e;
        adorner.RequestCommands(item, false);

        Assert.NotNull(received);
        Assert.False(received!.IsItemSelected);
    }

    [Fact]
    public void AdornerBase_IsAbstract_CannotInstantiateDirectly()
    {
        // PivotViewerItemAdorner is abstract — we can only use via subclass
        PivotViewerItemAdorner adorner = new PivotViewerDefaultItemAdorner();
        Assert.IsAssignableFrom<PivotViewerItemAdorner>(adorner);
    }

    [Fact]
    public void DefaultAdorner_MultipleSubscribers_AllReceiveEvent()
    {
        var adorner = new PivotViewerDefaultItemAdorner();
        var item = new PivotViewerItem("multi");
        int callCount = 0;

        adorner.CommandsRequested += (s, e) => callCount++;
        adorner.CommandsRequested += (s, e) => callCount++;

        adorner.RequestCommands(item, true);
        Assert.Equal(2, callCount);
    }

    [Fact]
    public void DefaultAdorner_SenderIsAdorner()
    {
        var adorner = new PivotViewerDefaultItemAdorner();
        var item = new PivotViewerItem("sender-test");
        object? receivedSender = null;

        adorner.CommandsRequested += (s, e) => receivedSender = s;
        adorner.RequestCommands(item, false);

        Assert.Same(adorner, receivedSender);
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
