using SkiaSharp;
using SkiaSharp.Extended.DeepZoom;
using Xunit;

namespace SkiaSharp.Extended.DeepZoom.Tests;

/// <summary>
/// Edge case and robustness tests for DeepZoom components.
/// </summary>
public class DeepZoomEdgeCaseTest
{
    // --- Spring edge cases ---

    [Fact]
    public void Spring_VeryLargeDeltaTime_Clamped()
    {
        var spring = new SpringAnimator();
        spring.Target = 1.0;
        spring.Update(100.0);
        Assert.InRange(spring.Current, 0.0, 1.1);
    }

    [Fact]
    public void Spring_TargetEqualsCurrent_IsSettled()
    {
        var spring = new SpringAnimator(5.0);
        spring.Target = 5.0;
        Assert.True(spring.IsSettled);
    }

    [Fact]
    public void Spring_SnapToTarget_Immediate()
    {
        var spring = new SpringAnimator(0.0);
        spring.Target = 10.0;
        spring.SnapToTarget();
        Assert.Equal(10.0, spring.Current);
        Assert.True(spring.IsSettled);
    }

    [Fact]
    public void Spring_ChangingTarget_ConvergesToNew()
    {
        var spring = new SpringAnimator(0.0);
        spring.Target = 1.0;
        spring.Update(0.01);

        spring.Target = -5.0;
        for (int i = 0; i < 200; i++)
            spring.Update(0.016);

        Assert.True(Math.Abs(spring.Current - (-5.0)) < 0.5);
    }

    [Fact]
    public void Spring_StiffnessAndDamping_Clamp()
    {
        var spring = new SpringAnimator();
        spring.Stiffness = -10;
        Assert.True(spring.Stiffness >= 0);
        spring.DampingRatio = -1;
        Assert.True(spring.DampingRatio >= 0);
    }

    // --- SKDeepZoomViewport edge cases ---

    [Fact]
    public void Viewport_VerySmallViewportWidth_HighZoom()
    {
        var vp = new SKDeepZoomViewport();
        vp.ControlWidth = 800;
        vp.ControlHeight = 600;
        vp.ViewportWidth = 0.001;
        var (x, y) = vp.LogicalToElementPoint(0.0005, 0.0005);
        Assert.True(x > 0 && x < 800);
    }

    [Fact]
    public void Viewport_NegativeOrigin_PushesContentRight()
    {
        var vp = new SKDeepZoomViewport();
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
        var vp = new SKDeepZoomViewport();
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
        var vp = new SKDeepZoomViewport();
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
        var vp = new SKDeepZoomViewport();
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
        var vp = new SKDeepZoomViewport();
        vp.ControlWidth = 800;
        vp.ControlHeight = 600;
        var origX = vp.ViewportOriginX;

        vp.PanByScreenDelta(100, 0);
        Assert.NotEqual(origX, vp.ViewportOriginX);
    }

    [Fact]
    public void Viewport_GetState_RoundTrip()
    {
        var vp = new SKDeepZoomViewport();
        vp.ViewportWidth = 0.3;
        vp.ViewportOriginX = 0.2;
        vp.ViewportOriginY = 0.1;

        var state = vp.GetState();
        var vp2 = new SKDeepZoomViewport();
        vp2.SetState(state);

        Assert.Equal(vp.ViewportWidth, vp2.ViewportWidth);
        Assert.Equal(vp.ViewportOriginX, vp2.ViewportOriginX);
        Assert.Equal(vp.ViewportOriginY, vp2.ViewportOriginY);
    }

    // --- DZI parsing edge cases ---

    [Fact]
    public void DziParse_MinimalDzi()
    {
        var xml = @"<Image xmlns='http://schemas.microsoft.com/deepzoom/2008'
                     Format='png' TileSize='256' Overlap='0'>
                     <Size Width='100' Height='100'/></Image>";
        var dzi = SKDeepZoomImageSource.Parse(xml, "http://example.com/img");
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
        var dzi = SKDeepZoomImageSource.Parse(xml, "http://example.com/big");
        Assert.Equal(100000, dzi.ImageWidth);
        Assert.True(dzi.MaxLevel > 15);
    }

