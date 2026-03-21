# Image Pyramid — Tile Providers

Tile providers implement `ISKImagePyramidTileProvider` and own the complete fetch-plus-cache pipeline. The controller asks for a tile by URL; the provider decides whether to serve it from disk, browser storage, or the network — the controller knows nothing about how or where the tile was obtained.

The pipeline in a provider is: **check cache** (disk or storage) → if miss, **fetch** raw bytes → buffer into `byte[]` → **decode** into an `SKImage` → **persist** (optional, CancellationToken.None) → wrap as `SKImagePyramidTile` → return. Raw bytes are stored alongside the decoded image so caches can persist without re-encoding.

## ISKImagePyramidTileProvider

```csharp
public interface ISKImagePyramidTileProvider : IDisposable
{
    /// <summary>
    /// Fetches and decodes a tile. Returns null if the tile is unavailable (e.g. 404).
    /// Throws OperationCanceledException if ct is cancelled.
    /// </summary>
    Task<SKImagePyramidTile?> GetTileAsync(string url, CancellationToken ct = default);
}
```

**Return convention:** Return `null` if the tile is unavailable (HTTP 404, file not found). The controller treats `null` as a permanent miss and uses a fallback tile. Throw `OperationCanceledException` when `ct` is cancelled (not swallowed) — the controller handles this without blacklisting the tile, so it will be retried on the next render cycle.

---

## SKImagePyramidHttpTileProvider

Fetches tiles over HTTP, buffers the response into a `byte[]`, then decodes with `SKImage.FromEncodedData`. Optionally caches fetched tiles to a local disk directory (URL-keyed, expiry-aware).

```csharp
// HTTP only — no disk cache
var provider = new SKImagePyramidHttpTileProvider();

// With disk cache (persists across app restarts)
var provider = new SKImagePyramidHttpTileProvider(
    diskCachePath: Path.Combine(FileSystem.CacheDirectory, "tiles"),
    expiry: TimeSpan.FromDays(30));

// Pass your app's shared HttpClient (you manage its lifetime)
var provider = new SKImagePyramidHttpTileProvider(httpClient: myHttpClient);
```

When you supply your own `HttpClient`, the provider **does not dispose it** — the caller retains ownership.

HTTP timeouts (`TaskCanceledException`) return `null` — treated as a transient miss, not a crash. Cancellation via `ct` propagates as `OperationCanceledException` so the controller can retry the tile.

---

## SKImagePyramidFileTileProvider

Reads tiles from the local filesystem. No disk caching is performed — the file IS the source, so caching would be redundant.

```csharp
var provider = new SKImagePyramidFileTileProvider();

// Works with both plain paths and file:// URIs
var source = SKImagePyramidDziSource.Parse(xml, "/data/images/map_files/");
controller.Load(source, provider);
```

Accepts both plain file paths and `file://` URIs. Returns `null` for missing files.

---

## Custom Providers

Implement `ISKImagePyramidTileProvider` to load tiles from any source — app-package assets, browser storage, databases, or any combination.

The pattern: check your cache first → if miss, fetch → persist → return. Buffer bytes into a `byte[]` (handles forward-only streams), then `SKImage.FromEncodedData()` to decode, and return `new SKImagePyramidTile(image, bytes)`.

### App Package Assets (MAUI)

```csharp
public sealed class AppPackageProvider : ISKImagePyramidTileProvider
{
    public async Task<SKImagePyramidTile?> GetTileAsync(string url, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync(url);
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms, ct);
            var bytes = ms.ToArray();
            var image = SKImage.FromEncodedData(bytes);
            return image != null ? new SKImagePyramidTile(image, bytes) : null;
        }
        catch (OperationCanceledException) { throw; }
        catch
        {
            return null; // missing tile — controller will use a fallback tile
        }
    }

    public void Dispose() { }
}
```

Use it directly:

```csharp
var provider = new AppPackageProvider();
controller.Load(source, provider);
```

Include tile assets in your MAUI project:

```xml
<ItemGroup>
    <MauiAsset Include="Assets\image.dzi" LogicalName="image.dzi" />
    <MauiAsset Include="Assets\image_files\**"
               LogicalName="image_files/%(RecursiveDir)%(Filename)%(Extension)" />
</ItemGroup>
```

Load the DZI from the package:

```csharp
using var stream = await FileSystem.OpenAppPackageFileAsync("image.dzi");
using var reader = new StreamReader(stream);
var xml = await reader.ReadToEndAsync();

// Use "image_files/" as base URI to match the MauiAsset LogicalName prefix
var source = SKImagePyramidDziSource.Parse(xml, "image_files/");
controller.Load(source, new AppPackageProvider());
```

### Custom Authentication or Headers

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

### Provider Decorator (e.g., artificial delay for testing)

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

## Passing a Provider to the Controller

```csharp
// Provider is passed at load time
controller.Load(tileSource, new SKImagePyramidHttpTileProvider());

// Swap source and provider together
controller.Load(anotherSource, new SKImagePyramidHttpTileProvider(diskCachePath: cachePath));

// Replace just the provider (preserves viewport and source)
controller.ReplaceProvider(new SKImagePyramidHttpTileProvider());
```

The controller takes ownership of the provider on `Load()`. Calling `Load()` or `Dispose()` also disposes the previous provider.

---

## Related

- [Image Pyramid overview](index.md)
- [Controller & Viewport](controller.md)
- [Caching](caching.md)
- [API Reference — ISKImagePyramidTileProvider](xref:SkiaSharp.Extended.ISKImagePyramidTileProvider)
- [API Reference — SKImagePyramidHttpTileProvider](xref:SkiaSharp.Extended.SKImagePyramidHttpTileProvider)
