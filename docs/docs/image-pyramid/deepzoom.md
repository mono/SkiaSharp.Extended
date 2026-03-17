# Deep Zoom (DZI / DZC)

Deep Zoom is a tile-based image format developed by Microsoft for Silverlight/Seadragon. It remains widely used for gigapixel image viewing. The format comes in two flavours:

| Format | Extension | Description |
| :----- | :-------- | :---------- |
| Deep Zoom Image | `.dzi` | A single image sliced into a tile pyramid |
| Deep Zoom Collection | `.dzc` | A mosaic of multiple DZI images in one pyramid |

Both formats plug into the `ISKImagePyramidSource` interface and are fully interchangeable in the controller.

---

## DZI Format

A `.dzi` file is a small XML descriptor:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Image xmlns="http://schemas.microsoft.com/deepzoom/2008"
       Format="jpeg"
       Overlap="1"
       TileSize="256">
  <Size Width="32768" Height="32768"/>
</Image>
```

| Attribute | Description |
| :-------- | :---------- |
| `Format` | Tile image format (`jpeg`, `png`) |
| `Overlap` | Pixel overlap between adjacent tiles (0–2 typical) |
| `TileSize` | Tile size in pixels (256 or 512 common) |
| `Width`, `Height` | Full image dimensions at maximum resolution |

Tiles are stored alongside the `.dzi` file in a directory named `{name}_files/`:

```
image.dzi
image_files/
  0/0_0.jpeg      ← level 0: 1×1 image
  1/0_0.jpeg
  ...
  12/3_5.jpeg     ← level 12: full res, tile at col=3, row=5
```

### Parsing

```csharp
using SkiaSharp.Extended;

// From a URL string
string xml = await httpClient.GetStringAsync("https://example.com/image.dzi");
string tilesBase = "https://example.com/image_files/";
var source = SKImagePyramidDziSource.Parse(xml, tilesBase);

// From a stream
using var stream = File.OpenRead("image.dzi");
var source = SKImagePyramidDziSource.Parse(stream, "/path/to/image_files/");
```

### Tile URL construction

```csharp
// Level 0 = lowest resolution (1×1 px), MaxLevel = full resolution
string url = source.GetFullTileUrl(level: 12, col: 3, row: 5);
// → "https://example.com/image_files/12/3_5.jpeg"

// Pyramid geometry helpers
int levelW = source.GetLevelWidth(12);    // image width at level 12
int levelH = source.GetLevelHeight(12);   // image height at level 12
int cols   = source.GetTileCountX(12);    // number of tile columns
int rows   = source.GetTileCountY(12);    // number of tile rows

// Pixel bounds of a specific tile (includes overlap)
SKImagePyramidRectI bounds = source.GetTileBounds(level: 12, col: 3, row: 5);
```

### Signed / authenticated URLs

Append a query string (e.g., SAS token) to every tile URL:

```csharp
source.TilesQueryString = "?sig=abc123&se=2025-12-31";
```

---

## DZC Format (Collections)

A `.dzc` file describes a mosaic of many images composited into a single tile pyramid using Morton (Z-order) indexing:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Collection xmlns="http://schemas.microsoft.com/deepzoom/2008"
            MaxLevel="7" TileSize="256" Format="jpg" NextItemId="4">
  <Items>
    <I Id="0" N="0" Source="image0.dzi"><Size Width="800" Height="600"/></I>
    <I Id="1" N="1" Source="image1.dzi"><Size Width="1024" Height="768"/></I>
  </Items>
</Collection>
```

### Loading a collection

```csharp
string xml = await httpClient.GetStringAsync("https://example.com/collection.dzc");
var collection = SKImagePyramidDziCollectionSource.Parse(xml);
collection.TilesBaseUri = "https://example.com/";

controller.Load(collection, new SKImagePyramidHttpTileFetcher(new SKImagePyramidImageTileDecoder()));

// Access sub-images
foreach (var sub in collection.Items)
{
    Console.WriteLine($"  Id={sub.Id}  {sub.Width}×{sub.Height}  source={sub.Source}");
}
```

### Morton grid layout

Items in a DZC collection are placed on a 2^N × 2^N Morton grid:

```csharp
int gridSize = collection.GetMortonGridSize();  // e.g., 4 for a 4×4 grid
var (col, row) = SKImagePyramidDziCollectionSource.MortonToGrid(morton: 3);
int morton = SKImagePyramidDziCollectionSource.GridToMorton(col: 1, row: 1);
```

---

## Serving DZI Files

You can generate DZI tile pyramids with:

| Tool | Platform | Notes |
| :--- | :------- | :---- |
| [deepzoom.py](https://github.com/openzoom/deepzoom.py) | Python | Most widely used |
| [VIPS](https://www.libvips.org/) | CLI | Fastest for huge images |
| [Sharp](https://sharp.pixelplumbing.com/) | Node.js | High-performance image processing |
| [Zoomify converter](https://www.zoomify.com/free.htm) | Windows GUI | Cross-format |

### Sample DZI URLs

Try these public DZI files in the demo apps (all support CORS):

| Source | URL | Dimensions |
| :--- | :--- | :--- |
| SkiaSharp.Extended testgrid | `https://raw.githubusercontent.com/mono/SkiaSharp.Extended/refs/heads/main/resources/collections/testgrid/testgrid.dzi` | Test grid |
| OpenSeadragon highsmith | `https://openseadragon.github.io/example-images/highsmith/highsmith.dzi` | 7026 × 9221 |
| OpenSeadragon duomo | `https://openseadragon.github.io/example-images/duomo/duomo.dzi` | 13920 × 10200 |

---

## Related

- [Image Pyramid overview](index.md)
- [IIIF Image API](iiif.md)
- [Tile Fetching](fetching.md)
- [API Reference — SKImagePyramidDziSource](xref:SkiaSharp.Extended.SKImagePyramidDziSource)
- [API Reference — SKImagePyramidDziCollectionSource](xref:SkiaSharp.Extended.SKImagePyramidDziCollectionSource)
