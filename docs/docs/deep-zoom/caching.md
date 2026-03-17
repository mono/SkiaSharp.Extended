# Deep Zoom — Caching

The tile cache stores decoded `SKBitmap` objects so tiles don't need to be re-fetched or re-decoded on every frame. The cache is **pluggable** — swap implementations to tune memory usage, add persistence, or chain multiple tiers.

## ISKDeepZoomTileCache

All caches implement this interface:

```csharp
public interface ISKDeepZoomTileCache : IDisposable
{
    /// <summary>Number of tiles currently in the cache.</summary>
    int Count { get; }

    /// <summary>Synchronous lookup — used by the renderer (no blocking I/O).</summary>
    bool TryGet(SKDeepZoomTileId id, out SKBitmap? bitmap);

    /// <summary>Async lookup — use for I/O-backed tiers (disk, browser storage).</summary>
    Task<SKBitmap?> TryGetAsync(SKDeepZoomTileId id, CancellationToken ct = default);

    /// <summary>Returns true if the tile is cached.</summary>
    bool Contains(SKDeepZoomTileId id);

    /// <summary>Synchronous write — for in-memory caches.</summary>
    void Put(SKDeepZoomTileId id, SKBitmap? bitmap);

    /// <summary>Async write — for I/O-backed caches or cache decorators.</summary>
    Task PutAsync(SKDeepZoomTileId id, SKBitmap? bitmap, CancellationToken ct = default);

    /// <summary>Removes a specific tile.</summary>
    bool Remove(SKDeepZoomTileId id);

    /// <summary>Clears all cached tiles.</summary>
    void Clear();

    /// <summary>
    /// Disposes bitmaps evicted since the last call.
    /// Call once per render frame before drawing to safely free GPU-bound bitmaps.
    /// </summary>
    void FlushEvicted();
}
```

### Sync vs Async

There are two read/write paths:

| Method | Called by | When to use |
| :----- | :-------- | :---------- |
| `TryGet` / `Put` | Renderer (sync paint callback) | Pure in-memory caches — must not block |
| `TryGetAsync` / `PutAsync` | Background tile loader | I/O-backed caches — disk, browser storage, network |

The controller's background tile loader calls `TryGetAsync` first. If it returns `null` (cache miss), it fetches from the network and then calls `PutAsync`. The renderer always uses the synchronous `TryGet`.

---

## SKDeepZoomMemoryTileCache

The built-in **LRU (Least Recently Used)** in-memory cache. When capacity is reached, the least-recently-accessed tile is evicted and scheduled for disposal.

```csharp
// Default capacity: 256 tiles
var cache = new SKDeepZoomMemoryTileCache();

// Custom capacity
var cache = new SKDeepZoomMemoryTileCache(maxEntries: 1024);

// Pass to controller
var controller = new SKDeepZoomController(cache: cache);
```

### Properties

| Property | Description |
| :------- | :---------- |
| `Count` | Current number of cached tiles. |
| `MaxEntries` | Maximum tiles before eviction begins. |

### Eviction and Disposal

Evicted bitmaps are held in a pending-dispose queue rather than immediately freed. Call `FlushEvicted()` once per frame (before `Render`) to safely free GPU-bound bitmaps:

```csharp
void OnPaintSurface(SKPaintSurfaceEventArgs e)
{
    controller.Cache.FlushEvicted(); // safe to call even if nothing was evicted
    controller.SetControlSize(e.Info.Width, e.Info.Height);
    controller.Update();
    controller.Render(e.Surface.Canvas);
}
```

> The controller calls `FlushEvicted()` automatically inside `Render()`, so you only need to call it manually if you access the cache directly.

### Capacity Guidelines

| Device | Recommended Capacity |
| :----- | :------------------- |
| Desktop / laptop | 1024–4096 |
| Mid-range mobile | 256–512 |
| Low-memory devices | 64–128 |

Each tile is typically a 256×256 JPEG/PNG decoded to an `SKBitmap` — roughly 256 KB at full colour. 1024 tiles ≈ 256 MB of bitmap RAM at maximum.

---

## Tiered Caching

For persistent tile storage (surviving app restarts or page reloads), add a second cache tier. The controller checks `TryGetAsync` before hitting the network:

```
Request tile
    ↓
TryGet (L1 memory cache)      — fast, synchronous
    ↓ miss
TryGetAsync (L2 disk/browser) — slower, async I/O
    ↓ miss
FetchTileAsync (network)      — slowest
    ↓
PutAsync (L2)
    ↓
Put (L1)
```

