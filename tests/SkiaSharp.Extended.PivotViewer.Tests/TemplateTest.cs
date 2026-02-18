using SkiaSharp.Extended.PivotViewer;
using Xunit;

namespace SkiaSharp.Extended.PivotViewer.Tests;

public class TemplateTest
{
    [Fact]
    public void ItemTemplateCollection_SelectTemplate_EmptyReturnsNull()
    {
        var coll = new PivotViewerItemTemplateCollection();
        Assert.Null(coll.SelectTemplate(100));
    }

    [Fact]
    public void ItemTemplateCollection_SelectTemplate_ExactMatch()
    {
        var coll = new PivotViewerItemTemplateCollection();
        coll.Add(new PivotViewerItemTemplate { MaxWidth = 50 });
        coll.Add(new PivotViewerItemTemplate { MaxWidth = 100 });
        coll.Add(new PivotViewerItemTemplate { MaxWidth = 200 });

        var result = coll.SelectTemplate(100);
        Assert.Equal(100, result!.MaxWidth);
    }

    [Fact]
    public void ItemTemplateCollection_SelectTemplate_PicksSmallestSufficient()
    {
        var coll = new PivotViewerItemTemplateCollection();
        coll.Add(new PivotViewerItemTemplate { MaxWidth = 50 });
        coll.Add(new PivotViewerItemTemplate { MaxWidth = 200 });
        coll.Add(new PivotViewerItemTemplate { MaxWidth = 400 });

        var result = coll.SelectTemplate(75);
        Assert.Equal(200, result!.MaxWidth);
    }

    [Fact]
    public void ItemTemplateCollection_SelectTemplate_LargerThanAll_ReturnsLargest()
    {
        var coll = new PivotViewerItemTemplateCollection();
        coll.Add(new PivotViewerItemTemplate { MaxWidth = 50 });
        coll.Add(new PivotViewerItemTemplate { MaxWidth = 100 });

        var result = coll.SelectTemplate(500);
        Assert.Equal(100, result!.MaxWidth);
    }

    [Fact]
    public void ItemTemplateCollection_SelectTemplate_SmallerThanAll_ReturnsSmallest()
    {
        var coll = new PivotViewerItemTemplateCollection();
        coll.Add(new PivotViewerItemTemplate { MaxWidth = 100 });
        coll.Add(new PivotViewerItemTemplate { MaxWidth = 200 });

        var result = coll.SelectTemplate(10);
        Assert.Equal(100, result!.MaxWidth);
    }

    [Fact]
    public void MultiSizeImage_HasSources()
    {
        var img = new PivotViewerMultiSizeImage();
        Assert.NotNull(img.Sources);
        Assert.Empty(img.Sources);
    }

    [Fact]
    public void MultiSizeImageSource_Properties()
    {
        var source = new PivotViewerMultiSizeImageSource
        {
            UriSource = "https://example.com/image.jpg",
            MaxWidth = 1024,
            MaxHeight = 768
        };
        Assert.Equal("https://example.com/image.jpg", source.UriSource);
        Assert.Equal(1024, source.MaxWidth);
        Assert.Equal(768, source.MaxHeight);
    }

    [Fact]
    public void MultiScaleSubImageHost_Properties()
    {
        var host = new PivotViewerMultiScaleSubImageHost();
        Assert.Null(host.CollectionSource);
        Assert.Null(host.ImageId);

        host.ImageId = "#5";
        Assert.Equal("#5", host.ImageId);
    }

    [Fact]
    public void ItemTemplate_RenderAction_CanBeSet()
    {
        bool called = false;
        var template = new PivotViewerItemTemplate
        {
            MaxWidth = 200,
            RenderAction = (canvas, item, bounds) => called = true
        };

        using var surface = SKSurface.Create(new SKImageInfo(100, 100));
        template.RenderAction!(surface.Canvas, new PivotViewerItem("test"), new SKRect(0, 0, 100, 100));
        Assert.True(called);
    }
}
