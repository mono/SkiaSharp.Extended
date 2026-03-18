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
        // GetTileBounds(level, 0, 0) gives the first tile's extent (includes DZI overlap).
        // For startCol we use effectiveTileW conservatively (tile c is visible if its
        // right edge > pixelLeft; tile 0's right = effectiveTileW, so startCol = pixelLeft/effectiveTileW).
        // For endCol we need the actual stride (left edge of col 1) so we don't over-include:
        // tile c is visible if c*stride <= pixelRight-1, i.e. endCol = (pixelRight-1)/stride.
        int tileCountX = tileSource.GetTileCountX(optimalLevel);
        int tileCountY = tileSource.GetTileCountY(optimalLevel);
        var firstTile = tileSource.GetTileBounds(optimalLevel, 0, 0);
        int effectiveTileW = Math.Max(1, firstTile.Width);
        int effectiveTileH = Math.Max(1, firstTile.Height);

        // Stride = distance between left edges of consecutive tiles.
        // For DZI: TileSize - Overlap. For IIIF: TileWidth. Falls back when only 1 tile.
        int strideW = tileCountX > 1 ? Math.Max(1, tileSource.GetTileBounds(optimalLevel, 1, 0).X) : effectiveTileW;
        int strideH = tileCountY > 1 ? Math.Max(1, tileSource.GetTileBounds(optimalLevel, 0, 1).Y) : effectiveTileH;

        // startCol: first tile whose right edge exceeds pixelLeft (conservative: use tile width)
        int startCol = Math.Max(0, pixelLeft / effectiveTileW);
        int startRow = Math.Max(0, pixelTop / effectiveTileH);
        // endCol: last tile whose left edge is before pixelRight (exact: use stride, no +1 buffer)
        int endCol = Math.Min(tileCountX - 1, pixelRight > 0 ? (pixelRight - 1) / strideW : 0);
        int endRow = Math.Min(tileCountY - 1, pixelBottom > 0 ? (pixelBottom - 1) / strideH : 0);

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
    /// <remarks>
    /// Corners are pixel-snapped using floor/ceiling so that adjacent tiles share the same
    /// integer pixel boundary, eliminating sub-pixel gaps that cause flickering seams.
    /// </remarks>
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

        // Pixel-snap: floor top-left, ceiling bottom-right.
        // This ensures adjacent tiles always share the same integer boundary, eliminating
        // sub-pixel floating-point gaps that cause flickering seams between tiles.
        // Adjacent tiles may overlap by at most 1px (drawn in row/column order so later
        // tiles are painted on top), which is always preferable to a transparent gap.
        float x      = (float)Math.Floor(topLeft.X);
        float y      = (float)Math.Floor(topLeft.Y);
        float right  = (float)Math.Ceiling(bottomRight.X);
        float bottom = (float)Math.Ceiling(bottomRight.Y);

        return new SKImagePyramidRectF(x, y, right - x, bottom - y);
    }
}
