using SkiaSharp.Extended.DeepZoom;

namespace SkiaSharp.Extended.DeepZoom.Tests;

public class DziTileSourceTest
{
    [Fact]
    public void Parse_ValidDziXml_ParsesCorrectly()
    {
        var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Image TileSize=""256"" Overlap=""2"" Format=""jpg"" xmlns=""http://schemas.microsoft.com/deepzoom/2008"">
    <Size Width=""7026"" Height=""9221""/>
</Image>";

        var dzi = DziTileSource.Parse(xml);

        Assert.Equal(7026, dzi.ImageWidth);
        Assert.Equal(9221, dzi.ImageHeight);
        Assert.Equal(256, dzi.TileSize);
        Assert.Equal(2, dzi.Overlap);
        Assert.Equal("jpg", dzi.Format);
    }

    [Fact]
    public void Parse_EmbeddedSampleDzi_ParsesCorrectly()
    {
        using var stream = TestDataHelper.GetStream("sample.dzi");
        var dzi = DziTileSource.Parse(stream);

        Assert.Equal(240, dzi.ImageWidth);
        Assert.Equal(160, dzi.ImageHeight);
        Assert.Equal(256, dzi.TileSize);
        Assert.Equal(0, dzi.Overlap);
        Assert.Equal("jpg", dzi.Format);
    }

    [Fact]
    public void Parse_DuomoLikeDzi_HandlesNonStandardTileSize()
    {
        var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Image TileSize=""254"" Overlap=""1"" Format=""jpg"" xmlns=""http://schemas.microsoft.com/deepzoom/2008"">
    <Size Width=""13920"" Height=""10200""/>
</Image>";

        var dzi = DziTileSource.Parse(xml);

        Assert.Equal(13920, dzi.ImageWidth);
        Assert.Equal(10200, dzi.ImageHeight);
        Assert.Equal(254, dzi.TileSize);
        Assert.Equal(1, dzi.Overlap);
    }

    [Fact]
    public void MaxLevel_CalculatedCorrectly()
    {
        // MaxLevel = ceil(log2(max(W, H)))
        var dzi = new DziTileSource(7026, 9221, 256, 2, "jpg");
        Assert.Equal(14, dzi.MaxLevel); // ceil(log2(9221)) = ceil(13.17) = 14

        var small = new DziTileSource(240, 160, 256, 0, "jpg");
        Assert.Equal(8, small.MaxLevel); // ceil(log2(240)) = 8
    }

    [Fact]
    public void AspectRatio_CalculatedCorrectly()
    {
        var dzi = new DziTileSource(7026, 9221, 256, 2, "jpg");
        Assert.Equal(7026.0 / 9221.0, dzi.AspectRatio, 6);
    }

    [Fact]
    public void GetLevelWidth_Level0_Returns1()
    {
        var dzi = new DziTileSource(7026, 9221, 256, 2, "jpg");
        // Level 0 should be 1 pixel wide
        Assert.Equal(1, dzi.GetLevelWidth(0));
        Assert.Equal(1, dzi.GetLevelHeight(0));
    }

    [Fact]
    public void GetLevelWidth_MaxLevel_ReturnsFullResolution()
    {
        var dzi = new DziTileSource(7026, 9221, 256, 2, "jpg");
        Assert.Equal(7026, dzi.GetLevelWidth(dzi.MaxLevel));
        Assert.Equal(9221, dzi.GetLevelHeight(dzi.MaxLevel));
    }

    [Fact]
    public void GetLevelWidth_MiddleLevels_ScaleCorrectly()
    {
        var dzi = new DziTileSource(1024, 512, 256, 0, "jpg");
        // MaxLevel = ceil(log2(1024)) = 10
        Assert.Equal(10, dzi.MaxLevel);

        // At level 10 (max): 1024x512
        Assert.Equal(1024, dzi.GetLevelWidth(10));
        Assert.Equal(512, dzi.GetLevelHeight(10));

        // At level 9: 512x256
        Assert.Equal(512, dzi.GetLevelWidth(9));
        Assert.Equal(256, dzi.GetLevelHeight(9));

        // At level 8: 256x128
        Assert.Equal(256, dzi.GetLevelWidth(8));
        Assert.Equal(128, dzi.GetLevelHeight(8));
    }

    [Fact]
    public void GetTileCountX_SingleTile_Returns1()
    {
        var dzi = new DziTileSource(200, 100, 256, 0, "jpg");
        // Level = max, width = 200, tileSize = 256 → 1 tile
        Assert.Equal(1, dzi.GetTileCountX(dzi.MaxLevel));
    }

