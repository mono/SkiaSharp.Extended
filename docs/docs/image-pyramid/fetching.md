# Image Pyramid — Tile Fetching

Tile fetchers implement `ISKImagePyramidTileFetcher` and supply `SKImagePyramidTile` instances (decoded image + original raw bytes) to the Image Pyramid controller on demand. Two built-in fetchers cover HTTP and local file sources; you can implement your own for app-package assets, databases, or any other storage.

The pipeline is: **fetch** raw bytes → buffer into `byte[]` (handles forward-only streams) → **decode** into an `SKImage` → wrap as `SKImagePyramidTile` → **cache**. The raw bytes are stored alongside the decoded image so L2 caches can persist without re-encoding.

## ISKImagePyramidTileFetcher

```csharp
public interface ISKImagePyramidTileFetcher : IDisposable
{
    /// <summary>
    /// Fetches and decodes a tile. Returns null if the tile is not available (e.g. 404).
    /// </summary>
    Task<SKImagePyramidTile?> FetchTileAsync(string url, CancellationToken cancellationToken = default);
}
```

**Return convention:** Return `null` if the tile is unavailable (HTTP 404, file not found). The controller treats `null` as "tile missing — keep using fallback" and doesn't retry indefinitely. Throw only for unexpected errors; the controller catches and fires `TileFailed`.

---

## SKImagePyramidHttpTileFetcher

Fetches tiles over HTTP, buffers the response into a `byte[]`, then decodes with `SKImage.FromEncodedData`. Thread-safe and reusable across multiple controllers.

```csharp
// Create with an internal HttpClient (fetcher manages its lifetime)
var fetcher = new SKImagePyramidHttpTileFetcher();

// Or pass your app's shared HttpClient (you manage its lifetime)
var fetcher = new SKImagePyramidHttpTileFetcher(myHttpClient);
```

The fetcher returns `null` for non-2xx responses and swallows `HttpRequestException` and `TaskCanceledException`, making it robust against transient network errors without crashing the tile pipeline.

When you supply your own `HttpClient`, the fetcher **does not dispose it** — the caller retains ownership.

---

## SKImagePyramidFileTileFetcher

Fetches tiles from the local file system and decodes them. Useful for offline datasets, tests, or locally-generated tile pyramids.

```csharp
var fetcher = new SKImagePyramidFileTileFetcher();

// Works with both plain paths and file:// URIs
var source = SKImagePyramidDziSource.Parse(xml, "/data/images/map_files/");
controller.Load(source, fetcher);
```

Accepts both plain file paths and `file://` URIs. Returns `null` for missing files.

---

## Custom Fetchers

Implement `ISKImagePyramidTileFetcher` to load tiles from any source. Buffer bytes into a `byte[]` first (handles forward-only streams), then call `SKImage.FromEncodedData()` to decode, and return `new SKImagePyramidTile(image, bytes)`.

### App Package Assets (MAUI)

```csharp
public sealed class AppPackageFetcher : ISKImagePyramidTileFetcher
{
    public async Task<SKImagePyramidTile?> FetchTileAsync(string url, CancellationToken ct = default)
    {
        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync(url);
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms, ct);
            var bytes = ms.ToArray();
            var image = SKImage.FromEncodedData(bytes);
            return image != null ? new SKImagePyramidTile(image, bytes) : null;
        }
        catch
        {
            return null; // missing tile — controller will use a fallback tile
        }
    }

    public void Dispose() { }
}
```

Use it directly (no decoder needed):

```csharp
var fetcher = new AppPackageFetcher();
controller.Load(source, fetcher);
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
controller.Load(source, new AppPackageFetcher());
```

### Custom Authentication or Headers

```csharp
public sealed class AuthenticatedFetcher(HttpClient http, string token)
    : ISKImagePyramidTileFetcher
{
    public async Task<SKImagePyramidTile?> FetchTileAsync(string url, CancellationToken ct = default)
    {
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
        catch { return null; }
    }

    public void Dispose() { }
}
```

### In-Memory or Pre-Loaded Tiles

```csharp
public sealed class PreloadedFetcher(Dictionary<string, byte[]> tiles)
    : ISKImagePyramidTileFetcher
{
    public Task<SKImagePyramidTile?> FetchTileAsync(string url, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        if (tiles.TryGetValue(url, out var bytes))
        {
            var image = SKImage.FromEncodedData(bytes);
            return Task.FromResult(image != null ? new SKImagePyramidTile(image, bytes) : null);
        }
        return Task.FromResult<SKImagePyramidTile?>(null);
    }

    public void Dispose() { }
}
```

---

## Passing a Fetcher to the Controller

```csharp
// Fetcher is passed at load time, not construction time
controller.Load(tileSource, fetcher);

// If you reload with a different source, you can pass a new fetcher too
controller.Load(anotherSource, differentFetcher);
```

The controller takes ownership of the fetcher for the duration of the load. Call `controller.Dispose()` to cancel all in-flight fetches and release the fetcher.

---

## Related

- [Image Pyramid overview](index.md)
- [Controller & Viewport](controller.md)
- [Caching](caching.md)
- [API Reference — ISKImagePyramidTileFetcher](xref:SkiaSharp.Extended.ISKImagePyramidTileFetcher)
- [API Reference — SKImagePyramidHttpTileFetcher](xref:SkiaSharp.Extended.SKImagePyramidHttpTileFetcher)
