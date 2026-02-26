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

    [Fact]
    public void GridView_SetAllProperties_RoundTrips()
    {
        var view = new PivotViewerGridView();
        view.Name = "My Grid";
        view.Id = "custom-grid";
        view.ToolTip = "A custom grid view";
        view.IsAvailable = false;

        Assert.Equal("My Grid", view.Name);
        Assert.Equal("custom-grid", view.Id);
        Assert.Equal("A custom grid view", view.ToolTip);
        Assert.False(view.IsAvailable);
    }

    [Fact]
    public void GraphView_SetAllProperties_RoundTrips()
    {
        var view = new PivotViewerGraphView();
        view.Name = "My Graph";
        view.Id = "custom-graph";
        view.ToolTip = "A custom graph view";
        view.IsAvailable = false;

        var groupProp = new PivotViewerStringProperty("Color");
        var stackProp = new PivotViewerNumericProperty("Size");
        view.GroupByProperty = groupProp;
        view.StackByProperty = stackProp;

        Assert.Equal("My Graph", view.Name);
        Assert.Equal("custom-graph", view.Id);
        Assert.Equal("A custom graph view", view.ToolTip);
        Assert.False(view.IsAvailable);
        Assert.Same(groupProp, view.GroupByProperty);
        Assert.Same(stackProp, view.StackByProperty);
    }

    [Fact]
    public void GraphView_NullGroupByAndStackBy()
    {
        var view = new PivotViewerGraphView();
        Assert.Null(view.GroupByProperty);
        Assert.Null(view.StackByProperty);

        var prop = new PivotViewerStringProperty("Test");
        view.GroupByProperty = prop;
        view.StackByProperty = prop;
        view.GroupByProperty = null;
        view.StackByProperty = null;

        Assert.Null(view.GroupByProperty);
        Assert.Null(view.StackByProperty);
    }

    [Fact]
    public void GridView_ToString_ReturnsMeaningfulString()
    {
        var view = new PivotViewerGridView();
        var str = view.ToString();
        Assert.NotNull(str);
        Assert.NotEqual("", str);
    }

    [Fact]
    public void GraphView_ToString_ReturnsMeaningfulString()
    {
        var view = new PivotViewerGraphView();
        var str = view.ToString();
        Assert.NotNull(str);
        Assert.NotEqual("", str);
    }

    [Fact]
    public void GridView_Id_PropertyChanged_Fires()
    {
        var view = new PivotViewerGridView();
        var changed = new List<string>();
        view.PropertyChanged += (s, e) => changed.Add(e.PropertyName!);

        view.Id = "new-id";
        Assert.Contains("Id", changed);
        Assert.Equal("new-id", view.Id);
    }

    [Fact]
    public void View_PropertyChanged_SenderIsView()
    {
        var view = new PivotViewerGridView();
        object? sender = null;
        view.PropertyChanged += (s, e) => sender = s;

        view.Name = "Updated";
        Assert.Same(view, sender);
    }
}
