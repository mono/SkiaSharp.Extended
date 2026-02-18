using SkiaSharp.Extended.PivotViewer;
using Xunit;

namespace SkiaSharp.Extended.PivotViewer.Tests;

public class EventArgsTest
{
    [Fact]
    public void PivotViewerItemDoubleClickEventArgs_RequiresItem()
    {
        var item = new PivotViewerItem("test");
        var args = new PivotViewerItemDoubleClickEventArgs(item);
        Assert.Same(item, args.Item);
    }

    [Fact]
    public void PivotViewerItemDoubleClickEventArgs_NullThrows()
    {
        Assert.Throws<ArgumentNullException>(() => new PivotViewerItemDoubleClickEventArgs(null!));
    }

    [Fact]
    public void PivotViewerLinkEventArgs_RequiresUri()
    {
        var uri = new Uri("https://example.com");
        var args = new PivotViewerLinkEventArgs(uri);
        Assert.Equal(uri, args.Link);
        Assert.False(args.Handled);
    }

    [Fact]
    public void PivotViewerLinkEventArgs_HandledCanBeSet()
    {
        var args = new PivotViewerLinkEventArgs(new Uri("https://test.com"));
        args.Handled = true;
        Assert.True(args.Handled);
    }

    [Fact]
    public void PivotViewerFilterEventArgs_HasFilter()
    {
        var args = new PivotViewerFilterEventArgs("color=red");
        Assert.Equal("color=red", args.Filter);
        Assert.False(args.Handled);
    }

    [Fact]
    public void PivotViewerFilterEventArgs_NullBecomesEmpty()
    {
        var args = new PivotViewerFilterEventArgs(null!);
        Assert.Equal("", args.Filter);
    }

    [Fact]
    public void PivotViewerViewUpdatingEventArgs_PropertiesSet()
    {
        var prop = new PivotViewerStringProperty("test") { DisplayName = "Test" };
        var args = new PivotViewerViewUpdatingEventArgs(prop, "user");
        Assert.Same(prop, args.SortPivotProperty);
        Assert.Equal("user", args.Source);
    }

    [Fact]
    public void PivotViewerViewUpdatingEventArgs_NullsAllowed()
    {
        var args = new PivotViewerViewUpdatingEventArgs(null, null);
        Assert.Null(args.SortPivotProperty);
        Assert.Null(args.Source);
    }

    [Fact]
    public void PivotViewerCommandsRequestedEventArgs_HasItem()
    {
        var item = new PivotViewerItem("cmd-test");
        var args = new PivotViewerCommandsRequestedEventArgs(item, true);
        Assert.Same(item, args.Item);
        Assert.True(args.IsItemSelected);
        Assert.Empty(args.Commands);
    }

    [Fact]
    public void PivotViewerCommandsRequestedEventArgs_CanAddCommands()
    {
        var item = new PivotViewerItem("cmd-test");
        var args = new PivotViewerCommandsRequestedEventArgs(item, false);
        var cmd = new TestCommand();
        args.Commands.Add(cmd);
        Assert.Single(args.Commands);
        Assert.Same(cmd, args.Commands[0]);
    }

    [Fact]
    public void PivotViewerCommandsRequestedEventArgs_NullItemThrows()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new PivotViewerCommandsRequestedEventArgs(null!, false));
    }

    private class TestCommand : IPivotViewerUICommand
    {
        public string DisplayName => "Test";
        public Uri? Icon => null;
        public object? ToolTip => "A test command";
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) { }
    }
}