    [Fact]
    public void GetTileCountX_MultiTile_CalculatesCorrectly()
    {
        var dzi = new DziTileSource(1024, 512, 256, 0, "jpg");
        // At max level: 1024/256 = 4 tiles
        Assert.Equal(4, dzi.GetTileCountX(dzi.MaxLevel));
        Assert.Equal(2, dzi.GetTileCountY(dzi.MaxLevel));
    }

    [Fact]
    public void GetTileBounds_FirstTile_NoOverlap()
    {
        var dzi = new DziTileSource(1024, 512, 256, 0, "jpg");
        var bounds = dzi.GetTileBounds(dzi.MaxLevel, 0, 0);
        Assert.Equal(0, bounds.X);
        Assert.Equal(0, bounds.Y);
        Assert.Equal(256, bounds.Width);
        Assert.Equal(256, bounds.Height);
    }

    [Fact]
    public void GetTileBounds_FirstTile_WithOverlap()
    {
        var dzi = new DziTileSource(1024, 512, 256, 2, "jpg");
        var bounds = dzi.GetTileBounds(dzi.MaxLevel, 0, 0);
        Assert.Equal(0, bounds.X);
        Assert.Equal(0, bounds.Y);
        Assert.Equal(258, bounds.Width); // 256 + 2 (right overlap only)
        Assert.Equal(258, bounds.Height);
    }

    [Fact]
    public void GetTileUrl_FormatsCorrectly()
    {
        var dzi = new DziTileSource(1024, 512, 256, 0, "jpg");
        Assert.Equal("8/3_2.jpg", dzi.GetTileUrl(8, 3, 2));
    }

    [Fact]
    public void GetFullTileUrl_WithBaseUri()
    {
        var dzi = new DziTileSource(1024, 512, 256, 0, "jpg");
        dzi.TilesBaseUri = "http://example.com/image_files/";
        Assert.Equal("http://example.com/image_files/8/3_2.jpg", dzi.GetFullTileUrl(8, 3, 2));
    }

    [Fact]
    public void GetFullTileUrl_NoBaseUri_ReturnsNull()
    {
        var dzi = new DziTileSource(1024, 512, 256, 0, "jpg");
        Assert.Null(dzi.GetFullTileUrl(8, 3, 2));
    }

    [Fact]
    public void Parse_WithBaseUri_SetsTilesBaseUri()
    {
        var xml = @"<Image TileSize=""256"" Overlap=""0"" Format=""jpg"" xmlns=""http://schemas.microsoft.com/deepzoom/2008""><Size Width=""100"" Height=""100""/></Image>";
        var dzi = DziTileSource.Parse(xml, "http://example.com/image_files/");
        Assert.Equal("http://example.com/image_files/", dzi.TilesBaseUri);
    }

    [Fact]
    public void Parse_MissingImage_ThrowsFormatException()
    {
        var xml = @"<Root xmlns=""http://schemas.microsoft.com/deepzoom/2008""></Root>";
        Assert.Throws<FormatException>(() => DziTileSource.Parse(xml));
    }

    [Fact]
    public void Parse_MissingSize_ThrowsFormatException()
    {
        var xml = @"<Image TileSize=""256"" Format=""jpg"" xmlns=""http://schemas.microsoft.com/deepzoom/2008""></Image>";
        Assert.Throws<FormatException>(() => DziTileSource.Parse(xml));
    }

