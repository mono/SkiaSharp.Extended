#nullable enable

using System;
using System.Collections.Generic;

namespace SkiaSharp.Extended.DeepZoom
{
    /// <summary>
    /// Determines which tiles are needed for the current viewport and prioritizes them.
    /// Handles parent tile fallback when tiles are loading or missing.
    /// </summary>
    public class SKDeepZoomTileScheduler
    {
        /// <summary>
        /// Computes the set of tiles visible in the current viewport at the optimal level.
        /// </summary>
        public IReadOnlyList<SKDeepZoomTileRequest> GetVisibleTiles(
            SKDeepZoomImageSource tileSource, SKDeepZoomViewport viewport)
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

            // Convert to tile coordinates
            int tileSize = tileSource.TileSize;
            int startCol = Math.Max(0, pixelLeft / tileSize);
            int startRow = Math.Max(0, pixelTop / tileSize);
            int endCol = Math.Min(tileSource.GetTileCountX(optimalLevel) - 1, (pixelRight - 1) / tileSize);
            int endRow = Math.Min(tileSource.GetTileCountY(optimalLevel) - 1, (pixelBottom - 1) / tileSize);

            var tiles = new List<SKDeepZoomTileRequest>();

            for (int row = startRow; row <= endRow; row++)
            {
                for (int col = startCol; col <= endCol; col++)
                {
                    var id = new SKDeepZoomTileId(optimalLevel, col, row);
                    double centerX = (col + 0.5) * tileSize / scaleX;
                    double centerY = (row + 0.5) * tileSize / scaleY;

                    // Priority: distance from viewport center (closer = higher priority)
                    double vpCenterX = (left + right) / 2;
                    double vpCenterY = (top + bottom) / 2;
                    double dist = Math.Sqrt(
                        (centerX - vpCenterX) * (centerX - vpCenterX) +
                        (centerY - vpCenterY) * (centerY - vpCenterY));

                    tiles.Add(new SKDeepZoomTileRequest(id, dist));
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
        public SKDeepZoomTileId? FindBestFallback(SKDeepZoomTileId requested, SKDeepZoomTileCache cache, int minLevel = 0)
        {
            int col = requested.Col;
            int row = requested.Row;

            for (int level = requested.Level - 1; level >= minLevel; level--)
            {
                col /= 2;
                row /= 2;
                var parentId = new SKDeepZoomTileId(level, col, row);
                if (cache.Contains(parentId))
                    return parentId;
            }

            return null;
        }

        /// <summary>
        /// Computes the source rect within a parent tile that corresponds to the child tile.
        /// Used for rendering a scaled portion of a lower-resolution tile as a fallback.
        /// </summary>
        public (float SrcX, float SrcY, float SrcW, float SrcH) GetFallbackSourceRect(
            SKDeepZoomTileId requested, SKDeepZoomTileId parent, SKDeepZoomImageSource tileSource)
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

            // Offset relative to parent tile
            float srcX = reqLeftInParent - parentBounds.X;
            float srcY = reqTopInParent - parentBounds.Y;

            return (srcX, srcY, reqWidthInParent, reqHeightInParent);
        }
    }
}
