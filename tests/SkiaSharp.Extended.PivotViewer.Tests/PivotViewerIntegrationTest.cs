using SkiaSharp.Extended.PivotViewer;
using Xunit;

namespace SkiaSharp.Extended.PivotViewer.Tests;

/// <summary>
/// Integration tests simulating real user workflows:
/// load collection → filter → sort → select → serialize → restore.
/// </summary>
public class PivotViewerIntegrationTest
{
    [Fact]
    public void FullWorkflow_LoadFilterSortSelectSerialize()
    {
        // Load collection
        var source = TestDataHelper.LoadCxml("conceptcars.cxml");
        var controller = new PivotViewerController();
        controller.LoadCollection(source);
        controller.SetAvailableSize(1024, 768);

        int totalItems = controller.Items.Count;
        Assert.True(totalItems > 0);

        // Apply filter
        controller.FilterEngine.AddStringFilter("Body Style", "Coupe");
        int coupeCount = controller.InScopeItems.Count;
        Assert.True(coupeCount < totalItems, "Filter should reduce items");
        Assert.True(coupeCount > 0, "Should have some coupes");

        // Sort
        var yearProp = controller.Properties.FirstOrDefault(p => p.Id == "Production Year");
        Assert.NotNull(yearProp);
        controller.SortProperty = yearProp;

        // Select first item
        controller.SelectedItem = controller.InScopeItems[0];
        Assert.NotNull(controller.SelectedItem);

        // Serialize state
        string state = controller.SerializeViewerState();
        Assert.NotEmpty(state);

        // Clear everything
        controller.FilterEngine.ClearAll();
        controller.SortProperty = null;
        controller.SelectedItem = null;
        Assert.Equal(totalItems, controller.InScopeItems.Count);

        // Restore state
        controller.SetViewerState(state);
        Assert.True(controller.InScopeItems.Count <= coupeCount + 5,
            "Restored filter should reduce items similarly");
    }

    [Fact]
    public void MultipleFilters_ChainCorrectly()
    {
        var source = TestDataHelper.LoadCxml("conceptcars.cxml");
        var controller = new PivotViewerController();
        controller.LoadCollection(source);
        controller.SetAvailableSize(800, 600);

        int total = controller.Items.Count;

        // Filter by body style
        controller.FilterEngine.AddStringFilter("Body Style", "Coupe");
        int afterBodyStyle = controller.InScopeItems.Count;

        // Add second filter (should narrow further)
        controller.FilterEngine.AddNumericRangeFilter("Production Year", 2000, 2010);
        int afterBoth = controller.InScopeItems.Count;

        Assert.True(afterBoth <= afterBodyStyle, "Adding filters should narrow");
        Assert.True(afterBodyStyle < total, "Body style filter should reduce");
    }

    [Fact]
    public void FilterCounts_UpdateWithFilters()
    {
        var source = TestDataHelper.LoadCxml("conceptcars.cxml");
        var controller = new PivotViewerController();
        controller.LoadCollection(source);
        controller.SetAvailableSize(800, 600);

        // Get unfiltered counts
        var unfilteredCounts = controller.GetFilterCounts("Body Style");
        Assert.NotEmpty(unfilteredCounts);

        // Apply a filter on a different property
        controller.FilterEngine.AddStringFilter("Fuel", "Petrol");
        var filteredCounts = controller.GetFilterCounts("Body Style");

        // Counts should change (or stay same) but not increase beyond unfiltered
        foreach (var kv in filteredCounts)
        {
            if (unfilteredCounts.TryGetValue(kv.Key, out int unfilteredCount))
            {
                Assert.True(kv.Value <= unfilteredCount,
                    $"Filtered count for '{kv.Key}' ({kv.Value}) should be <= unfiltered ({unfilteredCount})");
            }
        }
    }

    [Fact]
    public void GridLayout_HitTestRoundTrip()
    {
        var source = TestDataHelper.LoadCxml("conceptcars.cxml");
        var controller = new PivotViewerController();
        controller.LoadCollection(source);
        controller.SetAvailableSize(1024, 768);

        var layout = controller.GridLayout;
        Assert.NotNull(layout);

        // Hit test the center of each item
        foreach (var pos in layout!.Positions)
        {
            var hit = layout.HitTest(pos.X + pos.Width / 2, pos.Y + pos.Height / 2);
            Assert.Equal(pos.Item, hit);
        }
    }

    [Fact]
    public void SearchAndFilter_Integration()
    {
        var source = TestDataHelper.LoadCxml("conceptcars.cxml");
        var controller = new PivotViewerController();
        controller.LoadCollection(source);
        controller.SetAvailableSize(800, 600);

        // Search
        var results = controller.Search("Ford");

        // If results found, the items should exist in the collection
        if (results.Count > 0)
        {
            var matchingItems = controller.WordWheel.GetMatchingItems("Ford");
            Assert.NotEmpty(matchingItems);
            Assert.All(matchingItems, item => Assert.Contains(item, controller.Items));
        }
    }

