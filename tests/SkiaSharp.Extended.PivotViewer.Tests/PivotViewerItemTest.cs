using SkiaSharp.Extended.PivotViewer;

namespace SkiaSharp.Extended.PivotViewer.Tests;

public class PivotViewerItemTest
{
    [Fact]
    public void Constructor_SetsId()
    {
        var item = new PivotViewerItem("42");
        Assert.Equal("42", item.Id);
    }

    [Fact]
    public void StringIndexer_NoValues_ReturnsNull()
    {
        var item = new PivotViewerItem("1");
        Assert.Null(item["Name"]);
    }

    [Fact]
    public void Add_String_RetrievableByIndexer()
    {
        var item = new PivotViewerItem("1");
        var prop = new PivotViewerStringProperty("Name");
        item.Add(prop, "Widget");

        var values = item["Name"];
        Assert.NotNull(values);
        Assert.Single(values);
        Assert.Equal("Widget", values![0]);
    }

    [Fact]
    public void Add_MultipleValues_AllRetrievable()
    {
        var item = new PivotViewerItem("1");
        var prop = new PivotViewerStringProperty("Tags");
        item.Add(prop, "Red", "Blue", "Green");

        var values = item["Tags"];
        Assert.NotNull(values);
        Assert.Equal(3, values!.Count);
        Assert.Contains("Red", values);
        Assert.Contains("Blue", values);
        Assert.Contains("Green", values);
    }

    [Fact]
    public void Add_Number_RetrievableAsDouble()
    {
        var item = new PivotViewerItem("1");
        var prop = new PivotViewerNumericProperty("Price");
        item.Add(prop, 19.99);

        var values = item["Price"];
        Assert.NotNull(values);
        Assert.Equal(19.99, (double)values![0]);
    }

    [Fact]
    public void Add_DateTime_RetrievableAsDateTime()
    {
        var item = new PivotViewerItem("1");
        var prop = new PivotViewerDateTimeProperty("Created");
        var date = new DateTime(2023, 6, 15);
        item.Add(prop, date);

        var values = item["Created"];
        Assert.NotNull(values);
        Assert.Equal(date, (DateTime)values![0]);
    }

    [Fact]
    public void Add_Link_RetrievableAsHyperlink()
    {
        var item = new PivotViewerItem("1");
        var prop = new PivotViewerLinkProperty("Website");
        var link = new PivotViewerHyperlink("Example", new Uri("https://example.com"));
        item.Add(prop, link);

        var values = item["Website"];
        Assert.NotNull(values);
        Assert.IsType<PivotViewerHyperlink>(values![0]);
    }

    [Fact]
    public void PropertyIndexer_ByPropertyObject()
    {
        var item = new PivotViewerItem("1");
        var prop = new PivotViewerStringProperty("Name");
        item.Add(prop, "Widget");

        var values = item[prop];
        Assert.NotNull(values);
        Assert.Equal("Widget", values![0]);
    }

    [Fact]
    public void Set_ReplacesExistingValues()
    {
        var item = new PivotViewerItem("1");
        var prop = new PivotViewerStringProperty("Name");
        item.Add(prop, "Old");
        item.Set(prop, "New");

        var values = item["Name"];
        Assert.NotNull(values);
        Assert.Single(values);
        Assert.Equal("New", values![0]);
    }

    [Fact]
    public void Remove_ClearsValues()
    {
        var item = new PivotViewerItem("1");
        var prop = new PivotViewerStringProperty("Name");
        item.Add(prop, "Widget");

        Assert.True(item.Remove(prop));
        Assert.Null(item["Name"]);
    }

    [Fact]
    public void Remove_NonExistent_ReturnsFalse()
    {
        var item = new PivotViewerItem("1");
        var prop = new PivotViewerStringProperty("Name");
        Assert.False(item.Remove(prop));
    }

    [Fact]
    public void Properties_ListsAddedProperties()
    {
        var item = new PivotViewerItem("1");
        var prop1 = new PivotViewerStringProperty("Name");
        var prop2 = new PivotViewerNumericProperty("Price");
        item.Add(prop1, "Widget");
        item.Add(prop2, 19.99);

        Assert.Equal(2, item.Properties.Count);
    }

    [Fact]
    public void GetValues_TypedGeneric()
    {
        var item = new PivotViewerItem("1");
        var prop = new PivotViewerStringProperty("Tags");
        item.Add(prop, "Red", "Blue");

        var tags = item.GetValues<string>("Tags");
        Assert.NotNull(tags);
        Assert.Equal(2, tags!.Count);
        Assert.Contains("Red", tags);
    }

    [Fact]
    public void GetValues_WrongType_ReturnsNull()
    {
        var item = new PivotViewerItem("1");
        var prop = new PivotViewerStringProperty("Name");
        item.Add(prop, "Widget");

        var nums = item.GetValues<double>("Name");
        Assert.Null(nums);
    }

    [Fact]
    public void TryGetSingleValue_Success()
    {
        var item = new PivotViewerItem("1");
        var prop = new PivotViewerStringProperty("Name");
        item.Add(prop, "Widget");

        Assert.True(item.TryGetSingleValue<string>("Name", out var name));
        Assert.Equal("Widget", name);
    }

