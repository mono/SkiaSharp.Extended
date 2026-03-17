using SkiaSharp.Extended;

namespace SkiaSharp.Extended.DeepZoom.Tests;

public class DzcTileSourceTest
{
    [Fact]
    public void Parse_EmbeddedConceptCarsDzc_ParsesCorrectly()
    {
        using var stream = TestDataHelper.GetStream("conceptcars.dzc");
        var dzc = SKDeepZoomCollectionSource.Parse(stream);

        Assert.Equal(8, dzc.MaxLevel);
        Assert.Equal(256, dzc.TileSize);
        Assert.Equal("jpg", dzc.Format);
        Assert.True(dzc.ItemCount > 0);
        Assert.Equal(298, dzc.NextItemId);
    }

    [Fact]
    public void Parse_ConceptCarsDzc_HasCorrectItemCount()
    {
        using var stream = TestDataHelper.GetStream("conceptcars.dzc");
        var dzc = SKDeepZoomCollectionSource.Parse(stream);

        // The DZC has many items with IsPath=1
        Assert.True(dzc.ItemCount > 50);
    }

    [Fact]
    public void Parse_ConceptCarsDzc_FirstItemHasCorrectProperties()
    {
        using var stream = TestDataHelper.GetStream("conceptcars.dzc");
        var dzc = SKDeepZoomCollectionSource.Parse(stream);

        var first = dzc.Items[0];
        Assert.Equal(0, first.Id);
        Assert.Equal(0, first.MortonIndex);
        Assert.Equal(549, first.Width);
        Assert.Equal(366, first.Height);
        Assert.NotNull(first.Source);
    }

    [Fact]
    public void Parse_ValidDzcXml_ParsesCorrectly()
    {
        var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Collection MaxLevel=""7"" TileSize=""256"" Format=""jpg"" NextItemId=""3""
    xmlns=""http://schemas.microsoft.com/deepzoom/2008"">
    <Items>
        <I Id=""0"" N=""0"" IsPath=""1"" Source=""img0.dzi"">
            <Size Width=""800"" Height=""600"" />
        </I>
        <I Id=""1"" N=""1"" IsPath=""1"" Source=""img1.dzi"">
            <Size Width=""1024"" Height=""768"" />
        </I>
        <I Id=""2"" N=""2"">
            <Size Width=""500"" Height=""500"" />
            <Viewport Width=""0.5"" X=""-1.0"" Y=""-0.5"" />
        </I>
    </Items>
</Collection>";

        var dzc = SKDeepZoomCollectionSource.Parse(xml);

        Assert.Equal(7, dzc.MaxLevel);
        Assert.Equal(256, dzc.TileSize);
        Assert.Equal("jpg", dzc.Format);
        Assert.Equal(3, dzc.NextItemId);
        Assert.Equal(3, dzc.ItemCount);
    }

    [Fact]
    public void Parse_DzcWithViewport_ParsesViewportCoordinates()
    {
        var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Collection MaxLevel=""7"" TileSize=""256"" Format=""jpg"" xmlns=""http://schemas.microsoft.com/deepzoom/2008"">
    <Items>
        <I Id=""0"" N=""0"">
            <Size Width=""500"" Height=""500"" />
            <Viewport Width=""0.5"" X=""-1.0"" Y=""-0.5"" />
        </I>
    </Items>
</Collection>";

        var dzc = SKDeepZoomCollectionSource.Parse(xml);
        var item = dzc.Items[0];

        Assert.Equal(0.5, item.ViewportWidth);
        Assert.Equal(-1.0, item.ViewportX);
        Assert.Equal(-0.5, item.ViewportY);
    }

