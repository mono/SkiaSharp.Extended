# Image Pyramid — Tile Providers & Caching

Everything in the tile pipeline is a **provider**. There is a single public interface, and you
build a pipeline by **nesting** providers: an _origin_ produces encoded bytes (HTTP, file) and
_decorators_ wrap an inner provider to add behaviour (caching, fallback, latency).

```
Controller
    │  GetTileAsync(url)  →  encoded bytes
    ▼
SKDiskCacheTileProvider        (decorator: read-through disk cache)
    │
    └── SKHttpTileProvider     (origin: HTTP GET)
```

The controller asks a provider for the encoded bytes of a tile by URL. Everything below —
caching, fallback, retries — is the provider's responsibility.

---

## Two caches, one decode gate

There are two distinct caches in the system, with distinct owners — separated by a single
**decode gate**:

| Cache | Holds | Owner | Type |
| :---- | :---- | :---- | :--- |
| Render buffer | Decoded `SKImage` tiles | Controller (internal) | `ISKImagePyramidTileCache` |
| Persistent cache | Encoded bytes | Provider (you compose it) | `SKCachedTileProvider` |

Providers deal only in encoded bytes (`SKImagePyramidTileData`). The controller decodes each
tile exactly once, on the async load path, right before it enters the render buffer:

```
provider.GetTileAsync(url)   →  SKImagePyramidTileData   (encoded bytes, below the gate)
        │
        ▼  SKImage.FromEncodedData(...)                  ← the decode gate (controller)
        │
   SKImagePyramidTile        →  render buffer            (decoded SKImage, above the gate)
```

Because decoding happens in one place, no cache ever holds a decoded copy by accident: a
persistent cache stays small (encoded bytes), and a `SKImagePyramidTile` carries only a decoded
image — there is no second encoded copy to leak.

> **Why bytes and not a decoded tile?** Encoded bytes are small (a JPEG tile is ~15–40 KB; the
> same tile decoded to RGBA is ~256 KB). Keeping the pipeline in bytes means a cache can never
> accidentally hold a second, decoded copy.

---

## ISKImagePyramidTileProvider

The one and only provider interface. It returns encoded bytes wrapped in
`SKImagePyramidTileData`, or `null` for a permanent miss (404, file not found). It throws
`OperationCanceledException` when the token is cancelled.

```csharp
public interface ISKImagePyramidTileProvider : IDisposable
{
    Task<SKImagePyramidTileData?> GetTileAsync(string url, CancellationToken ct = default);
}
```

---

## Origins

An origin is a provider with no inner provider — it produces bytes from somewhere.

**`SKHttpTileProvider`** — HTTP GET. Pass your own `HttpClient` or let the provider manage one
internally (it only disposes a client it created).

```csharp
// Internal HttpClient (disposed with the provider)
var provider = new SKHttpTileProvider();

// Shared HttpClient (you own its lifetime; the provider won't dispose it)
var provider = new SKHttpTileProvider(myHttpClient);
```

**`SKFileTileProvider`** — Reads from the local filesystem. Accepts plain paths and `file://`
URIs.

```csharp
var provider = new SKFileTileProvider();
```

---

## Decorators

A decorator wraps an inner provider and adds behaviour. Because a decorator is itself a
provider, decorators nest arbitrarily.

**`SKCompositeTileProvider`** — Tries several inner providers in order; the first non-null
result wins. Use it for hybrid origins.

```csharp
// Try app-packaged tiles first, fall back to HTTP
var provider = new SKCompositeTileProvider(
    new MyAppPackageProvider(),
    new SKHttpTileProvider());
```

**`SKDiskCacheTileProvider`** — A persistent, read-through disk cache. It wraps an origin and
stores fetched tiles on disk using hashed, bucketed filenames with a configurable expiry.

```csharp
var provider = new SKDiskCacheTileProvider(
    inner: new SKHttpTileProvider(),
    basePath: Path.Combine(FileSystem.CacheDirectory, "tiles"),
    expiry: TimeSpan.FromDays(30)); // default: 30 days

provider.Clear();  // delete everything this cache has stored
```

`SKDiskCacheTileProvider` derives from `SKCachedTileProvider` (see below), so its flow is:
**read cache → on miss call inner → persist (fire-and-forget) → return bytes.**

---

## SKCachedTileProvider — the persistent-cache base class

`SKCachedTileProvider` is an abstract base for persistent cache decorators. It implements the
entire caching flow once; a subclass only provides the storage backend by overriding two
methods:

```csharp
protected abstract Task<SKImagePyramidTileData?> ReadAsync(string key, CancellationToken ct);
protected abstract Task WriteAsync(string key, SKImagePyramidTileData data, CancellationToken ct);
```

The base class handles everything else:

- **Key generation** — `ComputeKey(url)` returns a stable, filesystem-safe 64-bit FNV-1a hash
  as 16 hex characters. Subclasses use it to name their storage slots.
- **Read-through** — a cache hit short-circuits the inner provider; a read error is treated as
  a miss, never a failure.