    [Fact]
    public void DziParse_NoOverlap()
    {
        var xml = @"<Image xmlns='http://schemas.microsoft.com/deepzoom/2008'
                     Format='jpg' TileSize='256' Overlap='0'>
                     <Size Width='1024' Height='768'/></Image>";
        var dzi = SKDeepZoomImageSource.Parse(xml, "http://example.com/nooverlap");
        Assert.Equal(0, dzi.Overlap);
    }

    [Fact]
    public void DziParse_OnePixelImage()
    {
        var xml = @"<Image xmlns='http://schemas.microsoft.com/deepzoom/2008'
                     Format='png' TileSize='256' Overlap='0'>
                     <Size Width='1' Height='1'/></Image>";
        var dzi = SKDeepZoomImageSource.Parse(xml, "http://example.com/tiny");
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
        var dzc = SKDeepZoomCollectionSource.Parse(stream);
        Assert.Single(dzc.Items);
    }

    // --- Morton grid math ---

    [Fact]
    public void Morton_ZeroIndex()
    {
        var (col, row) = SKDeepZoomCollectionSource.MortonToGrid(0);
        Assert.Equal(0, col);
        Assert.Equal(0, row);
    }

    [Fact]
    public void Morton_SmallIndices()
    {
        var (c1, r1) = SKDeepZoomCollectionSource.MortonToGrid(1);
        Assert.Equal(1, c1);
        Assert.Equal(0, r1);

        var (c2, r2) = SKDeepZoomCollectionSource.MortonToGrid(2);
        Assert.Equal(0, c2);
        Assert.Equal(1, r2);

        var (c3, r3) = SKDeepZoomCollectionSource.MortonToGrid(3);
        Assert.Equal(1, c3);
        Assert.Equal(1, r3);
    }

