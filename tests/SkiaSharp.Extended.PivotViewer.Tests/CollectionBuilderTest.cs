using SkiaSharp.Extended.PivotViewer;
using Xunit;

namespace SkiaSharp.Extended.PivotViewer.Tests;

public class CollectionBuilderTest
{
    [Fact]
    public void Build_EmptyCollection()
    {
        var builder = new PivotViewerCollectionBuilder()
            .WithName("Empty");

        var (items, properties) = builder.Build();
        Assert.Empty(items);
        Assert.Empty(properties);
    }

    [Fact]
    public void Build_WithProperties_CreatesAll()
    {
        var builder = new PivotViewerCollectionBuilder()
            .WithName("Cars")
            .AddStringProperty("make", "Make")
            .AddNumericProperty("year", "Year", "###0")
            .AddDateTimeProperty("date", "Date")
            .AddLinkProperty("url", "Website");

        var (_, properties) = builder.Build();
        Assert.Equal(4, properties.Count);
    }

    [Fact]
    public void Build_WithItems_FluentValues()
    {
        var builder = new PivotViewerCollectionBuilder()
            .AddStringProperty("color", "Color")
            .AddNumericProperty("speed", "Speed");

        builder.AddItem("car-1", item =>
        {
            item.Set("color", "Red");
            item.Set("speed", 200.0);
        });
        builder.AddItem("car-2", item =>
        {
            item.Set("color", "Blue");
            item.Set("speed", 150.0);
        });

        var (items, _) = builder.Build();
        Assert.Equal(2, items.Count);
        Assert.Equal("Red", items[0]["color"]![0]!.ToString());
        Assert.Equal(200.0, items[0]["speed"]![0]);
    }

    [Fact]
    public void Build_PropertiesAreLocked()
    {
        var builder = new PivotViewerCollectionBuilder()
            .AddStringProperty("test", "Test");

        var (_, properties) = builder.Build();
        Assert.True(properties[0].IsLocked);
    }

    [Fact]
    public void Build_WithCopyright()
    {
        var builder = new PivotViewerCollectionBuilder()
            .WithName("Licensed")
            .WithCopyright("Test Corp", "https://test.com");

        builder.Build(); // Should not throw
    }

    [Fact]
    public void Build_AddPreConfiguredItem()
    {
        var prop = new PivotViewerStringProperty("name") { DisplayName = "Name" };
        var item = new PivotViewerItem("pre-1");
        item.Add(prop, "Preconfigured");

        var builder = new PivotViewerCollectionBuilder()
            .AddProperty(prop)
            .AddItem(item);

        var (items, properties) = builder.Build();
        Assert.Single(items);
        Assert.Single(properties);
    }

    [Fact]
    public void Build_WithCopyright_ChainableAndBuildable()
    {
        var builder = new PivotViewerCollectionBuilder()
            .WithName("Licensed")
            .WithCopyright("Acme Corp", "https://acme.com/license")
            .AddStringProperty("name", "Name");

        builder.AddItem("1", item => item.Set("name", "Widget"));

        var (items, properties) = builder.Build();
        Assert.Single(items);
        Assert.Single(properties);
        Assert.Equal("Widget", items[0]["name"]![0]!.ToString());
    }

    [Fact]
    public void ItemBuilder_SetsMultipleProperties()
    {
        var builder = new PivotViewerCollectionBuilder()
            .AddStringProperty("name", "Name")
            .AddNumericProperty("price", "Price")
            .AddStringProperty("color", "Color");

        builder.AddItem("item-1", item =>
        {
            item.Set("name", "Widget")
                .Set("price", 9.99)
                .Set("color", "Red", "Blue");
        });

        var (items, _) = builder.Build();
        Assert.Single(items);
        var item = items[0];
        Assert.Equal("Widget", item["name"]![0]!.ToString());
        Assert.Equal(9.99, item["price"]![0]);
        Assert.Equal(2, item["color"]!.Count);
    }

    [Fact]
    public void Build_NullName_Throws()
    {
        var builder = new PivotViewerCollectionBuilder();
        Assert.Throws<ArgumentNullException>(() => builder.WithName(null!));
    }

    [Fact]
    public void AddProperty_Null_Throws()
    {
        var builder = new PivotViewerCollectionBuilder();
        Assert.Throws<ArgumentNullException>(() => builder.AddProperty(null!));
    }

    [Fact]
    public void AddItem_NullItem_Throws()
    {
        var builder = new PivotViewerCollectionBuilder();
        Assert.Throws<ArgumentNullException>(() => builder.AddItem((PivotViewerItem)null!));
    }

    [Fact]
    public void ItemBuilder_SetUnknownPropertyId_Ignored()
    {
        var builder = new PivotViewerCollectionBuilder()
            .AddStringProperty("name", "Name");

        builder.AddItem("item-1", item =>
        {
            item.Set("nonexistent", "value");
        });

        var (items, _) = builder.Build();
        Assert.Single(items);
        Assert.Null(items[0]["nonexistent"]);
    }

    [Fact]
    public void ItemBuilder_SetWithPropertyObject()
    {
        var prop = new PivotViewerStringProperty("tag") { DisplayName = "Tag" };
        var builder = new PivotViewerCollectionBuilder()
            .AddProperty(prop);

        builder.AddItem("item-1", item =>
        {
            item.Set(prop, "Important");
        });

        var (items, _) = builder.Build();
        Assert.Equal("Important", items[0]["tag"]![0]!.ToString());
    }

    [Fact]
    public void Build_LocksAllProperties()
    {
        var builder = new PivotViewerCollectionBuilder()
            .AddStringProperty("a", "A")
            .AddNumericProperty("b", "B")
            .AddDateTimeProperty("c", "C")
            .AddLinkProperty("d", "D");

        var (_, properties) = builder.Build();

        foreach (var prop in properties)
            Assert.True(prop.IsLocked);
    }

    [Fact]
    public void Build_PropertyOptionsRespected()
    {
        var builder = new PivotViewerCollectionBuilder()
            .AddStringProperty("search", "Search", PivotViewerPropertyOptions.CanSearchText);

        var (_, properties) = builder.Build();

        Assert.True(properties[0].CanSearchText);
        Assert.False(properties[0].CanFilter);
    }

    [Fact]
    public void AddNumericProperty_WithFormat()
    {
        var builder = new PivotViewerCollectionBuilder()
            .AddNumericProperty("price", "Price", "$#,##0.00");

        var (_, properties) = builder.Build();

        Assert.Equal("$#,##0.00", properties[0].Format);
    }
}
