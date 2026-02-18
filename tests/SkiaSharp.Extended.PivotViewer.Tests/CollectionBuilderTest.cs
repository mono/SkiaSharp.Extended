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
    public void Build_NullName_Throws()
    {
        var builder = new PivotViewerCollectionBuilder();
        Assert.Throws<ArgumentNullException>(() => builder.WithName(null!));
    }
}