    [Fact]
    public void MortonToGrid_KnownValues()
    {
        // Morton index 0 → (0,0)
        Assert.Equal((0, 0), SKDeepZoomCollectionSource.MortonToGrid(0));
        // Morton index 1 → (1,0)
        Assert.Equal((1, 0), SKDeepZoomCollectionSource.MortonToGrid(1));
        // Morton index 2 → (0,1)
        Assert.Equal((0, 1), SKDeepZoomCollectionSource.MortonToGrid(2));
        // Morton index 3 → (1,1)
        Assert.Equal((1, 1), SKDeepZoomCollectionSource.MortonToGrid(3));
        // Morton index 4 → (2,0)
        Assert.Equal((2, 0), SKDeepZoomCollectionSource.MortonToGrid(4));
    }

    [Fact]
    public void GridToMorton_RoundTrips()
    {
        for (int i = 0; i < 100; i++)
        {
            var (col, row) = SKDeepZoomCollectionSource.MortonToGrid(i);
            int morton = SKDeepZoomCollectionSource.GridToMorton(col, row);
            Assert.Equal(i, morton);
        }
    }

    [Fact]
    public void GetMortonGridSize_CalculatesCorrectly()
    {
        var xml = @"<Collection MaxLevel=""7"" TileSize=""256"" Format=""jpg"" xmlns=""http://schemas.microsoft.com/deepzoom/2008"">
    <Items>
        <I Id=""0"" N=""0""><Size Width=""100"" Height=""100"" /></I>
        <I Id=""1"" N=""1""><Size Width=""100"" Height=""100"" /></I>
        <I Id=""2"" N=""2""><Size Width=""100"" Height=""100"" /></I>
        <I Id=""3"" N=""3""><Size Width=""100"" Height=""100"" /></I>
    </Items>
</Collection>";

        var dzc = SKDeepZoomCollectionSource.Parse(xml);
        // 4 items → ceil(sqrt(4)) = 2, ceil(log2(2)) = 1, 2^1 = 2
        Assert.Equal(2, dzc.GetMortonGridSize());
    }

    [Fact]
    public void GetCompositeTileUrl_FormatsCorrectly()
    {
        var dzc = new SKDeepZoomCollectionSource(8, 256, "jpg", Array.Empty<SKDeepZoomCollectionSubImage>());
        Assert.Equal("5/3_2.jpg", dzc.GetCompositeTileUrl(5, 3, 2));
    }

    [Fact]
    public void Parse_MissingCollection_ThrowsFormatException()
    {
        var xml = @"<Root xmlns=""http://schemas.microsoft.com/deepzoom/2008""></Root>";
        Assert.Throws<FormatException>(() => SKDeepZoomCollectionSource.Parse(xml));
    }

    [Fact]
    public void Parse_2009Namespace_ParsesCorrectly()
    {
        var xml = @"<Collection MaxLevel=""5"" TileSize=""256"" Format=""png"" xmlns=""http://schemas.microsoft.com/deepzoom/2009"">
    <Items>
        <I Id=""0"" N=""0""><Size Width=""100"" Height=""100"" /></I>
    </Items>
</Collection>";

        var dzc = SKDeepZoomCollectionSource.Parse(xml);
        Assert.Equal(5, dzc.MaxLevel);
        Assert.Equal("png", dzc.Format);
        Assert.Equal(1, dzc.ItemCount);
    }

    [Fact]
    public void SubImage_AspectRatio_CalculatedCorrectly()
    {
        var subImage = new SKDeepZoomCollectionSubImage(0, 0, 800, 600, null);
        Assert.Equal(800.0 / 600.0, subImage.AspectRatio, 6);
    }

    [Fact]
    public void SubImage_ZeroHeight_AspectRatioIs1()
    {
        var subImage = new SKDeepZoomCollectionSubImage(0, 0, 100, 0, null);
        Assert.Equal(1.0, subImage.AspectRatio);
    }

    [Fact]
    public void Parse_NoCollectionElement_ThrowsFormatException()
    {
        // XML with a root element that is not <Collection> in either DZ namespace
        var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?><Images><I Id=""0"" /></Images>";
        Assert.Throws<FormatException>(() => SKDeepZoomCollectionSource.Parse(xml));
    }

