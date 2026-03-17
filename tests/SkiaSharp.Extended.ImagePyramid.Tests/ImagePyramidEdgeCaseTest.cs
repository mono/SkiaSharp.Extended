using SkiaSharp;
using SkiaSharp.Extended;
using Xunit;

namespace SkiaSharp.Extended.ImagePyramid.Tests;

/// <summary>
/// Edge case and robustness tests for ImagePyramid components.
/// </summary>
public class ImagePyramidEdgeCaseTest
{
    // --- SKImagePyramidViewport edge cases ---

    [Fact]
    public void Viewport_VerySmallViewportWidth_HighZoom()
    {
        var vp = new SKImagePyramidViewport();
        vp.ControlWidth = 800;
        vp.ControlHeight = 600;
        vp.ViewportWidth = 0.001;
        var (x, y) = vp.LogicalToElementPoint(0.0005, 0.0005);
        Assert.True(x > 0 && x < 800);
    }

    [Fact]
    public void Viewport_NegativeOrigin_PushesContentRight()
    {
        var vp = new SKImagePyramidViewport();
        vp.ControlWidth = 800;
        vp.ControlHeight = 600;
        vp.ViewportOriginX = -0.5;
        vp.ViewportOriginY = -0.5;
        var (x, y) = vp.LogicalToElementPoint(0, 0);
        Assert.True(x > 0, "Negative origin should push content right");
    }

    [Fact]
    public void Viewport_ElementToLogical_RoundTrip()
    {
        var vp = new SKImagePyramidViewport();
        vp.ControlWidth = 800;
        vp.ControlHeight = 600;
        vp.ViewportWidth = 0.5;
        vp.ViewportOriginX = 0.2;
        vp.ViewportOriginY = 0.1;

        var (lx, ly) = vp.ElementToLogicalPoint(400, 300);
        var (ex, ey) = vp.LogicalToElementPoint(lx, ly);

        Assert.Equal(400, ex, 0.01);
        Assert.Equal(300, ey, 0.01);
    }

    [Fact]
    public void Viewport_ZoomAboutCenter_InvariantHolds()
    {
        var vp = new SKImagePyramidViewport();
        vp.ControlWidth = 800;
        vp.ControlHeight = 600;
        vp.AspectRatio = 4.0 / 3.0;

        // Zoom 2x about center
        vp.ZoomAboutLogicalPoint(2.0, 0.5, 0.5 / vp.AspectRatio);

        // After zoom, the center should still map to screen center
        var (sx, sy) = vp.LogicalToElementPoint(0.5, 0.5 / vp.AspectRatio);
        Assert.Equal(400, sx, 1.0);
    }

    [Fact]
    public void Viewport_Constrain_ClampsOrigin()
    {
        var vp = new SKImagePyramidViewport();
        vp.ControlWidth = 800;
        vp.ControlHeight = 600;
        vp.AspectRatio = 1.0;
        vp.ViewportWidth = 0.5;
        vp.ViewportOriginX = 0.9; // Would go past right edge
        vp.Constrain();
        Assert.True(vp.ViewportOriginX + vp.ViewportWidth <= 1.001);
    }

    [Fact]
    public void Viewport_Pan_MovesLogicalOrigin()
    {
        var vp = new SKImagePyramidViewport();
        vp.ControlWidth = 800;
        vp.ControlHeight = 600;
        var origX = vp.ViewportOriginX;

        vp.PanByScreenDelta(100, 0);
        Assert.NotEqual(origX, vp.ViewportOriginX);
    }

    [Fact]
    public void Viewport_GetState_RoundTrip()
    {
        var vp = new SKImagePyramidViewport();
        vp.ViewportWidth = 0.3;
        vp.ViewportOriginX = 0.2;
        vp.ViewportOriginY = 0.1;

        var state = vp.GetState();
        var vp2 = new SKImagePyramidViewport();
        vp2.SetState(state);

        Assert.Equal(vp.ViewportWidth, vp2.ViewportWidth);
        Assert.Equal(vp.ViewportOriginX, vp2.ViewportOriginX);
        Assert.Equal(vp.ViewportOriginY, vp2.ViewportOriginY);
    }

    [Fact]
    public void Viewport_FitToView_WideImage_ViewportWidthIs1()
    {
        // Wide image (16:9) in a 4:3 control → fits in width, ViewportWidth = 1.0
        var vp = new SKImagePyramidViewport();
        vp.ControlWidth = 800;
        vp.ControlHeight = 600;
        vp.AspectRatio = 16.0 / 9.0; // wide
        vp.FitToView();

        Assert.Equal(1.0, vp.ViewportWidth, 6);
        // Image is narrower than control height allows → centered vertically
        Assert.Equal(0.0, vp.ViewportOriginX, 6);
    }

