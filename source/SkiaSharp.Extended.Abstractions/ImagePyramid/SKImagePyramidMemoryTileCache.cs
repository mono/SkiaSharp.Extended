#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Extended;

/// <summary>
/// LRU cache for decoded tile images.
/// Evicted tiles are deferred for disposal to avoid race conditions with the renderer.
/// Call <see cref="FlushEvicted"/> at the start of each render frame.
/// </summary>
public class SKImagePyramidMemoryTileCache : ISKImagePyramidTileCache
{
    private readonly int _maxEntries;
    private readonly LinkedList<TileCacheEntry> _lruList;
    private readonly Dictionary<SKImagePyramidTileId, LinkedListNode<TileCacheEntry>> _map;
    private readonly object _lock = new object();
    private readonly List<ISKImagePyramidTile> _pendingDispose = new List<ISKImagePyramidTile>();
    private bool _disposed;

    public SKImagePyramidMemoryTileCache(int maxEntries = 256)
    {
        if (maxEntries <= 0) throw new ArgumentOutOfRangeException(nameof(maxEntries));
        _maxEntries = maxEntries;
        _lruList = new LinkedList<TileCacheEntry>();
        _map = new Dictionary<SKImagePyramidTileId, LinkedListNode<TileCacheEntry>>();
    }

    /// <summary>Current number of cached tiles.</summary>
    public int Count { get { lock (_lock) return _map.Count; } }

    /// <summary>Maximum number of cached tiles.</summary>
    public int MaxEntries => _maxEntries;

    // ---- ISKImagePyramidTileCache implementation (primary) ----

    /// <summary>Tries to get a cached tile.</summary>
    public bool TryGet(SKImagePyramidTileId id, out ISKImagePyramidTile? tile)
    {
        lock (_lock)
        {
            if (_disposed) { tile = null; return false; }

            if (_map.TryGetValue(id, out var node))
            {
                _lruList.Remove(node);
                _lruList.AddFirst(node);
                tile = node.Value.Tile;
                return true;
            }
            tile = null;
            return false;
        }
    }

    /// <summary>Async variant — completes synchronously for in-memory cache.</summary>
    public Task<ISKImagePyramidTile?> TryGetAsync(SKImagePyramidTileId id, CancellationToken ct = default)
    {
        TryGet(id, out ISKImagePyramidTile? tile);
        return Task.FromResult(tile);
    }

    /// <summary>Adds a tile to the cache via the async interface.</summary>
    public Task PutAsync(SKImagePyramidTileId id, ISKImagePyramidTile? tile, CancellationToken ct = default)
    {
        if (!ct.IsCancellationRequested)
            Put(id, tile);
        return Task.CompletedTask;
    }

    /// <summary>Adds a tile to the cache, evicting the LRU entry if at capacity.</summary>
    public void Put(SKImagePyramidTileId id, ISKImagePyramidTile? tile)
    {
        lock (_lock)
        {
            if (_disposed) { tile?.Dispose(); return; }

            if (_map.TryGetValue(id, out var existing))
            {
                if (existing.Value.Tile != null)
                    _pendingDispose.Add(existing.Value.Tile);
                existing.Value.Tile = tile;
                _lruList.Remove(existing);
                _lruList.AddFirst(existing);
                return;
            }

            while (_map.Count >= _maxEntries && _lruList.Last != null)
            {
                var lru = _lruList.Last;
                _lruList.RemoveLast();
                _map.Remove(lru.Value.Id);
                if (lru.Value.Tile != null)
                    _pendingDispose.Add(lru.Value.Tile);
            }

            var entry = new TileCacheEntry(id, tile);
            var newNode = _lruList.AddFirst(entry);
            _map[id] = newNode;
        }
    }

    /// <summary>Checks if a tile is in the cache.</summary>
    public bool Contains(SKImagePyramidTileId id) { lock (_lock) return _map.ContainsKey(id); }

    /// <summary>Removes a specific tile from the cache.</summary>
    public bool Remove(SKImagePyramidTileId id)
    {
        lock (_lock)
        {
            if (_map.TryGetValue(id, out var node))
            {
                _lruList.Remove(node);
                _map.Remove(id);
                if (node.Value.Tile != null)
                    _pendingDispose.Add(node.Value.Tile);
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
                node.Tile?.Dispose();
            _lruList.Clear();
            _map.Clear();
            foreach (var t in _pendingDispose)
                t.Dispose();
            _pendingDispose.Clear();
        }
    }

    /// <summary>
    /// Disposes tiles evicted since the last call.
    /// Call this at the start of each render frame on the UI thread.
    /// </summary>
    public void FlushEvicted()
    {
        List<ISKImagePyramidTile>? toDispose = null;
        lock (_lock)
        {
            if (_pendingDispose.Count > 0)
            {
                toDispose = new List<ISKImagePyramidTile>(_pendingDispose);
                _pendingDispose.Clear();
            }
        }
        if (toDispose != null)
        {
            foreach (var t in toDispose)
                t.Dispose();
        }
    }

    public void Dispose()
    {
        lock (_lock)
        {
            if (_disposed) return;
            _disposed = true;
        }
        Clear();
    }

    // ---- Private ----

    private class TileCacheEntry(SKImagePyramidTileId id, ISKImagePyramidTile? tile)
    {
        public SKImagePyramidTileId Id { get; } = id;
        public ISKImagePyramidTile? Tile { get; set; } = tile;
    }
}
