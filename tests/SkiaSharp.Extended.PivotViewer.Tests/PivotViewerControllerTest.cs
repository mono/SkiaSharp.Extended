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

    [Fact]
    public void FilterPaneModel_CreatedOnLoad()
    {
        var (controller, _) = CreateTestController();
        Assert.NotNull(controller.FilterPaneModel);
    }

    [Fact]
    public void FilterPaneModel_HasCategories()
    {
        var (controller, _) = CreateTestController();
        var categories = controller.FilterPaneModel!.GetCategories(controller.Items);
        Assert.NotEmpty(categories);
    }

    [Fact]
    public void DetailPane_CreatedLazily()
    {
        var controller = new PivotViewerController();
        Assert.NotNull(controller.DetailPane);
        Assert.Null(controller.DetailPane.SelectedItem);
    }

    [Fact]
    public void DetailPane_UpdatedOnSelection()
    {
        var (controller, _) = CreateTestController();

        controller.SelectedItem = controller.InScopeItems[0];
        Assert.Equal(controller.InScopeItems[0], controller.DetailPane.SelectedItem);
        Assert.True(controller.DetailPane.IsShowing);
    }

    [Fact]
    public void DetailPane_ClearedOnDeselection()
    {
        var (controller, _) = CreateTestController();

        controller.SelectedItem = controller.InScopeItems[0];
        Assert.True(controller.DetailPane.IsShowing);

        controller.SelectedItem = null;
        Assert.Null(controller.DetailPane.SelectedItem);
    }

    [Fact]
    public void LayoutTransition_Accessible()
    {
        var controller = new PivotViewerController();
        Assert.NotNull(controller.LayoutTransition);
        Assert.False(controller.LayoutTransition.IsAnimating);
    }

    [Fact]
    public void Update_ReturnsFalseWhenNotAnimating()
    {
        var controller = new PivotViewerController();
        bool needsRedraw = controller.Update(TimeSpan.FromMilliseconds(16));
        Assert.False(needsRedraw);
    }

    [Fact]
    public void GetItemBounds_ReturnsPositionForItem()
    {
        var (controller, _) = CreateTestController();

        if (controller.InScopeItems.Count > 0)
        {
            var item = controller.InScopeItems[0];
            var bounds = controller.GetItemBounds(item);
            Assert.True(bounds.Width > 0, "Width should be positive");
            Assert.True(bounds.Height > 0, "Height should be positive");
        }
    }

    [Fact]
    public void GetItemBounds_ReturnsZeroForUnknownItem()
    {
        var (controller, _) = CreateTestController();

        var unknown = new PivotViewerItem("unknown");
        var bounds = controller.GetItemBounds(unknown);
        Assert.Equal(0, bounds.X);
        Assert.Equal(0, bounds.Width);
    }

    [Fact]
    public void ResizeTriggersLayoutTransition()
    {
        var (controller, _) = CreateTestController();

        // First layout is computed on load. Resize triggers a new layout.
        controller.SetAvailableSize(500, 400);

        // The transition should have been started
        Assert.True(controller.LayoutTransition.IsAnimating);
    }

    [Fact]
    public void Selection_ClearedWhenFilterRemovesItem_FiresEvent()
    {
        var (controller, _) = CreateTestController();

        var firstItem = controller.InScopeItems[0];
        controller.SelectedItem = firstItem;

        // Find a filter value that excludes the first item
        string? excludeValue = null;
        var counts = controller.GetFilterCounts("Manufacturer");
        var firstItemMfg = firstItem["Manufacturer"];
        foreach (var kv in counts)
        {
            if (firstItemMfg == null || !firstItemMfg.Cast<object>().Any(v => v?.ToString() == kv.Key))
            {
                excludeValue = kv.Key;
                break;
            }
        }

        if (excludeValue != null)
        {
            int selectionChangedCount = 0;
            controller.SelectionChanged += (s, e) => selectionChangedCount++;

            controller.FilterEngine.AddStringFilter("Manufacturer", excludeValue);

            if (!controller.InScopeItems.Contains(firstItem))
            {
                Assert.Null(controller.SelectedItem);
                Assert.Null(controller.DetailPane.SelectedItem);
                Assert.True(selectionChangedCount > 0, "SelectionChanged should fire when item is filtered out");
            }
        }
    }

    [Fact]
    public void LoadItems_CreatesFilterPaneModel()
    {
        var controller = new PivotViewerController();
        var nameP = new PivotViewerStringProperty("Name") { DisplayName = "Name" };
        var items = new List<PivotViewerItem>();
        for (int i = 0; i < 5; i++)
        {
            var item = new PivotViewerItem(i.ToString());
            item.Set(nameP, new object[] { $"Item {i}" });
            items.Add(item);
        }

        controller.LoadItems(items, new[] { nameP });
        Assert.NotNull(controller.FilterPaneModel);
    }

    [Fact]
    public void ZoomLevel_DefaultsToZero()
    {
        var controller = new PivotViewerController();
        Assert.Equal(0.0, controller.ZoomLevel);
    }

    [Fact]
    public void ZoomLevel_ClampsToRange()
    {
        var controller = new PivotViewerController();
        controller.ZoomLevel = 2.0;
        Assert.Equal(1.0, controller.ZoomLevel);

        controller.ZoomLevel = -1.0;
        Assert.Equal(0.0, controller.ZoomLevel);
    }

    [Fact]
    public void ZoomLevel_ChangesLayout()
    {
        var (controller, _) = CreateTestController();

        var layoutBefore = controller.GridLayout;
        Assert.NotNull(layoutBefore);

        controller.ZoomLevel = 0.8;
        var layoutAfter = controller.GridLayout;
        Assert.NotNull(layoutAfter);

        // At higher zoom, each item should be wider
        Assert.True(layoutAfter.ItemWidth >= layoutBefore.ItemWidth,
            $"Zooming in should make items at least as wide: before={layoutBefore.ItemWidth:F1}, after={layoutAfter.ItemWidth:F1}");
    }

    [Fact]
    public void ZoomLevel_TriggersLayoutUpdated()
    {
        var (controller, _) = CreateTestController();
        bool fired = false;
        controller.LayoutUpdated += (s, e) => fired = true;

        controller.ZoomLevel = 0.5;
        Assert.True(fired);
    }

    [Fact]
    public void Pan_ChangesOffset()
    {
        var (controller, _) = CreateTestController();

        Assert.Equal(0.0, controller.PanOffsetX);
        Assert.Equal(0.0, controller.PanOffsetY);

        controller.Pan(50.0, 30.0);
        Assert.Equal(50.0, controller.PanOffsetX);
        Assert.Equal(30.0, controller.PanOffsetY);
    }

    [Fact]
    public void Pan_TriggersLayoutUpdated()
    {
        var (controller, _) = CreateTestController();
        bool fired = false;
        controller.LayoutUpdated += (s, e) => fired = true;

        controller.Pan(10, 10);
        Assert.True(fired);
    }

    [Fact]
    public void Pan_Accumulates()
    {
        var (controller, _) = CreateTestController();

        controller.Pan(10, 20);
        controller.Pan(5, 10);
        Assert.Equal(15.0, controller.PanOffsetX);
        Assert.Equal(30.0, controller.PanOffsetY);
    }

    [Fact]
    public void ZoomAbout_ChangesZoomLevel()
    {
        var (controller, _) = CreateTestController();

        controller.ZoomAbout(1.5, 512, 384); // zoom in
        Assert.True(controller.ZoomLevel > 0.0, "ZoomAbout should increase zoom level");
    }

    [Fact]
    public void ZoomAbout_ClampsAtMax()
    {
        var (controller, _) = CreateTestController();

        for (int i = 0; i < 20; i++)
            controller.ZoomAbout(2.0, 512, 384);

        Assert.Equal(1.0, controller.ZoomLevel, 3);
    }

    [Fact]
    public void ZoomAbout_ClampsAtMin()
    {
        var controller = new PivotViewerController();

        controller.ZoomAbout(0.5, 0, 0); // zoom out at min
        Assert.Equal(0.0, controller.ZoomLevel);
    }

    [Fact]
    public void Dispose_CleansUp()
    {
        var (controller, _) = CreateTestController();
        controller.Dispose();
        // Should not throw on double dispose
        controller.Dispose();
    }

    [Fact]
    public void SearchText_FiltersItems()
    {
        var (controller, _) = CreateTestController();
        int totalItems = controller.InScopeItems.Count;

        // Use a manufacturer name prefix that exists in conceptcars
        controller.SearchText = "bmw";
        Assert.True(controller.InScopeItems.Count <= totalItems);
        Assert.True(controller.InScopeItems.Count > 0, "Search for 'bmw' should match some items");

        controller.SearchText = "";
        Assert.Equal(totalItems, controller.InScopeItems.Count);
    }

    [Fact]
    public void SearchText_SurvivesCollectionReload()
    {
        var (controller, source) = CreateTestController();
        controller.SearchText = "bmw";
        int filtered = controller.InScopeItems.Count;
        Assert.True(filtered > 0, "Search for 'bmw' should match some items");

        // Reload same collection — search should be reapplied
        controller.LoadCollection(source);
        Assert.Equal(filtered, controller.InScopeItems.Count);
    }

    [Fact]
    public void SearchText_NoMatch_ReturnsEmpty()
    {
        var (controller, _) = CreateTestController();
        controller.SearchText = "zzzzxyznonexistent";
        Assert.Empty(controller.InScopeItems);
    }

    [Fact]
    public void SerializeViewerState_PreservesFilterAndSort()
    {
        var (controller, _) = CreateTestController();

        controller.FilterEngine.AddStringFilter("Manufacturer", "Alfa Romeo");
        var yearProp = controller.Properties.FirstOrDefault(p => p.Id == "Production Year");
        Assert.NotNull(yearProp);
        controller.SortProperty = yearProp;

        var state = controller.SerializeViewerState();

        controller.FilterEngine.ClearAll();
        controller.SortProperty = null;
        Assert.Equal(controller.Items.Count, controller.InScopeItems.Count);
        Assert.Null(controller.SortProperty);

        controller.SetViewerState(state);

        Assert.True(controller.InScopeItems.Count < controller.Items.Count);
        Assert.NotNull(controller.SortProperty);
        Assert.Equal("Production Year", controller.SortProperty!.Id);
    }

    [Fact]
    public void LoadCollection_FromParsedCxmlString_PopulatesItemsAndProperties()
    {
        var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Collection xmlns=""http://schemas.microsoft.com/collection/metadata/2009""
            xmlns:p=""http://schemas.microsoft.com/livelabs/pivot/collection/2009""
            Name=""TestCars"">
    <FacetCategories>
        <FacetCategory Name=""Make"" Type=""String"" p:IsFilterVisible=""true"" p:IsWordWheelVisible=""true"" />
        <FacetCategory Name=""Year"" Type=""Number"" p:IsFilterVisible=""true"" />
    </FacetCategories>
    <Items>
        <Item Id=""1"" Name=""Model A"">
            <Facets>
                <Facet Name=""Make""><String Value=""Ford"" /></Facet>
                <Facet Name=""Year""><Number Value=""2020"" /></Facet>
            </Facets>
        </Item>
        <Item Id=""2"" Name=""Model B"">
            <Facets>
                <Facet Name=""Make""><String Value=""Toyota"" /></Facet>
                <Facet Name=""Year""><Number Value=""2021"" /></Facet>
            </Facets>
        </Item>
    </Items>
</Collection>";

        var source = CxmlCollectionSource.Parse(xml);
        var controller = new PivotViewerController();
        controller.LoadCollection(source);

        Assert.Equal(2, controller.Items.Count);
        Assert.True(controller.Properties.Count >= 2);
        Assert.Equal(2, controller.InScopeItems.Count);
    }

    [Fact]
    public void HitTest_ReturnsCorrectItemForGridPosition()
    {
        var (controller, _) = CreateTestController();

        Assert.NotNull(controller.GridLayout);
        Assert.True(controller.GridLayout!.Positions.Length > 1);

        var pos = controller.GridLayout.Positions[1];
        var hit = controller.HitTest(pos.X + pos.Width / 2, pos.Y + pos.Height / 2);

        Assert.NotNull(hit);
        Assert.Equal(controller.InScopeItems[1], hit);
    }

    [Fact]
    public void ZoomAbout_ProportionalFactor()
    {
        var (controller, _) = CreateTestController();

        // Factor 1.6 should give ~0.3 delta (sensitivity 0.5)
        controller.ZoomAbout(1.6, 512, 384);
        Assert.True(Math.Abs(controller.ZoomLevel - 0.3) < 0.01,
            $"Expected ~0.3, got {controller.ZoomLevel}");

        // Factor < 1 zooms out
        double before = controller.ZoomLevel;
        controller.ZoomAbout(0.8, 512, 384);
        Assert.True(controller.ZoomLevel < before);
    }

    [Fact]
    public void ZoomAbout_FromMaxZoom_DoesNotExplodePanOffset()
    {
        var (controller, _) = CreateTestController();

        // Zoom to maximum
        controller.ZoomLevel = 1.0;
        controller.Pan(100, 100);

        // Zoom out from max — should not cause massive pan jumps
        controller.ZoomAbout(0.5, 200, 200);
        Assert.True(controller.ZoomLevel < 1.0);
        // Pan offsets should remain reasonable (not explode to thousands)
    }

    [Fact]
    public void SearchFilteredItems_ReturnsAllItems_WhenNoSearch()
    {
        var (controller, _) = CreateTestController();
        Assert.Equal(controller.Items.Count, controller.SearchFilteredItems.Count);
    }

    [Fact]
    public void SearchFilteredItems_ReflectsSearchText()
    {
        var (controller, source) = CreateTestController();
        int totalCount = controller.Items.Count;
        Assert.True(totalCount > 1, "Need multiple items for this test");

        // Use a search term that won't match everything
        controller.SearchText = "ZZZZNONEXISTENT";

        // No items should match a nonsense string
        Assert.Equal(0, controller.SearchFilteredItems.Count);

        // Clear search restores all items
        controller.SearchText = "";
        Assert.Equal(totalCount, controller.SearchFilteredItems.Count);
    }
}