    [Fact]
    public void MortonToGrid_HigherIndices()
    {
        // Index 5 → col=3, row=0
        Assert.Equal((3, 0), SKDeepZoomCollectionSource.MortonToGrid(5));
        // Index 6 → col=2, row=1
        Assert.Equal((2, 1), SKDeepZoomCollectionSource.MortonToGrid(6));
        // Index 7 → col=3, row=1
        Assert.Equal((3, 1), SKDeepZoomCollectionSource.MortonToGrid(7));
    }

    [Fact]
    public void GetCompositeTileUrl_DifferentLevelsAndPositions()
    {
        var dzc = new SKDeepZoomCollectionSource(8, 256, "png", Array.Empty<SKDeepZoomCollectionSubImage>());

        Assert.Equal("0/0_0.png", dzc.GetCompositeTileUrl(0, 0, 0));
        Assert.Equal("3/1_2.png", dzc.GetCompositeTileUrl(3, 1, 2));
        Assert.Equal("8/10_5.png", dzc.GetCompositeTileUrl(8, 10, 5));
    }

    [Fact]
    public void TilesBaseUri_CanBeSetAndRetrieved()
    {
        var dzc = new SKDeepZoomCollectionSource(8, 256, "jpg", Array.Empty<SKDeepZoomCollectionSubImage>());
        Assert.Null(dzc.TilesBaseUri);

        dzc.TilesBaseUri = "http://example.com/tiles";
        Assert.Equal("http://example.com/tiles", dzc.TilesBaseUri);
    }

    [Fact]
    public void Parse_WithBaseUri_SetsTilesBaseUri()
    {
        var xml = @"<Collection MaxLevel=""7"" TileSize=""256"" Format=""jpg"" xmlns=""http://schemas.microsoft.com/deepzoom/2008"">
    <Items>
        <I Id=""0"" N=""0""><Size Width=""100"" Height=""100"" /></I>
    </Items>
</Collection>";

        using var stream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(xml));
        var dzc = SKDeepZoomCollectionSource.Parse(stream, "http://example.com/collection.dzc");

