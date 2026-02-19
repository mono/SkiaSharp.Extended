using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharp;
using SkiaSharp.Extended.DeepZoom;

namespace SkiaSharp.Extended.PivotViewer
{
    /// <summary>
    /// Provides thumbnail images for PivotViewerItems by extracting sub-images
    /// from DZC composite tiles. This is the bridge between the CXML data model
    /// and the DZC tile pyramid.
    /// </summary>
    public class CollectionImageProvider : IDisposable
    {
        private readonly DzcTileSource _dzc;
        private readonly ITileFetcher _fetcher;
        private readonly TileCache _cache;
        private readonly ConcurrentDictionary<int, SKBitmap?> _thumbnailCache;
        private readonly ConcurrentDictionary<int, SemaphoreSlim> _loadLocks;
        private readonly string _basePath;
        private bool _disposed;

        /// <summary>
        /// Creates a new image provider for a DZC collection.
        /// </summary>
        /// <param name="dzc">The parsed DZC tile source.</param>
        /// <param name="fetcher">Tile fetcher for loading composite tiles.</param>
        /// <param name="basePath">Base path/URL for the DZC tiles directory (e.g., "conceptcars_files").</param>
        public CollectionImageProvider(DzcTileSource dzc, ITileFetcher fetcher, string basePath)
        {
            _dzc = dzc ?? throw new ArgumentNullException(nameof(dzc));
            _fetcher = fetcher ?? throw new ArgumentNullException(nameof(fetcher));
            _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));
            _cache = new TileCache(512);
            _thumbnailCache = new ConcurrentDictionary<int, SKBitmap?>();
            _loadLocks = new ConcurrentDictionary<int, SemaphoreSlim>();
        }

        /// <summary>Number of cached thumbnails.</summary>
        public int CachedThumbnailCount => _thumbnailCache.Count;

        /// <summary>
        /// Gets a thumbnail for the given DZC sub-image index.
        /// Returns null if the thumbnail hasn't been loaded yet (call LoadThumbnailAsync to load).
        /// </summary>
        public SKBitmap? GetThumbnail(int itemIndex)
        {
            _thumbnailCache.TryGetValue(itemIndex, out var bitmap);
            return bitmap;
        }

        /// <summary>
        /// Gets a thumbnail from the item's "#Image" property value (e.g., "#42").
        /// </summary>
        public SKBitmap? GetThumbnailForItem(PivotViewerItem item)
        {
            int? idx = GetItemImageIndex(item);
            return idx.HasValue ? GetThumbnail(idx.Value) : null;
        }

        /// <summary>
        /// Extracts the DZC index from a PivotViewerItem's "#Image" property.
        /// Returns null if the item has no image reference.
        /// </summary>
        public static int? GetItemImageIndex(PivotViewerItem item)
        {
            var imgValues = item["#Image"];
            if (imgValues != null && imgValues.Count > 0)
            {
                var img = imgValues[0]?.ToString();
                if (img != null && img.StartsWith("#") && int.TryParse(img.Substring(1), out int index))
                    return index;
            }
            return null;
        }

        /// <summary>
        /// Loads a thumbnail for the given DZC sub-image index by extracting it
        /// from the appropriate composite tile at a low pyramid level.
        /// </summary>
        public async Task<SKBitmap?> LoadThumbnailAsync(int itemIndex, int targetSize = 128, CancellationToken ct = default)
        {
            if (_disposed) return null;

            if (_thumbnailCache.TryGetValue(itemIndex, out var cached))
                return cached;

            if (itemIndex < 0 || itemIndex >= _dzc.ItemCount)
                return null;

            // Use per-item lock to prevent duplicate bitmap creation
            var loadLock = _loadLocks.GetOrAdd(itemIndex, _ => new SemaphoreSlim(1, 1));
            await loadLock.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                // Double-check after acquiring lock
                if (_disposed) return null;
                if (_thumbnailCache.TryGetValue(itemIndex, out cached))
                    return cached;

                var subImage = _dzc.Items.FirstOrDefault(i => i.Id == itemIndex);
                if (subImage == null)
                    return null;

                SKBitmap? thumbnail;
                if (subImage.Source != null)
                {
                    // IsPath items: load from individual DZI tile pyramid
                    thumbnail = await LoadIsPathThumbnailAsync(subImage, targetSize, ct).ConfigureAwait(false);
                }
                else
                {
                    // Composite mosaic: extract from DZC composite tiles
                    thumbnail = await LoadCompositeThumbnailAsync(subImage, targetSize, ct).ConfigureAwait(false);
                }

                _thumbnailCache[itemIndex] = thumbnail;
                return thumbnail;
            }
            finally
            {
                loadLock.Release();
            }
        }

        private async Task<SKBitmap?> LoadIsPathThumbnailAsync(DzcSubImage subImage, int targetSize, CancellationToken ct)
        {
            // For IsPath items, the source is a .dzi file. The tile pyramid is at
            // {basePath}/{source.Replace(".dzi","_files")}/{level}/{col}_{row}.{format}
            // We find the lowest level where the image fits in a single tile (0_0)
            // and is at least targetSize pixels.
            var source = subImage.Source!;
            // Robustly construct the _files directory path
            var filesDir = source.EndsWith(".dzi", StringComparison.OrdinalIgnoreCase)
                ? source.Substring(0, source.Length - 4) + "_files"
                : source + "_files";

            // Use DZC-level defaults for format/tileSize.
            // For better compatibility with external collections, try both jpg and png.
            string primaryFormat = _dzc.Format ?? "jpg";
            int tileSize = _dzc.TileSize > 0 ? _dzc.TileSize : 256;
            int maxDim = Math.Max(subImage.Width, subImage.Height);
            int maxLevel = maxDim > 0 ? (int)Math.Ceiling(Math.Log(maxDim) / Math.Log(2)) : 0;

            int maxSingleTileLevel = (int)Math.Floor(Math.Log(tileSize) / Math.Log(2));

            int bestLevel = 0;
            int effectiveMax = Math.Min(maxLevel, maxSingleTileLevel);
            for (int level = 0; level <= effectiveMax; level++)
            {
                bestLevel = level;
                int levelDim = 1 << level;
                if (levelDim >= targetSize)
                    break;
            }

            // Try primary format first, then fallback to alternative
            string url = $"{_basePath}/{filesDir}/{bestLevel}/0_0.{primaryFormat}";
            var tileBitmap = await _fetcher.FetchTileAsync(url, ct).ConfigureAwait(false);

            if (tileBitmap == null)
            {
                // Try alternative format (jpg ↔ png)
                string altFormat = primaryFormat == "jpg" ? "png" : "jpg";
                string altUrl = $"{_basePath}/{filesDir}/{bestLevel}/0_0.{altFormat}";
                tileBitmap = await _fetcher.FetchTileAsync(altUrl, ct).ConfigureAwait(false);
            }

            return tileBitmap;
        }

        private async Task<SKBitmap?> LoadCompositeThumbnailAsync(DzcSubImage subImage, int targetSize, CancellationToken ct)
        {
            int gridSize = _dzc.GetMortonGridSize();
            if (gridSize == 0) return null;

            // Choose a level where each item is roughly targetSize pixels
            int bestLevel = 0;
            for (int level = 0; level <= _dzc.MaxLevel; level++)
            {
                int levelWidth = _dzc.TileSize * (1 << level);
                double itemPixelSize = (double)levelWidth / gridSize;
                if (itemPixelSize >= targetSize)
                {
                    bestLevel = level;
                    break;
                }
                bestLevel = level;
            }

            var (col, row) = DzcTileSource.MortonToGrid(subImage.MortonIndex);

            int levelTotalWidth = _dzc.TileSize * (1 << bestLevel);
            double itemPixWidth = (double)levelTotalWidth / gridSize;
            double itemPixHeight = itemPixWidth / subImage.AspectRatio;

            double itemPxX = col * itemPixWidth;
            double itemPxY = row * itemPixHeight;

            int tileCol = (int)(itemPxX / _dzc.TileSize);
            int tileRow = (int)(itemPxY / _dzc.TileSize);

            var tileId = new TileId(bestLevel, tileCol, tileRow);
            SKBitmap? tileBitmap;

            if (!_cache.TryGet(tileId, out tileBitmap))
            {
                string url = $"{_basePath}/{_dzc.GetCompositeTileUrl(bestLevel, tileCol, tileRow)}";
                tileBitmap = await _fetcher.FetchTileAsync(url, ct).ConfigureAwait(false);

                if (_disposed) return null;
                if (tileBitmap != null)
                    _cache.Put(tileId, tileBitmap);
            }

            if (tileBitmap == null)
                return null;

            double localX = itemPxX - tileCol * _dzc.TileSize;
            double localY = itemPxY - tileRow * _dzc.TileSize;

            int srcX = Math.Max(0, (int)localX);
            int srcY = Math.Max(0, (int)localY);
            int srcW = Math.Min((int)Math.Ceiling(itemPixWidth), tileBitmap.Width - srcX);
            int srcH = Math.Min((int)Math.Ceiling(itemPixHeight), tileBitmap.Height - srcY);

            if (srcW <= 0 || srcH <= 0)
                return null;

            var srcRect = new SKRectI(srcX, srcY, srcX + srcW, srcY + srcH);
            var thumbnail = new SKBitmap(srcW, srcH);
            using (var canvas = new SKCanvas(thumbnail))
            {
                canvas.DrawBitmap(tileBitmap, srcRect, new SKRect(0, 0, srcW, srcH));
            }

            return thumbnail;
        }

        /// <summary>
        /// Loads thumbnails for all visible items in a batch.
        /// </summary>
        public async Task LoadThumbnailsAsync(IEnumerable<PivotViewerItem> items, int targetSize = 128, CancellationToken ct = default)
        {
            var tasks = new List<Task>();
            foreach (var item in items)
            {
                int? idx = GetItemImageIndex(item);
                if (idx.HasValue && !_thumbnailCache.ContainsKey(idx.Value))
                {
                    tasks.Add(LoadThumbnailAsync(idx.Value, targetSize, ct));
                }
            }
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        /// <summary>Clears all cached thumbnails.</summary>
        public void ClearCache()
        {
            foreach (var kv in _thumbnailCache)
                kv.Value?.Dispose();
            _thumbnailCache.Clear();
            _cache.Clear();
        }

        /// <summary>
        /// Flushes deferred bitmap disposals from the tile cache.
        /// Call this on the UI thread before rendering to safely dispose evicted bitmaps.
        /// </summary>
        public void FlushEvictedTiles() => _cache.FlushEvicted();

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                ClearCache();
                foreach (var kv in _loadLocks)
                    kv.Value?.Dispose();
                _loadLocks.Clear();
                _cache.Dispose();
            }
        }
    }
}
