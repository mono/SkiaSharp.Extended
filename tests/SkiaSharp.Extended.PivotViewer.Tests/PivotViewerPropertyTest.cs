using SkiaSharp.Extended.PivotViewer;

namespace SkiaSharp.Extended.PivotViewer.Tests;

public class PivotViewerPropertyTest
{
    [Fact]
    public void StringProperty_HasCorrectType()
    {
        var prop = new PivotViewerStringProperty("Name");
        Assert.Equal(PivotViewerPropertyType.Text, prop.PropertyType);
        Assert.Equal("Name", prop.Id);
        Assert.Equal("Name", prop.DisplayName);
    }

    [Fact]
    public void NumericProperty_HasCorrectType()
    {
        var prop = new PivotViewerNumericProperty("Price");
        Assert.Equal(PivotViewerPropertyType.Decimal, prop.PropertyType);
    }

    [Fact]
    public void DateTimeProperty_HasCorrectType()
    {
        var prop = new PivotViewerDateTimeProperty("Birthdate");
        Assert.Equal(PivotViewerPropertyType.DateTime, prop.PropertyType);
    }

    [Fact]
    public void LinkProperty_HasCorrectType()
    {
        var prop = new PivotViewerLinkProperty("Website");
        Assert.Equal(PivotViewerPropertyType.Link, prop.PropertyType);
    }

    [Fact]
    public void Property_SetDisplayName()
    {
        var prop = new PivotViewerStringProperty("body_style");
        prop.DisplayName = "Body Style";
        Assert.Equal("Body Style", prop.DisplayName);
    }

    [Fact]
    public void Property_SetFormat()
    {
        var prop = new PivotViewerNumericProperty("displacement");
        prop.Format = "0.#L";
        Assert.Equal("0.#L", prop.Format);
    }

    [Fact]
    public void Property_Options_CanFilter()
    {
        var prop = new PivotViewerStringProperty("Category");
        prop.Options = PivotViewerPropertyOptions.CanFilter;
        Assert.True(prop.CanFilter);
        Assert.False(prop.CanSearchText);
        Assert.False(prop.IsPrivate);
    }

    [Fact]
    public void Property_Options_Combined()
    {
        var prop = new PivotViewerStringProperty("Category");
        prop.Options = PivotViewerPropertyOptions.CanFilter | PivotViewerPropertyOptions.CanSearchText;
        Assert.True(prop.CanFilter);
        Assert.True(prop.CanSearchText);
    }

    [Fact]
    public void Property_WrappingText()
    {
        var prop = new PivotViewerStringProperty("Description");
        prop.Options = PivotViewerPropertyOptions.WrappingText;
        Assert.True(prop.IsWrappingText);
    }

    [Fact]
    public void Property_Lock_PreventsModification()
    {
        var prop = new PivotViewerStringProperty("Name");
        Assert.False(prop.IsLocked);

        prop.Lock();
        Assert.True(prop.IsLocked);

        Assert.Throws<InvalidOperationException>(() => prop.DisplayName = "New Name");
        Assert.Throws<InvalidOperationException>(() => prop.Format = "new");
        Assert.Throws<InvalidOperationException>(() => prop.Options = PivotViewerPropertyOptions.Private);
    }

    [Fact]
    public void Property_Equality_ById()
    {
        var a = new PivotViewerStringProperty("Name");
        var b = new PivotViewerStringProperty("Name");
        var c = new PivotViewerStringProperty("Other");

        Assert.True(a.Equals(b));
        Assert.False(a.Equals(c));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Property_CompareTo()
    {
        var a = new PivotViewerStringProperty("Alpha");
        var b = new PivotViewerStringProperty("Beta");

        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(new PivotViewerStringProperty("Alpha")));
    }

    [Fact]
    public void Property_ToString()
    {
        var prop = new PivotViewerStringProperty("Category");
        Assert.Equal("Category (Text)", prop.ToString());
    }

    [Fact]
    public void StringProperty_HasSortsCollection()
    {
        var prop = new PivotViewerStringProperty("Category");
        Assert.NotNull(prop.Sorts);
        Assert.Empty(prop.Sorts);
    }
}