    [Fact]
    public void Morton_RoundTrip()
    {
        for (int i = 0; i < 256; i++)
        {
            var (col, row) = SKDeepZoomCollectionSource.MortonToGrid(i);
            var back = SKDeepZoomCollectionSource.GridToMorton(col, row);
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

    // --- SKDeepZoomTileCache edge cases ---

    [Fact]
    public void TileCache_SingleCapacity()
    {
        var cache = new SKDeepZoomTileCache(1);
        var a = new SKDeepZoomTileId(0, 0, 0);
        var b = new SKDeepZoomTileId(1, 0, 0);
        var bmp1 = new SKBitmap(10, 10);
        var bmp2 = new SKBitmap(10, 10);

        cache.Put(a, bmp1);
        Assert.True(cache.TryGet(a, out _));

        cache.Put(b, bmp2);
        Assert.False(cache.TryGet(a, out _)); // Evicted
        Assert.True(cache.TryGet(b, out _));
    }

    [Fact]
    public void TileCache_Clear_EmptiesCache()
    {
        var cache = new SKDeepZoomTileCache(10);
        for (int i = 0; i < 5; i++)
        {
            cache.Put(new SKDeepZoomTileId(i, 0, 0), new SKBitmap(10, 10));
        }

        Assert.Equal(5, cache.Count);
        cache.Clear();
        Assert.Equal(0, cache.Count);
    }

    [Fact]
    public void TileCache_Remove_Returns()
    {
        var cache = new SKDeepZoomTileCache(10);
        var id = new SKDeepZoomTileId(0, 0, 0);
        cache.Put(id, new SKBitmap(10, 10));
        Assert.True(cache.Remove(id));
        Assert.False(cache.Remove(id));
    }

    [Fact]
    public void TileCache_LRU_EvictsOldest()
    {
        var cache = new SKDeepZoomTileCache(3);
        var a = new SKDeepZoomTileId(0, 0, 0);
        var b = new SKDeepZoomTileId(0, 1, 0);
        var c = new SKDeepZoomTileId(0, 0, 1);
        var d = new SKDeepZoomTileId(0, 1, 1);

        cache.Put(a, new SKBitmap(10, 10));
        cache.Put(b, new SKBitmap(10, 10));
        cache.Put(c, new SKBitmap(10, 10));

        cache.TryGet(a, out _); // Touch "a"

        cache.Put(d, new SKBitmap(10, 10)); // Evicts "b"

        Assert.True(cache.TryGet(a, out _));
        Assert.False(cache.TryGet(b, out _)); // evicted
        Assert.True(cache.TryGet(c, out _));
        Assert.True(cache.TryGet(d, out _));
    }

    // --- SKDeepZoomTileScheduler edge cases ---

    [Fact]
    public void TileScheduler_VeryHighZoom_FewTiles()
    {
        var dzi = CreateTestDzi(1024, 1024);
        var vp = new SKDeepZoomViewport
        {
            ControlWidth = 1024,
            ControlHeight = 768,
            ViewportWidth = 0.001,
            ViewportOriginX = 0.5,
            ViewportOriginY = 0.5
        };

        var scheduler = new SKDeepZoomTileScheduler();
        var tiles = scheduler.GetVisibleTiles(dzi, vp);
        Assert.True(tiles.Count > 0);
        Assert.True(tiles.Count < 20);
    }

    [Fact]
    public void TileScheduler_MinZoom_ReturnsTiles()
    {
        var dzi = CreateTestDzi(4096, 4096);
        var vp = new SKDeepZoomViewport
        {
            ControlWidth = 800,
            ControlHeight = 600,
            ViewportWidth = 1.0
        };

        var scheduler = new SKDeepZoomTileScheduler();
        var tiles = scheduler.GetVisibleTiles(dzi, vp);
        Assert.True(tiles.Count > 0);
    }

    // --- SKDeepZoomController edge cases ---

    [Fact]
    public void Controller_Load_SetsUpState()
    {
        using var controller = new SKDeepZoomController();
        var dzi = CreateTestDzi(2048, 1536);
        controller.SetControlSize(800, 600);
        controller.Load(dzi, new MemoryTileFetcher());

        Assert.NotNull(controller.TileSource);
        Assert.Equal(2048.0 / 1536.0, controller.AspectRatio, 2);
    }

    [Fact]
    public void Controller_ResetView_GoesToFitAll()
    {
        using var controller = new SKDeepZoomController();
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
        var controller = new SKDeepZoomController();
        controller.Dispose();
        controller.Dispose();
    }

    [Fact]
    public void Controller_Pan_MovesOrigin()
    {
        using var controller = new SKDeepZoomController();
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
        using var controller = new SKDeepZoomController();
        controller.SetControlSize(800, 600);
        controller.Load(CreateTestDzi(512, 512), new MemoryTileFetcher());

        // IsIdle = no pending tiles (spring is view-layer concern)
        Assert.True(controller.IsIdle);
    }

    [Fact]
    public void Controller_ImmediateTransition_NoSpring()
    {
        // Spring belongs to the view (SKDeepZoomView), not the controller.
        // SKDeepZoomViewport changes via ZoomAboutScreenPoint are immediate.
        using var controller = new SKDeepZoomController();
        controller.SetControlSize(800, 600);
        controller.Load(CreateTestDzi(2048, 1536), new MemoryTileFetcher());

        double before = controller.Viewport.ViewportWidth;
        controller.ZoomAboutScreenPoint(2.0, 400, 300);
        Assert.True(controller.Viewport.ViewportWidth < before);
    }

    private static SKDeepZoomImageSource CreateTestDzi(int width, int height)
    {
        var xml = $@"<Image xmlns='http://schemas.microsoft.com/deepzoom/2008'
                     Format='jpg' TileSize='256' Overlap='1'>
                     <Size Width='{width}' Height='{height}'/></Image>";
        return SKDeepZoomImageSource.Parse(xml, "http://test.com/img");
    }

    private static SKDeepZoomCollectionSource CreateTestDzc(int itemCount)
    {
        var items = new List<SKDeepZoomCollectionSubImage>();
        for (int i = 0; i < itemCount; i++)
        {
            items.Add(new SKDeepZoomCollectionSubImage(i, i, 256, 256, null));
        }
        return new SKDeepZoomCollectionSource(8, 256, "jpg", items);
    }
}
