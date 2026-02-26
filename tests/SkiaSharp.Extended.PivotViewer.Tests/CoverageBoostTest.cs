using SkiaSharp.Extended.PivotViewer;
using Xunit;

namespace SkiaSharp.Extended.PivotViewer.Tests;

/// <summary>
/// Tests targeting low-coverage areas to increase branch coverage.
/// </summary>
public class CoverageBoostTest
{
    // --- PivotViewerGraphView ---

    [Fact]
    public void GraphView_HasDefaultProperties()
    {
        var view = new PivotViewerGraphView();
        Assert.Equal("GraphView", view.Id);
        Assert.Equal("Graph", view.Name);
        Assert.Null(view.GroupByProperty);
        Assert.Null(view.StackByProperty);
    }

    [Fact]
    public void GraphView_GroupByProperty_RaisesPropertyChanged()
    {
        var view = new PivotViewerGraphView();
        bool fired = false;
        view.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == "GroupByProperty") fired = true;
        };

        var prop = new PivotViewerStringProperty("test");
        view.GroupByProperty = prop;
        Assert.True(fired);
        Assert.Equal(prop, view.GroupByProperty);
    }

    [Fact]
    public void GraphView_StackByProperty_RaisesPropertyChanged()
    {
        var view = new PivotViewerGraphView();
        bool fired = false;
        view.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == "StackByProperty") fired = true;
        };

        var prop = new PivotViewerNumericProperty("count");
        view.StackByProperty = prop;
        Assert.True(fired);
    }

    // --- FilterPaneModel ---

    [Fact]
    public void FilterPaneModel_ToggleFilter_TogglesOn()
    {
        var engine = new FilterEngine();
        var props = new List<PivotViewerProperty>
        {
            new PivotViewerStringProperty("Color") { DisplayName = "Color", Options = PivotViewerPropertyOptions.CanFilter }
        };
        var model = new FilterPaneModel(engine, props);

        model.ToggleStringFilter("Color", "Red");
        Assert.True(model.HasActiveFilters);
    }

    [Fact]
    public void FilterPaneModel_ToggleFilter_TogglesOff()
    {
        var engine = new FilterEngine();
        var props = new List<PivotViewerProperty>
        {
            new PivotViewerStringProperty("Color") { DisplayName = "Color", Options = PivotViewerPropertyOptions.CanFilter }
        };
        var model = new FilterPaneModel(engine, props);

        model.ToggleStringFilter("Color", "Red");
        Assert.True(model.HasActiveFilters);

        model.ToggleStringFilter("Color", "Red");
        Assert.False(model.HasActiveFilters);
    }

    [Fact]
    public void FilterPaneModel_SetNumericRange()
    {
        var engine = new FilterEngine();
        var props = new List<PivotViewerProperty>
        {
            new PivotViewerNumericProperty("Year") { DisplayName = "Year", Options = PivotViewerPropertyOptions.CanFilter }
        };
        var model = new FilterPaneModel(engine, props);

        model.SetNumericRangeFilter("Year", 2000, 2010);
        Assert.True(model.HasActiveFilters);
    }

    [Fact]
    public void FilterPaneModel_SetDateTimeRange()
    {
        var engine = new FilterEngine();
        var props = new List<PivotViewerProperty>
        {
            new PivotViewerDateTimeProperty("Born") { DisplayName = "Born", Options = PivotViewerPropertyOptions.CanFilter }
        };
        var model = new FilterPaneModel(engine, props);

        model.SetDateTimeRangeFilter("Born", new DateTime(2000, 1, 1), new DateTime(2010, 12, 31));
        Assert.True(model.HasActiveFilters);
    }

    [Fact]
    public void FilterPaneModel_ClearPropertyFilters()
    {
        var engine = new FilterEngine();
        var props = new List<PivotViewerProperty>
        {
            new PivotViewerStringProperty("Color") { DisplayName = "Color", Options = PivotViewerPropertyOptions.CanFilter }
        };
        var model = new FilterPaneModel(engine, props);

        model.ToggleStringFilter("Color", "Red");
        model.ClearPropertyFilters("Color");
        Assert.False(model.HasActiveFilters);
    }

    [Fact]
    public void FilterPaneModel_ClearAll()
    {
        var engine = new FilterEngine();
        var props = new List<PivotViewerProperty>
        {
            new PivotViewerStringProperty("Color") { DisplayName = "Color", Options = PivotViewerPropertyOptions.CanFilter }
        };
        var model = new FilterPaneModel(engine, props);

        model.ToggleStringFilter("Color", "Red");
        model.ClearAllFilters();
        Assert.False(model.HasActiveFilters);
    }

    [Fact]
    public void FilterPaneModel_PropertyChanged_Fires()
    {
        var engine = new FilterEngine();
        var props = new List<PivotViewerProperty>
        {
            new PivotViewerStringProperty("Color") { DisplayName = "Color", Options = PivotViewerPropertyOptions.CanFilter }
        };
        var model = new FilterPaneModel(engine, props);
        bool fired = false;
        model.PropertyChanged += (s, e) => fired = true;

        model.ToggleStringFilter("Color", "Blue");
        Assert.True(fired);
    }

    [Fact]
    public void FilterPaneModel_GetCategories_SkipsNonFilterable()
    {
        var engine = new FilterEngine();
        var props = new List<PivotViewerProperty>
        {
            new PivotViewerStringProperty("Name") { DisplayName = "Name", Options = PivotViewerPropertyOptions.None },
            new PivotViewerStringProperty("Color") { DisplayName = "Color", Options = PivotViewerPropertyOptions.CanFilter }
        };
        var model = new FilterPaneModel(engine, props);
        var items = new List<PivotViewerItem>();

        var categories = model.GetCategories(items);
        Assert.Single(categories);
        Assert.Equal("Color", categories[0].Property.DisplayName);
    }

    // --- DetailPaneModel ---

    [Fact]
    public void DetailPane_FacetValues_SkipsPrivate()
    {
        var detail = new DetailPaneModel();
        var pubProp = new PivotViewerStringProperty("Name") { DisplayName = "Name" };
        var privProp = new PivotViewerStringProperty("Internal") { Options = PivotViewerPropertyOptions.Private };

        var item = new PivotViewerItem("1");
        item.Set(pubProp, new object[] { "Test" });
        item.Set(privProp, new object[] { "Hidden" });

        detail.SelectedItem = item;
        var facets = detail.FacetValues;

        Assert.Single(facets);
        Assert.Equal("Name", facets[0].DisplayName);
    }

    [Fact]
    public void DetailPane_FacetValues_FormatsNumbers()
    {
        var detail = new DetailPaneModel();
        var yearProp = new PivotViewerNumericProperty("Year") { DisplayName = "Year", Format = "0000" };

        var item = new PivotViewerItem("1");
        item.Set(yearProp, new object[] { 2005.0 });

        detail.SelectedItem = item;
        var facets = detail.FacetValues;

        Assert.Single(facets);
        Assert.Equal("2005", facets[0].Values[0]);
    }

    [Fact]
    public void DetailPane_FacetValues_HandlesHyperlink()
    {
        var detail = new DetailPaneModel();
        var linkProp = new PivotViewerLinkProperty("Info") { DisplayName = "Info" };

        var item = new PivotViewerItem("1");
        item.Set(linkProp, new object[] { new PivotViewerHyperlink("Example", new Uri("http://example.com")) });

        detail.SelectedItem = item;
        var facets = detail.FacetValues;

        Assert.Single(facets);
        Assert.Equal("Example", facets[0].Values[0]);
    }

    [Fact]
    public void DetailPane_IsExpanded_TwoWay()
    {
        var detail = new DetailPaneModel();
        Assert.True(detail.IsExpanded);

        bool fired = false;
        detail.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == "IsExpanded") fired = true;
        };

        detail.IsExpanded = false;
        Assert.False(detail.IsExpanded);
        Assert.True(fired);
    }

    [Fact]
    public void DetailPane_LinkClicked_Event()
    {
        var detail = new DetailPaneModel();
        Uri? clickedUri = null;
        detail.LinkClicked += (s, e) => clickedUri = e.Link;

        detail.OnLinkClicked(new Uri("http://test.com"));
        Assert.Equal(new Uri("http://test.com"), clickedUri);
    }

    [Fact]
    public void DetailPane_ApplyFilter_Event()
    {
        var detail = new DetailPaneModel();
        string? filter = null;
        detail.ApplyFilter += (s, e) => filter = e.Filter;

        detail.OnApplyFilter("Manufacturer=BMW");
        Assert.Equal("Manufacturer=BMW", filter);
    }

    // --- HistogramBucketer ---

    [Fact]
    public void HistogramBucketer_NumericBuckets_SingleValue()
    {
        var buckets = HistogramBucketer.CreateNumericBuckets(new[] { 5.0, 5.0, 5.0 });
        Assert.Single(buckets);
        Assert.Equal(3, buckets[0].Count);
    }

    [Fact]
    public void HistogramBucketer_DateTimeBuckets_LessThanOneDay()
    {
        var values = new[] { DateTime.Today, DateTime.Today.AddHours(3) };
        var buckets = HistogramBucketer.CreateDateTimeBuckets(values);
        Assert.Single(buckets);
        Assert.Equal(2, buckets[0].Count);
    }

    [Fact]
    public void HistogramBucketer_DateTimeBuckets_DecadeGranularity()
    {
        var values = Enumerable.Range(1900, 120)
            .Select(y => new DateTime(y, 6, 15)).ToList();
        var buckets = HistogramBucketer.CreateDateTimeBuckets(values);
        Assert.True(buckets.Count <= HistogramBucketer.MaxBuckets);
    }

    [Fact]
    public void HistogramBucketer_StringBuckets_CaseInsensitive()
    {
        var buckets = HistogramBucketer.CreateStringBuckets(new[] { "Apple", "apple", "APPLE", "Banana" });
        Assert.Equal(2, buckets.Count); // Apple and Banana
        Assert.Equal(3, buckets[0].Count); // Apple (case-insensitive)
    }

    [Fact]
    public void HistogramBucket_ToString()
    {
        var bucket = new HistogramBucket<double>(1.0, 5.0, 10);
        Assert.Contains("1", bucket.ToString());
        Assert.Contains("10", bucket.ToString());
    }

    [Fact]
    public void HistogramBucket_Label_Settable()
    {
        var bucket = new HistogramBucket<string>("A", "Z", 5);
        bucket.Label = "All Letters";
        Assert.Equal("All Letters", bucket.Label);
    }

    // --- NumericRangeFilterPredicate ---

    [Fact]
    public void NumericRange_FilterInclusive()
    {
        var engine = new FilterEngine();

        var prop = new PivotViewerNumericProperty("Price") { DisplayName = "Price" };
        var inRange = new PivotViewerItem("1");
        inRange.Set(prop, new object[] { 150.0 });

        var below = new PivotViewerItem("2");
        below.Set(prop, new object[] { 50.0 });

        var above = new PivotViewerItem("3");
        above.Set(prop, new object[] { 300.0 });

        var atMin = new PivotViewerItem("4");
        atMin.Set(prop, new object[] { 100.0 });

        var atMax = new PivotViewerItem("5");
        atMax.Set(prop, new object[] { 200.0 });

        var items = new[] { inRange, below, above, atMin, atMax };
        engine.SetSource(items, new PivotViewerProperty[] { prop });
        engine.AddNumericRangeFilter("Price", 100, 200);
        var filtered = engine.GetFilteredItems();

        Assert.Contains(inRange, filtered);
        Assert.DoesNotContain(below, filtered);
        Assert.DoesNotContain(above, filtered);
        Assert.Contains(atMin, filtered);
        Assert.Contains(atMax, filtered);
    }

    // --- ViewerStateSerializer ---

    [Fact]
    public void ViewerState_EmptyController_SerializesCleanly()
    {
        var controller = new PivotViewerController();
        var state = controller.SerializeViewerState();
        Assert.NotNull(state);
    }

    [Fact]
    public void ViewerState_WithSelection_RoundTrips()
    {
        var controller = new PivotViewerController();
        var source = TestDataHelper.LoadCxml("conceptcars.cxml");
        controller.LoadCollection(source);
        controller.SetAvailableSize(800, 600);

        controller.SelectedItem = controller.InScopeItems[3];
        var state = controller.SerializeViewerState();

        var controller2 = new PivotViewerController();
        controller2.LoadCollection(source);
        controller2.SetAvailableSize(800, 600);
        controller2.SetViewerState(state);

        Assert.Equal(controller.InScopeItems[3].Id, controller2.SelectedItem?.Id);
    }

    // --- PivotViewerHyperlink ---

    [Fact]
    public void Hyperlink_Equality()
    {
        var a = new PivotViewerHyperlink("A", new Uri("http://a.com"));
        var b = new PivotViewerHyperlink("A", new Uri("http://a.com"));
        var c = new PivotViewerHyperlink("B", new Uri("http://b.com"));

        Assert.True(a.CompareTo(b) == 0);
        Assert.NotEqual(0, a.CompareTo(c));
    }

    [Fact]
    public void Hyperlink_Properties()
    {
        var h = new PivotViewerHyperlink("Test Link", new Uri("http://test.com"));
        Assert.Equal("Test Link", h.Text);
        Assert.Equal(new Uri("http://test.com"), h.Uri);
    }

    // --- PivotViewerView base ---

    [Fact]
    public void GridView_Properties()
    {
        var view = new PivotViewerGridView();
        Assert.Equal("GridView", view.Id);
        Assert.Equal("Grid", view.Name);
        Assert.True(view.IsAvailable);
        Assert.Null(view.ToolTip);
    }

    [Fact]
    public void View_ToolTip_RaisesPropertyChanged()
    {
        var view = new PivotViewerGridView();
        bool fired = false;
        view.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == "ToolTip") fired = true;
        };

        view.ToolTip = "Grid layout";
        Assert.True(fired);
    }

    [Fact]
    public void View_IsAvailable_RaisesPropertyChanged()
    {
        var view = new PivotViewerGridView();
        bool fired = false;
        view.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == "IsAvailable") fired = true;
        };

        view.IsAvailable = false;
        Assert.True(fired);
        Assert.False(view.IsAvailable);
    }

    // --- SearchResult ---

    [Fact]
    public void SearchResult_HasText()
    {
        var controller = new PivotViewerController();
        var source = TestDataHelper.LoadCxml("conceptcars.cxml");
        controller.LoadCollection(source);

        var results = controller.Search("F");
        Assert.NotEmpty(results);
        foreach (var r in results)
        {
            Assert.NotNull(r.Text);
            Assert.True(r.ItemCount > 0 || r.Text.Length > 0);
        }
    }

    // --- CollectionBuilder ---

    [Fact]
    public void CollectionBuilder_AddDateTimeProperty()
    {
        var (items, props) = new PivotViewerCollectionBuilder()
            .AddDateTimeProperty("Born", "Born")
            .AddItem("1", b => b.Set("Born", new DateTime(2000, 1, 1)))
            .Build();

        Assert.Single(props);
        Assert.Single(items);
        Assert.Equal(PivotViewerPropertyType.DateTime, props[0].PropertyType);
    }

    [Fact]
    public void CollectionBuilder_AddLinkProperty()
    {
        var (items, props) = new PivotViewerCollectionBuilder()
            .AddLinkProperty("Info", "Info")
            .AddItem("1", b => b.Set("Info", new PivotViewerHyperlink("X", new Uri("http://x.com"))))
            .Build();

        Assert.Single(props);
        Assert.Equal(PivotViewerPropertyType.Link, props[0].PropertyType);
    }
}
