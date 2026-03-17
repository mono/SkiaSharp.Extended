# Image Pyramid — Tile Fetching

Tile fetchers implement `ISKImagePyramidTileFetcher` and supply decoded `ISKImagePyramidTile` instances to the Image Pyramid controller on demand. Two built-in fetchers cover HTTP and local file sources; you can implement your own for app-package assets, databases, or any other storage.

The pipeline is: **fetch** raw bytes → **decode** into a tile → **cache**. The fetcher only fetches; an `ISKImagePyramidTileDecoder` (injected at construction) handles decoding. This keeps the fetcher backend-agnostic and the decoder swappable.

## ISKImagePyramidTileFetcher

```csharp
public interface ISKImagePyramidTileFetcher : IDisposable
{
    /// <summary>
    /// Fetches and decodes a tile. Returns null if the tile is not available (e.g. 404).
    /// </summary>
    Task<ISKImagePyramidTile?> FetchTileAsync(string url, CancellationToken cancellationToken = default);
}
```

**Return convention:** Return `null` if the tile is unavailable (HTTP 404, file not found). The controller treats `null` as "tile missing — keep using fallback" and doesn't retry indefinitely. Throw only for unexpected errors; the controller catches and fires `TileFailed`.

---

## SKImagePyramidHttpTileFetcher

Fetches tiles over HTTP and decodes them using an `ISKImagePyramidTileDecoder`. Thread-safe and reusable across multiple controllers.

```csharp
// Create with a decoder and an internal HttpClient (fetcher manages its lifetime)
var decoder = new SKImagePyramidImageTileDecoder();
var fetcher = new SKImagePyramidHttpTileFetcher(decoder);

// Or pass your app's shared HttpClient (you manage its lifetime)
var fetcher = new SKImagePyramidHttpTileFetcher(decoder, myHttpClient);
```

The fetcher returns `null` for non-2xx responses and swallows `HttpRequestException` and `TaskCanceledException`, making it robust against transient network errors without crashing the tile pipeline.

When you supply your own `HttpClient`, the fetcher **does not dispose it** — the caller retains ownership.

---

## SKImagePyramidFileTileFetcher

Fetches tiles from the local file system and decodes them. Useful for offline datasets, tests, or locally-generated tile pyramids.

```csharp
var decoder = new SKImagePyramidImageTileDecoder();
var fetcher = new SKImagePyramidFileTileFetcher(decoder);

// Works with both plain paths and file:// URIs
var source = SKImagePyramidDziSource.Parse(xml, "/data/images/map_files/");
controller.Load(source, fetcher);
```

Accepts both plain file paths and `file://` URIs. Returns `null` for missing files.

---

## Custom Fetchers

Implement `ISKImagePyramidTileFetcher` to load tiles from any source. Inject an `ISKImagePyramidTileDecoder` to convert raw bytes into `ISKImagePyramidTile` — this keeps your fetcher rendering-backend-agnostic.

### App Package Assets (MAUI)

```csharp
public sealed class AppPackageFetcher(ISKImagePyramidTileDecoder decoder) : ISKImagePyramidTileFetcher
{
    public async Task<ISKImagePyramidTile?> FetchTileAsync(string url, CancellationToken ct = default)
    {
        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync(url);
            // Buffer into MemoryStream so the decoder gets a seekable stream
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms, ct);
            ms.Position = 0;
            return decoder.Decode(ms);
        }
        catch
        {
            return null; // missing tile — controller will use a fallback tile
        }
    }

    public void Dispose() { }
}
```

Use it with the SkiaSharp decoder:

```csharp
var fetcher = new AppPackageFetcher(new SKImagePyramidImageTileDecoder());
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
controller.Load(source, new AppPackageFetcher(new SKImagePyramidImageTileDecoder()));
```

### Custom Authentication or Headers

```csharp
public sealed class AuthenticatedFetcher(HttpClient http, string token, ISKImagePyramidTileDecoder decoder)
    : ISKImagePyramidTileFetcher
{
    public async Task<ISKImagePyramidTile?> FetchTileAsync(string url, CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new("Bearer", token);

        try
        {
            using var response = await http.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode) return null;

            using var ms = new MemoryStream();
            using var stream = await response.Content.ReadAsStreamAsync(ct);
            await stream.CopyToAsync(ms, ct);
            ms.Position = 0;
            return decoder.Decode(ms);
        }
        catch { return null; }
    }

    public void Dispose() { }
}
```

### In-Memory or Pre-Loaded Tiles

```csharp
public sealed class PreloadedFetcher(Dictionary<string, byte[]> tiles, ISKImagePyramidTileDecoder decoder)
    : ISKImagePyramidTileFetcher
{
    public Task<ISKImagePyramidTile?> FetchTileAsync(string url, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        if (tiles.TryGetValue(url, out var bytes))
        {
            using var ms = new MemoryStream(bytes);
            return Task.FromResult(decoder.Decode(ms));
        }
        return Task.FromResult<ISKImagePyramidTile?>(null);
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