    [Fact]
    public void Viewport_FitToView_TallImage_ViewportWidthGreaterThan1()
    {
        // Square image in 800×600 control → image is taller relative to control
        // controlAspectRatio = 800/600 = 1.333, imageAspectRatio = 1.0
        // fitWidth = max(1, (1/1.0) * 800/600) = 1.333
        var vp = new SKImagePyramidViewport();
        vp.ControlWidth = 800;
        vp.ControlHeight = 600;
        vp.AspectRatio = 1.0; // square image
        vp.FitToView();

        double expected = 800.0 / 600.0; // ≈ 1.333
        Assert.Equal(expected, vp.ViewportWidth, 6);
        // MaxViewportWidth is left unchanged by FitToView so users can zoom out past fit
        Assert.Equal(double.MaxValue, vp.MaxViewportWidth);
    }

    [Fact]
    public void Viewport_FitToView_EntireImageVisible()
    {
        // After FitToView, the visible logical bounds must contain the full image [0,1]×[0, 1/AR]
        var vp = new SKImagePyramidViewport();
        vp.ControlWidth = 800;
        vp.ControlHeight = 600;
        vp.AspectRatio = 1.5; // landscape image, narrower than control
        vp.FitToView();

        var (left, top, right, bottom) = vp.GetLogicalBounds();
        // Full image spans x=[0,1], y=[0, 1/1.5]
        Assert.True(left <= 0.0, $"left={left} should be <= 0");
        Assert.True(top <= 0.0, $"top={top} should be <= 0");
        Assert.True(right >= 1.0, $"right={right} should be >= 1");
        Assert.True(bottom >= 1.0 / 1.5, $"bottom={bottom} should be >= {1.0 / 1.5}");
    }

    // --- DZI parsing edge cases ---

    [Fact]
    public void DziParse_MinimalDzi()
    {
        var xml = @"<Image xmlns='http://schemas.microsoft.com/deepzoom/2008'
                     Format='png' TileSize='256' Overlap='0'>
                     <Size Width='100' Height='100'/></Image>";
        var dzi = SKImagePyramidDziSource.Parse(xml, "http://example.com/img");
        Assert.Equal(100, dzi.ImageWidth);
        Assert.Equal(100, dzi.ImageHeight);
        Assert.Equal("png", dzi.Format);
    }

    [Fact]
    public void DziParse_LargeImage()
    {
        var xml = @"<Image xmlns='http://schemas.microsoft.com/deepzoom/2008'
                     Format='jpg' TileSize='512' Overlap='1'>
                     <Size Width='100000' Height='80000'/></Image>";
        var dzi = SKImagePyramidDziSource.Parse(xml, "http://example.com/big");
        Assert.Equal(100000, dzi.ImageWidth);
        Assert.True(dzi.MaxLevel > 15);
    }

    [Fact]
    public void DziParse_NoOverlap()
    {
        var xml = @"<Image xmlns='http://schemas.microsoft.com/deepzoom/2008'
                     Format='jpg' TileSize='256' Overlap='0'>
                     <Size Width='1024' Height='768'/></Image>";
        var dzi = SKImagePyramidDziSource.Parse(xml, "http://example.com/nooverlap");
        Assert.Equal(0, dzi.Overlap);
    }

    [Fact]
    public void DziParse_OnePixelImage()
    {
        var xml = @"<Image xmlns='http://schemas.microsoft.com/deepzoom/2008'
                     Format='png' TileSize='256' Overlap='0'>
                     <Size Width='1' Height='1'/></Image>";
        var dzi = SKImagePyramidDziSource.Parse(xml, "http://example.com/tiny");
        Assert.Equal(0, dzi.MaxLevel);
    }

    // --- DZC parsing ---

