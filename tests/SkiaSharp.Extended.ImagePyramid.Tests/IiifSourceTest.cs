using System;
using Xunit;

namespace SkiaSharp.Extended.ImagePyramid.Tests;

/// <summary>
/// Tests for <see cref="SKImagePyramidIiifSource"/> — IIIF Image API v2/v3 support.
/// </summary>
public class IiifSourceTest
{
    // Wellcome Collection info.json (IIIF v2)
    private const string WellcomeInfoJson = """
        {
          "@context": "http://iiif.io/api/image/2/context.json",
          "@id": "https://iiif.wellcomecollection.org/image/b20432033_B0008608.JP2",
          "@type": "iiif:Image",
          "profile": ["http://iiif.io/api/image/2/level2.json", {"formats":["jpg","tif","gif","png"]}],
          "protocol": "http://iiif.io/api/image",
          "width": 3543,
          "height": 2480,
          "tiles": [{"width": 1024, "height": 1024, "scaleFactors": [1, 2, 4, 8, 16, 32]}]
        }
        """;

    // IIIF v3 info.json format (uses "id" instead of "@id")
    private const string IiifV3InfoJson = """
        {
          "@context": "http://iiif.io/api/image/3/context.json",
          "id": "https://example.org/iiif/image/v3/test",
          "type": "ImageService3",
          "protocol": "http://iiif.io/api/image",
          "profile": "level2",
          "width": 2000,
          "height": 1500,
          "tiles": [{"width": 512, "height": 512, "scaleFactors": [1, 2, 4, 8]}]
        }
        """;

    // Minimal IIIF info.json without tiles array
    private const string MinimalInfoJson = """
        {
          "@id": "https://example.org/image/minimal",
          "width": 800,
          "height": 600
        }
        """;

    [Fact]
    public void Parse_WellcomeCollection_ReturnsCorrectDimensions()
    {
        var source = SKImagePyramidIiifSource.Parse(WellcomeInfoJson);

        Assert.Equal(3543, source.ImageWidth);
        Assert.Equal(2480, source.ImageHeight);
    }

    [Fact]
    public void Parse_WellcomeCollection_ReturnsCorrectMaxLevel()
    {
        var source = SKImagePyramidIiifSource.Parse(WellcomeInfoJson);

        // 6 scale factors [32, 16, 8, 4, 2, 1] → sorted descending → MaxLevel = 5
        Assert.Equal(5, source.MaxLevel);
    }

    [Fact]
    public void Parse_WellcomeCollection_MaxLevelIsFullResolution()
    {
        var source = SKImagePyramidIiifSource.Parse(WellcomeInfoJson);

        // Level MaxLevel has scaleFactor=1, so levelWidth = ImageWidth
        Assert.Equal(3543, source.GetLevelWidth(source.MaxLevel));
        Assert.Equal(2480, source.GetLevelHeight(source.MaxLevel));
    }

    [Fact]
    public void Parse_WellcomeCollection_Level0IsLowestResolution()
    {
        var source = SKImagePyramidIiifSource.Parse(WellcomeInfoJson);

        // Level 0 has scaleFactor=32, so levelWidth = ceil(3543/32) = 111
        Assert.Equal((int)Math.Ceiling(3543.0 / 32), source.GetLevelWidth(0));
        Assert.Equal((int)Math.Ceiling(2480.0 / 32), source.GetLevelHeight(0));
    }

    [Fact]
    public void Parse_WellcomeCollection_TileCountAtMaxLevel()
    {
        var source = SKImagePyramidIiifSource.Parse(WellcomeInfoJson);

        // At max level: ceil(3543/1024) = 4 columns, ceil(2480/1024) = 3 rows
        Assert.Equal((int)Math.Ceiling(3543.0 / 1024), source.GetTileCountX(source.MaxLevel));
        Assert.Equal((int)Math.Ceiling(2480.0 / 1024), source.GetTileCountY(source.MaxLevel));
    }

    [Fact]
    public void GetFullTileUrl_MaxLevel_FirstTile_CorrectRegion()
    {
        var source = SKImagePyramidIiifSource.Parse(WellcomeInfoJson);

        // Level 5 (MaxLevel), col=0, row=0, scaleFactor=1
        // Region: 0,0,1024,1024 (clamped to image size)
        // Size: 1024,1024
        var url = source.GetFullTileUrl(source.MaxLevel, 0, 0);

        Assert.NotNull(url);
        Assert.Contains("/0,0,1024,1024/1024,1024/0/default.jpg", url);
        Assert.StartsWith("https://iiif.wellcomecollection.org/image/b20432033_B0008608.JP2/", url);
    }

    [Fact]
    public void GetFullTileUrl_MaxLevel_EdgeTile_ClampedToImageBounds()
    {
        var source = SKImagePyramidIiifSource.Parse(WellcomeInfoJson);

        // Level 5 (MaxLevel), col=3, row=2, scaleFactor=1
        // Region x: 3*1024=3072, w: min(1024, 3543-3072)=471
        // Region y: 2*1024=2048, h: min(1024, 2480-2048)=432
        // Output: ceil(471/1)=471, ceil(432/1)=432
        var url = source.GetFullTileUrl(source.MaxLevel, 3, 2);

        Assert.NotNull(url);
        Assert.Contains("/3072,2048,471,432/471,432/0/default.jpg", url);
    }

    [Fact]
    public void GetFullTileUrl_Level4_FirstTile_CorrectScaling()
    {
        var source = SKImagePyramidIiifSource.Parse(WellcomeInfoJson);

        // Level 4 has scaleFactor=2 (sorted desc: [32,16,8,4,2,1], index 4 = 2)
        // col=0, row=0: regX=0, regY=0, regW=min(1024*2, 3543)=2048, regH=min(1024*2, 2480)=2048
        // outW=ceil(2048/2)=1024, outH=ceil(2048/2)=1024
        var url = source.GetFullTileUrl(4, 0, 0);

        Assert.NotNull(url);
        Assert.Contains("/0,0,2048,2048/1024,1024/0/default.jpg", url);
    }