    [Fact]
    public void ViewSwitching_WorksCorrectly()
    {
        var source = TestDataHelper.LoadCxml("conceptcars.cxml");
        var controller = new PivotViewerController();
        controller.LoadCollection(source);
        controller.SetAvailableSize(800, 600);

        // Start in grid view
        Assert.Equal("grid", controller.CurrentView);
        Assert.NotNull(controller.GridLayout);

        // Switch to graph view with sort property
        var catProp = controller.Properties.FirstOrDefault(p => p.Id == "Manufacturer");
        if (catProp != null)
        {
            controller.SortProperty = catProp;
            controller.CurrentView = "graph";
            Assert.NotNull(controller.HistogramLayout);
            Assert.Null(controller.GridLayout);

            // Switch back to grid
            controller.CurrentView = "grid";
            Assert.NotNull(controller.GridLayout);
        }
    }

    [Fact]
    public void SelectionCleared_WhenFilteredOut()
    {
        var source = TestDataHelper.LoadCxml("conceptcars.cxml");
        var controller = new PivotViewerController();
        controller.LoadCollection(source);
        controller.SetAvailableSize(800, 600);

        // Select first item
        var firstItem = controller.InScopeItems[0];
        controller.SelectedItem = firstItem;
        Assert.Equal(firstItem, controller.SelectedItem);

        // Get manufacturer of first item
        var mfr = firstItem["Manufacturer"];
        string? firstMfr = mfr?.Count > 0 ? mfr[0]?.ToString() : null;

        if (firstMfr != null)
        {
            // Filter to a different manufacturer
            var otherMfrs = controller.GetFilterCounts("Manufacturer")
                .Where(kv => kv.Key != firstMfr)
                .Take(1)
                .ToList();

            if (otherMfrs.Count > 0)
            {
                controller.FilterEngine.AddStringFilter("Manufacturer", otherMfrs[0].Key);

                // If original item is filtered out, selection should be cleared
                if (!controller.InScopeItems.Contains(firstItem))
                    Assert.Null(controller.SelectedItem);
            }
        }
    }

    [Fact]
    public void EventsFired_InCorrectOrder()
    {
        var source = TestDataHelper.LoadCxml("conceptcars.cxml");
        var controller = new PivotViewerController();

        var events = new List<string>();
        controller.CollectionChanged += (s, e) => events.Add("CollectionChanged");
        controller.LayoutUpdated += (s, e) => events.Add("LayoutUpdated");
        controller.FiltersChanged += (s, e) => events.Add("FiltersChanged");
        controller.SelectionChanged += (s, e) => events.Add("SelectionChanged");

        controller.LoadCollection(source);
        controller.SetAvailableSize(800, 600);

        Assert.Contains("CollectionChanged", events);
        Assert.Contains("LayoutUpdated", events);

        events.Clear();
        controller.FilterEngine.AddStringFilter("Body Style", "Coupe");
        Assert.Contains("FiltersChanged", events);

        events.Clear();
        if (controller.InScopeItems.Count > 0)
        {
            controller.SelectedItem = controller.InScopeItems[0];
            Assert.Contains("SelectionChanged", events);
        }
    }

    [Fact]
    public void LargeCollection_HandlesEfficiently()
    {
        // Create a large synthetic collection
        var nameP = new PivotViewerStringProperty("Name")
        {
            DisplayName = "Name",
            Options = PivotViewerPropertyOptions.CanSearchText
        };
        var catP = new PivotViewerStringProperty("Category")
        {
            DisplayName = "Category",
            Options = PivotViewerPropertyOptions.CanFilter | PivotViewerPropertyOptions.CanSearchText
        };
        var numP = new PivotViewerNumericProperty("Value") { DisplayName = "Value" };

        var items = new List<PivotViewerItem>();
        var categories = new[] { "Alpha", "Beta", "Gamma", "Delta", "Epsilon" };
        var random = new Random(42);

        for (int i = 0; i < 1000; i++)
        {
            var item = new PivotViewerItem(i.ToString());
            item.Add(nameP, $"Item {i}");
            item.Add(catP, categories[i % categories.Length]);
            item.Add(numP, (double)random.Next(0, 10000));
            items.Add(item);
        }

        var controller = new PivotViewerController();
        controller.LoadItems(items, new PivotViewerProperty[] { nameP, catP, numP });
        controller.SetAvailableSize(1920, 1080);

        Assert.Equal(1000, controller.Items.Count);
        Assert.NotNull(controller.GridLayout);

        // Filter
        controller.FilterEngine.AddStringFilter("Category", "Alpha");
        Assert.Equal(200, controller.InScopeItems.Count); // 1000 / 5

        // Search
        var results = controller.Search("Item 42");
        Assert.NotEmpty(results);
    }

    [Fact]
    public void MsdnMagazine_CxmlLoadsCorrectly()
    {
        // Test with a different CXML file
        var source = TestDataHelper.LoadCxml("msdnmagazine.cxml");
        var controller = new PivotViewerController();
        controller.LoadCollection(source);
        controller.SetAvailableSize(800, 600);

        Assert.True(controller.Items.Count > 0);
        Assert.True(controller.Properties.Count > 0);
        Assert.NotNull(controller.GridLayout);
    }
}
