using SkiaSharp.Extended.DeepZoom;

namespace SkiaSharp.Extended.DeepZoom.Tests;

/// <summary>
/// Compatibility tests for all real-world DZC and DZI collections.
/// Ensures parser handles every format variant in the wild.
/// </summary>
public class DzcCompatibilityTest
{
    [Theory]
    [InlineData("conceptcars.dzc", 298, "jpg", 256)]
    [InlineData("collection-dz.dzc", 50, "jpg", 256)]
    [InlineData("nigeria_images.dzc", 31, "jpg", 256)]
    [InlineData("ski_resorts.dzc", 5, "jpg", 256)]
    [InlineData("stockport_images.dzc", 1, "jpg", 256)]
    [InlineData("area.dzc", 1, "jpg", 256)]
    [InlineData("geometry.dzc", 1, "jpg", 256)]
    public void Parse_AllDzc_LoadsSuccessfully(string file, int expectedItems, string expectedFormat, int expectedTileSize)
    {
        using var stream = TestDataHelper.GetStream(file);
        var dzc = DzcTileSource.Parse(stream);

        Assert.Equal(expectedItems, dzc.Items.Count);
        Assert.Equal(expectedFormat, dzc.Format);
        Assert.Equal(expectedTileSize, dzc.TileSize);
    }

    [Theory]
    [InlineData("conceptcars.dzc")]
    [InlineData("collection-dz.dzc")]
    [InlineData("nigeria_images.dzc")]
    [InlineData("ski_resorts.dzc")]
    [InlineData("stockport_images.dzc")]
    public void Parse_DzcWithSources_ItemsHaveValidSources(string file)
    {
        using var stream = TestDataHelper.GetStream(file);
        var dzc = DzcTileSource.Parse(stream);

        foreach (var item in dzc.Items)
        {
            Assert.False(string.IsNullOrEmpty(item.Source),
                $"{file}: Item {item.Id} has empty Source");
            Assert.True(item.Source.EndsWith(".dzi"),
                $"{file}: Item {item.Id} Source '{item.Source}' should end with .dzi");
        }
    }

    [Fact]
    public void Parse_DzcWithoutSource_ItemsHaveNullSource()
    {
        // DZCs without Source attributes use pure mosaic tiles
        var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Collection MaxLevel=""7"" TileSize=""256"" Format=""jpg"" NextItemId=""2""
    xmlns=""http://schemas.microsoft.com/deepzoom/2009"">
  <Items>
    <I Id=""0"" N=""0""><Size Width=""640"" Height=""480""/></I>
    <I Id=""1"" N=""1""><Size Width=""320"" Height=""240""/></I>
  </Items>
</Collection>";
        var dzc = DzcTileSource.Parse(xml);

        Assert.Equal(2, dzc.Items.Count);
        Assert.Null(dzc.Items[0].Source);
        Assert.Null(dzc.Items[1].Source);
    }

    [Theory]
    [InlineData("conceptcars.dzc")]
    [InlineData("collection-dz.dzc")]
    [InlineData("nigeria_images.dzc")]
    [InlineData("ski_resorts.dzc")]
    public void Parse_AllDzc_ItemsHavePositiveDimensions(string file)
    {
        using var stream = TestDataHelper.GetStream(file);
        var dzc = DzcTileSource.Parse(stream);

        foreach (var item in dzc.Items)
        {
            Assert.True(item.Width > 0, $"{file}: Item {item.Id} Width should be > 0");
            Assert.True(item.Height > 0, $"{file}: Item {item.Id} Height should be > 0");
        }
    }

    [Fact]
    public void Parse_ConceptCars_FirstItemHasTileSource()
    {
        using var stream = TestDataHelper.GetStream("conceptcars.dzc");
        var dzc = DzcTileSource.Parse(stream);

        var first = dzc.Items[0];
        Assert.True(first.Width > 100, "First concept car should have substantial width");
        Assert.True(first.Height > 100, "First concept car should have substantial height");
    }

    [Fact]
    public void Parse_SampleDzi_LoadsSuccessfully()
    {
        var xml = TestDataHelper.GetString("sample.dzi");
        var dzi = DziTileSource.Parse(xml, "http://example.com/sample");

        Assert.True(dzi.ImageWidth > 0);
        Assert.True(dzi.ImageHeight > 0);
        Assert.True(dzi.TileSize > 0);
        Assert.True(dzi.MaxLevel >= 0);
    }

    [Theory]
    [InlineData("conceptcars.dzc")]
    [InlineData("collection-dz.dzc")]
    public void Parse_Dzc_MaxLevelIsReasonable(string file)
    {
        using var stream = TestDataHelper.GetStream(file);
        var dzc = DzcTileSource.Parse(stream);

        Assert.True(dzc.MaxLevel >= 0, "MaxLevel should be non-negative");
        Assert.True(dzc.MaxLevel <= 20, "MaxLevel should be <= 20 (reasonable bound)");
    }

    [Fact]
    public void Parse_AllDzc_UniqueItemIds()
    {
        var files = new[] { "conceptcars.dzc", "collection-dz.dzc", "nigeria_images.dzc",
                            "ski_resorts.dzc", "stockport_images.dzc" };

        foreach (var file in files)
        {
            using var stream = TestDataHelper.GetStream(file);
            var dzc = DzcTileSource.Parse(stream);

            var ids = dzc.Items.Select(i => i.Id).ToList();
            var uniqueIds = ids.Distinct().ToList();

            Assert.True(ids.Count == uniqueIds.Count,
                $"{file}: Found duplicate item IDs");
        }
    }
}