    [Fact]
    public void GetFullTileUrl_OutOfBoundsTile_ReturnsNull()
    {
        var source = SKImagePyramidIiifSource.Parse(WellcomeInfoJson);

        // Tile completely outside the image at max level (col=4 would start at x=4096 > 3543)
        var url = source.GetFullTileUrl(source.MaxLevel, 4, 0);

        Assert.Null(url);
    }

    [Fact]
    public void AspectRatio_IsCorrect()
    {
        var source = SKImagePyramidIiifSource.Parse(WellcomeInfoJson);

        Assert.Equal((double)3543 / 2480, source.AspectRatio, precision: 10);
    }

    [Fact]
    public void GetTileBounds_MaxLevel_FirstTile_IsCorrect()
    {
        var source = SKImagePyramidIiifSource.Parse(WellcomeInfoJson);

        var bounds = source.GetTileBounds(source.MaxLevel, 0, 0);

        Assert.Equal(0, bounds.X);
        Assert.Equal(0, bounds.Y);
        Assert.Equal(1024, bounds.Width);
        Assert.Equal(1024, bounds.Height);
    }

    [Fact]
    public void GetTileBounds_MaxLevel_EdgeTile_ClampedToLevel()
    {
        var source = SKImagePyramidIiifSource.Parse(WellcomeInfoJson);

        // col=3, row=2 at max level: x=3072, y=2048, w=min(1024, 3543-3072)=471, h=min(1024, 2480-2048)=432
        var bounds = source.GetTileBounds(source.MaxLevel, 3, 2);

        Assert.Equal(3072, bounds.X);
        Assert.Equal(2048, bounds.Y);
        Assert.Equal(471, bounds.Width);
        Assert.Equal(432, bounds.Height);
    }

    [Fact]
    public void Parse_IiifV3_ReadsIdProperty()
    {
        var source = SKImagePyramidIiifSource.Parse(IiifV3InfoJson);

        Assert.Equal("https://example.org/iiif/image/v3/test", source.BaseId);
        Assert.Equal(2000, source.ImageWidth);
        Assert.Equal(1500, source.ImageHeight);
        // 4 scale factors [8,4,2,1] sorted desc → MaxLevel = 3
        Assert.Equal(3, source.MaxLevel);
    }

    [Fact]
    public void Parse_MinimalJson_NoTilesArray_FallsBackToWholeImage()
    {
        var source = SKImagePyramidIiifSource.Parse(MinimalInfoJson);

        Assert.Equal(800, source.ImageWidth);
        Assert.Equal(600, source.ImageHeight);
        // Without tiles, scaleFactors defaults to [1], so MaxLevel = 0
        Assert.Equal(0, source.MaxLevel);
        // Tile is the whole image
        Assert.Equal(800, source.TileWidth);
        Assert.Equal(600, source.TileHeight);
    }

    [Fact]
    public void Parse_MissingId_ThrowsFormatException()
    {
        const string badJson = """{"width": 100, "height": 100}""";

        Assert.Throws<FormatException>(() => SKImagePyramidIiifSource.Parse(badJson));
    }

    [Fact]
    public void Parse_MissingId_ThrowsFormatExceptionWithMessage()
    {
        const string badJson = """{"width": 100, "height": 100}""";

        var ex = Assert.Throws<FormatException>(() => SKImagePyramidIiifSource.Parse(badJson));
        Assert.Contains("missing", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateLevel_OutOfRange_Throws()
    {
        var source = SKImagePyramidIiifSource.Parse(WellcomeInfoJson);

        Assert.Throws<ArgumentOutOfRangeException>(() => source.GetLevelWidth(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => source.GetLevelWidth(source.MaxLevel + 1));
    }

    [Fact]
    public void GetOptimalLevel_LargeViewport_ReturnsLowestLevel()
    {
        var source = SKImagePyramidIiifSource.Parse(WellcomeInfoJson);

        // Tiny control, very wide viewport = zoomed out = low level
        var level = source.GetOptimalLevel(viewportWidth: 100.0, controlWidth: 10.0);

        Assert.Equal(0, level);
    }

    [Fact]
    public void GetOptimalLevel_ZeroViewportWidth_ReturnsMaxLevel()
    {
        var source = SKImagePyramidIiifSource.Parse(WellcomeInfoJson);

        var level = source.GetOptimalLevel(viewportWidth: 0, controlWidth: 1000);

        Assert.Equal(source.MaxLevel, level);
    }

    [Fact]
    public void Constructor_InvalidArguments_Throw()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new SKImagePyramidIiifSource(null!, 100, 100, 256, 256, [1]));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new SKImagePyramidIiifSource("http://x", 0, 100, 256, 256, [1]));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new SKImagePyramidIiifSource("http://x", 100, 0, 256, 256, [1]));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new SKImagePyramidIiifSource("http://x", 100, 100, 0, 256, [1]));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new SKImagePyramidIiifSource("http://x", 100, 100, 256, 0, [1]));
        Assert.Throws<ArgumentException>(() =>
            new SKImagePyramidIiifSource("http://x", 100, 100, 256, 256, []));
    }

    [Fact]
    public void ImplementsISKImagePyramidSource()
    {
        var source = SKImagePyramidIiifSource.Parse(WellcomeInfoJson);

        Assert.IsAssignableFrom<ISKImagePyramidSource>(source);
    }
}