    [Fact]
    public void TryGetSingleValue_NoProperty_ReturnsFalse()
    {
        var item = new PivotViewerItem("1");
        Assert.False(item.TryGetSingleValue<string>("Missing", out _));
    }

    [Fact]
    public void HasProperty_True()
    {
        var item = new PivotViewerItem("1");
        var prop = new PivotViewerStringProperty("Name");
        item.Add(prop, "Widget");
        Assert.True(item.HasProperty("Name"));
    }

    [Fact]
    public void HasProperty_False()
    {
        var item = new PivotViewerItem("1");
        Assert.False(item.HasProperty("Missing"));
    }

    [Fact]
    public void HasAllProperties_True()
    {
        var prop1 = new PivotViewerStringProperty("A");
        var prop2 = new PivotViewerNumericProperty("B");
        var item = new PivotViewerItem("1");
        item.Add(prop1, "val");
        item.Add(prop2, 42.0m);
        Assert.True(item.HasAllProperties(new PivotViewerProperty[] { prop1, prop2 }));
    }

    [Fact]
    public void HasAllProperties_False_MissingOne()
    {
        var prop1 = new PivotViewerStringProperty("A");
        var prop2 = new PivotViewerNumericProperty("B");
        var item = new PivotViewerItem("1");
        item.Add(prop1, "val");
        Assert.False(item.HasAllProperties(new PivotViewerProperty[] { prop1, prop2 }));
    }

    [Fact]
    public void HasAllProperties_EmptyProperties()
    {
        var item = new PivotViewerItem("1");
        Assert.True(item.HasAllProperties(Array.Empty<PivotViewerProperty>()));
    }

    [Fact]
    public void GetPivotPropertyById()
    {
        var item = new PivotViewerItem("1");
        var prop = new PivotViewerStringProperty("Name");
        item.Add(prop, "Widget");

        var found = item.GetPivotPropertyById("Name");
        Assert.Same(prop, found);
    }

    [Fact]
    public void PropertyChanged_FiresOnAdd()
    {
        var item = new PivotViewerItem("1");
        var prop = new PivotViewerStringProperty("Name");

        PivotViewerPropertyChangedEventArgs? args = null;
        item.PropertyChanged += (s, e) => args = e;

        item.Add(prop, "Widget");

        Assert.NotNull(args);
        Assert.Same(prop, args!.PivotProperty);
    }

    [Fact]
    public void GetValues_Decimal_ReturnsTypedList()
    {
        var item = new PivotViewerItem("1");
        var prop = new PivotViewerNumericProperty("Price");
        item.Add(prop, 19.99m, 29.99m);

        var prices = item.GetValues<decimal>("Price");
        Assert.NotNull(prices);
        Assert.Equal(2, prices!.Count);
        Assert.Equal(19.99m, prices[0]);
        Assert.Equal(29.99m, prices[1]);
    }

    [Fact]
    public void TryGetSingleValue_WrongType_ReturnsFalse()
    {
        var item = new PivotViewerItem("1");
        var prop = new PivotViewerStringProperty("Name");
        item.Add(prop, "Widget");

        Assert.False(item.TryGetSingleValue<int>("Name", out _));
    }

    [Fact]
    public void Remove_AlsoRemovesFromPropertiesCollection()
    {
        var item = new PivotViewerItem("1");
        var prop = new PivotViewerStringProperty("Name");
        item.Add(prop, "Widget");
        Assert.Single(item.Properties);

        item.Remove(prop);
        Assert.Empty(item.Properties);
    }

    [Fact]
    public void Set_MultipleValues_OverwritesAll()
    {
        var item = new PivotViewerItem("1");
        var prop = new PivotViewerStringProperty("Tags");
        item.Add(prop, "Old1", "Old2");
        item.Set(prop, "New1", "New2", "New3");

        var values = item["Tags"];
        Assert.NotNull(values);
        Assert.Equal(3, values!.Count);
        Assert.Equal("New1", values[0]);
        Assert.Equal("New2", values[1]);
        Assert.Equal("New3", values[2]);
    }

    [Fact]
    public void ToString_UsesNameIfAvailable()
    {
        var item = new PivotViewerItem("42");
        var prop = new PivotViewerStringProperty("Name");
        item.Add(prop, "Widget");

        Assert.Equal("Widget (42)", item.ToString());
    }

    [Fact]
    public void ToString_FallsBackToId()
    {
        var item = new PivotViewerItem("42");
        Assert.Equal("42", item.ToString());
    }

    [Fact]
    public void GetPropertyValue_ReturnsValuesForExistingProperty()
    {
        var item = new PivotViewerItem("1");
        var prop = new PivotViewerStringProperty("Color") { DisplayName = "Color" };
        item.Add(prop, "Red", "Blue");

        var values = item.GetPropertyValue("Color");
        Assert.NotNull(values);
        Assert.Equal(2, values!.Count);
        Assert.Equal("Red", values[0]);
        Assert.Equal("Blue", values[1]);
    }

    [Fact]
    public void GetPropertyValue_ReturnsNullForMissingProperty()
    {
        var item = new PivotViewerItem("1");
        Assert.Null(item.GetPropertyValue("NonExistent"));
    }
}
