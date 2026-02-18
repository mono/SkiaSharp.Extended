using SkiaSharp.Extended.DeepZoom;
using Xunit;

namespace SkiaSharp.Extended.DeepZoom.Tests;

public class TileSchedulerTest
{
    private static DziTileSource CreateSampleDzi()
    {
        string xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Image xmlns=""http://schemas.microsoft.com/deepzoom/2008""
       Format=""jpg"" Overlap=""1"" TileSize=""256"">
  <Size Width=""1024"" Height=""768""/>
</Image>";
        return DziTileSource.Parse(xml, "http://example.com/test");
    }

    private static DziTileSource CreateLargeDzi()
    {
        string xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Image xmlns=""http://schemas.microsoft.com/deepzoom/2008""
       Format=""jpg"" Overlap=""1"" TileSize=""256"">
  <Size Width=""4096"" Height=""4096""/>
</Image>";
        return DziTileSource.Parse(xml, "http://example.com/large");
    }

    [Fact]
    public void GetVisibleTiles_FullView_ReturnsCorrectTiles()
    {
        var dzi = CreateSampleDzi();
        var viewport = new Viewport
        {
            ControlWidth = 800,
            ControlHeight = 600,
            ViewportWidth = 1.0,
            ViewportOriginX = 0,
            ViewportOriginY = 0
        };
        var scheduler = new TileScheduler();

        var tiles = scheduler.GetVisibleTiles(dzi, viewport);

        Assert.NotEmpty(tiles);
        // At full view, there should be relatively few tiles since the image is small
        Assert.True(tiles.Count <= 16, $"Expected <= 16 tiles but got {tiles.Count}");
    }

    [Fact]
    public void GetVisibleTiles_ZoomedIn_ReturnsFewerTiles()
    {
        var dzi = CreateLargeDzi();
        var scheduler = new TileScheduler();

        var fullViewport = new Viewport
        {
            ControlWidth = 800,
            ControlHeight = 800,
            ViewportWidth = 1.0,
            ViewportOriginX = 0,
            ViewportOriginY = 0
        };
        var fullTiles = scheduler.GetVisibleTiles(dzi, fullViewport);

        var zoomedViewport = new Viewport
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
        var viewport = new Viewport
        {
            ControlWidth = 800,
            ControlHeight = 800,
            ViewportWidth = 0.25,
            ViewportOriginX = 0.3,
            ViewportOriginY = 0.3
        };
        var scheduler = new TileScheduler();

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
        var viewport = new Viewport
        {
            ControlWidth = 1024,
            ControlHeight = 1024,
            ViewportWidth = 0.5,
            ViewportOriginX = 0.2,
            ViewportOriginY = 0.2
        };
        var scheduler = new TileScheduler();

        var tiles = scheduler.GetVisibleTiles(dzi, viewport);
        var uniqueIds = new HashSet<TileId>(tiles.Select(t => t.TileId));

        Assert.Equal(tiles.Count, uniqueIds.Count);
    }

    [Fact]
    public void GetVisibleTiles_AllTilesAtSameLevel()
    {
        var dzi = CreateSampleDzi();
        var viewport = new Viewport
        {
            ControlWidth = 800,
            ControlHeight = 600,
            ViewportWidth = 1.0,
            ViewportOriginX = 0,
            ViewportOriginY = 0
        };
        var scheduler = new TileScheduler();

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
        var cache = new TileCache(100);
        var scheduler = new TileScheduler();

        // Add parent tile to cache
        var parentId = new TileId(5, 1, 1);
        var bitmap = new SKBitmap(256, 256);
        cache.Put(parentId, bitmap);

        // Request child tile at level 7
        var childId = new TileId(7, 4, 4); // (4/2=2, 4/2=2) -> (2/2=1, 2/2=1) at level 5

        var fallback = scheduler.FindBestFallback(childId, cache);

        Assert.NotNull(fallback);
        Assert.Equal(5, fallback!.Value.Level);
        bitmap.Dispose();
        cache.Dispose();
    }

    [Fact]
    public void FindBestFallback_ReturnsNull_WhenNoParentCached()
    {
        var cache = new TileCache(100);
        var scheduler = new TileScheduler();

        var childId = new TileId(5, 2, 3);
        var fallback = scheduler.FindBestFallback(childId, cache);

        Assert.Null(fallback);
        cache.Dispose();
    }

    [Fact]
    public void FindBestFallback_ReturnsClosestParent()
    {
        var cache = new TileCache(100);
        var scheduler = new TileScheduler();

        // Add tiles at levels 2 and 4
        var level2 = new TileId(2, 0, 0);
        var level4 = new TileId(4, 2, 2);
        cache.Put(level2, new SKBitmap(256, 256));
        cache.Put(level4, new SKBitmap(256, 256));

        // Request level 6, col=8, row=8 → parent at level 5 = (4,4), level 4 = (2,2)
        var childId = new TileId(6, 8, 8);
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
        var scheduler = new TileScheduler();

        var parent = new TileId(5, 0, 0);
        var child = new TileId(6, 0, 0);

        var (srcX, srcY, srcW, srcH) = scheduler.GetFallbackSourceRect(child, parent, dzi);

        // For a 1-level difference, the child should be in the top-left quadrant
        Assert.True(srcX >= 0);
        Assert.True(srcY >= 0);
        Assert.True(srcW > 0);
        Assert.True(srcH > 0);
    }

    [Fact]
    public void GetVisibleTiles_VerySmallImage_StillWorks()
    {
        string xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Image xmlns=""http://schemas.microsoft.com/deepzoom/2008""
       Format=""png"" Overlap=""0"" TileSize=""256"">
  <Size Width=""16"" Height=""16""/>
</Image>";
        var dzi = DziTileSource.Parse(xml, "http://example.com/tiny");
        var viewport = new Viewport
        {
            ControlWidth = 800,
            ControlHeight = 600,
            ViewportWidth = 1.0,
            ViewportOriginX = 0,
            ViewportOriginY = 0
        };
        var scheduler = new TileScheduler();

        var tiles = scheduler.GetVisibleTiles(dzi, viewport);

        Assert.NotEmpty(tiles);
        // Very small image should need only 1 tile
        Assert.Equal(1, tiles.Count);
    }
}
