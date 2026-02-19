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
        private readonly string _queryString;
        private readonly List<SKBitmap> _pendingThumbnailDispose = new();
        private readonly object _thumbnailDisposeLock = new();
        private bool _disposed;

        /// <summary>
        /// Creates a new image provider for a DZC collection.
        /// </summary>
        /// <param name="dzc">The parsed DZC tile source.</param>
        /// <param name="fetcher">Tile fetcher for loading composite tiles.</param>
        /// <param name="basePath">Base path/URL for the DZC tiles directory (e.g., "conceptcars_files").</param>
        /// <param name="queryString">Optional query string for signed URLs (e.g., "?sig=ABC").</param>
        public CollectionImageProvider(DzcTileSource dzc, ITileFetcher fetcher, string basePath, string queryString = "")
        {
            _dzc = dzc ?? throw new ArgumentNullException(nameof(dzc));
            _fetcher = fetcher ?? throw new ArgumentNullException(nameof(fetcher));
            _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));
            _queryString = queryString ?? "";
            _cache = new TileCache(512);
            _thumbnailCache = new ConcurrentDictionary<int, SKBitmap?>();
            _loadLocks = new ConcurrentDictionary<int, SemaphoreSlim>();
        }

        /// <summary>Number of cached thumbnails.</summary>
        public int CachedThumbnailCount => _thumbnailCache.Count;

        /// <summary>
        /// Creates a CollectionImageProvider from a CXML collection source, auto-resolving
        /// ImageBase relative to the CXML URI. This is the recommended way to create an
        /// image provider for CXML-based collections.
        /// </summary>
        public static CollectionImageProvider? FromCxmlSource(
            CxmlCollectionSource source, DzcTileSource dzc, ITileFetcher fetcher)
        {
            if (source.ImageBase == null) return null;

            string basePath;
            string queryString = "";
            if (source.UriSource != null)
            {
                // Resolve ImageBase relative to the CXML URI
                var cxmlUri = source.UriSource;
                var baseUri = new Uri(cxmlUri, source.ImageBase);
                // DZC composite tiles live in {dzcName}_files/ alongside the .dzc file.
                // Use the URI path (ignoring query/fragment) to detect .dzc extension.
                // Store query/fragment separately to append after each tile path.
                var path = baseUri.GetLeftPart(UriPartial.Path);
                queryString = baseUri.ToString().Substring(path.Length); // query + fragment

                if (path.EndsWith(".dzc", StringComparison.OrdinalIgnoreCase))
                    basePath = path.Substring(0, path.Length - 4) + "_files";
                else
                {
                    // Fallback: strip filename to get directory (non-standard layout)
                    int lastSlash = path.LastIndexOf('/');
                    basePath = lastSlash >= 0
                        ? path.Substring(0, lastSlash)
                        : path;
                }
            }
            else
            {
                basePath = source.ImageBase;
                if (basePath.EndsWith(".dzc", StringComparison.OrdinalIgnoreCase))
                    basePath = basePath.Substring(0, basePath.Length - 4) + "_files";
            }

            return new CollectionImageProvider(dzc, fetcher, basePath, queryString);
        }

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

            if (itemIndex < 0)
                return null;

            // Use per-item lock to prevent duplicate bitmap creation
            var loadLock = _loadLocks.GetOrAdd(itemIndex, _ => new SemaphoreSlim(1, 1));
            if (_disposed)
            {
                // Race: semaphore may have been created after Dispose started
                if (_loadLocks.TryRemove(itemIndex, out var orphan))
                    orphan?.Dispose();
                return null;
            }

            bool lockTaken = false;
            try { await loadLock.WaitAsync(ct).ConfigureAwait(false); lockTaken = true; }
            catch (ObjectDisposedException) { return null; }
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

                // Don't cache if disposed — the bitmap will never be cleaned up
                if (_disposed)
                {
                    thumbnail?.Dispose();
                    return null;
                }

                // Only cache successful loads — null results should be retried
                if (thumbnail != null)
                    _thumbnailCache[itemIndex] = thumbnail;
                return thumbnail;
            }
            finally
            {
                if (lockTaken)
                {
                    try { loadLock.Release(); }
                    catch (ObjectDisposedException) { }
                }

                // Remove lock after item is cached to prevent unbounded growth
                // Only remove if no one else is waiting
                if (_thumbnailCache.ContainsKey(itemIndex))
                {
                    // We can't safely dispose the semaphore here because other threads might be waiting on it.
                    // We just remove it from the dictionary so future calls create a new one.
                    // The SemaphoreSlim will be GC'd eventually. It's a lightweight primitive (unlike WaitHandle).
                    // However, we should try to remove it only if we can.
                    
                    // Actually, since we are inside the lock, we know we hold it.
                    // But we released it above.
                    
                    // Safer strategy: Don't dispose it. Just remove it.
                    // SemaphoreSlim without AvailableWaitHandle access has trivial Dispose.
                    // Don't dispose here — other callers may still be awaiting this semaphore.
                    // GC will collect it once all references are released.
                    _loadLocks.TryRemove(itemIndex, out _);
                }
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
            int effectiveMax = Math.Min(maxLevel, Math.Min(maxSingleTileLevel, 30));
            for (int level = 0; level <= effectiveMax; level++)
            {
                bestLevel = level;
                int levelDim = 1 << level;
                if (levelDim >= targetSize)
                    break;
            }

            // Try primary format first, then fallback to alternative
            string url = $"{_basePath}/{filesDir}/{bestLevel}/0_0.{primaryFormat}{_queryString}";
            var tileBitmap = await _fetcher.FetchTileAsync(url, ct).ConfigureAwait(false);

            if (_disposed)
            {
                tileBitmap?.Dispose();
                return null;
            }

            if (tileBitmap == null)
            {
                // Try alternative format (jpg ↔ png)
                string altFormat = primaryFormat == "jpg" ? "png" : "jpg";
                string altUrl = $"{_basePath}/{filesDir}/{bestLevel}/0_0.{altFormat}{_queryString}";
                tileBitmap = await _fetcher.FetchTileAsync(altUrl, ct).ConfigureAwait(false);

                if (_disposed)
                {
                    tileBitmap?.Dispose();
                    return null;
                }
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
            double cellSize = (double)levelTotalWidth / gridSize;

            // Morton grid uses uniform square cells; the actual image is
            // letterboxed/pillarboxed inside the cell based on aspect ratio
            double itemPixWidth, itemPixHeight;
            if (subImage.AspectRatio >= 1.0)
            {
                // Landscape: full cell width, reduced height
                itemPixWidth = cellSize;
                itemPixHeight = cellSize / subImage.AspectRatio;
            }
            else
            {
                // Portrait: full cell height, reduced width
                itemPixHeight = cellSize;
                itemPixWidth = cellSize * subImage.AspectRatio;
            }

            // Cell origin in the composite image (uniform grid spacing)
            double cellPxX = col * cellSize;
            double cellPxY = row * cellSize;

            // Offset within cell for letterboxing/pillarboxing
            double offsetX = (cellSize - itemPixWidth) / 2.0;
            double offsetY = (cellSize - itemPixHeight) / 2.0;

            double itemPxX = cellPxX + offsetX;
            double itemPxY = cellPxY + offsetY;

            int tileCol = (int)(itemPxX / _dzc.TileSize);
            int tileRow = (int)(itemPxY / _dzc.TileSize);

            var tileId = new TileId(bestLevel, tileCol, tileRow);
            SKBitmap? tileBitmap;

            if (!_cache.TryGet(tileId, out tileBitmap))
            {
                string url = $"{_basePath}/{_dzc.GetCompositeTileUrl(bestLevel, tileCol, tileRow)}{_queryString}";
                tileBitmap = await _fetcher.FetchTileAsync(url, ct).ConfigureAwait(false);

                if (_disposed)
                {
                    tileBitmap?.Dispose();
                    return null;
                }
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

        /// <summary>Clears all cached thumbnails using deferred disposal.</summary>
        public void ClearCache()
        {
            lock (_thumbnailDisposeLock)
            {
                foreach (var kv in _thumbnailCache)
                {
                    if (kv.Value != null)
                        _pendingThumbnailDispose.Add(kv.Value);
                }
                _thumbnailCache.Clear();
            }
            _cache.Clear();
        }

        /// <summary>
        /// Flushes deferred bitmap disposals from the tile cache and thumbnail cache.
        /// Call this on the UI thread before rendering to safely dispose evicted bitmaps.
        /// </summary>
        public void FlushEvictedTiles()
        {
            _cache.FlushEvicted();

            List<SKBitmap>? toDispose = null;
            lock (_thumbnailDisposeLock)
            {
                if (_pendingThumbnailDispose.Count > 0)
                {
                    toDispose = new List<SKBitmap>(_pendingThumbnailDispose);
                    _pendingThumbnailDispose.Clear();
                }
            }
            if (toDispose != null)
            {
                foreach (var bmp in toDispose)
                    bmp.Dispose();
            }
        }

        public void Dispose()
        {
            lock (_thumbnailDisposeLock)
            {
                if (_disposed) return;
                _disposed = true;
            }

            // ClearCache uses deferred disposal; flush immediately since we're disposing
            ClearCache();
            lock (_thumbnailDisposeLock)
            {
                foreach (var bmp in _pendingThumbnailDispose)
                    bmp.Dispose();
                _pendingThumbnailDispose.Clear();
            }
            foreach (var kv in _loadLocks)
                kv.Value?.Dispose();
            _loadLocks.Clear();
            _cache.Dispose();
            (_fetcher as IDisposable)?.Dispose();
        }
    }
}