    [Fact]
    public void DzcParse_MinimalCollection()
    {
        var xml = @"<Collection xmlns='http://schemas.microsoft.com/deepzoom/2009'
                     MaxLevel='8' TileSize='256' Format='jpg'>
                     <Items>
                       <I Id='0' N='0' Source='items/0.dzi'>
                         <Size Width='400' Height='300'/>
                         <Viewport Width='2.0' X='-0.5' Y='-0.3'/>
                       </I>
                     </Items></Collection>";
        using var stream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(xml));
        var dzc = SKImagePyramidDziCollectionSource.Parse(stream);
        Assert.Single(dzc.Items);
    }

    // --- Morton grid math ---

    [Fact]
    public void Morton_ZeroIndex()
    {
        var (col, row) = SKImagePyramidDziCollectionSource.MortonToGrid(0);
        Assert.Equal(0, col);
        Assert.Equal(0, row);
    }

    [Fact]
    public void Morton_SmallIndices()
    {
        var (c1, r1) = SKImagePyramidDziCollectionSource.MortonToGrid(1);
        Assert.Equal(1, c1);
        Assert.Equal(0, r1);

        var (c2, r2) = SKImagePyramidDziCollectionSource.MortonToGrid(2);
        Assert.Equal(0, c2);
        Assert.Equal(1, r2);

        var (c3, r3) = SKImagePyramidDziCollectionSource.MortonToGrid(3);
        Assert.Equal(1, c3);
        Assert.Equal(1, r3);
    }

    [Fact]
    public void Morton_RoundTrip()
    {
        for (int i = 0; i < 256; i++)
        {
            var (col, row) = SKImagePyramidDziCollectionSource.MortonToGrid(i);
            var back = SKImagePyramidDziCollectionSource.GridToMorton(col, row);
            Assert.Equal(i, back);
        }
    }

    [Fact]
    public void Morton_GridSize_Powers()
    {
        var dzc = CreateTestDzc(1);
        Assert.Equal(1, dzc.GetMortonGridSize());

        dzc = CreateTestDzc(4);
        Assert.Equal(2, dzc.GetMortonGridSize());

        dzc = CreateTestDzc(5);
        Assert.Equal(4, dzc.GetMortonGridSize());
    }

    // --- SKImagePyramidMemoryTileCache edge cases ---

    [Fact]
    public void TileCache_SingleCapacity()
    {
        var cache = new SKImagePyramidMemoryTileCache(1);
        var a = new SKImagePyramidTileId(0, 0, 0);
        var b = new SKImagePyramidTileId(1, 0, 0);
        var bmp1 = new SKBitmap(10, 10);
        var bmp2 = new SKBitmap(10, 10);

        cache.Put(a, new SKImagePyramidImageTile(SKImage.FromBitmap(bmp1)));
        Assert.True(cache.TryGet(a, out _));

        cache.Put(b, new SKImagePyramidImageTile(SKImage.FromBitmap(bmp2)));
        Assert.False(cache.TryGet(a, out _)); // Evicted
        Assert.True(cache.TryGet(b, out _));
    }

    [Fact]
    public void TileCache_Clear_EmptiesCache()
    {
        var cache = new SKImagePyramidMemoryTileCache(10);
        for (int i = 0; i < 5; i++)
        {
            cache.Put(new SKImagePyramidTileId(i, 0, 0), new SKImagePyramidImageTile(SKImage.Create(new SKImageInfo(10, 10))));
        }

        Assert.Equal(5, cache.Count);
        cache.Clear();
        Assert.Equal(0, cache.Count);
    }

    [Fact]
    public void TileCache_Remove_Returns()
    {
        var cache = new SKImagePyramidMemoryTileCache(10);
        var id = new SKImagePyramidTileId(0, 0, 0);
        cache.Put(id, new SKImagePyramidImageTile(SKImage.Create(new SKImageInfo(10, 10))));
        Assert.True(cache.Remove(id));
        Assert.False(cache.Remove(id));
    }

    [Fact]
    public void TileCache_LRU_EvictsOldest()
    {
        var cache = new SKImagePyramidMemoryTileCache(3);
        var a = new SKImagePyramidTileId(0, 0, 0);
        var b = new SKImagePyramidTileId(0, 1, 0);
        var c = new SKImagePyramidTileId(0, 0, 1);
        var d = new SKImagePyramidTileId(0, 1, 1);

        cache.Put(a, new SKImagePyramidImageTile(SKImage.Create(new SKImageInfo(10, 10))));
        cache.Put(b, new SKImagePyramidImageTile(SKImage.Create(new SKImageInfo(10, 10))));
        cache.Put(c, new SKImagePyramidImageTile(SKImage.Create(new SKImageInfo(10, 10))));

        cache.TryGet(a, out _); // Touch "a"

        cache.Put(d, new SKImagePyramidImageTile(SKImage.Create(new SKImageInfo(10, 10)))); // Evicts "b"

        Assert.True(cache.TryGet(a, out _));
        Assert.False(cache.TryGet(b, out _)); // evicted
        Assert.True(cache.TryGet(c, out _));
        Assert.True(cache.TryGet(d, out _));
    }

    // --- SKImagePyramidTileLayout edge cases ---

    [Fact]
    public void TileLayout_VeryHighZoom_FewTiles()
    {
        var dzi = CreateTestDzi(1024, 1024);
        var vp = new SKImagePyramidViewport
        {
            ControlWidth = 1024,
            ControlHeight = 768,
            ViewportWidth = 0.001,
            ViewportOriginX = 0.5,
            ViewportOriginY = 0.5
        };

        var layout = new SKImagePyramidTileLayout();
        var tiles = layout.GetVisibleTiles(dzi, vp);
        Assert.True(tiles.Count > 0);
        Assert.True(tiles.Count < 20);
    }

    [Fact]
    public void TileLayout_MinZoom_ReturnsTiles()
    {
        var dzi = CreateTestDzi(4096, 4096);
        var vp = new SKImagePyramidViewport
        {
            ControlWidth = 800,
            ControlHeight = 600,
            ViewportWidth = 1.0
        };

        var layout = new SKImagePyramidTileLayout();
        var tiles = layout.GetVisibleTiles(dzi, vp);
        Assert.True(tiles.Count > 0);
    }

    // --- SKImagePyramidController edge cases ---

    [Fact]
    public void Controller_Load_SetsUpState()
    {
        using var controller = new SKImagePyramidController();
        var dzi = CreateTestDzi(2048, 1536);
        controller.SetControlSize(800, 600);
        controller.Load(dzi, new MemoryTileFetcher());

        Assert.NotNull(controller.TileSource);
        Assert.Equal(2048.0 / 1536.0, controller.AspectRatio, 2);
    }

    [Fact]
    public void Controller_ResetView_GoesToFitAll()
    {
        using var controller = new SKImagePyramidController();
        controller.SetControlSize(800, 600);
        controller.Load(CreateTestDzi(2048, 1536), new MemoryTileFetcher());

        controller.ZoomAboutScreenPoint(4.0, 400, 300);
        controller.ResetView();

        // Controller applies changes immediately (no spring)
        Assert.Equal(1.0, controller.Viewport.ViewportWidth, 0.01);
    }

    [Fact]
    public void Controller_DoubleDispose_Safe()
    {
        var controller = new SKImagePyramidController();
        controller.Dispose();
        controller.Dispose();
    }

    [Fact]
    public void Controller_Pan_MovesOrigin()
    {
        using var controller = new SKImagePyramidController();
        controller.SetControlSize(800, 600);
        controller.Load(CreateTestDzi(2048, 1536), new MemoryTileFetcher());

        // Zoom in first so pan has room
        controller.ZoomAboutScreenPoint(4.0, 400, 300);

        var originBefore = controller.Viewport.ViewportOriginX;
        controller.Pan(100, 50);
        Assert.NotEqual(originBefore, controller.Viewport.ViewportOriginX);
    }

    [Fact]
    public void Controller_IsIdle_WhenNoActivity()
    {
        using var controller = new SKImagePyramidController();
        controller.SetControlSize(800, 600);
        controller.Load(CreateTestDzi(512, 512), new MemoryTileFetcher());

        // IsIdle = no pending tiles (spring is view-layer concern)
        Assert.True(controller.IsIdle);
    }

    [Fact]
    public void Controller_ImmediateTransition_NoSpring()
    {
        // Spring belongs to the view (SKImagePyramidView), not the controller.
        // SKImagePyramidViewport changes via ZoomAboutScreenPoint are immediate.
        using var controller = new SKImagePyramidController();
        controller.SetControlSize(800, 600);
        controller.Load(CreateTestDzi(2048, 1536), new MemoryTileFetcher());

        double before = controller.Viewport.ViewportWidth;
        controller.ZoomAboutScreenPoint(2.0, 400, 300);
        Assert.True(controller.Viewport.ViewportWidth < before);
    }

    private static SKImagePyramidDziSource CreateTestDzi(int width, int height)
    {
        var xml = $@"<Image xmlns='http://schemas.microsoft.com/deepzoom/2008'
                     Format='jpg' TileSize='256' Overlap='1'>
                     <Size Width='{width}' Height='{height}'/></Image>";
        return SKImagePyramidDziSource.Parse(xml, "http://test.com/img");
    }

    private static SKImagePyramidDziCollectionSource CreateTestDzc(int itemCount)
    {
        var items = new List<SKImagePyramidDziCollectionSubImage>();
        for (int i = 0; i < itemCount; i++)
        {
            items.Add(new SKImagePyramidDziCollectionSubImage(i, i, 256, 256, null));
        }
        return new SKImagePyramidDziCollectionSource(8, 256, "jpg", items);
    }
}
