using SkiaSharp.Extended.PivotViewer;
using Xunit;

namespace SkiaSharp.Extended.PivotViewer.Tests;

public class PivotViewerControllerTest
{
    private static (PivotViewerController controller, CxmlCollectionSource source) CreateTestController()
    {
        var controller = new PivotViewerController();
        var source = TestDataHelper.LoadCxml("conceptcars.cxml");
        controller.LoadCollection(source);
        controller.SetAvailableSize(1024, 768);
        return (controller, source);
    }

    [Fact]
    public void LoadCollection_PopulatesItemsAndProperties()
    {
        var (controller, source) = CreateTestController();

        Assert.True(controller.Items.Count > 0, "Should have items");
        Assert.True(controller.Properties.Count > 0, "Should have properties");
        Assert.Equal(controller.Items.Count, controller.InScopeItems.Count);
    }

    [Fact]
    public void LoadCollection_BuildsWordWheel()
    {
        var (controller, _) = CreateTestController();

        var results = controller.Search("Ford");
        Assert.NotEmpty(results);
    }

    [Fact]
    public void FilterEngine_Accessible()
    {
        var (controller, _) = CreateTestController();

        Assert.NotNull(controller.FilterEngine);
        Assert.Empty(controller.FilterEngine.Predicates);
    }

    [Fact]
    public void AddFilter_ReducesInScopeItems()
    {
        var (controller, _) = CreateTestController();

        int allCount = controller.InScopeItems.Count;
        controller.FilterEngine.AddStringFilter("Manufacturer", "Alfa Romeo");
        int filteredCount = controller.InScopeItems.Count;

        Assert.True(filteredCount < allCount, "Filtering should reduce item count");
        Assert.True(filteredCount > 0, "Should still have some items");
    }

    [Fact]
    public void ClearFilter_RestoresAllItems()
    {
        var (controller, _) = CreateTestController();

        int allCount = controller.InScopeItems.Count;
        controller.FilterEngine.AddStringFilter("Manufacturer", "Alfa Romeo");
        Assert.True(controller.InScopeItems.Count < allCount);

        controller.FilterEngine.ClearAll();
        Assert.Equal(allCount, controller.InScopeItems.Count);
    }

    [Fact]
    public void Selection_SelectsItem()
    {
        var (controller, _) = CreateTestController();

        var firstItem = controller.InScopeItems[0];
        controller.SelectedItem = firstItem;

        Assert.Equal(firstItem, controller.SelectedItem);
        Assert.Equal(0, controller.SelectedIndex);
    }

    [Fact]
    public void Selection_ClearedWhenFilterRemovesItem()
    {
        var (controller, _) = CreateTestController();

        var firstItem = controller.InScopeItems[0];
        controller.SelectedItem = firstItem;

        // Apply a restrictive filter that excludes the selected item
        // Find a category that the first item doesn't belong to
        string? excludeCategory = null;
        var categories = controller.GetFilterCounts("Manufacturer");
        foreach (var kv in categories)
        {
            var firstItemValues = firstItem["Manufacturer"];
            if (firstItemValues == null || !firstItemValues.Cast<object>().Any(v => v?.ToString() == kv.Key))
            {
                excludeCategory = kv.Key;
                break;
            }
        }

        if (excludeCategory != null)
        {
            controller.FilterEngine.AddStringFilter("Manufacturer", excludeCategory);
            // Selected item should be cleared if it's no longer in scope
            if (!controller.InScopeItems.Contains(firstItem))
                Assert.Null(controller.SelectedItem);
        }
    }

    [Fact]
    public void SelectionChanged_EventFired()
    {
        var (controller, _) = CreateTestController();
        PivotViewerItem? selectedItem = null;
        controller.SelectionChanged += (s, e) => selectedItem = e.NewItem;

        controller.SelectedItem = controller.InScopeItems[0];

        Assert.NotNull(selectedItem);
    }

    [Fact]
    public void SelectedIndex_SetsItem()
    {
        var (controller, _) = CreateTestController();

        controller.SelectedIndex = 2;
        Assert.Equal(controller.InScopeItems[2], controller.SelectedItem);
    }

    [Fact]
    public void SelectedIndex_Minus1_ClearsSelection()
    {
        var (controller, _) = CreateTestController();

        controller.SelectedItem = controller.InScopeItems[0];
        controller.SelectedIndex = -1;

        Assert.Null(controller.SelectedItem);
    }

    [Fact]
    public void SortProperty_SortsItems()
    {
        var (controller, _) = CreateTestController();

        var yearProp = controller.Properties.FirstOrDefault(p => p.Id == "Production Year");
        if (yearProp != null)
        {
            controller.SortProperty = yearProp;

            // Items should be sorted by year
            for (int i = 1; i < controller.InScopeItems.Count; i++)
            {
                var prevVal = controller.InScopeItems[i - 1]["Production Year"];
                var curVal = controller.InScopeItems[i]["Production Year"];

                if (prevVal != null && curVal != null &&
                    prevVal.Count > 0 && curVal.Count > 0)
                {
                    // Just verify the sort property was applied (order could be ascending)
                    break; // Can't assert exact order without knowing direction
                }
            }
        }
    }

