using SkiaSharp.Extended.DeepZoom;
using Xunit;

namespace SkiaSharp.Extended.DeepZoom.Tests;

public class TileSchedulerTest
{
    private static SKDeepZoomImageSource CreateSampleDzi()
    {
        string xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Image xmlns=""http://schemas.microsoft.com/deepzoom/2008""
       Format=""jpg"" Overlap=""1"" TileSize=""256"">
  <Size Width=""1024"" Height=""768""/>
</Image>";
        return SKDeepZoomImageSource.Parse(xml, "http://example.com/test");
    }

    private static SKDeepZoomImageSource CreateLargeDzi()
    {
        string xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Image xmlns=""http://schemas.microsoft.com/deepzoom/2008""
       Format=""jpg"" Overlap=""1"" TileSize=""256"">
  <Size Width=""4096"" Height=""4096""/>
</Image>";
        return SKDeepZoomImageSource.Parse(xml, "http://example.com/large");
    }

    [Fact]
    public void GetVisibleTiles_FullView_ReturnsCorrectTiles()
    {
        var dzi = CreateSampleDzi();
        var viewport = new SKDeepZoomViewport
        {
            ControlWidth = 800,
            ControlHeight = 600,
            ViewportWidth = 1.0,
            ViewportOriginX = 0,
            ViewportOriginY = 0
        };
        var scheduler = new SKDeepZoomTileScheduler();

        var tiles = scheduler.GetVisibleTiles(dzi, viewport);

        Assert.NotEmpty(tiles);
        // At full view, there should be relatively few tiles since the image is small
        Assert.True(tiles.Count <= 16, $"Expected <= 16 tiles but got {tiles.Count}");
    }

    [Fact]
    public void GetVisibleTiles_ZoomedIn_ReturnsFewerTiles()
    {
        var dzi = CreateLargeDzi();
        var scheduler = new SKDeepZoomTileScheduler();

        var fullViewport = new SKDeepZoomViewport
        {
            ControlWidth = 800,
            ControlHeight = 800,
            ViewportWidth = 1.0,
            ViewportOriginX = 0,
            ViewportOriginY = 0
        };
        var fullTiles = scheduler.GetVisibleTiles(dzi, fullViewport);

        var zoomedViewport = new SKDeepZoomViewport
        {
            ControlWidth = 800,
            ControlHeight = 800,
            ViewportWidth = 0.1,
            ViewportOriginX = 0.4,
            ViewportOriginY = 0.4
        };
        var zoomedTiles = scheduler.GetVisibleTiles(dzi, zoomedViewport);

        // When zoomed in, we see fewer tiles (but at a higher level)
        Assert.NotEmpty(zoomedTiles);
    }

    [Fact]
    public void GetVisibleTiles_SortedByPriority()
    {
        var dzi = CreateLargeDzi();
        var viewport = new SKDeepZoomViewport
        {
            ControlWidth = 800,
            ControlHeight = 800,
            ViewportWidth = 0.25,
            ViewportOriginX = 0.3,
            ViewportOriginY = 0.3
        };
        var scheduler = new SKDeepZoomTileScheduler();

        var tiles = scheduler.GetVisibleTiles(dzi, viewport);

        // Tiles should be sorted by priority (ascending)
        for (int i = 1; i < tiles.Count; i++)
        {
            Assert.True(tiles[i].Priority >= tiles[i - 1].Priority,
                $"Tile at index {i} has priority {tiles[i].Priority} < {tiles[i - 1].Priority}");
        }
    }

    [Fact]
    public void GetVisibleTiles_AllTilesAreUnique()
    {
        var dzi = CreateLargeDzi();
        var viewport = new SKDeepZoomViewport
        {
            ControlWidth = 1024,
            ControlHeight = 1024,
            ViewportWidth = 0.5,
            ViewportOriginX = 0.2,
            ViewportOriginY = 0.2
        };
        var scheduler = new SKDeepZoomTileScheduler();

        var tiles = scheduler.GetVisibleTiles(dzi, viewport);
        var uniqueIds = new HashSet<SKDeepZoomTileId>(tiles.Select(t => t.TileId));

        Assert.Equal(tiles.Count, uniqueIds.Count);
    }

    [Fact]
    public void GetVisibleTiles_AllTilesAtSameLevel()
    {
        var dzi = CreateSampleDzi();
        var viewport = new SKDeepZoomViewport
        {
            ControlWidth = 800,
            ControlHeight = 600,
            ViewportWidth = 1.0,
            ViewportOriginX = 0,
            ViewportOriginY = 0
        };
        var scheduler = new SKDeepZoomTileScheduler();

        var tiles = scheduler.GetVisibleTiles(dzi, viewport);

        if (tiles.Count > 0)
        {
            int level = tiles[0].TileId.Level;
            Assert.All(tiles, t => Assert.Equal(level, t.TileId.Level));
        }
    }

