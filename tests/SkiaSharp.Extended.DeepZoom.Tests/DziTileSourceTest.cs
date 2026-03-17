using SkiaSharp.Extended;

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

        var dzi = SKDeepZoomImageSource.Parse(xml);

        Assert.Equal(7026, dzi.ImageWidth);
        Assert.Equal(9221, dzi.ImageHeight);
        Assert.Equal(256, dzi.TileSize);
        Assert.Equal(2, dzi.Overlap);
        Assert.Equal("jpg", dzi.Format);
    }

    [Fact]
    public void Parse_2009Namespace_ParsesCorrectly()
    {
        var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Image TileSize=""254"" Overlap=""1"" Format=""png"" xmlns=""http://schemas.microsoft.com/deepzoom/2009"">
    <Size Width=""1024"" Height=""768""/>
</Image>";

        var dzi = SKDeepZoomImageSource.Parse(xml);

        Assert.Equal(1024, dzi.ImageWidth);
        Assert.Equal(768, dzi.ImageHeight);
        Assert.Equal(254, dzi.TileSize);
        Assert.Equal(1, dzi.Overlap);
        Assert.Equal("png", dzi.Format);
    }

    [Fact]
    public void Parse_EmbeddedSampleDzi_ParsesCorrectly()
    {
        using var stream = TestDataHelper.GetStream("sample.dzi");
        var dzi = SKDeepZoomImageSource.Parse(stream);

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

        var dzi = SKDeepZoomImageSource.Parse(xml);

        Assert.Equal(13920, dzi.ImageWidth);
        Assert.Equal(10200, dzi.ImageHeight);
        Assert.Equal(254, dzi.TileSize);
        Assert.Equal(1, dzi.Overlap);
    }

    [Fact]
    public void MaxLevel_CalculatedCorrectly()
    {
        // MaxLevel = ceil(log2(max(W, H)))
        var dzi = new SKDeepZoomImageSource(7026, 9221, 256, 2, "jpg");
        Assert.Equal(14, dzi.MaxLevel); // ceil(log2(9221)) = ceil(13.17) = 14

        var small = new SKDeepZoomImageSource(240, 160, 256, 0, "jpg");
        Assert.Equal(8, small.MaxLevel); // ceil(log2(240)) = 8
    }

    [Fact]
    public void AspectRatio_CalculatedCorrectly()
    {
        var dzi = new SKDeepZoomImageSource(7026, 9221, 256, 2, "jpg");
        Assert.Equal(7026.0 / 9221.0, dzi.AspectRatio, 6);
    }

    [Fact]
    public void GetLevelWidth_Level0_Returns1()
    {
        var dzi = new SKDeepZoomImageSource(7026, 9221, 256, 2, "jpg");
        // Level 0 should be 1 pixel wide
        Assert.Equal(1, dzi.GetLevelWidth(0));
        Assert.Equal(1, dzi.GetLevelHeight(0));
    }

    [Fact]
    public void GetLevelWidth_MaxLevel_ReturnsFullResolution()
    {
        var dzi = new SKDeepZoomImageSource(7026, 9221, 256, 2, "jpg");
        Assert.Equal(7026, dzi.GetLevelWidth(dzi.MaxLevel));
        Assert.Equal(9221, dzi.GetLevelHeight(dzi.MaxLevel));
    }

    [Fact]
    public void GetLevelWidth_MiddleLevels_ScaleCorrectly()
    {
        var dzi = new SKDeepZoomImageSource(1024, 512, 256, 0, "jpg");
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
        var dzi = new SKDeepZoomImageSource(200, 100, 256, 0, "jpg");
        // Level = max, width = 200, tileSize = 256 → 1 tile
        Assert.Equal(1, dzi.GetTileCountX(dzi.MaxLevel));
    }

    [Fact]
    public void GetTileCountX_MultiTile_CalculatesCorrectly()
    {
        var dzi = new SKDeepZoomImageSource(1024, 512, 256, 0, "jpg");
        // At max level: 1024/256 = 4 tiles
        Assert.Equal(4, dzi.GetTileCountX(dzi.MaxLevel));
        Assert.Equal(2, dzi.GetTileCountY(dzi.MaxLevel));
    }

    [Fact]
    public void GetTileBounds_FirstTile_NoOverlap()
    {
        var dzi = new SKDeepZoomImageSource(1024, 512, 256, 0, "jpg");
        var bounds = dzi.GetTileBounds(dzi.MaxLevel, 0, 0);
        Assert.Equal(0, bounds.X);
        Assert.Equal(0, bounds.Y);
        Assert.Equal(256, bounds.Width);
        Assert.Equal(256, bounds.Height);
    }

    [Fact]
    public void GetTileBounds_FirstTile_WithOverlap()
    {
        var dzi = new SKDeepZoomImageSource(1024, 512, 256, 2, "jpg");
        var bounds = dzi.GetTileBounds(dzi.MaxLevel, 0, 0);
        Assert.Equal(0, bounds.X);
        Assert.Equal(0, bounds.Y);
        Assert.Equal(258, bounds.Width); // 256 + 2 (right overlap only)
        Assert.Equal(258, bounds.Height);
    }

    [Fact]
    public void GetTileUrl_FormatsCorrectly()
    {
        var dzi = new SKDeepZoomImageSource(1024, 512, 256, 0, "jpg");
        Assert.Equal("8/3_2.jpg", dzi.GetTileUrl(8, 3, 2));
    }

    [Fact]
    public void GetFullTileUrl_WithBaseUri()
    {
        var dzi = new SKDeepZoomImageSource(1024, 512, 256, 0, "jpg");
        dzi.TilesBaseUri = "http://example.com/image_files/";
        Assert.Equal("http://example.com/image_files/8/3_2.jpg", dzi.GetFullTileUrl(8, 3, 2));
    }

    [Fact]
    public void GetFullTileUrl_NoBaseUri_ReturnsNull()
    {
        var dzi = new SKDeepZoomImageSource(1024, 512, 256, 0, "jpg");
        Assert.Null(dzi.GetFullTileUrl(8, 3, 2));
    }

    [Fact]
    public void Parse_WithBaseUri_SetsTilesBaseUri()
    {
        var xml = @"<Image TileSize=""256"" Overlap=""0"" Format=""jpg"" xmlns=""http://schemas.microsoft.com/deepzoom/2008""><Size Width=""100"" Height=""100""/></Image>";
        var dzi = SKDeepZoomImageSource.Parse(xml, "http://example.com/image_files/");
        Assert.Equal("http://example.com/image_files/", dzi.TilesBaseUri);
    }

    [Fact]
    public void Parse_MissingImage_ThrowsFormatException()
    {
        var xml = @"<Root xmlns=""http://schemas.microsoft.com/deepzoom/2008""></Root>";
        Assert.Throws<FormatException>(() => SKDeepZoomImageSource.Parse(xml));
    }

    [Fact]
    public void Parse_MissingSize_ThrowsFormatException()
    {
        var xml = @"<Image TileSize=""256"" Format=""jpg"" xmlns=""http://schemas.microsoft.com/deepzoom/2008""></Image>";
        Assert.Throws<FormatException>(() => SKDeepZoomImageSource.Parse(xml));
    }

    [Fact]
    public void Constructor_InvalidWidth_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new SKDeepZoomImageSource(0, 100, 256, 0, "jpg"));
    }

    [Fact]
    public void Constructor_InvalidTileSize_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new SKDeepZoomImageSource(100, 100, 0, 0, "jpg"));
    }

    [Fact]
    public void Constructor_NegativeOverlap_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new SKDeepZoomImageSource(100, 100, 256, -1, "jpg"));
    }

    [Fact]
    public void GetLevelWidth_InvalidLevel_Throws()
    {
        var dzi = new SKDeepZoomImageSource(100, 100, 256, 0, "jpg");
        Assert.Throws<ArgumentOutOfRangeException>(() => dzi.GetLevelWidth(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => dzi.GetLevelWidth(dzi.MaxLevel + 1));
    }

    [Fact]
    public void GetLevelDimensions_NonPowerOfTwo_CeilsCorrectly()
    {
        // Non-power-of-two dimensions should round up at each level
        var dzi = new SKDeepZoomImageSource(1000, 750, 256, 0, "jpg");
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
        var dzi = SKDeepZoomImageSource.Parse(stream);

        Assert.Equal(500, dzi.ImageWidth);
        Assert.Equal(300, dzi.ImageHeight);
        Assert.Equal("png", dzi.Format);
        Assert.Equal(1, dzi.Overlap);
    }

    [Fact]
    public void GetOptimalLevel_FullyZoomedOut_ReturnsLowLevel()
    {
        var dzi = new SKDeepZoomImageSource(1024, 1024, 256, 0, "jpg");
        // ViewportWidth = 1.0 means full image fits in control
        // Control width = 256 → we need level where levelWidth ≈ 256
        int level = dzi.GetOptimalLevel(1.0, 256);
        Assert.InRange(level, 7, 9); // should be around 8 (256 = 2^8)
    }

    [Fact]
    public void GetOptimalLevel_FullyZoomedIn_ReturnsHighLevel()
    {
        var dzi = new SKDeepZoomImageSource(1024, 1024, 256, 0, "jpg");
        // ViewportWidth very small = very zoomed in — should return max or near max
        int level = dzi.GetOptimalLevel(0.001, 256);
        Assert.InRange(level, dzi.MaxLevel - 1, dzi.MaxLevel);
    }

    [Fact]
    public void GetOptimalLevel_MidZoom_ReturnsMiddleLevel()
    {
        var dzi = new SKDeepZoomImageSource(4096, 4096, 256, 0, "jpg");
        // MaxLevel = 12; viewportWidth=0.5, controlWidth=512
        int level = dzi.GetOptimalLevel(0.5, 512);
        Assert.InRange(level, 9, 12);
    }

    [Fact]
    public void GetOptimalLevel_TinyViewport_ReturnsMaxLevel()
    {
        var dzi = new SKDeepZoomImageSource(2048, 2048, 256, 0, "jpg");
        int level = dzi.GetOptimalLevel(0.01, 1024);
        Assert.Equal(dzi.MaxLevel, level);
    }

    [Fact]
    public void GetOptimalLevel_ZoomedInHigherThanZoomedOut()
    {
        // Regression: old bug had neededWidth = scale * viewportWidth = controlWidth
        // which made zoom level independent of viewportWidth
        var dzi = new SKDeepZoomImageSource(4096, 4096, 256, 0, "jpg");
        int levelZoomedOut = dzi.GetOptimalLevel(1.0, 800);
        int levelZoomedIn = dzi.GetOptimalLevel(0.01, 800);
        Assert.True(levelZoomedIn > levelZoomedOut,
            $"Zoomed in (vw=0.01) should use higher level ({levelZoomedIn}) than zoomed out (vw=1.0, level={levelZoomedOut})");
    }

    [Fact]
    public void GetFullTileUrl_MultipleCoordinates_FormatsCorrectly()
    {
        var dzi = new SKDeepZoomImageSource(2048, 2048, 256, 0, "png");
        dzi.TilesBaseUri = "https://cdn.example.com/tiles/";
        Assert.Equal("https://cdn.example.com/tiles/10/0_0.png", dzi.GetFullTileUrl(10, 0, 0));
        Assert.Equal("https://cdn.example.com/tiles/8/2_3.png", dzi.GetFullTileUrl(8, 2, 3));
        Assert.Equal("https://cdn.example.com/tiles/0/0_0.png", dzi.GetFullTileUrl(0, 0, 0));
    }

    [Fact]
    public void GetFullTileUrl_WithQueryString_AppendsAfterTilePath()
    {
        var dzi = new SKDeepZoomImageSource(2048, 2048, 256, 0, "jpg");
        dzi.TilesBaseUri = "https://cdn.example.com/image_files/";
        dzi.TilesQueryString = "?sig=ABC123&se=2030-01-01";

        var url = dzi.GetFullTileUrl(8, 3, 2);
        Assert.Equal("https://cdn.example.com/image_files/8/3_2.jpg?sig=ABC123&se=2030-01-01", url);
    }

    [Fact]
    public void GetFullTileUrl_NoQueryString_NoSuffix()
    {
        var dzi = new SKDeepZoomImageSource(2048, 2048, 256, 0, "jpg");
        dzi.TilesBaseUri = "https://cdn.example.com/image_files/";

        var url = dzi.GetFullTileUrl(8, 3, 2);
        Assert.Equal("https://cdn.example.com/image_files/8/3_2.jpg", url);
    }

    [Fact]
    public void EdgeTile_IsClipped()
    {
        // Image is 300 wide, tileSize 256. Last tile should be 44 pixels wide
        var dzi = new SKDeepZoomImageSource(300, 200, 256, 0, "jpg");
        var bounds = dzi.GetTileBounds(dzi.MaxLevel, 1, 0);
        Assert.Equal(256, bounds.X);
        Assert.Equal(44, bounds.Width); // 300 - 256 = 44
    }

    [Fact]
    public void Parse_MalformedXml_ThrowsException()
    {
        Assert.Throws<System.Xml.XmlException>(() =>
            SKDeepZoomImageSource.Parse("not valid xml"));
    }

    [Fact]
    public void Parse_MissingTileSize_ThrowsFormatException()
    {
        string xml = @"<?xml version='1.0'?>
<Image xmlns='http://schemas.microsoft.com/deepzoom/2008' Format='jpg'>
  <Size Width='1000' Height='800'/>
</Image>";
        Assert.Throws<FormatException>(() => SKDeepZoomImageSource.Parse(xml));
    }

    [Fact]
    public void Parse_WithBaseUri_RoundTrips()
    {
        var xml = @"<Image TileSize=""256"" Overlap=""0"" Format=""jpg"" xmlns=""http://schemas.microsoft.com/deepzoom/2008""><Size Width=""512"" Height=""512""/></Image>";
        var dzi = SKDeepZoomImageSource.Parse(xml, "http://example.com/tiles/");
        Assert.Equal("http://example.com/tiles/", dzi.TilesBaseUri);
        Assert.Equal(512, dzi.ImageWidth);
    }

    [Fact]
    public void GetOptimalLevel_ZeroViewportWidth_ReturnsMaxLevel()
    {
        var dzi = new SKDeepZoomImageSource(1024, 1024, 256, 0, "jpg");
        int level = dzi.GetOptimalLevel(0, 800);
        Assert.Equal(dzi.MaxLevel, level);
    }

    [Fact]
    public void GetOptimalLevel_NegativeViewportWidth_ReturnsMaxLevel()
    {
        var dzi = new SKDeepZoomImageSource(1024, 1024, 256, 0, "jpg");
        int level = dzi.GetOptimalLevel(-1, 800);
        Assert.Equal(dzi.MaxLevel, level);
    }

    [Fact]
    public void GetTileBounds_WithOverlap1_EdgeAndNonEdgeTiles()
    {
        // 1024x512, tileSize=256, overlap=1
        var dzi = new SKDeepZoomImageSource(1024, 512, 256, 1, "jpg");
        int maxLevel = dzi.MaxLevel;

        // First tile (0,0): x=0, right=min(256+1,1024)=257 → width=257
        var first = dzi.GetTileBounds(maxLevel, 0, 0);
        Assert.Equal(0, first.X);
        Assert.Equal(0, first.Y);
        Assert.Equal(257, first.Width);
        Assert.Equal(257, first.Height);

        // Interior tile (1,0): x=1*256-1=255, right=min((1+1)*256+1,1024)=513 → width=258
        var interior = dzi.GetTileBounds(maxLevel, 1, 0);
        Assert.Equal(255, interior.X);
        Assert.Equal(258, interior.Width);

        // Last column tile (3,0): x=3*256-1=767, right=min((3+1)*256+1,1024)=1024 → width=257
        var last = dzi.GetTileBounds(maxLevel, 3, 0);
        Assert.Equal(767, last.X);
        Assert.Equal(257, last.Width);
    }

    [Fact]
    public void Parse_WithBaseUri_TileUrlsIncorporateBaseUri()
    {
        var xml = @"<Image TileSize=""256"" Overlap=""0"" Format=""png"" xmlns=""http://schemas.microsoft.com/deepzoom/2008""><Size Width=""512"" Height=""512""/></Image>";
        var dzi = SKDeepZoomImageSource.Parse(xml, "https://cdn.example.com/myimage_files/");

        Assert.Equal("https://cdn.example.com/myimage_files/", dzi.TilesBaseUri);
        Assert.Equal("https://cdn.example.com/myimage_files/9/0_0.png", dzi.GetFullTileUrl(9, 0, 0));
        Assert.Equal("https://cdn.example.com/myimage_files/5/2_1.png", dzi.GetFullTileUrl(5, 2, 1));
    }

    [Fact]
    public void Parse_StreamWithBaseUri_SetsTilesBaseUri()
    {
        var xml = @"<Image TileSize=""254"" Overlap=""1"" Format=""jpg"" xmlns=""http://schemas.microsoft.com/deepzoom/2008""><Size Width=""800"" Height=""600""/></Image>";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(xml));
        var dzi = SKDeepZoomImageSource.Parse(stream, "file:///images/photo_files/");

        Assert.Equal("file:///images/photo_files/", dzi.TilesBaseUri);
        Assert.Equal(254, dzi.TileSize);
        Assert.Equal(1, dzi.Overlap);
        Assert.Equal("file:///images/photo_files/0/0_0.jpg", dzi.GetFullTileUrl(0, 0, 0));
    }

    // --- Additional GetTileCountX/Y tests at different levels ---

    [Fact]
    public void GetTileCountX_Level0_AlwaysReturns1()
    {
        var dzi = new SKDeepZoomImageSource(4096, 2048, 256, 0, "jpg");
        Assert.Equal(1, dzi.GetTileCountX(0));
        Assert.Equal(1, dzi.GetTileCountY(0));
    }

    [Fact]
    public void GetTileCountX_NonPowerOfTwo_CeilsCorrectly()
    {
        // 1000 pixels / 256 tiles = ceil(3.906) = 4
        var dzi = new SKDeepZoomImageSource(1000, 750, 256, 0, "jpg");
        Assert.Equal(4, dzi.GetTileCountX(dzi.MaxLevel));
        Assert.Equal(3, dzi.GetTileCountY(dzi.MaxLevel)); // ceil(750/256) = 3
    }

    [Fact]
    public void GetTileCountXY_AtLowerLevels_DecreaseCorrectly()
    {
        var dzi = new SKDeepZoomImageSource(2048, 2048, 256, 0, "jpg");
        // MaxLevel = 11, level width = 2048, tiles = 8
        Assert.Equal(8, dzi.GetTileCountX(dzi.MaxLevel));
        // Level MaxLevel-1: width = 1024, tiles = 4
        Assert.Equal(4, dzi.GetTileCountX(dzi.MaxLevel - 1));
        // Level MaxLevel-2: width = 512, tiles = 2
        Assert.Equal(2, dzi.GetTileCountX(dzi.MaxLevel - 2));
    }

    // --- Additional GetOptimalLevel tests ---

    [Fact]
    public void GetOptimalLevel_LargeControlWidth_ReturnsHigherLevel()
    {
        var dzi = new SKDeepZoomImageSource(4096, 4096, 256, 0, "jpg");
        int levelSmall = dzi.GetOptimalLevel(1.0, 256);
        int levelLarge = dzi.GetOptimalLevel(1.0, 2048);
        Assert.True(levelLarge >= levelSmall,
            $"Larger control ({levelLarge}) should use >= level than smaller ({levelSmall})");
    }

    [Fact]
    public void GetOptimalLevel_ExactMatchTileSize_ReturnsExpectedLevel()
    {
        // 256x256 image, tileSize=256. MaxLevel = 8
        var dzi = new SKDeepZoomImageSource(256, 256, 256, 0, "jpg");
        // viewportWidth=1.0, controlWidth=256 → neededWidth=256 → level 8
        int level = dzi.GetOptimalLevel(1.0, 256);
        Assert.Equal(dzi.MaxLevel, level);
    }

    // --- Additional GetLevelWidth/Height edge cases ---

    [Fact]
    public void GetLevelWidth_OddDimensions_CeilsCorrectly()
    {
        var dzi = new SKDeepZoomImageSource(999, 501, 256, 0, "jpg");
        int max = dzi.MaxLevel;
        Assert.Equal(999, dzi.GetLevelWidth(max));
        Assert.Equal(501, dzi.GetLevelHeight(max));
        // Level max-1: ceil(999/2) = 500, ceil(501/2) = 251
        Assert.Equal(500, dzi.GetLevelWidth(max - 1));
        Assert.Equal(251, dzi.GetLevelHeight(max - 1));
    }

    [Fact]
    public void GetLevelWidth_TinyImage_Level0Is1()
    {
        var dzi = new SKDeepZoomImageSource(3, 5, 256, 0, "jpg");
        Assert.Equal(1, dzi.GetLevelWidth(0));
        Assert.Equal(1, dzi.GetLevelHeight(0));
    }

    [Fact]
    public void Parse_WithDisplayRects_ParsesSparseImage()
    {
        var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Image TileSize=""256"" Overlap=""1"" Format=""jpg"" xmlns=""http://schemas.microsoft.com/deepzoom/2008"">
    <Size Width=""1024"" Height=""768""/>
    <DisplayRects>
        <DisplayRect MinLevel=""3"" MaxLevel=""8"">
            <Rect X=""100"" Y=""200"" Width=""400"" Height=""300""/>
        </DisplayRect>
        <DisplayRect MinLevel=""5"" MaxLevel=""10"">
            <Rect X=""0"" Y=""0"" Width=""1024"" Height=""768""/>
        </DisplayRect>
    </DisplayRects>
</Image>";

        var dzi = SKDeepZoomImageSource.Parse(xml);

        Assert.Equal(2, dzi.DisplayRects.Count);

        Assert.Equal(100, dzi.DisplayRects[0].X);
        Assert.Equal(200, dzi.DisplayRects[0].Y);
        Assert.Equal(400, dzi.DisplayRects[0].Width);
        Assert.Equal(300, dzi.DisplayRects[0].Height);
        Assert.Equal(3, dzi.DisplayRects[0].MinLevel);
        Assert.Equal(8, dzi.DisplayRects[0].MaxLevel);

        Assert.Equal(0, dzi.DisplayRects[1].X);
        Assert.Equal(0, dzi.DisplayRects[1].Y);
        Assert.Equal(1024, dzi.DisplayRects[1].Width);
        Assert.Equal(768, dzi.DisplayRects[1].Height);
        Assert.Equal(5, dzi.DisplayRects[1].MinLevel);
        Assert.Equal(10, dzi.DisplayRects[1].MaxLevel);
    }

    [Fact]
    public void Parse_WithoutDisplayRects_ReturnsEmptyList()
    {
        var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Image TileSize=""256"" Overlap=""1"" Format=""jpg"" xmlns=""http://schemas.microsoft.com/deepzoom/2008"">
    <Size Width=""1024"" Height=""768""/>
</Image>";

        var dzi = SKDeepZoomImageSource.Parse(xml);

        Assert.Empty(dzi.DisplayRects);
    }

    [Fact]
    public void Parse_DisplayRects_2009Namespace()
    {
        var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Image TileSize=""256"" Overlap=""0"" Format=""png"" xmlns=""http://schemas.microsoft.com/deepzoom/2009"">
    <Size Width=""512"" Height=""512""/>
    <DisplayRects>
        <DisplayRect MinLevel=""0"" MaxLevel=""9"">
            <Rect X=""10"" Y=""20"" Width=""100"" Height=""50""/>
        </DisplayRect>
    </DisplayRects>
</Image>";

        var dzi = SKDeepZoomImageSource.Parse(xml);

        Assert.Single(dzi.DisplayRects);
        Assert.Equal(10, dzi.DisplayRects[0].X);
        Assert.Equal(20, dzi.DisplayRects[0].Y);
        Assert.Equal(100, dzi.DisplayRects[0].Width);
        Assert.Equal(50, dzi.DisplayRects[0].Height);
        Assert.Equal(0, dzi.DisplayRects[0].MinLevel);
        Assert.Equal(9, dzi.DisplayRects[0].MaxLevel);
    }

    [Fact]
    public void DisplayRect_IsVisibleAtLevel_ReturnsCorrectly()
    {
        var rect = new SKDeepZoomDisplayRect(0, 0, 100, 100, 3, 8);

        Assert.False(rect.IsVisibleAtLevel(2));
        Assert.True(rect.IsVisibleAtLevel(3));
        Assert.True(rect.IsVisibleAtLevel(5));
        Assert.True(rect.IsVisibleAtLevel(8));
        Assert.False(rect.IsVisibleAtLevel(9));
    }

    [Fact]
    public void DisplayRect_Equality()
    {
        var a = new SKDeepZoomDisplayRect(10, 20, 100, 200, 3, 8);
        var b = new SKDeepZoomDisplayRect(10, 20, 100, 200, 3, 8);
        var c = new SKDeepZoomDisplayRect(10, 20, 100, 200, 4, 8);

        Assert.True(a == b);
        Assert.False(a == c);
        Assert.True(a != c);
        Assert.Equal(a, b);
        Assert.NotEqual(a, c);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void DisplayRect_ToString()
    {
        var rect = new SKDeepZoomDisplayRect(10, 20, 300, 400, 3, 8);
        Assert.Equal("SKDeepZoomDisplayRect(10, 20, 300x400, Levels 3-8)", rect.ToString());
    }

    [Fact]
    public void Constructor_DefaultDisplayRects_IsEmpty()
    {
        var dzi = new SKDeepZoomImageSource(1024, 768, 256, 1, "jpg");
        Assert.Empty(dzi.DisplayRects);
    }
}
