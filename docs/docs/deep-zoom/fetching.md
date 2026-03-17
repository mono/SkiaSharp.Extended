# Deep Zoom — Tile Fetching

Tile fetchers implement `ISKDeepZoomTileFetcher` and supply decoded `SKImage` objects to the Deep Zoom controller on demand. Two built-in fetchers cover HTTP and local file sources; you can implement your own for app-package assets, databases, or any other storage.

## ISKDeepZoomTileFetcher

```csharp
public interface ISKDeepZoomTileFetcher : IDisposable
{
    /// <summary>
    /// Fetches a tile as an SKImage. Returns null if the tile is not available (e.g. 404).
    /// </summary>
    Task<SKImage?> FetchTileAsync(string url, CancellationToken cancellationToken = default);
}
```

**Return convention:** Return `null` if the tile is unavailable (HTTP 404, file not found). The controller treats `null` as "tile missing — keep using fallback" and doesn't retry indefinitely. Throw only for unexpected errors; the controller catches and fires `TileFailed`.

---

## SKDeepZoomHttpTileFetcher

Fetches tiles over HTTP. Thread-safe and reusable across multiple controllers.

```csharp
// Create with an internal HttpClient (controller manages its lifetime)
var fetcher = new SKDeepZoomHttpTileFetcher();

// Or pass your app's shared HttpClient (you manage its lifetime)
var fetcher = new SKDeepZoomHttpTileFetcher(myHttpClient);
```

The fetcher returns `null` for non-2xx responses and swallows `HttpRequestException` and `TaskCanceledException`, making it robust against transient network errors without crashing the tile pipeline.

When you supply your own `HttpClient`, the fetcher **does not dispose it** — the caller retains ownership.

---

## SKDeepZoomFileTileFetcher

Fetches tiles from the local file system. Useful for offline datasets, tests, or locally-generated tile pyramids.

```csharp
var fetcher = new SKDeepZoomFileTileFetcher();

// Works with both plain paths and file:// URIs
var source = SKDeepZoomImageSource.Parse(xml, "/data/images/map_files/");
controller.Load(source, fetcher);
```

Accepts both plain file paths and `file://` URIs. Returns `null` for missing files.

---

## Custom Fetchers

Implement `ISKDeepZoomTileFetcher` to load tiles from any source.

### App Package Assets (MAUI)

```csharp
public sealed class AppPackageFetcher : ISKDeepZoomTileFetcher
{
    public async Task<SKImage?> FetchTileAsync(string url, CancellationToken ct = default)
    {
        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync(url);
            return SKImage.FromEncodedData(stream);
        }
        catch
        {
            return null; // missing tile — controller will use a fallback tile
        }
    }

    public void Dispose() { }
}
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
var source = SKDeepZoomImageSource.Parse(xml, "image_files/");
controller.Load(source, new AppPackageFetcher());
```

### Custom Authentication or Headers

```csharp
public sealed class AuthenticatedFetcher : ISKDeepZoomTileFetcher
{
    private readonly HttpClient _http;
    private readonly string _token;

    public AuthenticatedFetcher(HttpClient http, string token)
    {
        _http = http;
        _token = token;
    }

    public async Task<SKImage?> FetchTileAsync(string url, CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new("Bearer", _token);

        try
        {
            using var response = await _http.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode) return null;
            using var stream = await response.Content.ReadAsStreamAsync(ct);
            return SKImage.FromEncodedData(stream);
        }
        catch { return null; }
    }

    public void Dispose() { }
}
```

### In-Memory or Pre-Loaded Tiles

```csharp
public sealed class PreloadedFetcher : ISKDeepZoomTileFetcher
{
    private readonly Dictionary<string, byte[]> _tiles;

    public PreloadedFetcher(Dictionary<string, byte[]> tiles) => _tiles = tiles;

    public Task<SKImage?> FetchTileAsync(string url, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        if (_tiles.TryGetValue(url, out var bytes))
            return Task.FromResult<SKImage?>(SKImage.FromEncodedData(bytes));
        return Task.FromResult<SKImage?>(null);
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

- [Deep Zoom overview](index.md)
- [Controller & Viewport](controller.md)
- [Caching](caching.md)
- [API Reference — ISKDeepZoomTileFetcher](xref:SkiaSharp.Extended.ISKDeepZoomTileFetcher)
- [API Reference — SKDeepZoomHttpTileFetcher](xref:SkiaSharp.Extended.SKDeepZoomHttpTileFetcher)
