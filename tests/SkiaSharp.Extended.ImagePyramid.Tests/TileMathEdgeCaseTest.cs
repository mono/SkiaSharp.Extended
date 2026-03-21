using SkiaSharp;
using SkiaSharp.Extended;
using Xunit;

namespace SkiaSharp.Extended.ImagePyramid.Tests;

/// <summary>
/// Edge case and corner case tests for DZI/DZC parsing and tile math.
/// </summary>
public class TileMathEdgeCaseTest
{
    [Fact]
    public void DziTileSource_1x1Image_MaxLevelIsZero()
    {
        var xml = @"<?xml version='1.0'?>
<Image xmlns='http://schemas.microsoft.com/deepzoom/2008'
       Format='png' Overlap='0' TileSize='256'>
  <Size Width='1' Height='1'/>
</Image>";

        var dzi = SKImagePyramidDziSource.Parse(xml);
        Assert.Equal(0, dzi.MaxLevel);
        Assert.Equal(1, dzi.GetLevelWidth(0));
        Assert.Equal(1, dzi.GetLevelHeight(0));
    }

    [Fact]
    public void DziTileSource_ExactPowerOf2_CorrectLevel()
    {
        var xml = @"<?xml version='1.0'?>
<Image xmlns='http://schemas.microsoft.com/deepzoom/2008'
       Format='jpg' Overlap='1' TileSize='256'>
  <Size Width='1024' Height='1024'/>
</Image>";

        var dzi = SKImagePyramidDziSource.Parse(xml);
        Assert.Equal(10, dzi.MaxLevel); // log2(1024) = 10
        Assert.Equal(1024, dzi.GetLevelWidth(dzi.MaxLevel));
    }

    [Fact]
    public void DziTileSource_NonPowerOf2_CeilsUp()
    {
        var xml = @"<?xml version='1.0'?>
<Image xmlns='http://schemas.microsoft.com/deepzoom/2008'
       Format='jpg' Overlap='0' TileSize='256'>
  <Size Width='1000' Height='750'/>
</Image>";

        var dzi = SKImagePyramidDziSource.Parse(xml);
        Assert.Equal(10, dzi.MaxLevel); // ceil(log2(1000)) = 10
    }

    [Fact]
    public void DziTileSource_TallImage_WidthSmallerThanHeight()
    {
        var xml = @"<?xml version='1.0'?>
<Image xmlns='http://schemas.microsoft.com/deepzoom/2008'
       Format='jpg' Overlap='0' TileSize='256'>
  <Size Width='100' Height='10000'/>
</Image>";

        var dzi = SKImagePyramidDziSource.Parse(xml);
        Assert.Equal(14, dzi.MaxLevel); // ceil(log2(10000)) ≈ 14
    }

    [Fact]
    public void DziTileSource_GetTileCountX_SingleTile()
    {
        var xml = @"<?xml version='1.0'?>
<Image xmlns='http://schemas.microsoft.com/deepzoom/2008'
       Format='jpg' Overlap='0' TileSize='256'>
  <Size Width='200' Height='200'/>
</Image>";

        var dzi = SKImagePyramidDziSource.Parse(xml);
        // At max level, 200px wide with 256-tile = 1 tile
        Assert.Equal(1, dzi.GetTileCountX(dzi.MaxLevel));
        Assert.Equal(1, dzi.GetTileCountY(dzi.MaxLevel));
    }

    [Fact]
    public void DziTileSource_GetTileCountX_MultipleTiles()
    {
        var xml = @"<?xml version='1.0'?>
<Image xmlns='http://schemas.microsoft.com/deepzoom/2008'
       Format='jpg' Overlap='0' TileSize='256'>
  <Size Width='1000' Height='1000'/>
</Image>";

        var dzi = SKImagePyramidDziSource.Parse(xml);
        int maxLevel = dzi.MaxLevel;
        int width = dzi.GetLevelWidth(maxLevel);
        int expectedTiles = (int)Math.Ceiling((double)width / 256);
        Assert.Equal(expectedTiles, dzi.GetTileCountX(maxLevel));
    }