    [Fact]
    public void Constructor_InvalidWidth_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new DziTileSource(0, 100, 256, 0, "jpg"));
    }

    [Fact]
    public void Constructor_InvalidTileSize_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new DziTileSource(100, 100, 0, 0, "jpg"));
    }

    [Fact]
    public void Constructor_NegativeOverlap_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new DziTileSource(100, 100, 256, -1, "jpg"));
    }

    [Fact]
    public void GetLevelWidth_InvalidLevel_Throws()
    {
        var dzi = new DziTileSource(100, 100, 256, 0, "jpg");
        Assert.Throws<ArgumentOutOfRangeException>(() => dzi.GetLevelWidth(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => dzi.GetLevelWidth(dzi.MaxLevel + 1));
    }

    [Fact]
    public void GetLevelDimensions_NonPowerOfTwo_CeilsCorrectly()
    {
        // Non-power-of-two dimensions should round up at each level
        var dzi = new DziTileSource(1000, 750, 256, 0, "jpg");
        int maxLevel = dzi.MaxLevel;
        Assert.Equal(1000, dzi.GetLevelWidth(maxLevel));
        Assert.Equal(750, dzi.GetLevelHeight(maxLevel));

        // One level down: ceil(1000/2) = 500, ceil(750/2) = 375
        Assert.Equal(500, dzi.GetLevelWidth(maxLevel - 1));
        Assert.Equal(375, dzi.GetLevelHeight(maxLevel - 1));
    }

    [Fact]
    public void Parse_Stream_ParsesCorrectly()
    {
        var xml = @"<Image TileSize=""256"" Overlap=""1"" Format=""png"" xmlns=""http://schemas.microsoft.com/deepzoom/2008""><Size Width=""500"" Height=""300""/></Image>";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(xml));
        var dzi = DziTileSource.Parse(stream);

        Assert.Equal(500, dzi.ImageWidth);
        Assert.Equal(300, dzi.ImageHeight);
        Assert.Equal("png", dzi.Format);
        Assert.Equal(1, dzi.Overlap);
    }

    [Fact]
    public void GetOptimalLevel_FullyZoomedOut_ReturnsLowLevel()
    {
        var dzi = new DziTileSource(1024, 1024, 256, 0, "jpg");
        // ViewportWidth = 1.0 means full image fits in control
        // Control width = 256 → we need level where levelWidth ≈ 256
        int level = dzi.GetOptimalLevel(1.0, 256);
        Assert.InRange(level, 7, 9); // should be around 8 (256 = 2^8)
    }

    [Fact]
    public void GetOptimalLevel_FullyZoomedIn_ReturnsHighLevel()
    {
        var dzi = new DziTileSource(1024, 1024, 256, 0, "jpg");
        // ViewportWidth very small = very zoomed in — should return max or near max
        int level = dzi.GetOptimalLevel(0.001, 256);
        Assert.InRange(level, dzi.MaxLevel - 1, dzi.MaxLevel);
    }

    [Fact]
    public void GetOptimalLevel_MidZoom_ReturnsMiddleLevel()
    {
        var dzi = new DziTileSource(4096, 4096, 256, 0, "jpg");
        // MaxLevel = 12; viewportWidth=0.5, controlWidth=512
        int level = dzi.GetOptimalLevel(0.5, 512);
        Assert.InRange(level, 9, 12);
    }

    [Fact]
    public void GetOptimalLevel_TinyViewport_ReturnsMaxLevel()
    {
        var dzi = new DziTileSource(2048, 2048, 256, 0, "jpg");
        int level = dzi.GetOptimalLevel(0.01, 1024);
        Assert.Equal(dzi.MaxLevel, level);
    }

    [Fact]
    public void GetFullTileUrl_MultipleCoordinates_FormatsCorrectly()
    {
        var dzi = new DziTileSource(2048, 2048, 256, 0, "png");
        dzi.TilesBaseUri = "https://cdn.example.com/tiles/";
        Assert.Equal("https://cdn.example.com/tiles/10/0_0.png", dzi.GetFullTileUrl(10, 0, 0));
        Assert.Equal("https://cdn.example.com/tiles/8/2_3.png", dzi.GetFullTileUrl(8, 2, 3));
        Assert.Equal("https://cdn.example.com/tiles/0/0_0.png", dzi.GetFullTileUrl(0, 0, 0));
    }

    [Fact]
    public void EdgeTile_IsClipped()
    {
        // Image is 300 wide, tileSize 256. Last tile should be 44 pixels wide
        var dzi = new DziTileSource(300, 200, 256, 0, "jpg");
        var bounds = dzi.GetTileBounds(dzi.MaxLevel, 1, 0);
        Assert.Equal(256, bounds.X);
        Assert.Equal(44, bounds.Width); // 300 - 256 = 44
    }

    [Fact]
    public void Parse_MalformedXml_ThrowsException()
    {
        Assert.Throws<System.Xml.XmlException>(() =>
            DziTileSource.Parse("not valid xml"));
    }

    [Fact]
    public void Parse_MissingTileSize_ThrowsFormatException()
    {
        string xml = @"<?xml version='1.0'?>
<Image xmlns='http://schemas.microsoft.com/deepzoom/2008' Format='jpg'>
  <Size Width='1000' Height='800'/>
</Image>";
        Assert.Throws<FormatException>(() => DziTileSource.Parse(xml));
    }

    [Fact]
    public void Parse_WithBaseUri_RoundTrips()
    {
        var xml = @"<Image TileSize=""256"" Overlap=""0"" Format=""jpg"" xmlns=""http://schemas.microsoft.com/deepzoom/2008""><Size Width=""512"" Height=""512""/></Image>";
        var dzi = DziTileSource.Parse(xml, "http://example.com/tiles/");
        Assert.Equal("http://example.com/tiles/", dzi.TilesBaseUri);
        Assert.Equal(512, dzi.ImageWidth);
    }
}
