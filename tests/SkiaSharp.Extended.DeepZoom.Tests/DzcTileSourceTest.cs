using SkiaSharp.Extended.DeepZoom;

namespace SkiaSharp.Extended.DeepZoom.Tests;

public class DzcTileSourceTest
{
    [Fact]
    public void Parse_EmbeddedConceptCarsDzc_ParsesCorrectly()
    {
        using var stream = TestDataHelper.GetStream("conceptcars.dzc");
        var dzc = DzcTileSource.Parse(stream);

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
        var dzc = DzcTileSource.Parse(stream);

        // The DZC has many items with IsPath=1
        Assert.True(dzc.ItemCount > 50);
    }

    [Fact]
    public void Parse_ConceptCarsDzc_FirstItemHasCorrectProperties()
    {
        using var stream = TestDataHelper.GetStream("conceptcars.dzc");
        var dzc = DzcTileSource.Parse(stream);

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

        var dzc = DzcTileSource.Parse(xml);

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

        var dzc = DzcTileSource.Parse(xml);
        var item = dzc.Items[0];

        Assert.Equal(0.5, item.ViewportWidth);
        Assert.Equal(-1.0, item.ViewportX);
        Assert.Equal(-0.5, item.ViewportY);
    }

    [Fact]
    public void MortonToGrid_KnownValues()
    {
        // Morton index 0 → (0,0)
        Assert.Equal((0, 0), DzcTileSource.MortonToGrid(0));
        // Morton index 1 → (1,0)
        Assert.Equal((1, 0), DzcTileSource.MortonToGrid(1));
        // Morton index 2 → (0,1)
        Assert.Equal((0, 1), DzcTileSource.MortonToGrid(2));
        // Morton index 3 → (1,1)
        Assert.Equal((1, 1), DzcTileSource.MortonToGrid(3));
        // Morton index 4 → (2,0)
        Assert.Equal((2, 0), DzcTileSource.MortonToGrid(4));
    }

    [Fact]
    public void GridToMorton_RoundTrips()
    {
        for (int i = 0; i < 100; i++)
        {
            var (col, row) = DzcTileSource.MortonToGrid(i);
            int morton = DzcTileSource.GridToMorton(col, row);
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

        var dzc = DzcTileSource.Parse(xml);
        // 4 items → ceil(sqrt(4)) = 2, ceil(log2(2)) = 1, 2^1 = 2
        Assert.Equal(2, dzc.GetMortonGridSize());
    }

    [Fact]
    public void GetCompositeTileUrl_FormatsCorrectly()
    {
        var dzc = new DzcTileSource(8, 256, "jpg", Array.Empty<DzcSubImage>());
        Assert.Equal("5/3_2.jpg", dzc.GetCompositeTileUrl(5, 3, 2));
    }

    [Fact]
    public void Parse_MissingCollection_ThrowsFormatException()
    {
        var xml = @"<Root xmlns=""http://schemas.microsoft.com/deepzoom/2008""></Root>";
        Assert.Throws<FormatException>(() => DzcTileSource.Parse(xml));
    }

    [Fact]
    public void Parse_2009Namespace_ParsesCorrectly()
    {
        var xml = @"<Collection MaxLevel=""5"" TileSize=""256"" Format=""png"" xmlns=""http://schemas.microsoft.com/deepzoom/2009"">
    <Items>
        <I Id=""0"" N=""0""><Size Width=""100"" Height=""100"" /></I>
    </Items>
</Collection>";

        var dzc = DzcTileSource.Parse(xml);
        Assert.Equal(5, dzc.MaxLevel);
        Assert.Equal("png", dzc.Format);
        Assert.Equal(1, dzc.ItemCount);
    }

    [Fact]
    public void SubImage_AspectRatio_CalculatedCorrectly()
    {
        var subImage = new DzcSubImage(0, 0, 800, 600, null);
        Assert.Equal(800.0 / 600.0, subImage.AspectRatio, 6);
    }

    [Fact]
    public void SubImage_ZeroHeight_AspectRatioIs1()
    {
        var subImage = new DzcSubImage(0, 0, 100, 0, null);
        Assert.Equal(1.0, subImage.AspectRatio);
    }
}