    [Fact]
    public void DziTileSource_GetOptimalLevel_FullImage()
    {
        var xml = @"<?xml version='1.0'?>
<Image xmlns='http://schemas.microsoft.com/deepzoom/2008'
       Format='jpg' Overlap='0' TileSize='256'>
  <Size Width='2048' Height='2048'/>
</Image>";

        var dzi = SKImagePyramidDziSource.Parse(xml);
        // ViewportWidth=1.0 means full image fits in 800px wide control
        int level = dzi.GetOptimalLevel(1.0, 800);
        Assert.True(level < dzi.MaxLevel);
        Assert.True(level >= 0);
    }

    [Fact]
    public void DziTileSource_GetOptimalLevel_FullZoom()
    {
        var xml = @"<?xml version='1.0'?>
<Image xmlns='http://schemas.microsoft.com/deepzoom/2008'
       Format='jpg' Overlap='0' TileSize='256'>
  <Size Width='2048' Height='2048'/>
</Image>";

        var dzi = SKImagePyramidDziSource.Parse(xml);
        // ViewportWidth very small = very zoomed in
        int level = dzi.GetOptimalLevel(0.01, 800);
        // Should be at or near max level
        Assert.True(level >= dzi.MaxLevel - 1);
    }

    [Fact]
    public void DziTileSource_GetTileUrl_CorrectFormat()
    {
        var xml = @"<?xml version='1.0'?>
<Image xmlns='http://schemas.microsoft.com/deepzoom/2008'
       Format='jpg' Overlap='1' TileSize='256'>
  <Size Width='1024' Height='768'/>
</Image>";

        var dzi = SKImagePyramidDziSource.Parse(xml, "https://example.com/photo");
        var url = dzi.GetTileUrl(5, 2, 3);
        Assert.Contains("5/", url);
        Assert.Contains("2_3", url);
        Assert.EndsWith(".jpg", url);
    }

    [Fact]
    public void DziTileSource_PngFormat()
    {
        var xml = @"<?xml version='1.0'?>
<Image xmlns='http://schemas.microsoft.com/deepzoom/2008'
       Format='png' Overlap='0' TileSize='128'>
  <Size Width='512' Height='512'/>
</Image>";

        var dzi = SKImagePyramidDziSource.Parse(xml, "https://example.com/image");
        var url = dzi.GetTileUrl(0, 0, 0);
        Assert.EndsWith(".png", url);
    }

    [Fact]
    public void DziTileSource_LevelWidth_DecreasesByHalf()
    {
        var xml = @"<?xml version='1.0'?>
<Image xmlns='http://schemas.microsoft.com/deepzoom/2008'
       Format='jpg' Overlap='0' TileSize='256'>
  <Size Width='4096' Height='2048'/>
</Image>";

        var dzi = SKImagePyramidDziSource.Parse(xml);
        for (int level = dzi.MaxLevel; level > 0; level--)
        {
            int width = dzi.GetLevelWidth(level);
            int prevWidth = dzi.GetLevelWidth(level - 1);
            // Each level should be roughly half the next (ceiling)
            Assert.True(prevWidth <= width);
            Assert.True(prevWidth >= width / 2 - 1);
        }
    }

    [Fact]
    public void DzcTileSource_Parse_ReturnsSubImages()
    {
        var xml = TestDataHelper.GetString("conceptcars.dzc");
        var dzc = SKImagePyramidDziCollectionSource.Parse(xml);

        Assert.True(dzc.Items.Count > 0);
        Assert.Equal("jpg", dzc.Format);
        Assert.Equal(256, dzc.TileSize);
    }

    [Fact]
    public void DzcTileSource_SubImage_HasMortonIndex()
    {
        var xml = TestDataHelper.GetString("conceptcars.dzc");
        var dzc = SKImagePyramidDziCollectionSource.Parse(xml);

        var first = dzc.Items[0];
        Assert.True(first.MortonIndex >= 0);
    }

    [Fact]
    public void DzcTileSource_SubImage_HasViewport()
    {
        var xml = TestDataHelper.GetString("conceptcars.dzc");
        var dzc = SKImagePyramidDziCollectionSource.Parse(xml);

        var first = dzc.Items[0];
        // ViewportWidth may be 0 if no SKImagePyramidViewport element in DZC
        Assert.True(first.ViewportWidth >= 0);
    }
}