This is the **cache-aside** pattern — the controller actively manages both tiers. You don't need to chain fetchers; just implement `TryGetAsync` / `PutAsync` to read and write your L2 storage.

### Decorator Pattern

A simple way to add L2 behaviour is to wrap an existing cache:

```csharp
public sealed class TieredCache : ISKDeepZoomTileCache
{
    private readonly SKDeepZoomMemoryTileCache _l1;
    private readonly IMyDiskCache _l2;

    public TieredCache(int l1Capacity, IMyDiskCache l2)
    {
        _l1 = new SKDeepZoomMemoryTileCache(l1Capacity);
        _l2 = l2;
    }

    public bool TryGet(SKDeepZoomTileId id, out SKBitmap? bitmap)
        => _l1.TryGet(id, out bitmap);

    public async Task<SKBitmap?> TryGetAsync(SKDeepZoomTileId id, CancellationToken ct = default)
    {
        // Fast path: already in L1
        if (_l1.TryGet(id, out var bmp)) return bmp;

        // Slow path: check disk
        bmp = await _l2.ReadAsync(id, ct);
        if (bmp is not null)
            _l1.Put(id, bmp);   // promote to L1
        return bmp;
    }

    public void Put(SKDeepZoomTileId id, SKBitmap? bitmap)      => _l1.Put(id, bitmap);
    public async Task PutAsync(SKDeepZoomTileId id, SKBitmap? bitmap, CancellationToken ct = default)
    {
        _l1.Put(id, bitmap);
        await _l2.WriteAsync(id, bitmap, ct);
    }

    public bool Contains(SKDeepZoomTileId id)   => _l1.Contains(id);
    public bool Remove(SKDeepZoomTileId id)     => _l1.Remove(id);
    public void Clear()                         { _l1.Clear(); _l2.Clear(); }
    public void FlushEvicted()                  => _l1.FlushEvicted();
    public int Count                            => _l1.Count;
    public void Dispose()                       => _l1.Dispose();
}
```

---

## SKDeepZoomTileId

Each tile is identified by a `readonly record struct` with value equality — important since tile IDs are used as dictionary keys:

```csharp
// Level = pyramid level (0 = lowest resolution, MaxLevel = highest)
// Col   = column index at that level
// Row   = row index at that level
var id = new SKDeepZoomTileId(Level: 12, Col: 3, Row: 5);

Console.WriteLine(id);   // "(12,3,5)"

// Value equality — safe to use in Dictionary / ConcurrentDictionary
var same = new SKDeepZoomTileId(12, 3, 5);
Assert.Equal(id, same);  // ✅
```

---

## Writing a Custom Cache

Any class that implements `ISKDeepZoomTileCache` can be used. Minimal in-memory example:

```csharp
public sealed class BoundedDictionaryCache : ISKDeepZoomTileCache
{
    private readonly Dictionary<SKDeepZoomTileId, SKBitmap> _store = new();
    private readonly int _maxEntries;

    public BoundedDictionaryCache(int maxEntries) => _maxEntries = maxEntries;

    public int Count => _store.Count;

    public bool TryGet(SKDeepZoomTileId id, out SKBitmap? bitmap)
        => _store.TryGetValue(id, out bitmap);

    public Task<SKBitmap?> TryGetAsync(SKDeepZoomTileId id, CancellationToken ct = default)
        => Task.FromResult(_store.TryGetValue(id, out var bmp) ? bmp : null);

    public bool Contains(SKDeepZoomTileId id) => _store.ContainsKey(id);

    public void Put(SKDeepZoomTileId id, SKBitmap? bitmap)
    {
        if (bitmap is null || _store.Count >= _maxEntries) return;
        _store[id] = bitmap;
    }

    public Task PutAsync(SKDeepZoomTileId id, SKBitmap? bitmap, CancellationToken ct = default)
    {
        Put(id, bitmap);
        return Task.CompletedTask;
    }

    public bool Remove(SKDeepZoomTileId id)
    {
        if (_store.Remove(id, out var bmp)) { bmp?.Dispose(); return true; }
        return false;
    }

    public void Clear()
    {
        foreach (var bmp in _store.Values) bmp?.Dispose();
        _store.Clear();
    }

    public void FlushEvicted() { }

    public void Dispose() => Clear();
}
```

---

## Related

- [Deep Zoom overview](index.md)
- [Controller & Viewport](controller.md)
- [Tile Fetching](fetching.md)
- [API Reference — ISKDeepZoomTileCache](xref:SkiaSharp.Extended.ISKDeepZoomTileCache)
- [API Reference — SKDeepZoomMemoryTileCache](xref:SkiaSharp.Extended.SKDeepZoomMemoryTileCache)
