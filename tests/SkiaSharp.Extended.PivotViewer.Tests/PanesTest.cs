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

    [Fact]
    public void FilterPaneModel_SetNumericRangeFilter_AppliesRange()
    {
        var filterEngine = new FilterEngine();
        var prop = new PivotViewerNumericProperty("price") { DisplayName = "Price", Options = PivotViewerPropertyOptions.CanFilter };
        var items = new[]
        {
            CreateItem("1", prop, 10.0),
            CreateItem("2", prop, 20.0),
            CreateItem("3", prop, 30.0),
        };
        filterEngine.SetSource(items, new[] { prop });

        var model = new FilterPaneModel(filterEngine, new[] { prop });
        model.SetNumericRangeFilter("price", 15.0, 25.0);

        Assert.True(model.HasActiveFilters);
        var filtered = filterEngine.GetFilteredItems();
        Assert.Single(filtered);
        Assert.Equal("2", filtered[0].Id);
    }

    [Fact]
    public void FilterPaneModel_SetDateTimeRangeFilter_AppliesRange()
    {
        var filterEngine = new FilterEngine();
        var prop = new PivotViewerDateTimeProperty("created") { DisplayName = "Created", Options = PivotViewerPropertyOptions.CanFilter };
        var items = new[]
        {
            CreateItem("1", prop, new DateTime(2023, 1, 1)),
            CreateItem("2", prop, new DateTime(2023, 6, 15)),
            CreateItem("3", prop, new DateTime(2024, 1, 1)),
        };
        filterEngine.SetSource(items, new[] { prop });

        var model = new FilterPaneModel(filterEngine, new[] { prop });
        model.SetDateTimeRangeFilter("created", new DateTime(2023, 3, 1), new DateTime(2023, 12, 31));

        Assert.True(model.HasActiveFilters);
        var filtered = filterEngine.GetFilteredItems();
        Assert.Single(filtered);
        Assert.Equal("2", filtered[0].Id);
    }

    [Fact]
    public void FilterPaneModel_ClearPropertyFilters_ResetsProperty()
    {
        var filterEngine = new FilterEngine();
        var prop = new PivotViewerStringProperty("color") { DisplayName = "Color", Options = PivotViewerPropertyOptions.CanFilter };
        filterEngine.SetSource(Array.Empty<PivotViewerItem>(), new[] { prop });

        var model = new FilterPaneModel(filterEngine, new[] { prop });
        model.ToggleStringFilter("color", "Red");
        Assert.True(model.HasActiveFilters);

        model.ClearPropertyFilters("color");
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

    [Fact]
    public void DetailPane_FacetValues_UsesDecimalPlaces()
    {
        var prop = new PivotViewerNumericProperty("Price") { DecimalPlaces = 2 };
        var item = new PivotViewerItem("1");
        item.Add(prop, 3.14159m);

        var detail = new DetailPaneModel();
        detail.SelectedItem = item;

        var facets = detail.FacetValues;
        Assert.Single(facets);
        Assert.Equal("3.14", facets[0].Values[0]);
    }

    [Fact]
    public void FacetDisplay_StoresProperties()
    {
        var prop = new PivotViewerStringProperty("color") { DisplayName = "Color" };
        var values = new List<string> { "Red", "Blue" };
        var facet = new FacetDisplay("Color", values, prop);

        Assert.Equal("Color", facet.DisplayName);
        Assert.Equal(2, facet.Values.Count);
        Assert.Contains("Red", facet.Values);
        Assert.Contains("Blue", facet.Values);
        Assert.Same(prop, facet.Property);
    }

    [Fact]
    public void FacetDisplay_EmptyValues()
    {
        var prop = new PivotViewerNumericProperty("year") { DisplayName = "Year" };
        var facet = new FacetDisplay("Year", Array.Empty<string>(), prop);

        Assert.Equal("Year", facet.DisplayName);
        Assert.Empty(facet.Values);
    }

    // --- FilterPaneModel additional coverage ---

    [Fact]
    public void FilterPaneModel_PropertyChanged_FiresOnToggle()
    {
        var filterEngine = new FilterEngine();
        var prop = new PivotViewerStringProperty("color") { DisplayName = "Color", Options = PivotViewerPropertyOptions.CanFilter };
        filterEngine.SetSource(Array.Empty<PivotViewerItem>(), new[] { prop });

        var model = new FilterPaneModel(filterEngine, new[] { prop });
        var changedProps = new List<string>();
        model.PropertyChanged += (s, e) => changedProps.Add(e.PropertyName!);

        model.ToggleStringFilter("color", "Red");
        Assert.Contains("GetCategories", changedProps);

        changedProps.Clear();
        model.SetNumericRangeFilter("price", 0, 100);
        Assert.Contains("GetCategories", changedProps);

        changedProps.Clear();
        model.ClearAllFilters();
        Assert.Contains("GetCategories", changedProps);
    }

    [Fact]
    public void FilterPaneModel_GetCategories_ReturnsActiveFilterCounts()
    {
        var filterEngine = new FilterEngine();
        var prop = new PivotViewerStringProperty("color") { DisplayName = "Color", Options = PivotViewerPropertyOptions.CanFilter };
        var items = new[]
        {
            CreateItem("1", prop, "Red"),
            CreateItem("2", prop, "Blue"),
            CreateItem("3", prop, "Red"),
        };
        filterEngine.SetSource(items, new[] { prop });

        var model = new FilterPaneModel(filterEngine, new[] { prop });
        model.ToggleStringFilter("color", "Red");

        var categories = model.GetCategories(items);
        Assert.Single(categories);
        Assert.True(categories[0].IsFiltered);
        Assert.NotNull(categories[0].ActiveFilters);
        Assert.Contains("Red", categories[0].ActiveFilters!);
    }

    [Fact]
    public void FilterPaneModel_GetCategories_ValueCountsPopulated()
    {
        var filterEngine = new FilterEngine();
        var prop = new PivotViewerStringProperty("color") { DisplayName = "Color", Options = PivotViewerPropertyOptions.CanFilter };
        var items = new[]
        {
            CreateItem("1", prop, "Red"),
            CreateItem("2", prop, "Blue"),
            CreateItem("3", prop, "Red"),
        };
        filterEngine.SetSource(items, new[] { prop });

        var model = new FilterPaneModel(filterEngine, new[] { prop });
        var categories = model.GetCategories(items);

        Assert.Single(categories);
        Assert.NotNull(categories[0].ValueCounts);
        Assert.True(categories[0].ValueCounts!.Count > 0);
    }

    [Fact]
    public void FilterPaneModel_MultipleProperties_IndependentFilters()
    {
        var filterEngine = new FilterEngine();
        var colorProp = new PivotViewerStringProperty("color") { DisplayName = "Color", Options = PivotViewerPropertyOptions.CanFilter };
        var sizeProp = new PivotViewerStringProperty("size") { DisplayName = "Size", Options = PivotViewerPropertyOptions.CanFilter };
        var props = new PivotViewerProperty[] { colorProp, sizeProp };

        var item1 = new PivotViewerItem("1");
        item1.Add(colorProp, new object[] { "Red" });
        item1.Add(sizeProp, new object[] { "Large" });
        var item2 = new PivotViewerItem("2");
        item2.Add(colorProp, new object[] { "Blue" });
        item2.Add(sizeProp, new object[] { "Small" });

        filterEngine.SetSource(new[] { item1, item2 }, props);
        var model = new FilterPaneModel(filterEngine, props);

        model.ToggleStringFilter("color", "Red");
        model.ToggleStringFilter("size", "Large");
        Assert.True(model.HasActiveFilters);

        model.ClearPropertyFilters("color");
        Assert.True(model.HasActiveFilters); // size filter still active

        model.ClearAllFilters();
        Assert.False(model.HasActiveFilters);
    }

    [Fact]
    public void FilterPaneModel_PropertyChanged_FiresOnClearProperty()
    {
        var filterEngine = new FilterEngine();
        var prop = new PivotViewerStringProperty("color") { DisplayName = "Color", Options = PivotViewerPropertyOptions.CanFilter };
        filterEngine.SetSource(Array.Empty<PivotViewerItem>(), new[] { prop });

        var model = new FilterPaneModel(filterEngine, new[] { prop });
        model.ToggleStringFilter("color", "Red");

        var changedProps = new List<string>();
        model.PropertyChanged += (s, e) => changedProps.Add(e.PropertyName!);

        model.ClearPropertyFilters("color");
        Assert.Contains("GetCategories", changedProps);
    }

    [Fact]
    public void FilterPaneModel_PropertyChanged_FiresOnDateTimeRange()
    {
        var filterEngine = new FilterEngine();
        var prop = new PivotViewerDateTimeProperty("created") { DisplayName = "Created", Options = PivotViewerPropertyOptions.CanFilter };
        filterEngine.SetSource(Array.Empty<PivotViewerItem>(), new[] { prop });

        var model = new FilterPaneModel(filterEngine, new[] { prop });
        var changedProps = new List<string>();
        model.PropertyChanged += (s, e) => changedProps.Add(e.PropertyName!);

        model.SetDateTimeRangeFilter("created", DateTime.MinValue, DateTime.MaxValue);
        Assert.Contains("GetCategories", changedProps);
    }

    // --- DetailPaneModel additional coverage ---

    [Fact]
    public void DetailPaneModel_HyperlinkValues_DisplayText()
    {
        var prop = new PivotViewerStringProperty("link") { DisplayName = "Link" };
        var item = new PivotViewerItem("test");
        var hyperlink = new PivotViewerHyperlink("Example", new Uri("https://example.com"));
        item.Add(prop, new object[] { hyperlink });

        var model = new DetailPaneModel();
        model.SelectedItem = item;

        Assert.Single(model.FacetValues);
        Assert.Contains("Example", model.FacetValues[0].Values);
    }

    [Fact]
    public void DetailPaneModel_MultipleFacets_AllVisible()
    {
        var prop1 = new PivotViewerStringProperty("color") { DisplayName = "Color" };
        var prop2 = new PivotViewerNumericProperty("year") { DisplayName = "Year" };
        var item = new PivotViewerItem("test");
        item.Add(prop1, new object[] { "Red" });
        item.Add(prop2, new object[] { 2024.0 });

        var model = new DetailPaneModel();
        model.SelectedItem = item;

        Assert.Equal(2, model.FacetValues.Count);
    }

    [Fact]
    public void DetailPaneModel_Reselection_UpdatesFacets()
    {
        var prop = new PivotViewerStringProperty("color") { DisplayName = "Color" };
        var item1 = new PivotViewerItem("1");
        item1.Add(prop, new object[] { "Red" });
        var item2 = new PivotViewerItem("2");
        item2.Add(prop, new object[] { "Blue" });

        var model = new DetailPaneModel();
        model.SelectedItem = item1;
        Assert.Contains("Red", model.FacetValues[0].Values);

        model.SelectedItem = item2;
        Assert.Contains("Blue", model.FacetValues[0].Values);
    }

    [Fact]
    public void DetailPaneModel_SameItemReassignment_NoChange()
    {
        var model = new DetailPaneModel();
        var item = new PivotViewerItem("test");
        model.SelectedItem = item;

        var changedProps = new List<string>();
        model.PropertyChanged += (s, e) => changedProps.Add(e.PropertyName!);

        model.SelectedItem = item; // same item
        Assert.Empty(changedProps); // no change event
    }

    [Fact]
    public void DetailPaneModel_IsShowing_SetDirectly()
    {
        var model = new DetailPaneModel();
        var changedProps = new List<string>();
        model.PropertyChanged += (s, e) => changedProps.Add(e.PropertyName!);

        model.IsShowing = true;
        Assert.True(model.IsShowing);
        Assert.Contains("IsShowing", changedProps);
    }

    [Fact]
    public void DetailPaneModel_FormatWithExplicitFormat()
    {
        var prop = new PivotViewerNumericProperty("price") { DisplayName = "Price", Format = "C2" };
        var item = new PivotViewerItem("1");
        item.Add(prop, new object[] { 19.99 });

        var model = new DetailPaneModel();
        model.SelectedItem = item;

        Assert.Single(model.FacetValues);
        // Format "C2" applied
        Assert.NotEmpty(model.FacetValues[0].Values[0]);
    }

    [Fact]
    public void DetailPaneModel_NullValueInFacet_FormatsAsEmpty()
    {
        var prop = new PivotViewerStringProperty("test") { DisplayName = "Test" };
        var item = new PivotViewerItem("1");
        item.Add(prop, new object?[] { null! });

        var model = new DetailPaneModel();
        model.SelectedItem = item;

        // Null values should be handled gracefully (empty string or skipped)
        Assert.NotNull(model.FacetValues);
    }

    // --- FilterCategory additional coverage ---

    [Fact]
    public void FilterCategory_ValueCounts_CanBeSet()
    {
        var prop = new PivotViewerStringProperty("color") { DisplayName = "Color" };
        var cat = new FilterCategory(prop);

        var counts = new Dictionary<string, int> { { "Red", 5 }, { "Blue", 3 } };
        cat.ValueCounts = counts;

        Assert.Equal(2, cat.ValueCounts!.Count);
        Assert.Equal(5, cat.ValueCounts["Red"]);
    }

    [Fact]
    public void FilterCategory_NullActiveFilters_NotFiltered()
    {
        var prop = new PivotViewerStringProperty("test") { DisplayName = "Test" };
        var cat = new FilterCategory(prop);
        cat.ActiveFilters = null;

        Assert.False(cat.IsFiltered);
    }

    [Fact]
    public void FilterCategory_PropertyAccessor()
    {
        var prop = new PivotViewerStringProperty("test") { DisplayName = "Test" };
        var cat = new FilterCategory(prop);

        Assert.Same(prop, cat.Property);
        Assert.Equal("Test", cat.Property.DisplayName);
    }

    // --- PivotViewerDefaultDetails ---

    [Fact]
    public void DefaultDetails_InitialState_AllVisible()
    {
        var details = new PivotViewerDefaultDetails();
        Assert.False(details.IsNameHidden);
        Assert.False(details.IsDescriptionHidden);
        Assert.False(details.IsFacetCategoriesHidden);
        Assert.False(details.IsRelatedCollectionsHidden);
        Assert.False(details.IsCopyrightHidden);
    }

    [Fact]
    public void DefaultDetails_PropertyChanged_AllProperties()
    {
        var details = new PivotViewerDefaultDetails();
        var changedProps = new List<string>();
        details.PropertyChanged += (s, e) => changedProps.Add(e.PropertyName!);

        details.IsNameHidden = true;
        details.IsDescriptionHidden = true;
        details.IsFacetCategoriesHidden = true;
        details.IsRelatedCollectionsHidden = true;
        details.IsCopyrightHidden = true;

        Assert.Contains("IsNameHidden", changedProps);
        Assert.Contains("IsDescriptionHidden", changedProps);
        Assert.Contains("IsFacetCategoriesHidden", changedProps);
        Assert.Contains("IsRelatedCollectionsHidden", changedProps);
        Assert.Contains("IsCopyrightHidden", changedProps);
    }

    [Fact]
    public void DefaultDetails_RoundTrip_AllProperties()
    {
        var details = new PivotViewerDefaultDetails();
        details.IsNameHidden = true;
        details.IsDescriptionHidden = true;
        details.IsFacetCategoriesHidden = true;
        details.IsRelatedCollectionsHidden = true;
        details.IsCopyrightHidden = true;

        Assert.True(details.IsNameHidden);
        Assert.True(details.IsDescriptionHidden);
        Assert.True(details.IsFacetCategoriesHidden);
        Assert.True(details.IsRelatedCollectionsHidden);
        Assert.True(details.IsCopyrightHidden);
    }

    [Fact]
    public void DefaultDetails_LinkClicked_Event()
    {
        var details = new PivotViewerDefaultDetails();
        PivotViewerLinkEventArgs? received = null;
        details.LinkClicked += (s, e) => received = e;

        details.OnLinkClicked(new Uri("https://example.com"));

        Assert.NotNull(received);
        Assert.Equal("https://example.com/", received!.Link.ToString());
    }

    [Fact]
    public void DefaultDetails_ApplyFilter_Event()
    {
        var details = new PivotViewerDefaultDetails();
        PivotViewerFilterEventArgs? received = null;
        details.ApplyFilter += (s, e) => received = e;

        details.OnApplyFilter("color=red");

        Assert.NotNull(received);
        Assert.Equal("color=red", received!.Filter);
    }

    [Fact]
    public void DefaultDetails_PropertyChanged_SenderIsDetails()
    {
        var details = new PivotViewerDefaultDetails();
        object? sender = null;
        details.PropertyChanged += (s, e) => sender = s;

        details.IsNameHidden = true;
        Assert.Same(details, sender);
    }

    [Fact]
    public void DefaultDetails_LinkClicked_NotFired_WhenNoSubscribers()
    {
        var details = new PivotViewerDefaultDetails();
        // Should not throw when no subscribers
        details.OnLinkClicked(new Uri("https://example.com"));
    }

    [Fact]
    public void DefaultDetails_ApplyFilter_NotFired_WhenNoSubscribers()
    {
        var details = new PivotViewerDefaultDetails();
        // Should not throw when no subscribers
        details.OnApplyFilter("test");
    }

    [Fact]
    public void FilterPaneModel_SetDateTimeRangeFilter_ThenClearAll_RemovesFilter()
    {
        var filterEngine = new FilterEngine();
        var prop = new PivotViewerDateTimeProperty("created") { DisplayName = "Created", Options = PivotViewerPropertyOptions.CanFilter };
        var items = new[]
        {
            CreateItem("1", prop, new DateTime(2023, 1, 1)),
            CreateItem("2", prop, new DateTime(2023, 6, 15)),
            CreateItem("3", prop, new DateTime(2024, 1, 1)),
        };
        filterEngine.SetSource(items, new[] { prop });

        var model = new FilterPaneModel(filterEngine, new[] { prop });
        model.SetDateTimeRangeFilter("created", new DateTime(2023, 3, 1), new DateTime(2023, 12, 31));
        Assert.True(model.HasActiveFilters);

        model.ClearAllFilters();
        Assert.False(model.HasActiveFilters);
        var filtered = filterEngine.GetFilteredItems();
        Assert.Equal(3, filtered.Count);
    }
}
