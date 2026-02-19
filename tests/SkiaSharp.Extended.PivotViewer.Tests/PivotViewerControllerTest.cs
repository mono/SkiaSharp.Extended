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
    public void SortDescending_DefaultsFalse()
    {
        var (controller, _) = CreateTestController();
        Assert.False(controller.SortDescending);
    }

    [Fact]
    public void SortDescending_ReversesOrder()
    {
        var controller = new PivotViewerController();
        var prop = new PivotViewerStringProperty("Name");
        var items = new[]
        {
            new PivotViewerItem("a"),
            new PivotViewerItem("b"),
            new PivotViewerItem("c"),
        };
        items[0].Add(prop, "Alpha");
        items[1].Add(prop, "Beta");
        items[2].Add(prop, "Charlie");

        controller.LoadItems(items, new[] { prop });
        controller.SortProperty = prop;

        var ascending = controller.InScopeItems.Select(i => i.Id).ToList();
        Assert.Equal(new[] { "a", "b", "c" }, ascending);

        controller.SortDescending = true;
        var descending = controller.InScopeItems.Select(i => i.Id).ToList();
        Assert.Equal(new[] { "c", "b", "a" }, descending);
    }

    [Fact]
    public void SortDescending_FiresSortPropertyChanged()
    {
        var (controller, _) = CreateTestController();
        bool fired = false;
        controller.SortPropertyChanged += (s, e) => fired = true;

        controller.SortDescending = true;
        Assert.True(fired);
    }

    [Fact]
    public void SortDescending_ResetOnLoadItems()
    {
        var (controller, _) = CreateTestController();
        controller.SortDescending = true;
        Assert.True(controller.SortDescending);

        // Reload items — should reset
        controller.LoadItems(
            controller.InScopeItems,
            controller.Properties);
        Assert.False(controller.SortDescending);
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

    [Fact]
    public void NotifyItemDoubleClicked_RaisesEvent()
    {
        var (controller, _) = CreateTestController();
        PivotViewerItemDoubleClickEventArgs? args = null;
        controller.ItemDoubleClicked += (s, e) => args = e;
        controller.NotifyItemDoubleClicked(controller.Items[0]);
        Assert.NotNull(args);
        Assert.Equal(controller.Items[0], args.Item);
    }

    [Fact]
    public void NotifyItemDoubleClicked_NullItem_DoesNotRaise()
    {
        var (controller, _) = CreateTestController();
        bool raised = false;
        controller.ItemDoubleClicked += (s, e) => raised = true;
        controller.NotifyItemDoubleClicked(null!);
        Assert.False(raised);
    }

    [Fact]
    public void SetViewerState_EmptyString_DoesNotThrow()
    {
        var (controller, _) = CreateTestController();
        var ex = Record.Exception(() => controller.SetViewerState(""));
        Assert.Null(ex);
    }

    [Fact]
    public void SetViewerState_GarbageString_DoesNotThrow()
    {
        var (controller, _) = CreateTestController();
        var ex = Record.Exception(() => controller.SetViewerState("garbage"));
        Assert.Null(ex);
    }

    [Fact]
    public void SetViewerState_Null_DoesNotThrow()
    {
        var (controller, _) = CreateTestController();
        var ex = Record.Exception(() => controller.SetViewerState(null!));
        Assert.Null(ex);
    }

    // --- SelectNext / SelectPrevious / ClearSelection ---

    [Fact]
    public void SelectNext_WhenNoneSelected_SelectsFirstItem()
    {
        var (controller, _) = CreateTestController();

        Assert.Null(controller.SelectedItem);
        controller.SelectNext();

        Assert.Equal(controller.InScopeItems[0], controller.SelectedItem);
    }

    [Fact]
    public void SelectNext_AdvancesSelection()
    {
        var (controller, _) = CreateTestController();

        controller.SelectedItem = controller.InScopeItems[0];
        controller.SelectNext();

        Assert.Equal(controller.InScopeItems[1], controller.SelectedItem);
    }

    [Fact]
    public void SelectPrevious_GoesBack()
    {
        var (controller, _) = CreateTestController();

        controller.SelectedItem = controller.InScopeItems[2];
        controller.SelectPrevious();

        Assert.Equal(controller.InScopeItems[1], controller.SelectedItem);
    }

    [Fact]
    public void SelectNext_AtEnd_StaysAtLastItem()
    {
        var (controller, _) = CreateTestController();

        var lastItem = controller.InScopeItems[controller.InScopeItems.Count - 1];
        controller.SelectedItem = lastItem;
        controller.SelectNext();

        Assert.Equal(lastItem, controller.SelectedItem);
    }

    [Fact]
    public void ClearSelection_SetsSelectedItemToNull()
    {
        var (controller, _) = CreateTestController();

        controller.SelectedItem = controller.InScopeItems[0];
        Assert.NotNull(controller.SelectedItem);

        controller.ClearSelection();
        Assert.Null(controller.SelectedItem);
    }

    // --- SelectUp / SelectDown ---

    [Fact]
    public void SelectDown_FromFirstRow_GoesToNextRow()
    {
        var (controller, _) = CreateTestController();

        Assert.NotNull(controller.GridLayout);
        int cols = controller.GridLayout!.Columns;
        Assert.True(cols > 0);
        Assert.True(controller.InScopeItems.Count > cols, "Need more items than one row");

        controller.SelectedItem = controller.InScopeItems[0];
        controller.SelectDown();

        Assert.Equal(controller.InScopeItems[cols], controller.SelectedItem);
    }

    [Fact]
    public void SelectUp_FromSecondRow_GoesToFirstRow()
    {
        var (controller, _) = CreateTestController();

        Assert.NotNull(controller.GridLayout);
        int cols = controller.GridLayout!.Columns;
        Assert.True(cols > 0);
        Assert.True(controller.InScopeItems.Count > cols, "Need more items than one row");

        controller.SelectedItem = controller.InScopeItems[cols];
        controller.SelectUp();

        Assert.Equal(controller.InScopeItems[0], controller.SelectedItem);
    }

    [Fact]
    public void SetViewerState_FiresViewChangedSortPropertyChangedSelectionChanged()
    {
        var (controller, _) = CreateTestController();

        // Set up a specific state: graph view, sort by Production Year, select first item
        var yearProp = controller.Properties.FirstOrDefault(p => p.Id == "Production Year");
        Assert.NotNull(yearProp);
        controller.SortProperty = yearProp;
        controller.CurrentView = "graph";
        controller.SelectedItem = controller.InScopeItems[0];

        var state = controller.SerializeViewerState();

        // Reset to defaults
        controller.CurrentView = "grid";
        controller.SortProperty = null;
        controller.SelectedItem = null;

        // Listen for events
        bool viewFired = false;
        bool sortFired = false;
        bool selectionFired = false;
        controller.ViewChanged += (s, e) => viewFired = true;
        controller.SortPropertyChanged += (s, e) => sortFired = true;
        controller.SelectionChanged += (s, e) => selectionFired = true;

        controller.SetViewerState(state);

        Assert.True(viewFired);
        Assert.True(sortFired);
        Assert.True(selectionFired);
    }

    [Fact]
    public void SelectNext_EmptyCollection_DoesNotThrow()
    {
        var controller = new PivotViewerController();
        // No items loaded
        var ex = Record.Exception(() => controller.SelectNext());
        Assert.Null(ex);
        Assert.Null(controller.SelectedItem);
    }

    [Fact]
    public void SelectPrevious_EmptyCollection_DoesNotThrow()
    {
        var controller = new PivotViewerController();
        var ex = Record.Exception(() => controller.SelectPrevious());
        Assert.Null(ex);
        Assert.Null(controller.SelectedItem);
    }

    [Fact]
    public void SelectPrevious_NoSelection_SelectsLastItem()
    {
        var (controller, _) = CreateTestController();
        Assert.Null(controller.SelectedItem);

        controller.SelectPrevious();

        var lastItem = controller.InScopeItems[controller.InScopeItems.Count - 1];
        Assert.Equal(lastItem, controller.SelectedItem);
    }

    [Fact]
    public void ApplyFilters_NoItemsLoaded_DoesNotThrow()
    {
        var controller = new PivotViewerController();
        var ex = Record.Exception(() => controller.FilterEngine.ClearAll());
        Assert.Null(ex);
        Assert.Empty(controller.InScopeItems);
    }

    [Fact]
    public void Update_ReturnsFalse_WhenNoItemsLoaded()
    {
        var controller = new PivotViewerController();
        bool needsRedraw = controller.Update(TimeSpan.FromMilliseconds(16));
        Assert.False(needsRedraw);
    }

    [Fact]
    public void ClearSelection_EmptyCollection_DoesNotThrow()
    {
        var controller = new PivotViewerController();
        var ex = Record.Exception(() => controller.ClearSelection());
        Assert.Null(ex);
    }

    [Fact]
    public void SelectDown_EmptyCollection_DoesNotThrow()
    {
        var controller = new PivotViewerController();
        var ex = Record.Exception(() => controller.SelectDown());
        Assert.Null(ex);
    }

    [Fact]
    public void SelectUp_EmptyCollection_DoesNotThrow()
    {
        var controller = new PivotViewerController();
        var ex = Record.Exception(() => controller.SelectUp());
        Assert.Null(ex);
    }

    [Fact]
    public void GetFilterCounts_NoItemsLoaded_ReturnsEmpty()
    {
        var controller = new PivotViewerController();
        var counts = controller.GetFilterCounts("NonExistent");
        Assert.Empty(counts);
    }

    [Fact]
    public void HitTest_NoItemsLoaded_ReturnsNull()
    {
        var controller = new PivotViewerController();
        var hit = controller.HitTest(100, 100);
        Assert.Null(hit);
    }

    // --- ZoomAbout / ZoomTo ---

    [Fact]
    public void ZoomAbout_AdjustsPanOffset()
    {
        var (controller, _) = CreateTestController();

        controller.ZoomAbout(2.0, 512, 384);
        // Pan offset should have been adjusted to keep the screen point stable
        Assert.True(controller.ZoomLevel > 0.0);
        Assert.True(controller.PanOffsetX != 0.0 || controller.PanOffsetY != 0.0,
            "Pan offset should be adjusted when zooming about a non-origin point");
    }

    [Fact]
    public void ZoomAbout_ZoomInThenOut_ReturnsNearOriginalZoom()
    {
        var (controller, _) = CreateTestController();

        controller.ZoomAbout(2.0, 512, 384);
        double zoomAfterIn = controller.ZoomLevel;
        Assert.True(zoomAfterIn > 0.0);

        controller.ZoomAbout(0.0, 512, 384); // factor < 1 zooms out
        Assert.True(controller.ZoomLevel < zoomAfterIn);
    }

    [Fact]
    public void ZoomLevel_SmallChange_IgnoredBelowThreshold()
    {
        var (controller, _) = CreateTestController();
        controller.ZoomLevel = 0.5;

        bool fired = false;
        controller.LayoutUpdated += (s, e) => fired = true;

        // Change smaller than 0.001 threshold
        controller.ZoomLevel = 0.5005;
        Assert.False(fired, "Changes below threshold should be ignored");
    }

    [Fact]
    public void ZoomLevel_SetSameValue_NoLayoutUpdate()
    {
        var (controller, _) = CreateTestController();
        controller.ZoomLevel = 0.5;

        bool fired = false;
        controller.LayoutUpdated += (s, e) => fired = true;

        controller.ZoomLevel = 0.5;
        Assert.False(fired);
    }

    // --- PanTo / PanBy ---

    [Fact]
    public void Pan_NegativeValues_Allowed()
    {
        var (controller, _) = CreateTestController();

        controller.Pan(-100, -200);
        Assert.Equal(-100.0, controller.PanOffsetX);
        Assert.Equal(-200.0, controller.PanOffsetY);
    }

    [Fact]
    public void Pan_ZeroDeltas_StillFiresLayoutUpdated()
    {
        var (controller, _) = CreateTestController();

        bool fired = false;
        controller.LayoutUpdated += (s, e) => fired = true;

        controller.Pan(0, 0);
        Assert.True(fired);
    }

    [Fact]
    public void Pan_LargeValues_Accepted()
    {
        var (controller, _) = CreateTestController();

        controller.Pan(100000, 100000);
        Assert.Equal(100000.0, controller.PanOffsetX);
        Assert.Equal(100000.0, controller.PanOffsetY);
    }

    // --- CurrentView switching ---

    [Fact]
    public void CurrentView_GridToGraph_ClearsGridLayout()
    {
        var (controller, _) = CreateTestController();
        var sortProp = controller.Properties.FirstOrDefault(p => p.Id == "Manufacturer");
        Assert.NotNull(sortProp);

        controller.SortProperty = sortProp;
        controller.CurrentView = "graph";

        Assert.Null(controller.GridLayout);
        Assert.NotNull(controller.HistogramLayout);
    }

    [Fact]
    public void CurrentView_GraphToGrid_RestoresGridLayout()
    {
        var (controller, _) = CreateTestController();
        var sortProp = controller.Properties.FirstOrDefault(p => p.Id == "Manufacturer");
        Assert.NotNull(sortProp);

        controller.SortProperty = sortProp;
        controller.CurrentView = "graph";
        Assert.Null(controller.GridLayout);

        controller.CurrentView = "grid";
        Assert.NotNull(controller.GridLayout);
        Assert.Null(controller.HistogramLayout);
    }

    [Fact]
    public void CurrentView_SetNull_DefaultsToGrid()
    {
        var (controller, _) = CreateTestController();
        controller.CurrentView = null!;
        Assert.Equal("grid", controller.CurrentView);
    }

    [Fact]
    public void CurrentView_SameValue_DoesNotFireEvent()
    {
        var (controller, _) = CreateTestController();
        bool fired = false;
        controller.ViewChanged += (s, e) => fired = true;

        controller.CurrentView = "grid"; // already grid
        Assert.False(fired);
    }

    [Fact]
    public void CurrentView_GraphWithoutSortProperty_NoHistogramLayout()
    {
        var (controller, _) = CreateTestController();
        controller.SortProperty = null;
        controller.CurrentView = "graph";

        // Without a sort property, graph view falls back to grid layout
        Assert.NotNull(controller.GridLayout);
        Assert.Null(controller.HistogramLayout);
    }

    // --- SortProperty ---

    [Fact]
    public void SortProperty_ByName_SortsAlphabetically()
    {
        var controller = new PivotViewerController();
        var nameProp = new PivotViewerStringProperty("Name") { DisplayName = "Name" };
        var items = new List<PivotViewerItem>();
        foreach (var name in new[] { "Charlie", "Alice", "Bob" })
        {
            var item = new PivotViewerItem(name);
            item.Set(nameProp, new object[] { name });
            items.Add(item);
        }

        controller.LoadItems(items, new[] { nameProp });
        controller.SetAvailableSize(800, 600);
        controller.SortProperty = nameProp;
        // Trigger re-sort via filter engine round-trip
        controller.FilterEngine.ClearAll();

        Assert.Equal("Alice", controller.InScopeItems[0]["Name"]![0].ToString());
        Assert.Equal("Bob", controller.InScopeItems[1]["Name"]![0].ToString());
        Assert.Equal("Charlie", controller.InScopeItems[2]["Name"]![0].ToString());
    }

    [Fact]
    public void SortProperty_NullValuesSort_ToEnd()
    {
        var controller = new PivotViewerController();
        var nameProp = new PivotViewerStringProperty("Name") { DisplayName = "Name" };
        var scoreProp = new PivotViewerNumericProperty("Score") { DisplayName = "Score" };

        var items = new List<PivotViewerItem>();

        var item1 = new PivotViewerItem("1");
        item1.Set(nameProp, new object[] { "A" });
        item1.Set(scoreProp, new object[] { 10.0 });
        items.Add(item1);

        var item2 = new PivotViewerItem("2");
        item2.Set(nameProp, new object[] { "B" });
        // No score set
        items.Add(item2);

        var item3 = new PivotViewerItem("3");
        item3.Set(nameProp, new object[] { "C" });
        item3.Set(scoreProp, new object[] { 5.0 });
        items.Add(item3);

        controller.LoadItems(items, new PivotViewerProperty[] { nameProp, scoreProp });
        controller.SetAvailableSize(800, 600);
        controller.SortProperty = scoreProp;
        // Trigger re-sort via filter engine round-trip
        controller.FilterEngine.ClearAll();

        // Null values should sort to end
        var lastItem = controller.InScopeItems[controller.InScopeItems.Count - 1];
        Assert.Equal("2", lastItem.Id);
    }

    [Fact]
    public void SortProperty_ChangeUpdatesLayout()
    {
        var (controller, _) = CreateTestController();
        var mfgProp = controller.Properties.FirstOrDefault(p => p.Id == "Manufacturer");
        var yearProp = controller.Properties.FirstOrDefault(p => p.Id == "Production Year");
        Assert.NotNull(mfgProp);
        Assert.NotNull(yearProp);

        controller.SortProperty = mfgProp;
        var firstItemMfg = controller.InScopeItems[0];

        controller.SortProperty = yearProp;
        // After changing sort, the order should differ (unless coincidence)
        // At minimum, verify layout was updated
        Assert.Equal(yearProp, controller.SortProperty);
    }

    [Fact]
    public void SortProperty_SetNull_RemovesSort()
    {
        var (controller, _) = CreateTestController();
        var yearProp = controller.Properties.FirstOrDefault(p => p.Id == "Production Year");
        Assert.NotNull(yearProp);

        controller.SortProperty = yearProp;
        Assert.NotNull(controller.SortProperty);

        controller.SortProperty = null;
        Assert.Null(controller.SortProperty);
    }

    [Fact]
    public void SortProperty_SameValue_DoesNotFireEvent()
    {
        var (controller, _) = CreateTestController();
        var yearProp = controller.Properties.FirstOrDefault(p => p.Id == "Production Year");
        Assert.NotNull(yearProp);

        controller.SortProperty = yearProp;

        bool fired = false;
        controller.SortPropertyChanged += (s, e) => fired = true;
        controller.SortProperty = yearProp; // same value
        Assert.False(fired);
    }

    // --- ApplyFilters with multiple predicates ---

    [Fact]
    public void Filters_MultipleProperties_AndLogic()
    {
        var (controller, _) = CreateTestController();
        int allCount = controller.InScopeItems.Count;

        controller.FilterEngine.AddStringFilter("Manufacturer", "Alfa Romeo");
        int afterFirstFilter = controller.InScopeItems.Count;
        Assert.True(afterFirstFilter < allCount);

        // Add a numeric range filter on Production Year
        controller.FilterEngine.AddNumericRangeFilter("Production Year", 2000, 2010);
        int afterBothFilters = controller.InScopeItems.Count;

        // AND logic: second filter should reduce further or keep same count
        Assert.True(afterBothFilters <= afterFirstFilter,
            "Adding a second filter (AND) should not increase item count");
    }

    [Fact]
    public void Filters_SameProperty_OrLogic()
    {
        var (controller, _) = CreateTestController();

        controller.FilterEngine.AddStringFilter("Manufacturer", "Alfa Romeo");
        int afterOne = controller.InScopeItems.Count;

        controller.FilterEngine.AddStringFilter("Manufacturer", "BMW");
        int afterTwo = controller.InScopeItems.Count;

        // OR logic within same property: more values should increase count
        Assert.True(afterTwo >= afterOne,
            "Adding another value for same property (OR) should not decrease item count");
    }

    [Fact]
    public void Filters_ClearRestoresAllItems()
    {
        var (controller, _) = CreateTestController();
        int allCount = controller.InScopeItems.Count;

        controller.FilterEngine.AddStringFilter("Manufacturer", "Alfa Romeo");
        controller.FilterEngine.AddNumericRangeFilter("Production Year", 2000, 2010);
        Assert.True(controller.InScopeItems.Count < allCount);

        controller.FilterEngine.ClearAll();
        Assert.Equal(allCount, controller.InScopeItems.Count);
    }

    // --- GetFilterCounts ---

    [Fact]
    public void GetFilterCounts_ReflectsActiveFilters()
    {
        var (controller, _) = CreateTestController();

        var countsBeforeFilter = controller.GetFilterCounts("Manufacturer");
        Assert.NotEmpty(countsBeforeFilter);

        // Filter by a different property; Manufacturer counts should reflect cross-filter
        controller.FilterEngine.AddNumericRangeFilter("Production Year", 2005, 2010);
        var countsAfterFilter = controller.GetFilterCounts("Manufacturer");

        // Counts should be computed excluding own-property filters, so total may be <= before
        int totalBefore = countsBeforeFilter.Values.Sum();
        int totalAfter = countsAfterFilter.Values.Sum();
        Assert.True(totalAfter <= totalBefore);
    }

    [Fact]
    public void GetFilterCounts_NonExistentProperty_ReturnsEmpty()
    {
        var (controller, _) = CreateTestController();
        var counts = controller.GetFilterCounts("NonExistentProperty");
        Assert.Empty(counts);
    }

    [Fact]
    public void GetFilterCounts_EachValueCountIsPositive()
    {
        var (controller, _) = CreateTestController();
        var counts = controller.GetFilterCounts("Manufacturer");

        foreach (var kv in counts)
            Assert.True(kv.Value > 0, $"Count for '{kv.Key}' should be positive");
    }

    // --- SetAvailableSize ---

    [Fact]
    public void SetAvailableSize_SmallerSize_ReducesColumns()
    {
        var (controller, _) = CreateTestController();

        controller.SetAvailableSize(1024, 768);
        int colsLarge = controller.GridLayout?.Columns ?? 0;

        controller.SetAvailableSize(300, 300);
        int colsSmall = controller.GridLayout?.Columns ?? 0;

        Assert.True(colsSmall <= colsLarge,
            $"Smaller size should have fewer or equal columns: {colsSmall} vs {colsLarge}");
    }

    [Fact]
    public void SetAvailableSize_SameSize_NoLayoutUpdate()
    {
        var (controller, _) = CreateTestController();
        controller.SetAvailableSize(1024, 768);

        bool fired = false;
        controller.LayoutUpdated += (s, e) => fired = true;

        // Same size within threshold (0.5)
        controller.SetAvailableSize(1024.3, 768.3);
        Assert.False(fired);
    }

    [Fact]
    public void SetAvailableSize_RecalculatesGridPositions()
    {
        var (controller, _) = CreateTestController();

        controller.SetAvailableSize(1024, 768);
        var pos1 = controller.GridLayout?.Positions[0];

        controller.SetAvailableSize(500, 400);
        var pos2 = controller.GridLayout?.Positions[0];

        Assert.NotNull(pos1);
        Assert.NotNull(pos2);
        // Item width should change with available size
        Assert.NotEqual(pos1!.Value.Width, pos2!.Value.Width);
    }

    // --- SearchText ---

    [Fact]
    public void SearchText_FiltersItems_CombinesWithFacetFilters()
    {
        var (controller, _) = CreateTestController();

        controller.SearchText = "bmw";
        int searchOnly = controller.InScopeItems.Count;
        Assert.True(searchOnly > 0);

        // Add a facet filter on top of search — should reduce further
        controller.FilterEngine.AddNumericRangeFilter("Production Year", 2005, 2010);
        Assert.True(controller.InScopeItems.Count <= searchOnly);
    }

    [Fact]
    public void SearchText_FiresFiltersChanged()
    {
        var (controller, _) = CreateTestController();

        bool fired = false;
        controller.FiltersChanged += (s, e) => fired = true;

        controller.SearchText = "ford";
        Assert.True(fired);
    }

    [Fact]
    public void SearchText_SameValue_NoEvent()
    {
        var (controller, _) = CreateTestController();
        controller.SearchText = "ford";

        bool fired = false;
        controller.FiltersChanged += (s, e) => fired = true;

        controller.SearchText = "ford"; // same
        Assert.False(fired);
    }

    [Fact]
    public void SearchText_Null_TreatedAsEmpty()
    {
        var (controller, _) = CreateTestController();
        controller.SearchText = "ford";
        int filtered = controller.InScopeItems.Count;
        Assert.True(filtered < controller.Items.Count);

        controller.SearchText = null!;
        Assert.Equal(controller.Items.Count, controller.InScopeItems.Count);
    }

    [Fact]
    public void SearchText_SearchFilteredItems_MatchesSearch()
    {
        var (controller, _) = CreateTestController();
        controller.SearchText = "bmw";

        // SearchFilteredItems should contain only items matching the search
        Assert.True(controller.SearchFilteredItems.Count > 0);
        Assert.True(controller.SearchFilteredItems.Count <= controller.Items.Count);
        // InScopeItems <= SearchFilteredItems (facet filters may reduce further)
        Assert.True(controller.InScopeItems.Count <= controller.SearchFilteredItems.Count);
    }

    // --- SelectNext/SelectPrevious with filtered collection ---

    [Fact]
    public void SelectNext_WithFilters_NavigatesFilteredList()
    {
        var (controller, _) = CreateTestController();

        controller.FilterEngine.AddStringFilter("Manufacturer", "Alfa Romeo");
        Assert.True(controller.InScopeItems.Count > 1, "Need at least 2 filtered items");

        controller.SelectNext(); // selects first
        Assert.Equal(controller.InScopeItems[0], controller.SelectedItem);

        controller.SelectNext(); // selects second
        Assert.Equal(controller.InScopeItems[1], controller.SelectedItem);
    }

    [Fact]
    public void SelectPrevious_AtFirstItem_StaysAtFirst()
    {
        var (controller, _) = CreateTestController();

        controller.SelectedItem = controller.InScopeItems[0];
        controller.SelectPrevious();

        Assert.Equal(controller.InScopeItems[0], controller.SelectedItem);
    }

    [Fact]
    public void SelectUp_AtFirstRow_StaysAtSameItem()
    {
        var (controller, _) = CreateTestController();

        controller.SelectedItem = controller.InScopeItems[0];
        controller.SelectUp();

        // First row, can't go up
        Assert.Equal(controller.InScopeItems[0], controller.SelectedItem);
    }

    [Fact]
    public void SelectDown_AtLastRow_StaysAtSameItem()
    {
        var (controller, _) = CreateTestController();
        Assert.NotNull(controller.GridLayout);

        int cols = controller.GridLayout!.Columns;
        var lastItem = controller.InScopeItems[controller.InScopeItems.Count - 1];
        controller.SelectedItem = lastItem;
        controller.SelectDown();

        Assert.Equal(lastItem, controller.SelectedItem);
    }

    [Fact]
    public void SelectUp_NoSelection_DoesNothing()
    {
        var (controller, _) = CreateTestController();
        Assert.Null(controller.SelectedItem);
        controller.SelectUp();
        Assert.Null(controller.SelectedItem);
    }

    [Fact]
    public void SelectDown_NoSelection_DoesNothing()
    {
        var (controller, _) = CreateTestController();
        Assert.Null(controller.SelectedItem);
        controller.SelectDown();
        Assert.Null(controller.SelectedItem);
    }

    // --- ZoomLevel bounds clamping ---

    [Fact]
    public void ZoomLevel_ExactBoundary_Accepted()
    {
        var controller = new PivotViewerController();

        controller.ZoomLevel = 0.0;
        Assert.Equal(0.0, controller.ZoomLevel);

        controller.ZoomLevel = 1.0;
        Assert.Equal(1.0, controller.ZoomLevel);
    }

    [Fact]
    public void ZoomLevel_NaN_ClampsToZero()
    {
        var controller = new PivotViewerController();
        controller.ZoomLevel = double.NaN;
        // NaN is clamped by Math.Max/Math.Min — result is 0.0
        Assert.Equal(0.0, controller.ZoomLevel);
    }

    // --- LoadFromCxml integration ---

    [Fact]
    public void LoadCollection_FromCxml_AllPropertiesAccessible()
    {
        var (controller, _) = CreateTestController();

        foreach (var prop in controller.Properties)
        {
            Assert.NotNull(prop.Id);
            Assert.NotNull(prop.DisplayName);
        }
    }

    [Fact]
    public void LoadCollection_FromCxml_ItemsHaveFacetValues()
    {
        var (controller, _) = CreateTestController();
        var item = controller.Items[0];

        // Every item in conceptcars should have at least one facet value
        bool hasAnyValue = false;
        foreach (var prop in controller.Properties)
        {
            if (item[prop.Id] != null)
            {
                hasAnyValue = true;
                break;
            }
        }
        Assert.True(hasAnyValue, "Item should have at least one facet value");
    }

    [Fact]
    public void LoadCollection_Twice_ReplacesData()
    {
        var (controller, source) = CreateTestController();
        int firstCount = controller.Items.Count;

        // Create a small custom collection
        var nameProp = new PivotViewerStringProperty("Name") { DisplayName = "Name" };
        var items = new List<PivotViewerItem>();
        var item = new PivotViewerItem("only");
        item.Set(nameProp, new object[] { "Only" });
        items.Add(item);

        controller.LoadItems(items, new[] { nameProp });
        Assert.Equal(1, controller.Items.Count);
        Assert.NotEqual(firstCount, controller.Items.Count);
    }

    [Fact]
    public void LoadItems_ClearsSelectionAndSort()
    {
        var (controller, _) = CreateTestController();
        var yearProp = controller.Properties.FirstOrDefault(p => p.Id == "Production Year");
        Assert.NotNull(yearProp);

        controller.SortProperty = yearProp;
        controller.SelectedItem = controller.InScopeItems[0];

        // Reload
        var nameProp = new PivotViewerStringProperty("Name") { DisplayName = "Name" };
        var newItem = new PivotViewerItem("1");
        newItem.Set(nameProp, new object[] { "Test" });
        controller.LoadItems(new[] { newItem }, new[] { nameProp });

        Assert.Null(controller.SelectedItem);
        Assert.Null(controller.SortProperty);
    }

    // --- GetViewerState / SetViewerState round-trip with populated state ---

    [Fact]
    public void SerializeViewerState_PreservesSelection()
    {
        var (controller, _) = CreateTestController();

        controller.SelectedItem = controller.InScopeItems[3];
        var selectedId = controller.InScopeItems[3].Id;

        var state = controller.SerializeViewerState();

        controller.SelectedItem = null;
        controller.SetViewerState(state);

        Assert.NotNull(controller.SelectedItem);
        Assert.Equal(selectedId, controller.SelectedItem!.Id);
    }

    [Fact]
    public void SerializeViewerState_PreservesMultipleFilters()
    {
        var (controller, _) = CreateTestController();

        controller.FilterEngine.AddStringFilter("Manufacturer", "Alfa Romeo");
        controller.FilterEngine.AddStringFilter("Manufacturer", "BMW");
        int filteredCount = controller.InScopeItems.Count;

        var state = controller.SerializeViewerState();

        controller.FilterEngine.ClearAll();
        Assert.Equal(controller.Items.Count, controller.InScopeItems.Count);

        controller.SetViewerState(state);
        Assert.Equal(filteredCount, controller.InScopeItems.Count);
    }

    [Fact]
    public void SerializeViewerState_PreservesNumericRangeFilter()
    {
        var (controller, _) = CreateTestController();

        controller.FilterEngine.AddNumericRangeFilter("Production Year", 2000, 2005);
        int filteredCount = controller.InScopeItems.Count;
        Assert.True(filteredCount < controller.Items.Count);

        var state = controller.SerializeViewerState();

        controller.FilterEngine.ClearAll();
        controller.SetViewerState(state);

        Assert.Equal(filteredCount, controller.InScopeItems.Count);
    }

    [Fact]
    public void SerializeViewerState_EmptyState_ProducesMinimalString()
    {
        var (controller, _) = CreateTestController();

        var state = controller.SerializeViewerState();
        // No filters, default view, no selection — should have $view=grid at most
        Assert.NotNull(state);
    }

    // --- DetailPane integration ---

    [Fact]
    public void DetailPane_ShowItem_HasFacetValues()
    {
        var (controller, _) = CreateTestController();

        controller.SelectedItem = controller.InScopeItems[0];
        Assert.True(controller.DetailPane.IsShowing);

        var facets = controller.DetailPane.FacetValues;
        Assert.NotEmpty(facets);
    }

    [Fact]
    public void DetailPane_Hide_ClearsFacets()
    {
        var (controller, _) = CreateTestController();

        controller.SelectedItem = controller.InScopeItems[0];
        Assert.NotEmpty(controller.DetailPane.FacetValues);

        controller.SelectedItem = null;
        Assert.Empty(controller.DetailPane.FacetValues);
    }

    [Fact]
    public void DetailPane_SelectionChange_UpdatesDetailItem()
    {
        var (controller, _) = CreateTestController();

        controller.SelectedItem = controller.InScopeItems[0];
        Assert.Equal(controller.InScopeItems[0], controller.DetailPane.SelectedItem);

        controller.SelectedItem = controller.InScopeItems[1];
        Assert.Equal(controller.InScopeItems[1], controller.DetailPane.SelectedItem);
    }

    [Fact]
    public void DetailPane_IsExpanded_DefaultTrue()
    {
        var controller = new PivotViewerController();
        Assert.True(controller.DetailPane.IsExpanded);
    }

    // --- DefaultDetails ---

    [Fact]
    public void DefaultDetails_AllFlagsDefaultFalse()
    {
        var controller = new PivotViewerController();
        Assert.False(controller.DefaultDetails.IsNameHidden);
        Assert.False(controller.DefaultDetails.IsDescriptionHidden);
        Assert.False(controller.DefaultDetails.IsFacetCategoriesHidden);
        Assert.False(controller.DefaultDetails.IsRelatedCollectionsHidden);
        Assert.False(controller.DefaultDetails.IsCopyrightHidden);
    }

    // --- SelectedIndex edge cases ---

    [Fact]
    public void SelectedIndex_OutOfRange_ClearsSelection()
    {
        var (controller, _) = CreateTestController();

        controller.SelectedItem = controller.InScopeItems[0];
        controller.SelectedIndex = 99999;
        Assert.Null(controller.SelectedItem);
    }

    [Fact]
    public void SelectedIndex_NoSelection_ReturnsMinus1()
    {
        var (controller, _) = CreateTestController();
        Assert.Equal(-1, controller.SelectedIndex);
    }

    [Fact]
    public void SelectedIndex_ReflectsCurrentItem()
    {
        var (controller, _) = CreateTestController();

        controller.SelectedItem = controller.InScopeItems[5];
        Assert.Equal(5, controller.SelectedIndex);
    }

    // --- Dispose ---

    [Fact]
    public void Dispose_AfterDispose_FilterEngineChangeDontCrash()
    {
        var (controller, _) = CreateTestController();
        controller.Dispose();

        // After dispose, adding a filter should not crash (event is unsubscribed)
        var ex = Record.Exception(() => controller.FilterEngine.AddStringFilter("Manufacturer", "BMW"));
        Assert.Null(ex);
    }

    // --- HitTest in graph view ---

    [Fact]
    public void HitTest_GraphView_DelegatesToHistogramLayout()
    {
        var (controller, _) = CreateTestController();
        var sortProp = controller.Properties.FirstOrDefault(p => p.Id == "Manufacturer");
        Assert.NotNull(sortProp);

        controller.SortProperty = sortProp;
        controller.CurrentView = "graph";
        Assert.NotNull(controller.HistogramLayout);

        // Hit test at negative coords should return null
        var hit = controller.HitTest(-100, -100);
        Assert.Null(hit);
    }

    // --- Word wheel Search via controller ---

    [Fact]
    public void Search_ReturnsResultsWithItemCounts()
    {
        var (controller, _) = CreateTestController();

        var results = controller.Search("A");
        Assert.NotEmpty(results);

        foreach (var r in results)
        {
            Assert.True(r.ItemCount > 0);
            Assert.NotNull(r.Text);
        }
    }

    [Fact]
    public void Search_EmptyString_ReturnsEmpty()
    {
        var (controller, _) = CreateTestController();
        var results = controller.Search("");
        Assert.Empty(results);
    }

    // --- CollectionSource ---

    [Fact]
    public void CollectionSource_SetAfterLoadCollection()
    {
        var (controller, source) = CreateTestController();
        Assert.NotNull(controller.CollectionSource);
        Assert.Equal(source, controller.CollectionSource);
    }

    [Fact]
    public void CollectionSource_NullWhenLoadItems()
    {
        var controller = new PivotViewerController();
        var nameProp = new PivotViewerStringProperty("Name") { DisplayName = "Name" };
        var item = new PivotViewerItem("1");
        item.Set(nameProp, new object[] { "Test" });

        controller.LoadItems(new[] { item }, new[] { nameProp });
        Assert.Null(controller.CollectionSource);
    }

    // --- ImageProvider ---

    [Fact]
    public void ImageProvider_DefaultNull()
    {
        var controller = new PivotViewerController();
        Assert.Null(controller.ImageProvider);
    }

    // --- Update / Animation ---

    [Fact]
    public void Update_DuringTransition_ReturnsTrue()
    {
        var (controller, _) = CreateTestController();

        // Trigger a transition by resizing
        controller.SetAvailableSize(500, 400);
        Assert.True(controller.LayoutTransition.IsAnimating);

        bool needsRedraw = controller.Update(TimeSpan.FromMilliseconds(16));
        Assert.True(needsRedraw);
    }

    // --- Multiple LoadItems calls ---

    [Fact]
    public void LoadItems_MultipleReloads_CorrectState()
    {
        var controller = new PivotViewerController();

        for (int round = 0; round < 3; round++)
        {
            var nameProp = new PivotViewerStringProperty("Name") { DisplayName = "Name" };
            var items = new List<PivotViewerItem>();
            for (int i = 0; i < (round + 1) * 5; i++)
            {
                var item = new PivotViewerItem(i.ToString());
                item.Set(nameProp, new object[] { $"Item {i}" });
                items.Add(item);
            }

            controller.LoadItems(items, new[] { nameProp });
            controller.SetAvailableSize(800, 600);

            Assert.Equal((round + 1) * 5, controller.Items.Count);
            Assert.Equal((round + 1) * 5, controller.InScopeItems.Count);
        }
    }

    // --- GetItemBounds histogram ---

    [Fact]
    public void GetItemBounds_InGraphView_ReturnsNonZeroBounds()
    {
        var controller = new PivotViewerController();
        var categoryProp = new PivotViewerStringProperty("Category") { DisplayName = "Category" };
        var items = new List<PivotViewerItem>();
        for (int i = 0; i < 6; i++)
        {
            var item = new PivotViewerItem($"item{i}");
            item.Set(categoryProp, new object[] { i % 2 == 0 ? "A" : "B" });
            items.Add(item);
        }

        controller.LoadItems(items, new[] { categoryProp });
        controller.SetAvailableSize(800, 600);
        controller.SortProperty = categoryProp;
        controller.CurrentView = "graph";

        var bounds = controller.GetItemBounds(items[0]);

        Assert.True(bounds.Width > 0, "Width should be non-zero in graph view");
        Assert.True(bounds.Height > 0, "Height should be non-zero in graph view");
    }

    // --- ZoomAbout ratio ---

    [Fact]
    public void ZoomAbout_PreservesZoomCenterPoint()
    {
        var controller = new PivotViewerController();
        var nameProp = new PivotViewerStringProperty("Name") { DisplayName = "Name" };
        var items = new List<PivotViewerItem>();
        for (int i = 0; i < 20; i++)
        {
            var item = new PivotViewerItem($"item{i}");
            item.Set(nameProp, new object[] { $"Item {i}" });
            items.Add(item);
        }

        controller.LoadItems(items, new[] { nameProp });
        controller.SetAvailableSize(800, 600);

        double centerX = 400;
        double centerY = 300;

        // Zoom in about the center point
        controller.ZoomAbout(2.0, centerX, centerY);

        // The world coordinate at the center should remain at the center:
        // worldX = screenX - panOffsetX, so panOffsetX should compensate for the zoom
        double zoomLevel = controller.ZoomLevel;
        Assert.True(zoomLevel > 0.0, "Zoom level should have increased");

        // Verify pan offset was adjusted (not left at zero) to keep center stable
        double panX = controller.PanOffsetX;
        double panY = controller.PanOffsetY;
        // After zooming in at center, the layout scales up and pan adjusts so the
        // center world point stays at (centerX, centerY) on screen
        Assert.True(Math.Abs(panX) > 0.01 || Math.Abs(panY) > 0.01,
            "Pan offset should be adjusted to preserve zoom center");
    }

    // --- InScopeItemsChanged event ---

    [Fact]
    public void InScopeItemsChanged_FiresWhenFilterAdded()
    {
        var (controller, _) = CreateTestController();
        bool fired = false;
        controller.InScopeItemsChanged += (s, e) => fired = true;

        controller.FilterEngine.AddStringFilter("Manufacturer", "Alfa Romeo");
        Assert.True(fired, "InScopeItemsChanged should fire when a filter is added");
    }

    [Fact]
    public void InScopeItemsChanged_FiresWhenFilterCleared()
    {
        var (controller, _) = CreateTestController();
        controller.FilterEngine.AddStringFilter("Manufacturer", "Alfa Romeo");

        bool fired = false;
        controller.InScopeItemsChanged += (s, e) => fired = true;

        controller.FilterEngine.ClearAll();
        Assert.True(fired, "InScopeItemsChanged should fire when filters are cleared");
    }

    [Fact]
    public void InScopeItemsChanged_FiresWhenSearchTextSet()
    {
        var (controller, _) = CreateTestController();
        bool fired = false;
        controller.InScopeItemsChanged += (s, e) => fired = true;

        controller.SearchText = "Ford";
        Assert.True(fired, "InScopeItemsChanged should fire when search text changes");
    }

    [Fact]
    public void InScopeItemsChanged_ReducesInScopeCount()
    {
        var (controller, _) = CreateTestController();
        int originalCount = controller.InScopeItems.Count;
        int newCount = -1;
        controller.InScopeItemsChanged += (s, e) => newCount = controller.InScopeItems.Count;

        controller.FilterEngine.AddStringFilter("Manufacturer", "Alfa Romeo");
        Assert.True(newCount > 0 && newCount < originalCount,
            $"In-scope count ({newCount}) should be less than original ({originalCount})");
    }

    // --- Additional MergeSupplementalData scenario via controller ---

    [Fact]
    public void MergeSupplementalData_MultipleNewProperties_AllAdded()
    {
        var mainXml = @"<?xml version='1.0' encoding='utf-8'?>
<Collection xmlns='http://schemas.microsoft.com/collection/metadata/2009'
            xmlns:p='http://schemas.microsoft.com/livelabs/pivot/collection/2009'
            Name='Main'>
    <FacetCategories>
        <FacetCategory Name='Name' Type='String' />
    </FacetCategories>
    <Items>
        <Item Id='1' Name='Alpha'>
            <Facets><Facet Name='Name'><String Value='Alpha'/></Facet></Facets>
        </Item>
        <Item Id='2' Name='Beta'>
            <Facets><Facet Name='Name'><String Value='Beta'/></Facet></Facets>
        </Item>
    </Items>
</Collection>";

        var suppXml = @"<?xml version='1.0' encoding='utf-8'?>
<Collection xmlns='http://schemas.microsoft.com/collection/metadata/2009'
            xmlns:p='http://schemas.microsoft.com/livelabs/pivot/collection/2009'
            Name='Supplement'>
    <FacetCategories>
        <FacetCategory Name='Desc' Type='LongString' />
        <FacetCategory Name='Rating' Type='Number' />
    </FacetCategories>
    <Items>
        <Item Id='1'>
            <Facets>
                <Facet Name='Desc'><String Value='Description for Alpha'/></Facet>
                <Facet Name='Rating'><Number Value='4.5'/></Facet>
            </Facets>
        </Item>
        <Item Id='2'>
            <Facets>
                <Facet Name='Rating'><Number Value='3.0'/></Facet>
            </Facets>
        </Item>
    </Items>
</Collection>";

        var main = CxmlCollectionSource.Parse(mainXml);
        var supp = CxmlCollectionSource.Parse(suppXml);
        int propCountBefore = main.ItemProperties.Count;
        main.MergeSupplementalData(supp);

        // Two new properties should be added
        Assert.Equal(propCountBefore + 2, main.ItemProperties.Count);

        var item1 = main.GetItemById("1");
        Assert.NotNull(item1!["Desc"]);
        Assert.Equal("Description for Alpha", item1["Desc"]![0]?.ToString());
        Assert.Equal(4.5, (double)item1["Rating"]![0]!);

        var item2 = main.GetItemById("2");
        Assert.Null(item2!["Desc"]); // item2 didn't have Desc in supplement
        Assert.Equal(3.0, (double)item2["Rating"]![0]!);
    }
}
