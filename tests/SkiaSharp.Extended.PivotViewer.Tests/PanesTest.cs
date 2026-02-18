using SkiaSharp.Extended.PivotViewer;
using Xunit;

namespace SkiaSharp.Extended.PivotViewer.Tests;

public class PanesTest
{
    // --- FilterPaneModel ---

    [Fact]
    public void FilterPaneModel_GetCategories_OnlyFilterable()
    {
        var filterEngine = new FilterEngine();
        var props = new PivotViewerProperty[]
        {
            new PivotViewerStringProperty("color") { DisplayName = "Color", Options = PivotViewerPropertyOptions.CanFilter },
            new PivotViewerStringProperty("desc") { DisplayName = "Description", Options = PivotViewerPropertyOptions.None },
            new PivotViewerNumericProperty("year") { DisplayName = "Year", Options = PivotViewerPropertyOptions.CanFilter }
        };

        var items = new[]
        {
            CreateItem("1", props[0], "Red"),
            CreateItem("2", props[0], "Blue"),
            CreateItem("3", props[0], "Red"),
        };
        filterEngine.SetSource(items, props);

        var model = new FilterPaneModel(filterEngine, props);
        var categories = model.GetCategories(items);

        Assert.Equal(2, categories.Count); // color and year, not desc
        Assert.Equal("Color", categories[0].Property.DisplayName);
    }

    [Fact]
    public void FilterPaneModel_ToggleStringFilter()
    {
        var filterEngine = new FilterEngine();
        var prop = new PivotViewerStringProperty("color") { DisplayName = "Color", Options = PivotViewerPropertyOptions.CanFilter };
        var items = new[]
        {
            CreateItem("1", prop, "Red"),
            CreateItem("2", prop, "Blue"),
        };
        filterEngine.SetSource(items, new[] { prop });

        var model = new FilterPaneModel(filterEngine, new[] { prop });

        // Toggle on
        model.ToggleStringFilter("color", "Red");
        Assert.True(filterEngine.HasStringFilter("color", "Red"));

        // Toggle off
        model.ToggleStringFilter("color", "Red");
        Assert.False(filterEngine.HasStringFilter("color", "Red"));
    }

    [Fact]
    public void FilterPaneModel_HasActiveFilters()
    {
        var filterEngine = new FilterEngine();
        var prop = new PivotViewerStringProperty("type") { DisplayName = "Type", Options = PivotViewerPropertyOptions.CanFilter };
        filterEngine.SetSource(Array.Empty<PivotViewerItem>(), new[] { prop });

        var model = new FilterPaneModel(filterEngine, new[] { prop });

        Assert.False(model.HasActiveFilters);
        model.ToggleStringFilter("type", "A");
        Assert.True(model.HasActiveFilters);
    }

    [Fact]
    public void FilterPaneModel_ClearAllFilters()
    {
        var filterEngine = new FilterEngine();
        var prop = new PivotViewerStringProperty("color") { DisplayName = "Color", Options = PivotViewerPropertyOptions.CanFilter };
        filterEngine.SetSource(Array.Empty<PivotViewerItem>(), new[] { prop });

        var model = new FilterPaneModel(filterEngine, new[] { prop });
        model.ToggleStringFilter("color", "Red");
        Assert.True(model.HasActiveFilters);

        model.ClearAllFilters();
        Assert.False(model.HasActiveFilters);
    }

    // --- DetailPaneModel ---

    [Fact]
    public void DetailPaneModel_InitialState()
    {
        var model = new DetailPaneModel();
        Assert.Null(model.SelectedItem);
        Assert.False(model.IsShowing);
        Assert.True(model.IsExpanded);
        Assert.Empty(model.FacetValues);
    }

    [Fact]
    public void DetailPaneModel_SelectItem_ShowsFacets()
    {
        var prop = new PivotViewerStringProperty("color") { DisplayName = "Color" };
        var item = new PivotViewerItem("test");
        item.Add(prop, new object[] { "Red" });

        var model = new DetailPaneModel();
        model.SelectedItem = item;

        Assert.True(model.IsShowing);
        Assert.NotEmpty(model.FacetValues);
        Assert.Equal("Color", model.FacetValues[0].DisplayName);
        Assert.Contains("Red", model.FacetValues[0].Values);
    }

    [Fact]
    public void DetailPaneModel_ClearSelection_Hides()
    {
        var model = new DetailPaneModel();
        var item = new PivotViewerItem("test");
        model.SelectedItem = item;
        Assert.True(model.IsShowing);

        model.SelectedItem = null;
        Assert.False(model.IsShowing);
        Assert.Empty(model.FacetValues);
    }

    [Fact]
    public void DetailPaneModel_PrivateProperties_Hidden()
    {
        var visible = new PivotViewerStringProperty("name") { DisplayName = "Name" };
        var hidden = new PivotViewerStringProperty("internal")
        {
            DisplayName = "Internal",
            Options = PivotViewerPropertyOptions.Private
        };

        var item = new PivotViewerItem("test");
        item.Add(visible, new object[] { "Test Item" });
        item.Add(hidden, new object[] { "secret" });

        var model = new DetailPaneModel();
        model.SelectedItem = item;

        Assert.Single(model.FacetValues);
        Assert.Equal("Name", model.FacetValues[0].DisplayName);
    }

    [Fact]
    public void DetailPaneModel_PropertyChanged_Fires()
    {
        var model = new DetailPaneModel();
        var changedProps = new List<string>();
        model.PropertyChanged += (s, e) => changedProps.Add(e.PropertyName!);

        model.SelectedItem = new PivotViewerItem("test");

        Assert.Contains("SelectedItem", changedProps);
        Assert.Contains("IsShowing", changedProps);
        Assert.Contains("FacetValues", changedProps);
    }

    [Fact]
    public void DetailPaneModel_IsExpanded_Bindable()
    {
        var model = new DetailPaneModel();
        bool changed = false;
        model.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == "IsExpanded") changed = true;
        };

        model.IsExpanded = false;
        Assert.False(model.IsExpanded);
        Assert.True(changed);
    }

    [Fact]
    public void DetailPaneModel_LinkClicked_Event()
    {
        var model = new DetailPaneModel();
        PivotViewerLinkEventArgs? received = null;
        model.LinkClicked += (s, e) => received = e;

        model.OnLinkClicked(new Uri("https://test.com"));

        Assert.NotNull(received);
        Assert.Equal("https://test.com/", received!.Link.ToString());
    }

    [Fact]
    public void DetailPaneModel_ApplyFilter_Event()
    {
        var model = new DetailPaneModel();
        PivotViewerFilterEventArgs? received = null;
        model.ApplyFilter += (s, e) => received = e;

        model.OnApplyFilter("color=red");

        Assert.NotNull(received);
        Assert.Equal("color=red", received!.Filter);
    }

    // --- FilterCategory ---

    [Fact]
    public void FilterCategory_IsFiltered()
    {
        var prop = new PivotViewerStringProperty("test") { DisplayName = "Test" };
        var cat = new FilterCategory(prop);

        Assert.False(cat.IsFiltered);

        cat.ActiveFilters = new[] { "Red" };
        Assert.True(cat.IsFiltered);

        cat.ActiveFilters = Array.Empty<string>();
        Assert.False(cat.IsFiltered);
    }

    // Helpers

    private static PivotViewerItem CreateItem(string id, PivotViewerProperty prop, object value)
    {
        var item = new PivotViewerItem(id);
        item.Add(prop, new[] { value });
        return item;
    }
}
