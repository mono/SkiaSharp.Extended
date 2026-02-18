using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharp;

namespace SkiaSharp.Extended.DeepZoom
{
    /// <summary>
    /// Identifies a single tile in the pyramid.
    /// </summary>
    public readonly struct TileId : IEquatable<TileId>
    {
        public TileId(int level, int col, int row)
        {
            Level = level;
            Col = col;
            Row = row;
        }

        public int Level { get; }
        public int Col { get; }
        public int Row { get; }

        public bool Equals(TileId other) => Level == other.Level && Col == other.Col && Row == other.Row;
        public override bool Equals(object? obj) => obj is TileId id && Equals(id);
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + Level;
                hash = hash * 31 + Col;
                hash = hash * 31 + Row;
                return hash;
            }
        }
        public override string ToString() => $"({Level},{Col},{Row})";

        public static bool operator ==(TileId left, TileId right) => left.Equals(right);
        public static bool operator !=(TileId left, TileId right) => !left.Equals(right);
    }

    /// <summary>
    /// Interface for fetching tile image data from various sources (HTTP, file, memory).
    /// </summary>
    public interface ITileFetcher : IDisposable
    {
        /// <summary>
        /// Fetches a tile as an SKBitmap. Returns null if the tile is not available (404).
        /// </summary>
        Task<SKBitmap?> FetchTileAsync(string url, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// LRU cache for decoded tile bitmaps.
    /// </summary>
    public class TileCache : IDisposable
    {
        private readonly int _maxEntries;
        private readonly LinkedList<TileCacheEntry> _lruList;
        private readonly Dictionary<TileId, LinkedListNode<TileCacheEntry>> _map;
        private bool _disposed;

        public TileCache(int maxEntries = 256)
        {
            if (maxEntries <= 0) throw new ArgumentOutOfRangeException(nameof(maxEntries));
            _maxEntries = maxEntries;
            _lruList = new LinkedList<TileCacheEntry>();
            _map = new Dictionary<TileId, LinkedListNode<TileCacheEntry>>();
        }

        /// <summary>Current number of cached tiles.</summary>
        public int Count => _map.Count;

        /// <summary>Maximum number of cached tiles.</summary>
        public int MaxEntries => _maxEntries;

        /// <summary>Tries to get a cached tile bitmap.</summary>
        public bool TryGet(TileId id, out SKBitmap? bitmap)
        {
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

        /// <summary>Adds a tile bitmap to the cache, evicting LRU entries if needed.</summary>
        public void Put(TileId id, SKBitmap? bitmap)
        {
            if (_map.TryGetValue(id, out var existing))
            {
                // Update existing entry
                _lruList.Remove(existing);
                existing.Value.Bitmap?.Dispose();
                existing.Value = new TileCacheEntry(id, bitmap);
                _lruList.AddFirst(existing);
                return;
            }

            // Evict if at capacity
            while (_map.Count >= _maxEntries && _lruList.Last != null)
            {
                var lru = _lruList.Last!;
                _lruList.RemoveLast();
                _map.Remove(lru.Value.Id);
                lru.Value.Bitmap?.Dispose();
            }

            var entry = new TileCacheEntry(id, bitmap);
            var newNode = _lruList.AddFirst(entry);
            _map[id] = newNode;
        }

        /// <summary>Checks if a tile is in the cache.</summary>
        public bool Contains(TileId id) => _map.ContainsKey(id);

        /// <summary>Removes a specific tile from the cache.</summary>
        public bool Remove(TileId id)
        {
            if (_map.TryGetValue(id, out var node))
            {
                _lruList.Remove(node);
                _map.Remove(id);
                node.Value.Bitmap?.Dispose();
                return true;
            }
            return false;
        }

        /// <summary>Clears all cached tiles.</summary>
        public void Clear()
        {
            foreach (var node in _lruList)
            {
                node.Bitmap?.Dispose();
            }
            _lruList.Clear();
            _map.Clear();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Clear();
                _disposed = true;
            }
        }

        private class TileCacheEntry
        {
            public TileCacheEntry(TileId id, SKBitmap? bitmap)
            {
                Id = id;
                Bitmap = bitmap;
            }

            public TileId Id { get; }
            public SKBitmap? Bitmap { get; set; }
        }
    }
}
