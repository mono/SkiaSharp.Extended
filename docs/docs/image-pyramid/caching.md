# Image Pyramid — Caching

There are two distinct caches in the system, with distinct owners — and a single **decode
gate** that separates them:

| Cache | Holds | Owner | Type |
| :---- | :---- | :---- | :--- |
| Render buffer | Decoded `SKImage` tiles | Controller (internal) | `ISKImagePyramidTileCache` |
| Persistent cache | Encoded bytes | Provider (you compose it) | `SKCachedTileProvider` |

The persistent cache lives **below** the decode gate and stores small encoded bytes; the render
buffer lives **above** it and holds decoded images ready to draw.

---

## The decode gate

Providers deal only in encoded bytes (`SKImagePyramidTileData`). The controller decodes each
tile exactly once, on the async load path, right before it enters the render buffer:

```
provider.GetTileAsync(url)   →  SKImagePyramidTileData   (encoded bytes, below the gate)
        │
        ▼  SKImage.FromEncodedData(...)                  ← the decode gate (controller)
        │
   SKImagePyramidTile        →  render buffer            (decoded SKImage, above the gate)
```

Because decoding happens in one place, no cache ever holds a decoded copy by accident, and a
`SKImagePyramidTile` carries only a decoded image — there is no second encoded copy to leak.

---

## The Render Buffer (ISKImagePyramidTileCache)

The render buffer is a **sync-only, in-memory LRU cache** owned entirely by the controller. Its
job is to hold decoded tiles so the renderer can draw the current viewport without any I/O.

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

The controller creates and manages this cache internally — you don't create or configure it.
The `Cache` property on the controller exposes it for read-only monitoring (e.g. showing a tile
count in a debug overlay):

```csharp
// Read-only monitoring — do not call Put/Remove directly
int cachedTileCount = controller.Cache.Count;
```

> **Note:** The cache's `FlushEvicted()` is called automatically inside `Render()` — you do not
> need to call it yourself.

---

## Persistent Storage (SKCachedTileProvider)

Persistent tile storage is the **provider's** responsibility, not the controller's. The
controller simply calls `provider.GetTileAsync(url)`; how that request is fulfilled — from a
disk cache, browser storage, or directly from the network — is decided by how you nest your
providers.

Persistent caches deal in encoded bytes and derive from `SKCachedTileProvider`, which
implements the read-through/write-back flow once. See [Tile Providers](fetching.md) for the
full design.

### Remote tiles (HTTP + disk cache)

```csharp
// SKDiskCacheTileProvider wraps an HTTP origin and persists fetched tiles across app restarts
var provider = new SKDiskCacheTileProvider(
    new SKHttpTileProvider(),
    Path.Combine(FileSystem.CacheDirectory, "tiles"));

controller.Load(source, provider);
```

### Local tiles (no disk cache needed)

```csharp
// SKFileTileProvider reads tiles directly from the filesystem — no extra caching
controller.Load(source, new SKFileTileProvider());
```

### Custom persistent cache

To add your own persistent storage, derive from `SKCachedTileProvider` and implement only the
storage backend — the caching flow is inherited:

```csharp
public sealed class MyPersistentProvider : SKCachedTileProvider
{
    private readonly IMyStorage _storage;

    public MyPersistentProvider(ISKImagePyramidTileProvider inner, IMyStorage storage)
        : base(inner) => _storage = storage;

    protected override async Task<SKImagePyramidTileData?> ReadAsync(string key, CancellationToken ct)
    {
        var bytes = await _storage.TryReadAsync(key, ct);
        return bytes is null ? null : new SKImagePyramidTileData(bytes);
    }

    protected override Task WriteAsync(string key, SKImagePyramidTileData data, CancellationToken ct)
        => _storage.WriteAsync(key, data.Data, ct);  // CancellationToken.None is passed by the base
}
```

```csharp
// Nest an origin inside it
controller.Load(source, new MyPersistentProvider(new SKHttpTileProvider(), myStorage));
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

The controller creates its render buffer with a default capacity of 256 tiles. Each tile is
typically a 256×256 decoded image — roughly 256 KB at full colour.

| Device | Approximate capacity |
| :----- | :------------------- |
| Desktop / laptop | 1024–4096 |
| Mid-range mobile | 256–512 |
| Low-memory devices | 64–128 |

> **Custom capacity** is not currently exposed via the public API. The 256-tile default suits
> most use cases.

---

## Related

- [Image Pyramid overview](index.md)
- [Controller & Viewport](controller.md)
- [Tile Providers](fetching.md)
- [API Reference — ISKImagePyramidTileCache](xref:SkiaSharp.Extended.ISKImagePyramidTileCache)
- [API Reference — SKCachedTileProvider](xref:SkiaSharp.Extended.SKCachedTileProvider)
