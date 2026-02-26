using SkiaSharp.Extended.PivotViewer;
using Xunit;

namespace SkiaSharp.Extended.PivotViewer.Tests;

/// <summary>
/// Tests parsing and using multiple CXML collections to verify robustness.
/// </summary>
public class MultiCollectionTest
{
    [Theory]
    [InlineData("conceptcars.cxml")]
    [InlineData("venues.cxml")]
    [InlineData("msdnmagazine.cxml")]
    public void Parse_AllTestCollections(string filename)
    {
        var source = TestDataHelper.LoadCxml(filename);
        Assert.NotNull(source);
        Assert.True(source.Items.Count() > 0, $"{filename} should have items");
        Assert.True(source.ItemProperties.Count() > 0, $"{filename} should have properties");
    }

    [Theory]
    [InlineData("conceptcars.cxml")]
    [InlineData("venues.cxml")]
    [InlineData("msdnmagazine.cxml")]
    public void Load_AllTestCollections(string filename)
    {
        var source = TestDataHelper.LoadCxml(filename);
        var controller = new PivotViewerController();
        controller.LoadCollection(source);
        controller.SetAvailableSize(1024, 768);

        Assert.True(controller.Items.Count > 0);
        Assert.True(controller.Properties.Count > 0);
        Assert.NotNull(controller.GridLayout);
        Assert.Equal(controller.Items.Count, controller.GridLayout.Positions.Length);
    }

    [Theory]
    [InlineData("conceptcars.cxml")]
    [InlineData("venues.cxml")]
    [InlineData("msdnmagazine.cxml")]
    public void Filter_AllTestCollections(string filename)
    {
        var source = TestDataHelper.LoadCxml(filename);
        var controller = new PivotViewerController();
        controller.LoadCollection(source);
        controller.SetAvailableSize(800, 600);

        // Get first filterable text property
        var textProp = controller.Properties
            .FirstOrDefault(p => p.PropertyType == PivotViewerPropertyType.Text &&
                                 p.Options.HasFlag(PivotViewerPropertyOptions.CanFilter));

        if (textProp != null)
        {
            var counts = controller.GetFilterCounts(textProp.Id);
            if (counts.Count > 0)
            {
                var firstValue = counts.First().Key;
                controller.FilterEngine.AddStringFilter(textProp.Id, firstValue);

                Assert.True(controller.InScopeItems.Count <= controller.Items.Count);
                Assert.True(controller.InScopeItems.Count > 0);

                controller.FilterEngine.ClearAll();
                Assert.Equal(controller.Items.Count, controller.InScopeItems.Count);
            }
        }
    }

    [Theory]
    [InlineData("conceptcars.cxml")]
    [InlineData("venues.cxml")]
    [InlineData("msdnmagazine.cxml")]
    public void Sort_AllTestCollections(string filename)
    {
        var source = TestDataHelper.LoadCxml(filename);
        var controller = new PivotViewerController();
        controller.LoadCollection(source);
        controller.SetAvailableSize(800, 600);

        // Sort by each property
        foreach (var prop in controller.Properties.Take(3))
        {
            controller.SortProperty = prop;
            Assert.NotNull(controller.GridLayout);
        }
    }

    [Theory]
    [InlineData("conceptcars.cxml")]
    [InlineData("venues.cxml")]
    [InlineData("msdnmagazine.cxml")]
    public void Graph_AllTestCollections(string filename)
    {
        var source = TestDataHelper.LoadCxml(filename);
        var controller = new PivotViewerController();
        controller.LoadCollection(source);
        controller.SetAvailableSize(800, 600);

        // Switch to graph with a text property
        var textProp = controller.Properties
            .FirstOrDefault(p => p.PropertyType == PivotViewerPropertyType.Text);

        if (textProp != null)
        {
            controller.SortProperty = textProp;
            controller.CurrentView = "graph";

            Assert.NotNull(controller.HistogramLayout);
            Assert.True(controller.HistogramLayout.Columns.Length > 0);
        }
    }

    [Theory]
    [InlineData("conceptcars.cxml")]
    [InlineData("venues.cxml")]
    [InlineData("msdnmagazine.cxml")]
    public void SelectAndDetail_AllTestCollections(string filename)
    {
        var source = TestDataHelper.LoadCxml(filename);
        var controller = new PivotViewerController();
        controller.LoadCollection(source);
        controller.SetAvailableSize(800, 600);

        // Select each item and verify detail pane
        foreach (var item in controller.Items.Take(5))
        {
            controller.SelectedItem = item;
            Assert.True(controller.DetailPane.IsShowing);
            Assert.NotNull(item.Id);
        }
    }

    [Theory]
    [InlineData("conceptcars.cxml")]
    [InlineData("venues.cxml")]
    [InlineData("msdnmagazine.cxml")]
    public void StateSerialization_AllTestCollections(string filename)
    {
        var source = TestDataHelper.LoadCxml(filename);
        var controller = new PivotViewerController();
        controller.LoadCollection(source);
        controller.SetAvailableSize(800, 600);

        var state = controller.SerializeViewerState();
        Assert.NotNull(state);

        // Round-trip
        var controller2 = new PivotViewerController();
        controller2.LoadCollection(TestDataHelper.LoadCxml(filename));
        controller2.SetAvailableSize(800, 600);
        controller2.SetViewerState(state);

        Assert.Equal(controller.InScopeItems.Count, controller2.InScopeItems.Count);
    }

    [Theory]
    [InlineData("conceptcars.cxml")]
    [InlineData("venues.cxml")]
    [InlineData("msdnmagazine.cxml")]
    public void WordWheelSearch_AllTestCollections(string filename)
    {
        var source = TestDataHelper.LoadCxml(filename);
        var controller = new PivotViewerController();
        controller.LoadCollection(source);

        // Search with various prefixes
        var results1 = controller.Search("a");
        var results2 = controller.Search("the");
        // Shouldn't crash regardless of results
    }
}