- **Write-back** — on a miss it calls the inner provider, then persists the result
  fire-and-forget (using `CancellationToken.None`) so a slow disk never blocks rendering and a
  cancelled request still warms the cache it just paid for. A failed write is swallowed — the
  tile was already returned to the caller.

`SKDiskCacheTileProvider` is the built-in subclass; the Blazor sample's
`BrowserCacheTileProvider` is another, storing tiles in browser `sessionStorage`. Both reuse
the identical flow — they differ only in `ReadAsync`/`WriteAsync`.

---

## The render buffer (ISKImagePyramidTileCache)

The render buffer is a **sync-only, in-memory LRU cache** owned entirely by the controller. It
lives _above_ the decode gate and holds decoded tiles so the renderer can draw the current
viewport without any I/O.

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

### Capacity

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

## Common compositions

### HTTP only (no persistence)

```csharp
controller.Load(source, new SKHttpTileProvider());
```

### HTTP + disk cache

```csharp
var provider = new SKDiskCacheTileProvider(
    new SKHttpTileProvider(),
    Path.Combine(FileSystem.CacheDirectory, "tiles"));

controller.Load(source, provider);
```

### Local file (no cache needed)

```csharp
controller.Load(source, new SKFileTileProvider());
```

### Hybrid origin (app-package, then HTTP)

```csharp
var provider = new SKCompositeTileProvider(
    new MyAppPackageProvider(),  // reads from the app bundle
    new SKHttpTileProvider());   // falls back to the network

controller.Load(localDziSource, provider);
```

### Blazor WASM (browser storage cache)

```csharp
// BrowserCacheTileProvider (a sample SKCachedTileProvider subclass) caches to sessionStorage
var provider = new BrowserCacheTileProvider(new SKHttpTileProvider(http), js);

controller.Load(source, provider);
```

---

## Provider lifecycle

The caller owns the provider tree. Disposing the **root** provider cascades `Dispose` to every
nested provider, so you only ever dispose the outermost one. The controller does **not** own
the provider lifecycle.

```csharp
private ISKImagePyramidTileProvider? _provider;

private void SwitchProvider(ISKImagePyramidTileProvider newProvider)
{
    var old = _provider;
    _provider = newProvider;
    _controller.SetProvider(newProvider);
    old?.Dispose();              // disposes the whole old tree
}

public override void Dispose()
{
    _controller.Dispose();
    _provider?.Dispose();        // disposes the whole tree
}
```

---

## Custom origin

Implement `ISKImagePyramidTileProvider` directly for full control (authentication, custom
headers, app-package assets, etc.). Return **encoded bytes** — never decode.

```csharp
public sealed class AuthenticatedProvider(HttpClient http, string token)
    : ISKImagePyramidTileProvider
{
    public async Task<SKImagePyramidTileData?> GetTileAsync(string url, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new("Bearer", token);
        try
        {
            using var response = await http.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode) return null;
            var bytes = await response.Content.ReadAsByteArrayAsync(ct);
            return new SKImagePyramidTileData(bytes);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch { return null; }
    }

    public void Dispose() { }
}
```

**Return `null`** for permanent misses (404, file not found) — the controller records a
temporary failure with exponential backoff via `TileFailureTracker`.

**Throw `OperationCanceledException`** when `ct` is cancelled — the controller handles this
without recording a failure, so the tile is retried.

---

## Custom persistent cache

To add your own persistent storage, derive from `SKCachedTileProvider` and implement only the
two storage methods — the read-through/write-back flow is inherited:

```csharp
public sealed class MyStorageCacheProvider : SKCachedTileProvider
{
    private readonly IMyStorage _storage;

    public MyStorageCacheProvider(ISKImagePyramidTileProvider inner, IMyStorage storage)
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

Use it by nesting an origin inside it:

```csharp
controller.Load(source, new MyStorageCacheProvider(new SKHttpTileProvider(), myStorage));
```

---

## Custom decorator

Wrap any provider to add cross-cutting behaviour — logging, latency simulation, metrics.
Because the pipeline is bytes, a decorator just forwards the inner result:

```csharp
public sealed class DelayTileProvider(ISKImagePyramidTileProvider inner, int delayMs)
    : ISKImagePyramidTileProvider
{
    public async Task<SKImagePyramidTileData?> GetTileAsync(string url, CancellationToken ct = default)
    {
        await Task.Delay(delayMs, ct);
        return await inner.GetTileAsync(url, ct);
    }

    public void Dispose() => inner.Dispose();
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

## Related

- [Image Pyramid overview](index.md)
- [Controller & Viewport](controller.md)
- [API Reference — ISKImagePyramidTileProvider](xref:SkiaSharp.Extended.ISKImagePyramidTileProvider)
- [API Reference — SKCachedTileProvider](xref:SkiaSharp.Extended.SKCachedTileProvider)
- [API Reference — SKDiskCacheTileProvider](xref:SkiaSharp.Extended.SKDiskCacheTileProvider)
- [API Reference — ISKImagePyramidTileCache](xref:SkiaSharp.Extended.ISKImagePyramidTileCache)