        Assert.NotNull(dzc.TilesBaseUri);
        Assert.Contains("example.com", dzc.TilesBaseUri!);
    }

    [Fact]
    public void SubImage_WidthAndHeight_AreCorrect()
    {
        var subImage = new SKDeepZoomCollectionSubImage(0, 0, 1920, 1080, "test.dzi");
        Assert.Equal(1920, subImage.Width);
        Assert.Equal(1080, subImage.Height);
        Assert.Equal("test.dzi", subImage.Source);
    }

    [Fact]
    public void SubImage_ViewportProperties_Default()
    {
        var subImage = new SKDeepZoomCollectionSubImage(0, 0, 100, 100, null);
        // Default viewport properties
        Assert.Equal(0.0, subImage.ViewportX);
        Assert.Equal(0.0, subImage.ViewportY);
    }

    [Fact]
    public void GridToMorton_SpecificValues()
    {
        Assert.Equal(0, SKDeepZoomCollectionSource.GridToMorton(0, 0));
        Assert.Equal(1, SKDeepZoomCollectionSource.GridToMorton(1, 0));
        Assert.Equal(2, SKDeepZoomCollectionSource.GridToMorton(0, 1));
        Assert.Equal(3, SKDeepZoomCollectionSource.GridToMorton(1, 1));
        Assert.Equal(4, SKDeepZoomCollectionSource.GridToMorton(2, 0));
        Assert.Equal(5, SKDeepZoomCollectionSource.GridToMorton(3, 0));
    }

    [Fact]
    public void Parse_ItemsWithIsPathAndSource()
    {
        var xml = @"<Collection MaxLevel=""8"" TileSize=""256"" Format=""jpg"" NextItemId=""3""
            xmlns=""http://schemas.microsoft.com/deepzoom/2008"">
            <Items>
                <I Id=""0"" N=""0"" IsPath=""1"" Source=""images/img0.dzi"">
                    <Size Width=""1024"" Height=""768"" />
                </I>
                <I Id=""1"" N=""1"" IsPath=""1"" Source=""images/img1.dzi"">
                    <Size Width=""640"" Height=""480"" />
                </I>
                <I Id=""2"" N=""2"">
                    <Size Width=""320"" Height=""240"" />
                </I>
            </Items>
        </Collection>";

        var dzc = SKDeepZoomCollectionSource.Parse(xml);
        Assert.Equal(3, dzc.ItemCount);

        // Items with IsPath=1 have Source
        Assert.Equal("images/img0.dzi", dzc.Items[0].Source);
        Assert.Equal("images/img1.dzi", dzc.Items[1].Source);
        // Item without IsPath has no Source
        Assert.Null(dzc.Items[2].Source);
    }

    [Fact]
    public void Parse_MultipleItemsWithDifferentAspectRatios()
    {
        var xml = @"<Collection MaxLevel=""7"" TileSize=""256"" Format=""png""
            xmlns=""http://schemas.microsoft.com/deepzoom/2008"">
            <Items>
                <I Id=""0"" N=""0""><Size Width=""1920"" Height=""1080"" /></I>
                <I Id=""1"" N=""1""><Size Width=""500"" Height=""500"" /></I>
                <I Id=""2"" N=""2""><Size Width=""300"" Height=""600"" /></I>
                <I Id=""3"" N=""3""><Size Width=""800"" Height=""200"" /></I>
            </Items>
        </Collection>";

        var dzc = SKDeepZoomCollectionSource.Parse(xml);
        Assert.Equal(4, dzc.ItemCount);

        // 16:9
        Assert.Equal(1920.0 / 1080.0, dzc.Items[0].AspectRatio, 6);
        // 1:1
        Assert.Equal(1.0, dzc.Items[1].AspectRatio, 6);
        // 1:2 (portrait)
        Assert.Equal(0.5, dzc.Items[2].AspectRatio, 6);
        // 4:1 (wide)
        Assert.Equal(4.0, dzc.Items[3].AspectRatio, 6);
    }

    [Fact]
    public void Parse_MissingOptionalAttributes_UsesDefaults()
    {
        var xml = @"<Collection xmlns=""http://schemas.microsoft.com/deepzoom/2008"">
            <Items>
                <I><Size Width=""100"" Height=""100"" /></I>
            </Items>
        </Collection>";

        var dzc = SKDeepZoomCollectionSource.Parse(xml);

        // MaxLevel defaults to 0, TileSize defaults to 256, Format to "jpg"
        Assert.Equal(0, dzc.MaxLevel);
        Assert.Equal(256, dzc.TileSize);
        Assert.Equal("jpg", dzc.Format);
        Assert.Equal(0, dzc.NextItemId);
        Assert.Single(dzc.Items);

        // Item defaults: Id=0, N=0, no Source
        Assert.Equal(0, dzc.Items[0].Id);
        Assert.Equal(0, dzc.Items[0].MortonIndex);
        Assert.Null(dzc.Items[0].Source);
    }

    [Fact]
    public void Parse_ItemsWithFullViewportProperties()
    {
        var xml = @"<Collection MaxLevel=""7"" TileSize=""256"" Format=""jpg""
            xmlns=""http://schemas.microsoft.com/deepzoom/2008"">
            <Items>
                <I Id=""0"" N=""0"">
                    <Size Width=""800"" Height=""600"" />
                    <Viewport Width=""2.5"" X=""-0.75"" Y=""-0.33"" />
                </I>
                <I Id=""1"" N=""1"">
                    <Size Width=""640"" Height=""480"" />
                    <Viewport Width=""1.0"" X=""0.0"" Y=""0.0"" />
                </I>
                <I Id=""2"" N=""2"">
                    <Size Width=""1024"" Height=""768"" />
                </I>
            </Items>
        </Collection>";

        var dzc = SKDeepZoomCollectionSource.Parse(xml);

        // First item has viewport
        Assert.Equal(2.5, dzc.Items[0].ViewportWidth);
        Assert.Equal(-0.75, dzc.Items[0].ViewportX);
        Assert.Equal(-0.33, dzc.Items[0].ViewportY);

        // Second item has viewport at origin
        Assert.Equal(1.0, dzc.Items[1].ViewportWidth);
        Assert.Equal(0.0, dzc.Items[1].ViewportX);
        Assert.Equal(0.0, dzc.Items[1].ViewportY);

        // Third item has no viewport → defaults
        Assert.Equal(0.0, dzc.Items[2].ViewportWidth);
        Assert.Equal(0.0, dzc.Items[2].ViewportX);
        Assert.Equal(0.0, dzc.Items[2].ViewportY);
    }

    [Fact]
    public void GetCompositeTileUrl_VaryingLevelsAndFormats()
    {
        var jpgDzc = new SKDeepZoomCollectionSource(8, 256, "jpg", Array.Empty<SKDeepZoomCollectionSubImage>());
        var pngDzc = new SKDeepZoomCollectionSource(8, 256, "png", Array.Empty<SKDeepZoomCollectionSubImage>());

        Assert.Equal("0/0_0.jpg", jpgDzc.GetCompositeTileUrl(0, 0, 0));
        Assert.Equal("0/0_0.png", pngDzc.GetCompositeTileUrl(0, 0, 0));
        Assert.Equal("7/15_31.jpg", jpgDzc.GetCompositeTileUrl(7, 15, 31));
        Assert.Equal("10/100_200.jpg", jpgDzc.GetCompositeTileUrl(10, 100, 200));
    }

    [Fact]
    public void Parse_EmptyItems_Element()
    {
        var xml = @"<Collection MaxLevel=""5"" TileSize=""128"" Format=""png""
            xmlns=""http://schemas.microsoft.com/deepzoom/2008"">
            <Items />
        </Collection>";

        var dzc = SKDeepZoomCollectionSource.Parse(xml);
        Assert.Equal(0, dzc.ItemCount);
        Assert.Empty(dzc.Items);
        Assert.Equal(0, dzc.GetMortonGridSize());
    }

    [Fact]
    public void Constructor_InvalidTileSize_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new SKDeepZoomCollectionSource(8, 0, "jpg", Array.Empty<SKDeepZoomCollectionSubImage>()));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new SKDeepZoomCollectionSource(8, -1, "jpg", Array.Empty<SKDeepZoomCollectionSubImage>()));
    }

    [Fact]
    public void Constructor_EmptyFormat_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new SKDeepZoomCollectionSource(8, 256, "", Array.Empty<SKDeepZoomCollectionSubImage>()));
    }

    [Fact]
    public void SubImage_ViewportProperties_SetAndGet()
    {
        var sub = new SKDeepZoomCollectionSubImage(0, 0, 100, 100, null);
        sub.ViewportWidth = 3.5;
        sub.ViewportX = -1.5;
        sub.ViewportY = -0.75;

        Assert.Equal(3.5, sub.ViewportWidth);
        Assert.Equal(-1.5, sub.ViewportX);
        Assert.Equal(-0.75, sub.ViewportY);
    }

    [Fact]
    public void Parse_LargeCollection_MortonGridSizeScales()
    {
        // 16 items → ceil(sqrt(16)) = 4, ceil(log2(4)) = 2, 2^2 = 4
        var items = string.Join("\n",
            Enumerable.Range(0, 16).Select(i =>
                $"<I Id=\"{i}\" N=\"{i}\"><Size Width=\"100\" Height=\"100\" /></I>"));

        var xml = $@"<Collection MaxLevel=""7"" TileSize=""256"" Format=""jpg""
            xmlns=""http://schemas.microsoft.com/deepzoom/2008"">
            <Items>{items}</Items>
        </Collection>";

        var dzc = SKDeepZoomCollectionSource.Parse(xml);
        Assert.Equal(16, dzc.ItemCount);
        Assert.Equal(4, dzc.GetMortonGridSize());
    }

    // --- Additional MortonToGrid tests ---

    [Fact]
    public void MortonToGrid_LargeIndices()
    {
        // Index 8 → (0,2), Index 9 → (1,2), Index 10 → (0,3), Index 15 → (3,3)
        Assert.Equal((0, 2), SKDeepZoomCollectionSource.MortonToGrid(8));
        Assert.Equal((1, 2), SKDeepZoomCollectionSource.MortonToGrid(9));
        Assert.Equal((0, 3), SKDeepZoomCollectionSource.MortonToGrid(10));
        Assert.Equal((3, 3), SKDeepZoomCollectionSource.MortonToGrid(15));
    }

    [Fact]
    public void MortonToGrid_PowerOfFourIndices()
    {
        // Powers of 4 map to increasing columns: 4^n → col=2^n, row=0
        // 16 = 4^2, deinterleave: bit4 → col bit 2 → col=4, row=0
        var (c16, r16) = SKDeepZoomCollectionSource.MortonToGrid(16);
        Assert.Equal(4, c16);
        Assert.Equal(0, r16);
    }

    // --- Additional GetMortonGridSize tests ---

    [Fact]
    public void GetMortonGridSize_SingleItem_Returns1()
    {
        var xml = @"<Collection MaxLevel=""7"" TileSize=""256"" Format=""jpg""
            xmlns=""http://schemas.microsoft.com/deepzoom/2008"">
            <Items><I Id=""0"" N=""0""><Size Width=""100"" Height=""100"" /></I></Items>
        </Collection>";
        var dzc = SKDeepZoomCollectionSource.Parse(xml);
        Assert.Equal(1, dzc.GetMortonGridSize());
    }

    [Fact]
    public void GetMortonGridSize_FiveItems_Returns4()
    {
        // 5 items → ceil(sqrt(5)) ≈ 3, ceil(log2(3)) = 2, 2^2 = 4
        var items = string.Join("\n",
            Enumerable.Range(0, 5).Select(i =>
                $"<I Id=\"{i}\" N=\"{i}\"><Size Width=\"100\" Height=\"100\" /></I>"));
        var xml = $@"<Collection MaxLevel=""7"" TileSize=""256"" Format=""jpg""
            xmlns=""http://schemas.microsoft.com/deepzoom/2008"">
            <Items>{items}</Items>
        </Collection>";
        var dzc = SKDeepZoomCollectionSource.Parse(xml);
        Assert.Equal(4, dzc.GetMortonGridSize());
    }

    [Fact]
    public void GetMortonGridSize_TwoItems_Returns2()
    {
        var items = string.Join("\n",
            Enumerable.Range(0, 2).Select(i =>
                $"<I Id=\"{i}\" N=\"{i}\"><Size Width=\"100\" Height=\"100\" /></I>"));
        var xml = $@"<Collection MaxLevel=""7"" TileSize=""256"" Format=""jpg""
            xmlns=""http://schemas.microsoft.com/deepzoom/2008"">
            <Items>{items}</Items>
        </Collection>";
        var dzc = SKDeepZoomCollectionSource.Parse(xml);
        Assert.Equal(2, dzc.GetMortonGridSize());
    }

    // --- Additional GetCompositeTileUrl tests ---

    [Fact]
    public void GetCompositeTileUrl_MatchesDziTileUrlFormat()
    {
        var dzc = new SKDeepZoomCollectionSource(8, 256, "jpg", Array.Empty<SKDeepZoomCollectionSubImage>());
        // URL format should match "{level}/{col}_{row}.{format}"
        Assert.Equal("0/0_0.jpg", dzc.GetCompositeTileUrl(0, 0, 0));
        Assert.Equal("8/255_127.jpg", dzc.GetCompositeTileUrl(8, 255, 127));
    }
}
