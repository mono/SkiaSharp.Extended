# IIIF Image API Support

The Image Pyramid system supports [IIIF Image API](https://iiif.io/api/image/) v2 and v3 through
`SKImagePyramidIiifSource`. IIIF (International Image Interoperability Framework) is widely used
by libraries, museums, and archives to serve high-resolution images.

## What is IIIF?

IIIF Image API provides a standard way to serve image tiles. Each image server exposes an
`info.json` endpoint that describes the image dimensions and available tile sizes:

```json
{
  "@context": "http://iiif.io/api/image/2/context.json",
  "@id": "https://iiif.wellcomecollection.org/image/b20432033_B0008608.JP2",
  "width": 3543,
  "height": 2480,
  "tiles": [
    { "width": 1024, "height": 1024, "scaleFactors": [1, 2, 4, 8, 16, 32] }
  ]
}
```

## Using `SKImagePyramidIiifSource`

### Parse from info.json

```csharp
// Fetch and parse the IIIF info.json
using var http = new HttpClient();
var json = await http.GetStringAsync("https://example.org/iiif/image/my-image/info.json");
var source = SKImagePyramidIiifSource.Parse(json);

// Load into the controller (same as DZI)
controller.Load(source, new SKImagePyramidHttpTileFetcher(new SKImagePyramidImageTileDecoder()));
```

### Construct manually

```csharp
var source = new SKImagePyramidIiifSource(
    baseId: "https://iiif.wellcomecollection.org/image/b20432033_B0008608.JP2",
    imageWidth: 3543,
    imageHeight: 2480,
    tileWidth: 1024,
    tileHeight: 1024,
    scaleFactorsDescending: new[] { 32, 16, 8, 4, 2, 1 }
);
```

## Auto-detecting Source Type

In the sample apps, the URL is inspected to select the right parser:

```csharp
private async Task LoadFromUrlAsync(string url)
{
    var content = await http.GetStringAsync(url);

    if (url.EndsWith(".dzc", StringComparison.OrdinalIgnoreCase))
    {
        var coll = SKImagePyramidDziCollectionSource.Parse(content);
        coll.TilesBaseUri = url[..url.LastIndexOf('/')] + "/";
        controller.Load(coll, fetcher);
    }
    else if (url.Contains("info.json") || url.Contains("/iiif/") || url.Contains("iiif.io"))
    {
        var source = SKImagePyramidIiifSource.Parse(content);
        controller.Load(source, fetcher);
    }
    else
    {
        // DZI
        string baseDir = url[..url.LastIndexOf('/')] + "/";
        string stem = Path.GetFileNameWithoutExtension(url);
        var source = SKImagePyramidDziSource.Parse(content, $"{baseDir}{stem}_files/");
        controller.Load(source, fetcher);
    }
}
```

## How IIIF Levels Map to the Pyramid

IIIF uses **scale factors** to describe resolution levels. A scale factor of `S` means the image
is scaled down by `S` relative to full resolution. The `scaleFactors` array is sorted in descending
order internally so that pyramid level 0 = lowest resolution (largest scale factor) and
`MaxLevel` = full resolution (scale factor = 1):

| IIIF scaleFactors | Pyramid Level | Resolution |
|---|---|---|
| 32 | 0 | Lowest (thumbnail) |
| 16 | 1 | |
| 8  | 2 | |
| 4  | 3 | |
| 2  | 4 | |
| 1  | 5 (MaxLevel) | Full resolution |

## Tile URL Format

IIIF tile URLs follow the pattern:

```
{baseId}/{region}/{size}/{rotation}/{quality}.{format}
```

For example, a full-resolution tile at column 0, row 0:

```
https://iiif.wellcomecollection.org/image/b20432033_B0008608.JP2/0,0,1024,1024/1024,1024/0/default.jpg
```

Where:
- `0,0,1024,1024` — region in full-resolution pixel coordinates (x, y, w, h)
- `1024,1024` — output size at the requested level
- `0` — rotation (always 0)
- `default.jpg` — quality and format

## The `ISKImagePyramidSource` Interface

Both `SKImagePyramidDziSource` and `SKImagePyramidIiifSource` implement `ISKImagePyramidSource`,
which abstracts the tile pyramid math:

```csharp
public interface ISKImagePyramidSource
{
    int ImageWidth { get; }
    int ImageHeight { get; }
    int MaxLevel { get; }
    double AspectRatio { get; }

    int GetLevelWidth(int level);
    int GetLevelHeight(int level);
    int GetTileCountX(int level);
    int GetTileCountY(int level);
    SKImagePyramidRectI GetTileBounds(int level, int col, int row);
    string? GetFullTileUrl(int level, int col, int row);
    int GetOptimalLevel(double viewportWidth, double controlWidth);
}
```

The `SKImagePyramidController.Load(ISKImagePyramidSource, ISKImagePyramidTileFetcher)` overload
accepts any source type, making it easy to add support for other formats.

## Adding Your Own Format (e.g., Zoomify)

To support [Zoomify](http://www.zoomify.com/), implement `ISKImagePyramidSource`:

```csharp
public class SKImagePyramidZoomifySource : ISKImagePyramidSource
{
    // Zoomify uses 256×256 tiles, stored as ZoomifyImage/TileGroup{N}/Level-Col-Row.jpg
    // Implement GetFullTileUrl to return the correct URL for each tile.
    // ...
}
```

## Sample IIIF URLs

Try these public IIIF endpoints in the demo apps:

| Institution | info.json URL |
|---|---|
| Wellcome Collection | `https://iiif.wellcomecollection.org/image/b20432033_B0008608.JP2/info.json` |
| British Library | `https://api.bl.uk/image/iiif/ark:/81055/vdc_100022589218.0x000002/info.json` |
