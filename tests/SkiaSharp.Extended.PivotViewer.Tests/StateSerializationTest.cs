using SkiaSharp.Extended.PivotViewer;
using Xunit;

namespace SkiaSharp.Extended.PivotViewer.Tests;

/// <summary>
/// Tests for state serialization/deserialization round-trips.
/// </summary>
public class StateSerializationTest
{
    [Fact]
    public void RoundTrip_EmptyState()
    {
        var controller = new PivotViewerController();
        var state = controller.SerializeViewerState();
        Assert.NotNull(state);

        controller.SetViewerState(state);
        var state2 = controller.SerializeViewerState();
        Assert.Equal(state, state2);
    }

    [Fact]
    public void RoundTrip_WithFiltersAndSort()
    {
        var controller = CreateControllerWithConceptCars();

        controller.FilterEngine.AddStringFilter("Manufacturer", "BMW");
        controller.SortProperty = controller.Properties.First(p =>
            p.PropertyType == PivotViewerPropertyType.Text);

        var state = controller.SerializeViewerState();

        // Create fresh controller
        var controller2 = CreateControllerWithConceptCars();
        controller2.SetViewerState(state);

        Assert.Equal(controller.InScopeItems.Count, controller2.InScopeItems.Count);
        Assert.Equal(controller.SortProperty?.Id, controller2.SortProperty?.Id);
    }

    [Fact]
    public void RoundTrip_WithSelection()
    {
        var controller = CreateControllerWithConceptCars();
        controller.SelectedItem = controller.Items.First();

        var state = controller.SerializeViewerState();

        var controller2 = CreateControllerWithConceptCars();
        controller2.SetViewerState(state);

        Assert.Equal(controller.SelectedItem?.Id, controller2.SelectedItem?.Id);
    }

    [Fact]
    public void RoundTrip_GraphView()
    {
        var controller = CreateControllerWithConceptCars();
        controller.SortProperty = controller.Properties.First(p =>
            p.PropertyType == PivotViewerPropertyType.Text);
        controller.CurrentView = "graph";

        var state = controller.SerializeViewerState();

        var controller2 = CreateControllerWithConceptCars();
        controller2.SetViewerState(state);

        Assert.Equal("graph", controller2.CurrentView);
    }

    [Fact]
    public void RoundTrip_MultipleStringFilters()
    {
        var controller = CreateControllerWithConceptCars();

        controller.FilterEngine.AddStringFilter("Manufacturer", "BMW");
        controller.FilterEngine.AddStringFilter("Manufacturer", "Audi");

        var beforeCount = controller.InScopeItems.Count;
        var state = controller.SerializeViewerState();

        var controller2 = CreateControllerWithConceptCars();
        controller2.SetViewerState(state);

        Assert.Equal(beforeCount, controller2.InScopeItems.Count);
    }

    [Fact]
    public void SetViewerState_ClearsExistingFilters()
    {
        var controller = CreateControllerWithConceptCars();
        controller.FilterEngine.AddStringFilter("Manufacturer", "BMW");

        // Serialize empty state
        var fresh = new PivotViewerController();
        var emptyState = fresh.SerializeViewerState();

        // Apply empty state should clear existing filters
        controller.SetViewerState(emptyState);
        Assert.Equal(controller.Items.Count, controller.InScopeItems.Count);
    }

    [Fact]
    public void SetViewerState_InvalidState_HandledGracefully()
    {
        var controller = CreateControllerWithConceptCars();
        // Should not throw on empty or garbage strings
        controller.SetViewerState("");
        controller.SetViewerState("garbage-state-data");
    }

    [Fact]
    public void ViewerState_FilterCounts_Preserved()
    {
        var controller = CreateControllerWithConceptCars();
        controller.FilterEngine.AddStringFilter("Manufacturer", "BMW");

        var counts = controller.GetFilterCounts("Manufacturer");
        Assert.True(counts.Count > 0);

        var state = controller.SerializeViewerState();
        var controller2 = CreateControllerWithConceptCars();
        controller2.SetViewerState(state);

        var counts2 = controller2.GetFilterCounts("Manufacturer");
        Assert.Equal(counts.Count, counts2.Count);
    }

    private static PivotViewerController CreateControllerWithConceptCars()
    {
        var source = TestDataHelper.LoadCxml("conceptcars.cxml");
        var controller = new PivotViewerController();
        controller.LoadCollection(source);
        controller.SetAvailableSize(800, 600);
        return controller;
    }
}
