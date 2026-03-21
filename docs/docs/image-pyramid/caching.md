# Image Pyramid — Caching

The tile caching system is split into two separate concerns with distinct owners:

| Concern | Owner | Interface |
| :------ | :---- | :-------- |
| Hot render buffer | Controller (internal) | `ISKImagePyramidTileCache` |
| Persistent storage (disk, browser) | Provider | `ISKImagePyramidTileProvider` |

---

## The Render Buffer (ISKImagePyramidTileCache)

The render buffer is a **sync-only, in-memory LRU cache** owned entirely by the controller. Its job is to hold decoded tiles so the renderer can draw the current viewport without any I/O.

```csharp
public interface ISKImagePyramidTileCache : IDisposable
{
    int Count { get; }
    bool Contains(SKImagePyramidTileId id);
    bool TryGet(SKImagePyramidTileId id, out SKImagePyramidTile? tile);
    void Put(SKImagePyramidTileId id, SKImagePyramidTile tile);
    bool Remove(SKImagePyramidTileId id);
    void Clear();

    // Call once per frame before drawing to safely dispose evicted tiles
    void FlushEvicted();
}
```

The controller creates and manages this cache internally — you don't create or configure it. The `Cache` property on the controller exposes it for read-only monitoring (e.g. showing a tile count in a debug overlay):

```csharp
// Read-only monitoring — do not call Put/Remove directly
int cachedTileCount = controller.Cache.Count;
```

> **Note:** The cache's `FlushEvicted()` is called automatically inside `Render()` — you do not need to call it yourself.

---

## Persistent Storage (ISKImagePyramidTileProvider)

Persistent tile storage is the **provider's** responsibility, not the controller's. The controller simply calls `provider.GetTileAsync(url)` and the provider decides how to fulfil that request — from a disk cache, browser storage, or directly from the network.

See [Tile Fetching](fetching.md) for the full provider design and built-in implementations.

### Remote tiles (HTTP + disk cache)

```csharp
// SKTieredTileProvider with disk cache persists fetched tiles across app restarts
var provider = new SKTieredTileProvider(
    new SKHttpTileFetcher(),
    new SKDiskTileCacheStore("/tmp/mycache"));
controller.Load(source, provider);
```

### Local tiles (no disk cache needed)

```csharp
// SKFileTileFetcher reads tiles directly from the filesystem — no extra caching
var provider = new SKTieredTileProvider(new SKFileTileFetcher());
controller.Load(source, provider);
```

### Custom persistent cache

To add your own persistent storage, wrap a provider in a decorator:

```csharp
public sealed class MyPersistentProvider : ISKImagePyramidTileProvider
{
    private readonly ISKImagePyramidTileProvider _inner;
    private readonly IMyStorage _storage;

    public MyPersistentProvider(ISKImagePyramidTileProvider inner, IMyStorage storage)
    {
        _inner = inner;
        _storage = storage;
    }

    public async Task<SKImagePyramidTile?> GetTileAsync(string url, CancellationToken ct = default)
    {
        // 1. Check your persistent store first
        var cached = await _storage.TryReadAsync(url, ct);
        if (cached is not null) return cached;

        // 2. Delegate to the inner provider (e.g. SKTieredTileProvider)
        var tile = await _inner.GetTileAsync(url, ct);
        if (tile is null) return null;

        // 3. Persist for next time — use CancellationToken.None so a cancellation
        //    after fetch doesn't leave the tile un-stored
        await _storage.WriteAsync(url, tile, CancellationToken.None);
        return tile;
    }

    public void Dispose() => _inner.Dispose();
}
```

---

## SKImagePyramidTileId

Each tile is identified by a `readonly record struct` with value equality:

```csharp
// Level = pyramid level (0 = lowest resolution, MaxLevel = highest)
// Col   = column index at that level
// Row   = row index at that level
var id = new SKImagePyramidTileId(Level: 12, Col: 3, Row: 5);

Console.WriteLine(id);   // "(12,3,5)"

// Value equality — safe to use as a dictionary key
var same = new SKImagePyramidTileId(12, 3, 5);
Assert.Equal(id, same);  // ✅
```

---

## Render Buffer Capacity

The controller creates its render buffer with a default capacity of 256 tiles. Each tile is typically a 256×256 decoded image — roughly 256 KB at full colour.

| Device | Approximate capacity |
| :----- | :------------------- |
| Desktop / laptop | 1024–4096 |
| Mid-range mobile | 256–512 |
| Low-memory devices | 64–128 |

> **Custom capacity** is not currently exposed via the public API. The 256-tile default suits most use cases.

---

## Related

- [Image Pyramid overview](index.md)
- [Controller & Viewport](controller.md)
- [Tile Fetching](fetching.md)
- [API Reference — ISKImagePyramidTileCache](xref:SkiaSharp.Extended.ISKImagePyramidTileCache)
- [API Reference — ISKImagePyramidTileProvider](xref:SkiaSharp.Extended.ISKImagePyramidTileProvider)

