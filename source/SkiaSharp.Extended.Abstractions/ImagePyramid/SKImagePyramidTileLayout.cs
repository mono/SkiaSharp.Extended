#nullable enable

using System;
using System.Collections.Generic;

namespace SkiaSharp.Extended;

/// <summary>
/// Computes tile geometry for the ImagePyramid pipeline: which tiles are visible, what
/// fallback tiles exist, and where tiles should be drawn on screen.
/// Works with any <see cref="ISKImagePyramidSource"/> including DZI and IIIF sources.
/// </summary>
public class SKImagePyramidTileLayout
{
    /// <summary>
    /// Computes the set of tiles visible in the current viewport at the optimal level.
    /// </summary>
    public IReadOnlyList<SKImagePyramidTileRequest> GetVisibleTiles(
        ISKImagePyramidSource tileSource, SKImagePyramidViewport viewport)
    {
        int optimalLevel = tileSource.GetOptimalLevel(viewport.ViewportWidth, viewport.ControlWidth);

        // Clamp to valid range
        optimalLevel = Math.Max(0, Math.Min(optimalLevel, tileSource.MaxLevel));

        var (left, top, right, bottom) = viewport.GetLogicalBounds();

        // Convert logical bounds to pixel coords at this level
        int levelWidth = tileSource.GetLevelWidth(optimalLevel);
        int levelHeight = tileSource.GetLevelHeight(optimalLevel);

        double imageLogicalHeight = 1.0 / tileSource.AspectRatio;

        // Logical to level-pixel conversion
        double scaleX = levelWidth; // logical X range is 0..1 maps to 0..levelWidth
        double scaleY = levelHeight / imageLogicalHeight;

        int pixelLeft = (int)Math.Floor(left * scaleX);
        int pixelTop = (int)Math.Floor(top * scaleY);
        int pixelRight = (int)Math.Ceiling(right * scaleX);
        int pixelBottom = (int)Math.Ceiling(bottom * scaleY);

        // Clamp to level bounds
        pixelLeft = Math.Max(0, pixelLeft);
        pixelTop = Math.Max(0, pixelTop);
        pixelRight = Math.Min(levelWidth, pixelRight);
        pixelBottom = Math.Min(levelHeight, pixelBottom);

        // Convert to tile coordinates using actual tile stride from source bounds.
        // GetTileBounds(level, 0, 0) gives the real pixel extent of the first tile
        // (includes DZI overlap), used conservatively for startCol.
        // For endCol we need the actual stride (left edge of tile 1), which for DZI is
        // TileSize-Overlap (shorter than firstTile.Width). Using firstTile.Width for endCol
        // would underestimate it and miss tiles at the right/bottom edges.
        int tileCountX = tileSource.GetTileCountX(optimalLevel);
        int tileCountY = tileSource.GetTileCountY(optimalLevel);
        var firstTile = tileSource.GetTileBounds(optimalLevel, 0, 0);
        int effectiveTileW = Math.Max(1, firstTile.Width);
        int effectiveTileH = Math.Max(1, firstTile.Height);

        // Use the left edge of tile (1,0) and (0,1) as the stride for endCol/endRow.
        // This equals TileSize-Overlap for DZI (conservative, never misses a tile) and
        // TileWidth for IIIF (exact). Falls back to effectiveTileW when only 1 tile.
        int strideW = tileCountX > 1 ? Math.Max(1, tileSource.GetTileBounds(optimalLevel, 1, 0).X) : effectiveTileW;
        int strideH = tileCountY > 1 ? Math.Max(1, tileSource.GetTileBounds(optimalLevel, 0, 1).Y) : effectiveTileH;

        int startCol = Math.Max(0, pixelLeft / effectiveTileW);
        int startRow = Math.Max(0, pixelTop / effectiveTileH);
        // Use stride (not effectiveTileW) for endCol so tiles at the right/bottom boundary
        // are never missed. +1 adds a one-tile safety buffer for sub-pixel rounding.
        int endCol = Math.Min(tileCountX - 1, pixelRight > 0 ? pixelRight / strideW + 1 : 0);
        int endRow = Math.Min(tileCountY - 1, pixelBottom > 0 ? pixelBottom / strideH + 1 : 0);

        var tiles = new List<SKImagePyramidTileRequest>();

        for (int row = startRow; row <= endRow; row++)
        {
            for (int col = startCol; col <= endCol; col++)
            {
                var id = new SKImagePyramidTileId(optimalLevel, col, row);
                double centerX = (col + 0.5) * effectiveTileW / scaleX;
                double centerY = (row + 0.5) * effectiveTileH / scaleY;

                // Priority: distance from viewport center (closer = higher priority)
                double vpCenterX = (left + right) / 2;
                double vpCenterY = (top + bottom) / 2;
                double dist = Math.Sqrt(
                    (centerX - vpCenterX) * (centerX - vpCenterX) +
                    (centerY - vpCenterY) * (centerY - vpCenterY));

                tiles.Add(new SKImagePyramidTileRequest(id, dist));
            }
        }

        // Sort by priority (nearest to center first)
        tiles.Sort((a, b) => a.Priority.CompareTo(b.Priority));
        return tiles;
    }