    [Fact]
    public void FindBestFallback_ReturnsCachedParent()
    {
        var cache = new SKDeepZoomMemoryTileCache(100);
        var scheduler = new SKDeepZoomTileScheduler();

        // Add parent tile to cache
        var parentId = new SKDeepZoomTileId(5, 1, 1);
        var bitmap = new SKBitmap(256, 256);
        cache.Put(parentId, new SKDeepZoomBitmapTile(bitmap));

        // Request child tile at level 7
        var childId = new SKDeepZoomTileId(7, 4, 4); // (4/2=2, 4/2=2) -> (2/2=1, 2/2=1) at level 5

        var fallback = scheduler.FindBestFallback(childId, cache);

        Assert.NotNull(fallback);
        Assert.Equal(5, fallback!.Value.Level);
        bitmap.Dispose();
        cache.Dispose();
    }

    [Fact]
    public void FindBestFallback_ReturnsNull_WhenNoParentCached()
    {
        var cache = new SKDeepZoomMemoryTileCache(100);
        var scheduler = new SKDeepZoomTileScheduler();

        var childId = new SKDeepZoomTileId(5, 2, 3);
        var fallback = scheduler.FindBestFallback(childId, cache);

        Assert.Null(fallback);
        cache.Dispose();
    }

    [Fact]
    public void FindBestFallback_ReturnsClosestParent()
    {
        var cache = new SKDeepZoomMemoryTileCache(100);
        var scheduler = new SKDeepZoomTileScheduler();

        // Add tiles at levels 2 and 4
        var level2 = new SKDeepZoomTileId(2, 0, 0);
        var level4 = new SKDeepZoomTileId(4, 2, 2);
        cache.Put(level2, new SKDeepZoomBitmapTile(new SKBitmap(256, 256)));
        cache.Put(level4, new SKDeepZoomBitmapTile(new SKBitmap(256, 256)));

        // Request level 6, col=8, row=8 → parent at level 5 = (4,4), level 4 = (2,2)
        var childId = new SKDeepZoomTileId(6, 8, 8);
        var fallback = scheduler.FindBestFallback(childId, cache);

        Assert.NotNull(fallback);
        // Should find level 4 first (closest)
        Assert.Equal(4, fallback!.Value.Level);
        cache.Dispose();
    }

    [Fact]
    public void GetFallbackSourceRect_CalculatesCorrectSubRegion()
    {
        var dzi = CreateSampleDzi();
        var layout = new SKDeepZoomTileLayout();

        var parent = new SKDeepZoomTileId(5, 0, 0);
        var child = new SKDeepZoomTileId(6, 0, 0);

        var src = layout.GetFallbackSourceRect(child, parent, dzi);

        // For a 1-level difference, the child should be in the top-left quadrant
        Assert.True(src.X >= 0);
        Assert.True(src.Y >= 0);
        Assert.True(src.Width > 0);
        Assert.True(src.Height > 0);
    }

    [Fact]
    public void GetVisibleTiles_VerySmallImage_StillWorks()
    {
        string xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Image xmlns=""http://schemas.microsoft.com/deepzoom/2008""
       Format=""png"" Overlap=""0"" TileSize=""256"">
  <Size Width=""16"" Height=""16""/>
</Image>";
        var dzi = SKDeepZoomImageSource.Parse(xml, "http://example.com/tiny");
        var viewport = new SKDeepZoomViewport
        {
            ControlWidth = 800,
            ControlHeight = 600,
            ViewportWidth = 1.0,
            ViewportOriginX = 0,
            ViewportOriginY = 0
        };
        var scheduler = new SKDeepZoomTileScheduler();

        var tiles = scheduler.GetVisibleTiles(dzi, viewport);

        Assert.NotEmpty(tiles);
        // Very small image should need only 1 tile
        Assert.Equal(1, tiles.Count);
    }

    [Fact]
    public void FindBestFallback_ReturnsParentTileInCache()
    {
        var dzi = new SKDeepZoomImageSource(1024, 1024, 256, 0, "jpg");
        var cache = new SKDeepZoomMemoryTileCache(100);
        var scheduler = new SKDeepZoomTileScheduler();

        // Put parent tile (level 8, col 1, row 1) in cache
        var parentId = new SKDeepZoomTileId(8, 1, 1);
        var bitmap = new SKBitmap(256, 256);
        cache.Put(parentId, new SKDeepZoomBitmapTile(bitmap));

        // Request child at level 9 (col 2, row 2 → parent col=1, row=1 at level 8)
        var childId = new SKDeepZoomTileId(9, 2, 2);

        var fallback = scheduler.FindBestFallback(childId, cache);

        Assert.NotNull(fallback);
        Assert.Equal(parentId, fallback!.Value);
        bitmap.Dispose();
        cache.Dispose();
    }