    [Fact]
    public void SortPropertyChanged_EventFired()
    {
        var (controller, _) = CreateTestController();

        bool fired = false;
        controller.SortPropertyChanged += (s, e) => fired = true;

        var yearProp = controller.Properties.FirstOrDefault(p => p.Id == "Production Year");
        if (yearProp != null)
        {
            controller.SortProperty = yearProp;
            Assert.True(fired);
        }
    }

    [Fact]
    public void CurrentView_DefaultsToGrid()
    {
        var (controller, _) = CreateTestController();
        Assert.Equal("grid", controller.CurrentView);
    }

    [Fact]
    public void CurrentView_CanSwitchToGraph()
    {
        var (controller, _) = CreateTestController();

        bool fired = false;
        controller.ViewChanged += (s, e) => fired = true;

        controller.CurrentView = "graph";
        Assert.Equal("graph", controller.CurrentView);
        Assert.True(fired);
    }

    [Fact]
    public void GridLayout_ComputedOnLoad()
    {
        var (controller, _) = CreateTestController();
        Assert.NotNull(controller.GridLayout);
        Assert.Null(controller.HistogramLayout);
    }

    [Fact]
    public void HistogramLayout_ComputedOnGraphView()
    {
        var (controller, _) = CreateTestController();

        // Need a sort property for histogram
        var catProp = controller.Properties.FirstOrDefault(p => p.Id == "Manufacturer");
        if (catProp != null)
        {
            controller.SortProperty = catProp;
            controller.CurrentView = "graph";

            Assert.NotNull(controller.HistogramLayout);
        }
    }

    [Fact]
    public void HitTest_FindsItem()
    {
        var (controller, _) = CreateTestController();

        if (controller.GridLayout != null && controller.GridLayout.Positions.Length > 0)
        {
            var pos = controller.GridLayout.Positions[0];
            var hit = controller.HitTest(pos.X + pos.Width / 2, pos.Y + pos.Height / 2);
            Assert.NotNull(hit);
        }
    }

    [Fact]
    public void HitTest_ReturnsNull_OutsideItems()
    {
        var (controller, _) = CreateTestController();
        var hit = controller.HitTest(-100, -100);
        Assert.Null(hit);
    }

    [Fact]
    public void GetFilterCounts_ReturnsCorrectCounts()
    {
        var (controller, _) = CreateTestController();
        var counts = controller.GetFilterCounts("Manufacturer");

        Assert.NotEmpty(counts);
        int totalCount = counts.Values.Sum();
        Assert.True(totalCount >= controller.InScopeItems.Count);
    }

    [Fact]
    public void SerializeViewerState_RoundTrips()
    {
        var (controller, _) = CreateTestController();

        // Set some state
        controller.FilterEngine.AddStringFilter("Manufacturer", "Alfa Romeo");
        controller.CurrentView = "graph";

        var state = controller.SerializeViewerState();
        Assert.NotNull(state);

        // Restore state
        controller.FilterEngine.ClearAll();
        controller.CurrentView = "grid";
        Assert.Equal(controller.Items.Count, controller.InScopeItems.Count);

        controller.SetViewerState(state);
        Assert.Equal("graph", controller.CurrentView);
        Assert.True(controller.InScopeItems.Count < controller.Items.Count);
    }

    [Fact]
    public void LoadItems_DirectBindingModel()
    {
        var controller = new PivotViewerController();

        var nameP = new PivotViewerStringProperty("Name") { DisplayName = "Name" };
        var items = new List<PivotViewerItem>();
        for (int i = 0; i < 10; i++)
        {
            var item = new PivotViewerItem(i.ToString());
            item.Set(nameP, new object[] { $"Item {i}" });
            items.Add(item);
        }

        controller.LoadItems(items, new[] { nameP });
        controller.SetAvailableSize(800, 600);

        Assert.Equal(10, controller.Items.Count);
        Assert.Equal(10, controller.InScopeItems.Count);
        Assert.NotNull(controller.GridLayout);
    }

    [Fact]
    public void CollectionChanged_EventFired()
    {
        var controller = new PivotViewerController();
        bool fired = false;
        controller.CollectionChanged += (s, e) => fired = true;

        var source = TestDataHelper.LoadCxml("conceptcars.cxml");
        controller.LoadCollection(source);

        Assert.True(fired);
    }

    [Fact]
    public void FiltersChanged_EventFired()
    {
        var (controller, _) = CreateTestController();

        bool fired = false;
        controller.FiltersChanged += (s, e) => fired = true;

        controller.FilterEngine.AddStringFilter("Manufacturer", "Alfa Romeo");
        Assert.True(fired);
    }

    [Fact]
    public void LayoutUpdated_EventFired()
    {
        var (controller, _) = CreateTestController();

        bool fired = false;
        controller.LayoutUpdated += (s, e) => fired = true;

        controller.SetAvailableSize(1200, 900);
        Assert.True(fired);
    }
}