    /// <summary>
    /// For a missing tile, finds the best available parent tile as a fallback.
    /// Walks up the pyramid from the requested level until a cached tile is found.
    /// </summary>
    public SKImagePyramidTileId? FindBestFallback(SKImagePyramidTileId requested, ISKImagePyramidTileCache cache, int minLevel = 0)
    {
        int col = requested.Col;
        int row = requested.Row;

        for (int level = requested.Level - 1; level >= minLevel; level--)
        {
            col /= 2;
            row /= 2;
            var parentId = new SKImagePyramidTileId(level, col, row);
            if (cache.Contains(parentId))
                return parentId;
        }

        return null;
    }

    /// <summary>
    /// Computes the source rect within a parent tile that corresponds to the child tile.
    /// Returns an <see cref="SKImagePyramidRectF"/> in parent-bitmap pixel coordinates.
    /// </summary>
    public SKImagePyramidRectF GetFallbackSourceRect(
        SKImagePyramidTileId requested, SKImagePyramidTileId parent, ISKImagePyramidSource tileSource)
    {
        int levelDiff = requested.Level - parent.Level;
        int scale = 1 << levelDiff;

        var parentBounds = tileSource.GetTileBounds(parent.Level, parent.Col, parent.Row);
        var requestedBounds = tileSource.GetTileBounds(requested.Level, requested.Col, requested.Row);

        // Map requested bounds back to parent level
        float reqLeftInParent = (float)requestedBounds.X / scale;
        float reqTopInParent = (float)requestedBounds.Y / scale;
        float reqWidthInParent = (float)requestedBounds.Width / scale;
        float reqHeightInParent = (float)requestedBounds.Height / scale;

        // Offset relative to parent tile's origin
        float srcX = reqLeftInParent - parentBounds.X;
        float srcY = reqTopInParent - parentBounds.Y;

        return new SKImagePyramidRectF(srcX, srcY, reqWidthInParent, reqHeightInParent);
    }

    /// <summary>
    /// Computes the screen-space destination rect for a tile given the current viewport.
    /// Previously on <see cref="SKImagePyramidRenderer"/>; moved here because it is pure geometry,
    /// independent of any rendering backend.
    /// </summary>
    public SKImagePyramidRectF GetTileDestRect(
        ISKImagePyramidSource tileSource,
        SKImagePyramidViewport viewport,
        SKImagePyramidTileId tileId)
    {
        var tileBounds = tileSource.GetTileBounds(tileId.Level, tileId.Col, tileId.Row);
        int levelWidth  = tileSource.GetLevelWidth(tileId.Level);
        int levelHeight = tileSource.GetLevelHeight(tileId.Level);

        double imageLogicalHeight = 1.0 / tileSource.AspectRatio;

        double logicalLeft   = (double)tileBounds.X / levelWidth;
        double logicalTop    = (double)tileBounds.Y / levelHeight * imageLogicalHeight;
        double logicalRight  = (double)tileBounds.Right / levelWidth;
        double logicalBottom = (double)tileBounds.Bottom / levelHeight * imageLogicalHeight;

        var topLeft     = viewport.LogicalToElementPoint(logicalLeft, logicalTop);
        var bottomRight = viewport.LogicalToElementPoint(logicalRight, logicalBottom);

        return new SKImagePyramidRectF(
            (float)topLeft.X,
            (float)topLeft.Y,
            (float)(bottomRight.X - topLeft.X),
            (float)(bottomRight.Y - topLeft.Y));
    }
}
