# Image Pyramid — Tile Fetching

The fetching system is split into three layers with clear separation of concerns:

```
Controller
    │  GetTileAsync(url)
    ▼
SKTieredTileProvider : ISKImagePyramidTileProvider
    │
    ├── ISKTileCacheStore  (persistent cache: disk, browser, etc.)
    │
    └── ISKTileFetcher     (origin fetch: HTTP, file, composite)
```

The controller only asks for a decoded tile by URL. Everything below — caching and fetching — is the provider's responsibility.

---

## ISKTileFetcher

Pure origin fetch — no caching logic. Returns raw encoded bytes, or `null` for a permanent miss (404, file not found). Throws on retriable failures (network errors) so the caller can decide the retry policy.

```csharp
public interface ISKTileFetcher : IDisposable
{
    Task<SKImagePyramidTileData?> FetchAsync(string url, CancellationToken ct = default);
}
```

### Built-in fetchers

**`SKHttpTileFetcher`** — HTTP GET. Pass your own `HttpClient` or let the fetcher manage one internally.

```csharp
// Internal HttpClient (manages its own lifetime)
var fetcher = new SKHttpTileFetcher();

// Shared HttpClient (you manage its lifetime)
var fetcher = new SKHttpTileFetcher(myHttpClient);
```

**`SKFileTileFetcher`** — Reads from the local filesystem. Accepts plain paths and `file://` URIs.

```csharp
var fetcher = new SKFileTileFetcher();
```

**`SKCompositeTileFetcher`** — Tries multiple fetchers in order; first non-null result wins.

```csharp
// Try app-package assets first, fall back to HTTP
var fetcher = new SKCompositeTileFetcher(
    new MauiAssetFetcher(),
    new SKHttpTileFetcher());
```

---

## ISKTileCacheStore

Persistent tile storage keyed by URL hash. Platform-specific implementations provide disk, browser storage, or database backends.

```csharp
public interface ISKTileCacheStore : IDisposable
{
    Task<SKImagePyramidTileData?> TryGetAsync(string key, CancellationToken ct = default);
    Task SetAsync(string key, SKImagePyramidTileData data, CancellationToken ct = default);
    Task RemoveAsync(string key, CancellationToken ct = default);
    Task ClearAsync(CancellationToken ct = default);
}
```

### Built-in stores

**`SKDiskTileCacheStore`** — Filesystem-backed. Uses FNV-1a URL hashing, bucketed directories, and configurable expiry.

```csharp
var store = new SKDiskTileCacheStore(
    basePath: Path.Combine(FileSystem.CacheDirectory, "tiles"),
    expiry: TimeSpan.FromDays(30)); // default: 30 days
```

**`SKNullTileCacheStore`** — No-op. Useful for testing or when persistence is unwanted.

```csharp
var store = new SKNullTileCacheStore();
```

**`SKChainedTileCacheStore`** — Tries stores in order for reads; writes go to all stores.

```csharp
// Read from an app-bundle read-only store, write to disk
var store = new SKChainedTileCacheStore(
    new AppBundleReadOnlyStore(),
    new SKDiskTileCacheStore(cachePath));
```

---

## SKTieredTileProvider

Composes a fetcher and optional persistent cache into an `ISKImagePyramidTileProvider`. This is the standard implementation for most use cases.

```csharp
public sealed class SKTieredTileProvider : ISKImagePyramidTileProvider
{
    public SKTieredTileProvider(
        ISKTileFetcher fetcher,
        ISKTileCacheStore? persistentCache = null) { ... }
}
```

**Flow:** persistent cache hit → return decoded tile. Cache miss → fetch from origin → persist (fire-and-forget, `CancellationToken.None`) → decode → return.

---

## Common Compositions

### HTTP only (no persistence)

```csharp
var provider = new SKTieredTileProvider(
    new SKHttpTileFetcher());

controller.Load(source, provider);
```

### HTTP + disk cache

```csharp
var provider = new SKTieredTileProvider(
    fetcher: new SKHttpTileFetcher(),
    persistentCache: new SKDiskTileCacheStore(cachePath));

controller.SetProvider(provider);
controller.Load(source);
```

### Local file (no cache needed)

```csharp
var provider = new SKTieredTileProvider(new SKFileTileFetcher());
controller.Load(source, provider);
```

### MAUI app-bundled tiles

```csharp
var provider = new SKTieredTileProvider(
    fetcher: new MauiAssetFetcher()); // reads from app package

controller.Load(localDziSource, provider);
```

### Blazor WASM (browser storage)

```csharp
// Browser sessionStorage L2 cache via JS interop
var provider = new BrowserStorageTileProvider(
    new SKTieredTileProvider(new SKHttpTileFetcher()), js);

controller.Load(source, provider);
```

---

## Provider Lifecycle

The controller does **NOT** own the provider lifecycle — the caller manages disposal. Always dispose the old provider before replacing it:

```csharp
// Correct — dispose old before assigning new
private ISKImagePyramidTileProvider? _provider;

private void SwitchProvider(ISKImagePyramidTileProvider newProvider)
{
    var old = _provider;
    _provider = newProvider;
    _controller.SetProvider(newProvider);
    old?.Dispose();
}

// Correct — dispose on page/component teardown
public override void Dispose()
{
    _controller.Dispose();
    _provider?.Dispose();
}
```

---

## Custom Provider

Implement `ISKImagePyramidTileProvider` directly for full control (authentication, custom headers, etc.):

```csharp
public sealed class AuthenticatedProvider(HttpClient http, string token)
    : ISKImagePyramidTileProvider
{
    public async Task<SKImagePyramidTile?> GetTileAsync(string url, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new("Bearer", token);
        try
        {
            using var response = await http.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode) return null;
            var bytes = await response.Content.ReadAsByteArrayAsync(ct);
            var image = SKImage.FromEncodedData(bytes);
            return image != null ? new SKImagePyramidTile(image, bytes) : null;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch { return null; }
    }

    public void Dispose() { }
}
```

**Return `null`** for permanent misses (404, file not found) — the controller records a temporary failure with exponential backoff via `TileFailureTracker`.

**Throw `OperationCanceledException`** when `ct` is cancelled — the controller handles this without recording a failure, so the tile will be retried.

---

## Provider Decorator

Wrap any provider to add behaviour (logging, delay simulation, browser storage):

```csharp
public sealed class DelayTileProvider(ISKImagePyramidTileProvider inner, int delayMs)
    : ISKImagePyramidTileProvider
{
    public async Task<SKImagePyramidTile?> GetTileAsync(string url, CancellationToken ct = default)
    {
        await Task.Delay(delayMs, ct);
        return await inner.GetTileAsync(url, ct);
    }

    public void Dispose() => inner.Dispose();
}
```

---

## Related

- [Image Pyramid overview](index.md)
- [Controller & Viewport](controller.md)
- [Caching](caching.md)
- [API Reference — ISKImagePyramidTileProvider](xref:SkiaSharp.Extended.ISKImagePyramidTileProvider)
- [API Reference — ISKTileFetcher](xref:SkiaSharp.Extended.ISKTileFetcher)
- [API Reference — ISKTileCacheStore](xref:SkiaSharp.Extended.ISKTileCacheStore)
- [API Reference — SKTieredTileProvider](xref:SkiaSharp.Extended.SKTieredTileProvider)
