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

    [Fact]
    public void ItemTemplate_MaxWidth_DefaultIsZero()
    {
        var template = new PivotViewerItemTemplate();
        Assert.Equal(0, template.MaxWidth);
    }

    [Fact]
    public void ItemTemplate_RenderAction_DefaultIsNull()
    {
        var template = new PivotViewerItemTemplate();
        Assert.Null(template.RenderAction);
    }

    [Fact]
    public void ItemTemplate_MaxWidth_SetAndGet()
    {
        var template = new PivotViewerItemTemplate { MaxWidth = 512 };
        Assert.Equal(512, template.MaxWidth);
    }

    [Fact]
    public void ItemTemplateCollection_AddRemove_Works()
    {
        var coll = new PivotViewerItemTemplateCollection();
        var t1 = new PivotViewerItemTemplate { MaxWidth = 100 };
        var t2 = new PivotViewerItemTemplate { MaxWidth = 200 };

        coll.Add(t1);
        coll.Add(t2);
        Assert.Equal(2, coll.Count);

        coll.Remove(t1);
        Assert.Single(coll);
        Assert.Same(t2, coll[0]);
    }

    [Fact]
    public void ItemTemplateCollection_SelectTemplate_SingleTemplate_AlwaysReturnsIt()
    {
        var coll = new PivotViewerItemTemplateCollection();
        var t = new PivotViewerItemTemplate { MaxWidth = 100 };
        coll.Add(t);

        Assert.Same(t, coll.SelectTemplate(50));
        Assert.Same(t, coll.SelectTemplate(100));
        Assert.Same(t, coll.SelectTemplate(500));
    }

    [Fact]
    public void ItemTemplate_RenderAction_InvokedWithCorrectArgs()
    {
        PivotViewerItem? receivedItem = null;
        SKRect receivedBounds = default;

        var template = new PivotViewerItemTemplate
        {
            MaxWidth = 300,
            RenderAction = (canvas, item, bounds) =>
            {
                receivedItem = item;
                receivedBounds = bounds;
            }
        };

        var testItem = new PivotViewerItem("test-id");
        var testBounds = new SKRect(10, 20, 110, 120);

        using var surface = SKSurface.Create(new SKImageInfo(200, 200));
        template.RenderAction!(surface.Canvas, testItem, testBounds);

        Assert.Same(testItem, receivedItem);
        Assert.Equal(testBounds, receivedBounds);
    }

    [Fact]
    public void MultiSizeImage_Stretch_DefaultIsUniform()
    {
        var img = new PivotViewerMultiSizeImage();
        Assert.Equal(PivotViewerStretch.Uniform, img.Stretch);
    }

    [Fact]
    public void MultiSizeImage_Stretch_CanBeChanged()
    {
        var img = new PivotViewerMultiSizeImage();
        img.Stretch = PivotViewerStretch.Fill;
        Assert.Equal(PivotViewerStretch.Fill, img.Stretch);
    }

    [Fact]
    public void MultiSizeImage_Sources_CanAddItems()
    {
        var img = new PivotViewerMultiSizeImage();
        img.Sources.Add(new PivotViewerMultiSizeImageSource { UriSource = "a.jpg", MaxWidth = 100, MaxHeight = 100 });
        img.Sources.Add(new PivotViewerMultiSizeImageSource { UriSource = "b.jpg", MaxWidth = 200, MaxHeight = 200 });

        Assert.Equal(2, img.Sources.Count);
        Assert.Equal("a.jpg", img.Sources[0].UriSource);
        Assert.Equal("b.jpg", img.Sources[1].UriSource);
    }

    [Fact]
    public void MultiSizeImageSource_AllProperties()
    {
        var src = new PivotViewerMultiSizeImageSource();
        Assert.Null(src.UriSource);
        Assert.Equal(0, src.MaxWidth);
        Assert.Equal(0, src.MaxHeight);

        src.UriSource = "http://example.com/img.png";
        src.MaxWidth = 640;
        src.MaxHeight = 480;

        Assert.Equal("http://example.com/img.png", src.UriSource);
        Assert.Equal(640, src.MaxWidth);
        Assert.Equal(480, src.MaxHeight);
    }

    [Fact]
    public void MultiScaleSubImageHost_DefaultStretchIsUniform()
    {
        var host = new PivotViewerMultiScaleSubImageHost();
        Assert.Equal(PivotViewerStretch.Uniform, host.Stretch);
    }

    [Fact]
    public void MultiScaleSubImageHost_AllProperties()
    {
        var host = new PivotViewerMultiScaleSubImageHost();

        Assert.Null(host.CollectionSource);
        Assert.Null(host.ImageId);
        Assert.Equal(PivotViewerStretch.Uniform, host.Stretch);

        host.ImageId = "#10";
        host.Stretch = PivotViewerStretch.UniformToFill;

        Assert.Equal("#10", host.ImageId);
        Assert.Equal(PivotViewerStretch.UniformToFill, host.Stretch);
    }

    [Fact]
    public void MultiScaleSubImageHost_CollectionSource_CanBeSet()
    {
        var host = new PivotViewerMultiScaleSubImageHost();
        var source = CxmlCollectionSource.Parse(
            @"<?xml version=""1.0"" encoding=""utf-8""?>
            <Collection xmlns=""http://schemas.microsoft.com/collection/metadata/2009"" Name=""Test"">
                <FacetCategories /><Items />
            </Collection>");

        host.CollectionSource = source;
        Assert.Same(source, host.CollectionSource);
    }

    [Fact]
    public void PivotViewerStretch_EnumValues()
    {
        Assert.Equal(0, (int)PivotViewerStretch.None);
        Assert.Equal(1, (int)PivotViewerStretch.Fill);
        Assert.Equal(2, (int)PivotViewerStretch.Uniform);
        Assert.Equal(3, (int)PivotViewerStretch.UniformToFill);
    }
}
