using SkiaSharp.Extended.DeepZoom;

namespace SkiaSharp.Extended.DeepZoom.Tests;

/// <summary>
/// Integration tests that parse real DZI files and verify image dimensions,
/// tile pyramid levels, and tile URL generation.
/// </summary>
public class DziCompatibilityIntegrationTest
{
    [Fact]
    public void Parse_ConceptCarSample_HasCorrectDimensions()
    {
        // A real DZI from conceptcars collection (2008 namespace)
        using var stream = TestDataHelper.GetStream("conceptcar_sample.dzi");
        var dzi = DziTileSource.Parse(stream);

        Assert.Equal(394, dzi.ImageWidth);
        Assert.Equal(263, dzi.ImageHeight);
        Assert.Equal(254, dzi.TileSize);
        Assert.Equal(1, dzi.Overlap);
        Assert.Equal("jpg", dzi.Format);
    }

    [Fact]
    public void Parse_ConceptCarSample_TilePyramidLevelsAreCorrect()
    {
        using var stream = TestDataHelper.GetStream("conceptcar_sample.dzi");
        var dzi = DziTileSource.Parse(stream);

        // MaxLevel = ceil(log2(max(394, 263))) = ceil(log2(394)) = ceil(8.62) = 9
        Assert.Equal(9, dzi.MaxLevel);

        // Level 0: 1x1
        Assert.Equal(1, dzi.GetLevelWidth(0));
        Assert.Equal(1, dzi.GetLevelHeight(0));

        // Max level: full resolution
        Assert.Equal(394, dzi.GetLevelWidth(dzi.MaxLevel));
        Assert.Equal(263, dzi.GetLevelHeight(dzi.MaxLevel));

        // One level down: ceil(394/2)=197, ceil(263/2)=132
        Assert.Equal(197, dzi.GetLevelWidth(dzi.MaxLevel - 1));
        Assert.Equal(132, dzi.GetLevelHeight(dzi.MaxLevel - 1));

        // Verify monotonically increasing dimensions
        for (int level = 1; level <= dzi.MaxLevel; level++)
        {
            Assert.True(dzi.GetLevelWidth(level) >= dzi.GetLevelWidth(level - 1));
            Assert.True(dzi.GetLevelHeight(level) >= dzi.GetLevelHeight(level - 1));
        }
    }

    [Fact]
    public void Parse_ConceptCarSample_TileUrlsAreCorrect()
    {
        using var stream = TestDataHelper.GetStream("conceptcar_sample.dzi");
        var dzi = DziTileSource.Parse(stream);
        dzi.TilesBaseUri = "http://example.com/deepzoom/10014035633840664012396073_files/";

        // Relative URL
        Assert.Equal("9/0_0.jpg", dzi.GetTileUrl(dzi.MaxLevel, 0, 0));
        Assert.Equal("0/0_0.jpg", dzi.GetTileUrl(0, 0, 0));

        // Full URL with base
        Assert.Equal("http://example.com/deepzoom/10014035633840664012396073_files/9/0_0.jpg",
            dzi.GetFullTileUrl(dzi.MaxLevel, 0, 0));

        // At max level with TileSize=254: ceil(394/254)=2 columns, ceil(263/254)=2 rows
        Assert.Equal(2, dzi.GetTileCountX(dzi.MaxLevel));
        Assert.Equal(2, dzi.GetTileCountY(dzi.MaxLevel));
    }

    [Fact]
    public void Parse_ConceptCarSample_TileBoundsWithOverlap()
    {
        using var stream = TestDataHelper.GetStream("conceptcar_sample.dzi");
        var dzi = DziTileSource.Parse(stream);

        // First tile at max level: x=0, width=min(254+1, 394)=255
        var first = dzi.GetTileBounds(dzi.MaxLevel, 0, 0);
        Assert.Equal(0, first.X);
        Assert.Equal(0, first.Y);
        Assert.Equal(255, first.Width);  // 254 + 1 overlap on right
        Assert.Equal(255, first.Height); // 254 + 1 overlap on bottom

        // Second (last) column tile: x=254-1=253, right=min(508+1, 394)=394, width=141
        var second = dzi.GetTileBounds(dzi.MaxLevel, 1, 0);
        Assert.Equal(253, second.X);
        Assert.Equal(141, second.Width); // 394 - 253

        // Second (last) row tile: y=254-1=253, bottom=min(508+1, 263)=263, height=10
        var bottomTile = dzi.GetTileBounds(dzi.MaxLevel, 0, 1);
        Assert.Equal(253, bottomTile.Y);
        Assert.Equal(10, bottomTile.Height); // 263 - 253
    }

    [Fact]
    public void Parse_SampleDzi_FullPyramidValidation()
    {
        using var stream = TestDataHelper.GetStream("sample.dzi");
        var dzi = DziTileSource.Parse(stream);

        Assert.Equal(240, dzi.ImageWidth);
        Assert.Equal(160, dzi.ImageHeight);
        Assert.Equal(256, dzi.TileSize);
        Assert.Equal(0, dzi.Overlap);
        Assert.Equal("jpg", dzi.Format);

        // MaxLevel = ceil(log2(240)) = 8
        Assert.Equal(8, dzi.MaxLevel);

        // At max level with TileSize=256: single tile (240 < 256)
        Assert.Equal(1, dzi.GetTileCountX(dzi.MaxLevel));
        Assert.Equal(1, dzi.GetTileCountY(dzi.MaxLevel));

        // Tile URL
        Assert.Equal("8/0_0.jpg", dzi.GetTileUrl(dzi.MaxLevel, 0, 0));
    }

    [Fact]
    public void Parse_RealDzi_AspectRatioIsCorrect()
    {
        using var stream = TestDataHelper.GetStream("conceptcar_sample.dzi");
        var dzi = DziTileSource.Parse(stream);

        Assert.Equal(394.0 / 263.0, dzi.AspectRatio, 4);
    }

    [Fact]
    public void Parse_CollectionDz0_Uses2009Namespace_SucceedsWithFallback()
    {
        // collection-dz DZI files use the 2009 namespace, which the parser now supports via fallback
        using var stream = TestDataHelper.GetStream("collection-dz_0.dzi");
        var dzi = DziTileSource.Parse(stream);
        Assert.True(dzi.ImageWidth > 0);
        Assert.True(dzi.ImageHeight > 0);
    }
}
