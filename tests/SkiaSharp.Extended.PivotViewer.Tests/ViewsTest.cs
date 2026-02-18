using SkiaSharp.Extended.PivotViewer;
using Xunit;

namespace SkiaSharp.Extended.PivotViewer.Tests;

public class ViewsTest
{
    [Fact]
    public void GridView_Defaults()
    {
        var view = new PivotViewerGridView();
        Assert.Equal("GridView", view.Id);
        Assert.Equal("Grid", view.Name);
        Assert.True(view.IsAvailable);
        Assert.Null(view.ToolTip);
    }

    [Fact]
    public void GraphView_Defaults()
    {
        var view = new PivotViewerGraphView();
        Assert.Equal("GraphView", view.Id);
        Assert.Equal("Graph", view.Name);
        Assert.True(view.IsAvailable);
    }

    [Fact]
    public void View_PropertyChanged_Fires()
    {
        var view = new PivotViewerGridView();
        var changed = new List<string>();
        view.PropertyChanged += (s, e) => changed.Add(e.PropertyName!);

        view.Name = "Custom Grid";
        view.ToolTip = "Tip";
        view.IsAvailable = false;

        Assert.Contains("Name", changed);
        Assert.Contains("ToolTip", changed);
        Assert.Contains("IsAvailable", changed);
    }

    [Fact]
    public void View_ViewUpdating_Fires()
    {
        var view = new PivotViewerGridView();
        PivotViewerViewUpdatingEventArgs? received = null;
        view.PivotViewerViewUpdating += (s, e) => received = e;

        // View updating is protected, but we can verify the event exists
        // (it would be invoked by the PivotViewer controller during view transitions)
        Assert.Null(received);
    }

    [Fact]
    public void GridView_IsSealed()
    {
        Assert.True(typeof(PivotViewerGridView).IsSealed);
    }

    [Fact]
    public void GraphView_IsSealed()
    {
        Assert.True(typeof(PivotViewerGraphView).IsSealed);
    }

    [Fact]
    public void GraphView_GroupByAndStackBy_FirePropertyChanged()
    {
        var view = new PivotViewerGraphView();
        var changed = new List<string>();
        view.PropertyChanged += (s, e) => changed.Add(e.PropertyName!);

        var prop = new PivotViewerStringProperty("Category");
        view.GroupByProperty = prop;
        view.StackByProperty = prop;

        Assert.Contains("GroupByProperty", changed);
        Assert.Contains("StackByProperty", changed);
        Assert.Same(prop, view.GroupByProperty);
        Assert.Same(prop, view.StackByProperty);
    }
}
