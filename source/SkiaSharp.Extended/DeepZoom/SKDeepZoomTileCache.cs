#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Extended.DeepZoom
{
    /// <summary>
    /// LRU cache for decoded tile bitmaps.
    /// Evicted bitmaps are deferred for disposal to avoid race conditions
    /// with the renderer (which may hold references to recently-returned bitmaps).
    /// Call <see cref="FlushEvicted"/> at the start of each render frame.
    /// </summary>
    public class SKDeepZoomTileCache : ISKDeepZoomTileCache
    {
        private readonly int _maxEntries;
        private readonly LinkedList<TileCacheEntry> _lruList;
        private readonly Dictionary<SKDeepZoomTileId, LinkedListNode<TileCacheEntry>> _map;
        private readonly object _lock = new object();
        private readonly List<SKBitmap> _pendingDispose = new List<SKBitmap>();
        private bool _disposed;

        public SKDeepZoomTileCache(int maxEntries = 256)
        {
            if (maxEntries <= 0) throw new ArgumentOutOfRangeException(nameof(maxEntries));
            _maxEntries = maxEntries;
            _lruList = new LinkedList<TileCacheEntry>();
            _map = new Dictionary<SKDeepZoomTileId, LinkedListNode<TileCacheEntry>>();
        }

        /// <summary>Current number of cached tiles.</summary>
        public int Count { get { lock (_lock) return _map.Count; } }

        /// <summary>Maximum number of cached tiles.</summary>
        public int MaxEntries => _maxEntries;

        /// <summary>Tries to get a cached tile bitmap.</summary>
        public bool TryGet(SKDeepZoomTileId id, out SKBitmap? bitmap)
        {
            lock (_lock)
            {
                if (_disposed)
                {
                    bitmap = null;
                    return false;
                }

                if (_map.TryGetValue(id, out var node))
                {
                    // Move to front (most recently used)
                    _lruList.Remove(node);
                    _lruList.AddFirst(node);
                    bitmap = node.Value.Bitmap;
                    return true;
                }
                bitmap = null;
                return false;
            }
        }

        /// <summary>Adds a tile bitmap to the cache via the async interface. Calls <see cref="Put"/> synchronously.</summary>
        public Task PutAsync(SKDeepZoomTileId id, SKBitmap? bitmap, CancellationToken ct = default)
        {
            if (!ct.IsCancellationRequested)
                Put(id, bitmap);
            return Task.CompletedTask;
        }

        /// <summary>Adds a tile bitmap to the cache, evicting LRU entries if needed.</summary>
        public void Put(SKDeepZoomTileId id, SKBitmap? bitmap)
        {
            lock (_lock)
            {
                if (_disposed)
                {
                    bitmap?.Dispose();
                    return;
                }

                if (_map.TryGetValue(id, out var existing))
                {
                    // Update existing entry — defer old bitmap disposal
                    if (existing.Value.Bitmap != null)
                        _pendingDispose.Add(existing.Value.Bitmap);

                    existing.Value.Bitmap = bitmap;

                    // Move to front
                    _lruList.Remove(existing);
                    _lruList.AddFirst(existing);
                    return;
                }

                // Evict if at capacity — defer evicted bitmap disposal
                while (_map.Count >= _maxEntries && _lruList.Last != null)
                {
                    var lru = _lruList.Last;
                    _lruList.RemoveLast();
                    _map.Remove(lru.Value.Id);
                    if (lru.Value.Bitmap != null)
                        _pendingDispose.Add(lru.Value.Bitmap);
                }

                var entry = new TileCacheEntry(id, bitmap);
                var newNode = _lruList.AddFirst(entry);
                _map[id] = newNode;
            }
        }

        /// <summary>
        /// Disposes all bitmaps that were evicted since the last call.
        /// Call this at the start of each render frame (on the UI thread)
        /// to safely dispose bitmaps that are no longer in use.
        /// </summary>
        public void FlushEvicted()
        {
            List<SKBitmap>? toDispose = null;
            lock (_lock)
            {
                if (_pendingDispose.Count > 0)
                {
                    toDispose = new List<SKBitmap>(_pendingDispose);
                    _pendingDispose.Clear();
                }
            }
            if (toDispose != null)
            {
                foreach (var bmp in toDispose)
                    bmp.Dispose();
            }
        }

        /// <summary>Checks if a tile is in the cache.</summary>
        public bool Contains(SKDeepZoomTileId id) { lock (_lock) return _map.ContainsKey(id); }

        /// <summary>Removes a specific tile from the cache.</summary>
        public bool Remove(SKDeepZoomTileId id)
        {
            lock (_lock)
            {
                if (_map.TryGetValue(id, out var node))
                {
                    _lruList.Remove(node);
                    _map.Remove(id);
                    if (node.Value.Bitmap != null)
                        _pendingDispose.Add(node.Value.Bitmap);
                    return true;
                }
                return false;
            }
        }

        /// <summary>Clears all cached tiles.</summary>
        public void Clear()
        {
            lock (_lock)
            {
                foreach (var node in _lruList)
                {
                    node.Bitmap?.Dispose();
                }
                _lruList.Clear();
                _map.Clear();
                foreach (var bmp in _pendingDispose)
                    bmp.Dispose();
                _pendingDispose.Clear();
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                if (_disposed)
                    return;
                _disposed = true;
            }
            // Safe to call Clear() outside the lock: _disposed=true prevents
            // new Put() calls from inserting items (Put checks _disposed under lock).
            Clear();
        }

        private class TileCacheEntry
        {
            public TileCacheEntry(SKDeepZoomTileId id, SKBitmap? bitmap)
            {
                Id = id;
                Bitmap = bitmap;
            }

            public SKDeepZoomTileId Id { get; }
            public SKBitmap? Bitmap { get; set; }
        }
    }
}
