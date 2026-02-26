using SkiaSharp.Extended.PivotViewer;
using Xunit;

namespace SkiaSharp.Extended.PivotViewer.Tests;

public class ControlBarModelTest
{
    private static (PivotViewerController controller, ControlBarModel model) CreateTestModel()
    {
        var controller = new PivotViewerController();
        var source = TestDataHelper.LoadCxml("conceptcars.cxml");
        controller.LoadCollection(source);
        controller.SetAvailableSize(1024, 768);
        return (controller, controller.ControlBar);
    }

    [Fact]
    public void Default_IsFilterPaneVisible_True()
    {
        var (_, model) = CreateTestModel();
        Assert.True(model.IsFilterPaneVisible);
    }

    [Fact]
    public void ToggleFilterPane_Toggles()
    {
        var (_, model) = CreateTestModel();

        model.ToggleFilterPane();
        Assert.False(model.IsFilterPaneVisible);

        model.ToggleFilterPane();
        Assert.True(model.IsFilterPaneVisible);
    }

    [Fact]
    public void SearchText_Syncs_WithController()
    {
        var (controller, model) = CreateTestModel();

        model.SearchText = "Ford";
        Assert.Equal("Ford", controller.SearchText);
        Assert.Equal("Ford", model.SearchText);
    }

    [Fact]
    public void SetView_Updates_Controller()
    {
        var (controller, model) = CreateTestModel();

        model.SetView("graph");
        Assert.Equal("graph", controller.CurrentView);
        Assert.Equal("graph", model.CurrentView);
    }

    [Fact]
    public void CountDisplay_AllItems_ShowsTotal()
    {
        var (controller, model) = CreateTestModel();

        int total = controller.Items.Count;
        Assert.Equal($"{total}", model.CountDisplay);
    }

    [Fact]
    public void CountDisplay_Filtered_ShowsFraction()
    {
        var (controller, model) = CreateTestModel();

        controller.FilterEngine.AddStringFilter("Manufacturer", "Alfa Romeo");
        int inScope = controller.InScopeItems.Count;
        int total = controller.Items.Count;

        Assert.True(inScope < total);
        Assert.Equal($"{inScope} of {total}", model.CountDisplay);
    }

    [Fact]
    public void ToggleSortDirection_FlipsDescending()
    {
        var (controller, model) = CreateTestModel();

        bool initial = model.SortDescending;
        model.ToggleSortDirection();
        Assert.Equal(!initial, controller.SortDescending);
        Assert.Equal(!initial, model.SortDescending);
    }

    [Fact]
    public void ClearAll_ResetsSearchAndFilters()
    {
        var (controller, model) = CreateTestModel();

        model.SearchText = "Ford";
        controller.FilterEngine.AddStringFilter("Manufacturer", "Alfa Romeo");
        Assert.True(controller.FilterPaneModel!.HasActiveFilters);

        model.ClearAll();

        Assert.Equal("", model.SearchText);
        Assert.False(controller.FilterPaneModel!.HasActiveFilters);
    }

    [Fact]
    public void INPC_Fires_OnFilterPaneToggle()
    {
        var (_, model) = CreateTestModel();
        var changed = new List<string>();
        model.PropertyChanged += (s, e) => changed.Add(e.PropertyName!);

        model.ToggleFilterPane();

        Assert.Contains("IsFilterPaneVisible", changed);
    }

    [Fact]
    public void CurrentView_ReflectsController()
    {
        var (controller, model) = CreateTestModel();

        Assert.Equal("grid", model.CurrentView);

        controller.CurrentView = "graph";
        Assert.Equal("graph", model.CurrentView);
    }
}