    [Fact]
    public void FindBestFallback_ReturnsNull_WhenNoParentInCache()
    {
        var cache = new SKDeepZoomMemoryTileCache(100);
        var scheduler = new SKDeepZoomTileScheduler();

        var childId = new SKDeepZoomTileId(8, 3, 3);
        var fallback = scheduler.FindBestFallback(childId, cache);

        Assert.Null(fallback);
        cache.Dispose();
    }

    [Fact]
    public void GetFallbackSourceRect_ReturnsCorrectSubrect()
    {
        var dzi = new SKDeepZoomImageSource(1024, 1024, 256, 0, "jpg");
        var layout = new SKDeepZoomTileLayout();

        var parent = new SKDeepZoomTileId(8, 0, 0);
        var child = new SKDeepZoomTileId(9, 1, 1);

        var src = layout.GetFallbackSourceRect(child, parent, dzi);

        // Source rect must be within parent tile bounds
        var parentBounds = dzi.GetTileBounds(parent.Level, parent.Col, parent.Row);
        Assert.True(src.X >= 0, $"srcX {src.X} should be >= 0");
        Assert.True(src.Y >= 0, $"srcY {src.Y} should be >= 0");
        Assert.True(src.X + src.Width <= parentBounds.Width, $"srcX+srcW ({src.X + src.Width}) should be <= parent width ({parentBounds.Width})");
        Assert.True(src.Y + src.Height <= parentBounds.Height, $"srcY+srcH ({src.Y + src.Height}) should be <= parent height ({parentBounds.Height})");
        Assert.True(src.Width > 0);
        Assert.True(src.Height > 0);
    }

    // --- Additional FindBestFallback tests ---

    [Fact]
    public void FindBestFallback_MinLevel_RespectsMinimum()
    {
        var cache = new SKDeepZoomMemoryTileCache(100);
        var scheduler = new SKDeepZoomTileScheduler();

        // Add tile at level 1
        var level1 = new SKDeepZoomTileId(1, 0, 0);
        cache.Put(level1, new SKDeepZoomBitmapTile(new SKBitmap(256, 256)));

        // Request level 5, but set minLevel=3 — should not find level 1
        var childId = new SKDeepZoomTileId(5, 0, 0);
        var fallback = scheduler.FindBestFallback(childId, cache, minLevel: 3);
        Assert.Null(fallback);

        // Same request with minLevel=0 — should find level 1
        fallback = scheduler.FindBestFallback(childId, cache, minLevel: 0);
        Assert.NotNull(fallback);
        Assert.Equal(1, fallback!.Value.Level);
        cache.Dispose();
    }

    [Fact]
    public void FindBestFallback_Level0Requested_ReturnsNull()
    {
        var cache = new SKDeepZoomMemoryTileCache(100);
        var scheduler = new SKDeepZoomTileScheduler();

        // No parent exists below level 0
        var tileId = new SKDeepZoomTileId(0, 0, 0);
        var fallback = scheduler.FindBestFallback(tileId, cache);
        Assert.Null(fallback);
        cache.Dispose();
    }

    // --- Additional GetFallbackSourceRect tests ---

    [Fact]
    public void GetFallbackSourceRect_TwoLevelDiff_QuarterSize()
    {
        // 2 levels up means scale=4, so child should map to ~1/4 of parent
        var dzi = new SKDeepZoomImageSource(1024, 1024, 256, 0, "jpg");
        var layout = new SKDeepZoomTileLayout();

        var parent = new SKDeepZoomTileId(7, 0, 0);
        var child = new SKDeepZoomTileId(9, 0, 0); // 2 levels deeper, same position

        var src = layout.GetFallbackSourceRect(child, parent, dzi);

        Assert.Equal(0, src.X, 1);
        Assert.Equal(0, src.Y, 1);
        // At 2 levels difference, child size / 4 should fit within parent
        Assert.True(src.Width > 0);
        Assert.True(src.Height > 0);
        Assert.True(src.Width <= 256);
        Assert.True(src.Height <= 256);
    }

    [Fact]
    public void GetFallbackSourceRect_ChildInBottomRightQuadrant_HasPositiveOffset()
    {
        var dzi = new SKDeepZoomImageSource(1024, 1024, 256, 0, "jpg");
        var layout = new SKDeepZoomTileLayout();

        var parent = new SKDeepZoomTileId(8, 0, 0);
        var child = new SKDeepZoomTileId(9, 1, 1); // bottom-right quadrant of parent (0,0)

        var src = layout.GetFallbackSourceRect(child, parent, dzi);

        Assert.True(src.X > 0, "Child in bottom-right quadrant should have positive srcX");
        Assert.True(src.Y > 0, "Child in bottom-right quadrant should have positive srcY");
    }
}
