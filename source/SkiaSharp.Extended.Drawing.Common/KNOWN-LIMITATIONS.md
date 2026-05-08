# SkiaSharp.Extended.Drawing.Common — Known Limitations

This document describes the known differences and limitations between
`SkiaSharp.Extended.Drawing.Common` and the official `System.Drawing.Common` (GDI+).

## Rendering Differences

### Anti-Aliasing

GDI+ and SkiaSharp use **different anti-aliasing algorithms**. When
`SmoothingMode.AntiAlias` is enabled, edge pixels will have different
blending intensities. GDI+ typically produces a wider AA fringe than Skia.

- **Non-AA geometry** (SmoothingMode.None): <0.5% pixel error — near pixel-perfect
- **AA geometry** (SmoothingMode.AntiAlias): up to 5% pixel error at edges
- **Solid fills and colors**: <0.1% error — effectively identical

This is a fundamental difference between the two rasterizers and cannot
be eliminated. The shape outlines and interiors are correct; only the
boundary pixel blending differs.

### SmoothingMode Mapping

`SmoothingMode.Default` and `SmoothingMode.Invalid` map to **no anti-aliasing**
(matching GDI+ behavior). Only `SmoothingMode.AntiAlias` and
`SmoothingMode.HighQuality` enable AA in SkiaSharp.

### Half-Pixel Coordinate Offset

GDI+ treats integer coordinates with a +0.5 pixel center offset for
curve rasterization (ellipses, arcs, pies). SkiaSharp does not.

**Mitigation:** SkiaSharp.Extended.Drawing.Common applies the +0.5 offset automatically
via `GdiCurveRect()` and `GdiPolygonPath()` helpers. This brings curve
rendering from ~2-4% error down to ~0.08% vs GDI+.

If you observe 1px boundary shifts on curves, this offset is the cause
and is already handled for all standard API calls.

### Text Rendering

GDI+ and SkiaSharp use different font rasterizers:
- **GDI+**: Windows ClearType with hinting
- **SkiaSharp**: FreeType/HarfBuzz

Differences to expect:
- Glyph shapes may differ by 1-2 pixels, especially at small sizes
- `MeasureString` padding: GDI+ adds ~1/6 em padding on each side.
  SkiaSharp.Extended.Drawing.Common replicates this, but exact widths may differ slightly
- Line spacing calculations may produce different `Height` values
- ClearType subpixel rendering is not available in SkiaSharp

**Recommendation:** For text-heavy pixel comparison tests, use a higher
tolerance (2-5%) or compare at larger font sizes where rasterizer
differences are proportionally smaller.

### Gradient Interpolation

GDI+ interpolates gradients in sRGB color space. SkiaSharp can
interpolate in either sRGB or linear space. The default behavior may
produce slightly different color values in gradient midpoints.

### Gradient Color Rounding

GDI+ and Skia use different float-to-byte rounding for gradient color
interpolation. This produces ±1 RGB value differences at ~30% of pixels.
Visually identical (MAE < 0.5). The pixel diff tests use a per-pixel
tolerance of 3 (±1 per channel) to account for this.

### Cardinal Spline Tension

The cardinal spline implementation uses a `tension * 0.3` factor (matching
the Wine/ReactOS GDI+ implementation) instead of the mathematically
standard `tension / 3` (~0.333). The difference is < 1% for default
tension and is consistent with how GDI+ actually behaves.

## Unimplemented Features

### Printing

`PrintDocument.Print()` outputs to **PDF via SkiaSharp's SKDocument**
rather than sending to a physical printer via the Windows spooler.

- ✅ `PrintDocument` event model (BeginPrint, PrintPage, EndPrint)
- ✅ `PageSettings` (paper size, margins, landscape, resolution)
- ✅ `PrinterSettings` (copies, page range, print to file)
- ✅ `PreviewPrintController` (renders pages to bitmaps)
- ✅ Multi-page PDF output
- ❌ Physical printer discovery (`InstalledPrinters` returns only "PDF")
- ❌ Direct spooler integration (no `StandardPrintController` → spooler)
- ❌ `GetHdevmode` / `SetHdevmode` (Windows handle-based APIs)
- ❌ Duplex printing, collation (properties exist but no effect on PDF)

### Metafile / EMF / WMF

Enhanced Metafile (EMF) and Windows Metafile (WMF) formats are not
supported. All `Metafile` class methods throw `PlatformNotSupportedException`.

- ❌ `Metafile` constructors and playback
- ❌ `MetafileHeader` properties
- ❌ `Graphics.EnumerateMetafile()`
- ❌ `Graphics.AddMetafileComment()`
- ❌ WMF/EMF file loading

### Platform-Specific APIs

These APIs require Windows GDI handles and are not implementable
cross-platform:

- ❌ `Graphics.FromHwnd()` / `Graphics.FromHdc()` — window/device context
- ❌ `Graphics.GetHdc()` / `Graphics.ReleaseHdc()` — device context handle
- ❌ `Graphics.CopyFromScreen()` — screen capture
- ❌ `Bitmap.GetHbitmap()` / `Bitmap.GetHicon()` — GDI handles
- ❌ `Bitmap.FromHicon()` / `Bitmap.FromResource()` — Windows resources
- ❌ `Icon.FromHandle()` / `Icon.Handle` — icon handles
- ❌ `Font.ToHfont()` / `Font.FromHfont()` — font handles
- ❌ `Font.ToLogFont()` — LOGFONT structure

### Image Formats

- ✅ PNG, JPEG, BMP, GIF, WEBP — full support via SkiaSharp codecs
- ❌ TIFF — limited support (SkiaSharp codec dependent)
- ❌ EMF/WMF — not supported (see Metafile section)
- ❌ ICO — can load via Icon class but limited format support
- ❌ EXIF metadata — `PropertyItems` returns empty array
- ❌ Multi-frame images (animated GIF) — `SelectActiveFrame` decodes individual frames; `ImageAnimator` basic stub only
- ❌ Indexed pixel formats (8bpp, 4bpp, 1bpp) — converted to 32bpp on load

### Clip Region CombineModes

`SetClip()` supports all six combine modes:
- ✅ `Replace`, `Intersect`, `Exclude` — direct SKCanvas clip operations
- ✅ `Union`, `Xor`, `Complement` — computed via `SKPath.Op()`, then re-applied as a clip path

## Partial Implementations

These features are implemented but have known gaps or approximations.

### ImageAttributes

`SetColorMatrix()` is applied during rendering via `SKColorFilter.CreateColorMatrix()`.
The following methods store values but are **not yet applied** during drawing:
- `SetGamma()` — gamma value stored, not applied
- `SetThreshold()` — threshold value stored, not applied
- `SetColorKey()` — color key range stored, not applied

### Pen

- **Pen.Transform** — Only uniform scale extraction. GDI+ applies the full
  matrix (anisotropic scaling, rotation, shear) to the stroke pattern. We
  extract only the X-axis scale magnitude and multiply stroke width
  uniformly. A pen transform that scales X by 2 and Y by 1 produces
  asymmetric strokes in GDI+ but uniform 2px strokes in our implementation.
- **StartCap vs EndCap** — When StartCap differs from EndCap, Skia can only
  apply one cap per paint. We prefer the non-Flat cap and fall back to
  EndCap. GDI+ draws different cap shapes at each end of a line. Proper fix
  requires drawing the line body without caps, then manually rendering cap
  shapes at endpoints.
- **Anchor caps** (RoundAnchor, SquareAnchor, DiamondAnchor, ArrowAnchor,
  Triangle, AnchorMask) — All map to flat (butt) cap. GDI+ renders circles,
  squares, diamonds, or arrows at line endpoints. These would require custom
  path shapes drawn at each endpoint.
- **DashCap** — Stored but not applied to `SKPaint`. GDI+ applies separate
  cap styles to dash segment ends (Round, Triangle, Flat). Skia uses the
  same stroke cap for dashes and line ends.
- **PenAlignment.Inset** — Implemented via clip-to-path trick (clip to shape
  boundary, double stroke width). Works for closed shapes but may produce
  different AA at boundaries than GDI+'s true inset stroke. Does not work
  for open paths (lines).
- **CompoundArray** — Implemented via `GetFillPath()` and
  `SKPathOp.Difference` to carve out sub-strokes within the pen width. May
  not match GDI+ pixel-for-pixel at stroke boundaries, especially on
  curves.

### HatchBrush

All 53 standard hatch patterns are rendered as tiled 8×8 pixel bitmaps using
`SKShader.CreateImage()` with `SKShaderTileMode.Repeat`. Patterns may not
match GDI+ pixel-for-pixel due to different rasterization approaches.
Unrecognized styles fall back to a cross pattern.

**Diagonal hatch anti-aliasing:** GDI+ applies anti-aliasing to diagonal
hatch lines (ForwardDiagonal, BackwardDiagonal, DiagonalCross), producing
sub-pixel blending at tile boundaries. Skia renders these as 1-bit patterns
without AA, resulting in ~37-63% pixel error for diagonal styles. The
patterns are structurally correct but lack the edge smoothing.

### PathGradientBrush

Implemented using `SKCanvas.DrawVertices` with a triangle fan. The center
vertex has `CenterColor`; boundary vertices have `SurroundColors`. The path
is flattened (beziers/conics subdivided to polylines) and clipped for
concave shapes.

- **Interpolation difference**: Skia uses barycentric interpolation within
  each triangle; GDI+ uses ray-from-center interpolation. This produces
  visible differences (25-65% pixel error) especially for multi-color
  gradients, though shapes and colors are correct.
- **Blend/InterpolationColors**: Not applied to the triangle fan. The Blend
  and InterpolationColors properties affect the center-to-edge falloff
  curve, which would require inserting concentric rings of intermediate
  vertices.
- **FocusScales**: Stored but not applied to rendering.
- **WrapMode**: Not applied (triangle fan fills the path interior only).

### LinearGradientBrush

Basic two-color blends work correctly. Complex multi-stop
`InterpolationColors` blends may produce slightly different results than
GDI+ due to color space interpolation differences.

### DrawString

Word wrapping is implemented for `StringFormat` layout rectangles.
Character-level trimming modes (`EllipsisCharacter`, `EllipsisWord`,
`EllipsisPath`) are not fully implemented — text may be clipped rather
than truncated with an ellipsis.

### MeasureString

Padding matches the GDI+ convention of ~1/6 em on each side, but exact
width and height metrics may differ slightly due to different font
rasterizers (FreeType/HarfBuzz vs Windows ClearType).

### LockBits

Works correctly for `Format32bppArgb` and `Format32bppPArgb`. Sub-byte
pixel formats (`Format1bppIndexed`, `Format4bppIndexed`) are not supported
and will throw.

### Printing (PDF-only)

PDF output via `SKDocument` is fully functional. No physical printer
spooler integration — `InstalledPrinters` returns only "PDF", and duplex
printing / collation properties have no effect on PDF output.

### Font

All constructors and style properties work. Font metrics (`Height`,
`GetHeight()`, `Size`) may differ slightly from GDI+ due to different font
rasterizers. `GdiCharSet` and `GdiVerticalFont` are stored but have no
rendering effect.

### GraphicsPath

- ✅ AddLine, AddRectangle, AddEllipse, AddArc, AddBezier, AddPolygon, AddPath, AddPie
- ✅ `AddCurve` / `AddClosedCurve` — cardinal spline curves on path
- ✅ `AddString` — text outlines via `SKFont.GetTextPath()`
- ✅ `Flatten()` — De Casteljau subdivision of curves to line segments
- ✅ `Widen(Pen)` — stroke to fill conversion via `SKPaint.GetFillPath()`
- ✅ `IsOutlineVisible()` — hit testing on widened stroke path
- ✅ `PathData` — returns points and types arrays
- ❌ `Warp()` — perspective/bilinear warp (complex, no SkiaSharp equivalent)

### Region

- ✅ Boolean operations (Union, Intersect, Exclude, Complement, Xor)
- ✅ Visibility testing, bounds, transform
- ❌ `GetRegionData()` — serialization to byte array
- ❌ `GetRegionScans(Matrix)` — decomposition to rectangles

### TypeConverters

`FontConverter`, `ImageConverter`, `ImageFormatConverter`, `ColorConverter`,
`MarginsConverter`, `IconConverter` — most conversion methods throw PNSE.
These are primarily used by the Windows Forms designer and property grid.

### PixelOffsetMode

`PixelOffsetMode.Half` and `PixelOffsetMode.HighQuality` are stored but not
fully implemented. These modes should shift rendering by 0.5 pixels for
quality improvement. Currently has no effect on output.

### CompositingMode

`CompositingMode.SourceCopy` is stored but not applied during rendering.
GDI+ SourceCopy overwrites destination pixels without alpha blending. Our
implementation always uses SourceOver (alpha blend).

### DrawImage Interpolation

Uses `SKSamplingOptions` with `SKFilterMode.Linear` for default bilinear.
Boundary pixel sampling may differ from GDI+'s bilinear implementation,
producing visible differences at color edges when upscaling small images.

## API Compatibility

The assembly has **API compatibility** with `System.Drawing.Common`
validated by `dotnet apicompat`. Every public type, method,
property, and event exists with the correct signature.

Unimplemented methods throw `PlatformNotSupportedException` with a
descriptive message. This allows code that references System.Drawing APIs
to compile and link against SkiaSharp.Extended.Drawing.Common, failing gracefully at
runtime only when unimplemented features are actually called.

## Performance Notes

- `GetPixel()` / `SetPixel()` are implemented but slow for bulk operations.
  Use `LockBits()` for direct pixel buffer access instead.
- `Graphics.FromImage()` creates an `SKCanvas` per call. Reuse the
  Graphics object when drawing multiple operations on the same image.
- `Brushes.*` and `Pens.*` factories use lazy initialization with caching.
  They are thread-safe for read access.

## All Unimplemented APIs — Complete Reference (130 stubs)

Every method below throws `PlatformNotSupportedException`. Use Ctrl+F to
search for any specific method name.

| Category | Count | Reason |
|----------|-------|--------|
| Metafile class | 46 | EMF/WMF format construction and playback |
| EnumerateMetafile | 36 | EMF/WMF record enumeration |
| HDC/HWND (Graphics) | 9 | Windows device context handles |
| Font GDI handles | 8 | HFONT / LOGFONT / HDC interop |
| Printing GDI handles | 7 | DEVMODE printer handles |
| Image | 6 | FromHbitmap, serialization, encoder params |
| Bitmap GDI handles | 5 | HBITMAP / HICON handles |
| CopyFromScreen | 4 | Platform-specific screen capture |
| Icon handles | 4 | HICON handle interop |
| BufferedGraphics | 2 | HDC-based device context rendering |
| Other (Graphics) | 3 | AddMetafileComment, GetHalftonePalette, GetContextInfo |

---

### Metafile (46 methods)

The entire `System.Drawing.Imaging.Metafile` class is unimplemented.
EMF/WMF is a Windows-specific vector format with no cross-platform equivalent.

**Constructors (39):**

```
Metafile(nint henhmetafile, bool deleteEmf)
Metafile(nint referenceHdc, EmfType emfType)
Metafile(nint referenceHdc, EmfType emfType, string? description)
Metafile(nint hmetafile, WmfPlaceableFileHeader wmfHeader)
Metafile(nint hmetafile, WmfPlaceableFileHeader wmfHeader, bool deleteWmf)
Metafile(nint referenceHdc, Rectangle frameRect)
Metafile(nint referenceHdc, Rectangle frameRect, MetafileFrameUnit frameUnit)
Metafile(nint referenceHdc, Rectangle frameRect, MetafileFrameUnit frameUnit, EmfType type)
Metafile(nint referenceHdc, Rectangle frameRect, MetafileFrameUnit frameUnit, EmfType type, string? desc)
Metafile(nint referenceHdc, RectangleF frameRect)
Metafile(nint referenceHdc, RectangleF frameRect, MetafileFrameUnit frameUnit)
Metafile(nint referenceHdc, RectangleF frameRect, MetafileFrameUnit frameUnit, EmfType type)
Metafile(nint referenceHdc, RectangleF frameRect, MetafileFrameUnit frameUnit, EmfType type, string? description)
Metafile(Stream stream)
Metafile(Stream stream, nint referenceHdc)
Metafile(Stream stream, nint referenceHdc, EmfType type)
Metafile(Stream stream, nint referenceHdc, EmfType type, string? description)
Metafile(Stream stream, nint referenceHdc, Rectangle frameRect)
Metafile(Stream stream, nint referenceHdc, Rectangle frameRect, MetafileFrameUnit frameUnit)
Metafile(Stream stream, nint referenceHdc, Rectangle frameRect, MetafileFrameUnit frameUnit, EmfType type)
Metafile(Stream stream, nint referenceHdc, Rectangle frameRect, MetafileFrameUnit frameUnit, EmfType type, string? description)
Metafile(Stream stream, nint referenceHdc, RectangleF frameRect)
Metafile(Stream stream, nint referenceHdc, RectangleF frameRect, MetafileFrameUnit frameUnit)
Metafile(Stream stream, nint referenceHdc, RectangleF frameRect, MetafileFrameUnit frameUnit, EmfType type)
Metafile(Stream stream, nint referenceHdc, RectangleF frameRect, MetafileFrameUnit frameUnit, EmfType type, string? description)
Metafile(string filename)
Metafile(string fileName, nint referenceHdc)
Metafile(string fileName, nint referenceHdc, EmfType type)
Metafile(string fileName, nint referenceHdc, EmfType type, string? description)
Metafile(string fileName, nint referenceHdc, Rectangle frameRect)
Metafile(string fileName, nint referenceHdc, Rectangle frameRect, MetafileFrameUnit frameUnit)
Metafile(string fileName, nint referenceHdc, Rectangle frameRect, MetafileFrameUnit frameUnit, EmfType type)
Metafile(string fileName, nint referenceHdc, Rectangle frameRect, MetafileFrameUnit frameUnit, EmfType type, string? description)
Metafile(string fileName, nint referenceHdc, Rectangle frameRect, MetafileFrameUnit frameUnit, string? description)
Metafile(string fileName, nint referenceHdc, RectangleF frameRect)
Metafile(string fileName, nint referenceHdc, RectangleF frameRect, MetafileFrameUnit frameUnit)
Metafile(string fileName, nint referenceHdc, RectangleF frameRect, MetafileFrameUnit frameUnit, EmfType type)
Metafile(string fileName, nint referenceHdc, RectangleF frameRect, MetafileFrameUnit frameUnit, EmfType type, string? description)
Metafile(string fileName, nint referenceHdc, RectangleF frameRect, MetafileFrameUnit frameUnit, string? desc)
```

**Instance methods (3):**

```
nint GetHenhmetafile()
MetafileHeader GetMetafileHeader()
void PlayRecord(EmfPlusRecordType recordType, int flags, int dataSize, byte[] data)
```

**Static methods (4):**

```
static MetafileHeader GetMetafileHeader(nint henhmetafile)
static MetafileHeader GetMetafileHeader(nint hmetafile, WmfPlaceableFileHeader wmfHeader)
static MetafileHeader GetMetafileHeader(Stream stream)
static MetafileHeader GetMetafileHeader(string fileName)
```

---

### Graphics (52 methods)

#### EnumerateMetafile — 36 overloads

All `EnumerateMetafile` overloads throw PNSE. They enumerate EMF/WMF
records for playback, which requires a metafile parser.

```
void EnumerateMetafile(Metafile metafile, Point destPoint, Graphics.EnumerateMetafileProc callback)
void EnumerateMetafile(Metafile metafile, Point destPoint, Graphics.EnumerateMetafileProc callback, nint callbackData)
void EnumerateMetafile(Metafile metafile, Point destPoint, Graphics.EnumerateMetafileProc callback, nint callbackData, ImageAttributes? imageAttr)
void EnumerateMetafile(Metafile metafile, Point destPoint, Rectangle srcRect, GraphicsUnit srcUnit, Graphics.EnumerateMetafileProc callback)
void EnumerateMetafile(Metafile metafile, Point destPoint, Rectangle srcRect, GraphicsUnit srcUnit, Graphics.EnumerateMetafileProc callback, nint callbackData)
void EnumerateMetafile(Metafile metafile, Point destPoint, Rectangle srcRect, GraphicsUnit unit, Graphics.EnumerateMetafileProc callback, nint callbackData, ImageAttributes? imageAttr)
void EnumerateMetafile(Metafile metafile, PointF destPoint, Graphics.EnumerateMetafileProc callback)
void EnumerateMetafile(Metafile metafile, PointF destPoint, Graphics.EnumerateMetafileProc callback, nint callbackData)
void EnumerateMetafile(Metafile metafile, PointF destPoint, Graphics.EnumerateMetafileProc callback, nint callbackData, ImageAttributes? imageAttr)
void EnumerateMetafile(Metafile metafile, PointF destPoint, RectangleF srcRect, GraphicsUnit srcUnit, Graphics.EnumerateMetafileProc callback)
void EnumerateMetafile(Metafile metafile, PointF destPoint, RectangleF srcRect, GraphicsUnit srcUnit, Graphics.EnumerateMetafileProc callback, nint callbackData)
void EnumerateMetafile(Metafile metafile, PointF destPoint, RectangleF srcRect, GraphicsUnit unit, Graphics.EnumerateMetafileProc callback, nint callbackData, ImageAttributes? imageAttr)
void EnumerateMetafile(Metafile metafile, PointF[] destPoints, Graphics.EnumerateMetafileProc callback)
void EnumerateMetafile(Metafile metafile, PointF[] destPoints, Graphics.EnumerateMetafileProc callback, nint callbackData)
void EnumerateMetafile(Metafile metafile, PointF[] destPoints, Graphics.EnumerateMetafileProc callback, nint callbackData, ImageAttributes? imageAttr)
void EnumerateMetafile(Metafile metafile, PointF[] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, Graphics.EnumerateMetafileProc callback)
void EnumerateMetafile(Metafile metafile, PointF[] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, Graphics.EnumerateMetafileProc callback, nint callbackData)
void EnumerateMetafile(Metafile metafile, PointF[] destPoints, RectangleF srcRect, GraphicsUnit unit, Graphics.EnumerateMetafileProc callback, nint callbackData, ImageAttributes? imageAttr)
void EnumerateMetafile(Metafile metafile, Point[] destPoints, Graphics.EnumerateMetafileProc callback)
void EnumerateMetafile(Metafile metafile, Point[] destPoints, Graphics.EnumerateMetafileProc callback, nint callbackData)
void EnumerateMetafile(Metafile metafile, Point[] destPoints, Graphics.EnumerateMetafileProc callback, nint callbackData, ImageAttributes? imageAttr)
void EnumerateMetafile(Metafile metafile, Point[] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, Graphics.EnumerateMetafileProc callback)
void EnumerateMetafile(Metafile metafile, Point[] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, Graphics.EnumerateMetafileProc callback, nint callbackData)
void EnumerateMetafile(Metafile metafile, Point[] destPoints, Rectangle srcRect, GraphicsUnit unit, Graphics.EnumerateMetafileProc callback, nint callbackData, ImageAttributes? imageAttr)
void EnumerateMetafile(Metafile metafile, Rectangle destRect, Graphics.EnumerateMetafileProc callback)
void EnumerateMetafile(Metafile metafile, Rectangle destRect, Graphics.EnumerateMetafileProc callback, nint callbackData)
void EnumerateMetafile(Metafile metafile, Rectangle destRect, Graphics.EnumerateMetafileProc callback, nint callbackData, ImageAttributes? imageAttr)
void EnumerateMetafile(Metafile metafile, Rectangle destRect, Rectangle srcRect, GraphicsUnit srcUnit, Graphics.EnumerateMetafileProc callback)
void EnumerateMetafile(Metafile metafile, Rectangle destRect, Rectangle srcRect, GraphicsUnit srcUnit, Graphics.EnumerateMetafileProc callback, nint callbackData)
void EnumerateMetafile(Metafile metafile, Rectangle destRect, Rectangle srcRect, GraphicsUnit unit, Graphics.EnumerateMetafileProc callback, nint callbackData, ImageAttributes? imageAttr)
void EnumerateMetafile(Metafile metafile, RectangleF destRect, Graphics.EnumerateMetafileProc callback)
void EnumerateMetafile(Metafile metafile, RectangleF destRect, Graphics.EnumerateMetafileProc callback, nint callbackData)
void EnumerateMetafile(Metafile metafile, RectangleF destRect, Graphics.EnumerateMetafileProc callback, nint callbackData, ImageAttributes? imageAttr)
void EnumerateMetafile(Metafile metafile, RectangleF destRect, RectangleF srcRect, GraphicsUnit srcUnit, Graphics.EnumerateMetafileProc callback)
void EnumerateMetafile(Metafile metafile, RectangleF destRect, RectangleF srcRect, GraphicsUnit srcUnit, Graphics.EnumerateMetafileProc callback, nint callbackData)
void EnumerateMetafile(Metafile metafile, RectangleF destRect, RectangleF srcRect, GraphicsUnit unit, Graphics.EnumerateMetafileProc callback, nint callbackData, ImageAttributes? imageAttr)
```

#### HDC / HWND handles — 9 methods

```
static Graphics FromHdc(nint hdc)
static Graphics FromHdc(nint hdc, nint hdevice)
static Graphics FromHdcInternal(nint hdc)
static Graphics FromHwnd(nint hwnd)
static Graphics FromHwndInternal(nint hwnd)
nint GetHdc()
void ReleaseHdc()
void ReleaseHdc(nint hdc)
void ReleaseHdcInternal(nint hdc)
```

#### Screen capture — 4 methods

```
void CopyFromScreen(Point upperLeftSource, Point upperLeftDestination, Size blockRegionSize)
void CopyFromScreen(Point upperLeftSource, Point upperLeftDestination, Size blockRegionSize, CopyPixelOperation copyPixelOperation)
void CopyFromScreen(int sourceX, int sourceY, int destinationX, int destinationY, Size blockRegionSize)
void CopyFromScreen(int sourceX, int sourceY, int destinationX, int destinationY, Size blockRegionSize, CopyPixelOperation copyPixelOperation)
```

#### Metafile comment — 1 method

```
void AddMetafileComment(byte[] data)
```

#### Other — 2 methods

```
static nint GetHalftonePalette()
object GetContextInfo()
```

---

### Font (8 methods)

Windows GDI font handle interop — requires HFONT, LOGFONT, and HDC
structures that do not exist cross-platform.

```
static Font FromHdc(nint hdc)
static Font FromHfont(nint hfont)
static Font FromLogFont(object lf)
static Font FromLogFont(object lf, nint hdc)
nint ToHfont()
void ToLogFont(object logFont)
void ToLogFont(object logFont, Graphics graphics)
void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
```

---

### Image (6 methods)

GDI bitmap handles, multi-frame encoder operations, and serialization.

```
static Bitmap FromHbitmap(nint hbitmap)
static Bitmap FromHbitmap(nint hbitmap, nint hpalette)
EncoderParameters? GetEncoderParameterList(Guid encoder)
void SaveAdd(Image image, EncoderParameters? encoderParams)
void SaveAdd(EncoderParameters? encoderParams)
void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
```

---

### Bitmap (5 methods)

Windows GDI bitmap and icon handle interop.

```
static Bitmap FromHicon(nint hicon)
static Bitmap FromResource(nint hinstance, string bitmapName)
nint GetHbitmap()
nint GetHbitmap(Color background)
nint GetHicon()
```

---

### PrinterSettings (5 methods)

Windows DEVMODE printer handle interop.

```
nint GetHdevmode()
nint GetHdevmode(PageSettings pageSettings)
nint GetHdevnames()
void SetHdevmode(nint hdevmode)
void SetHdevnames(nint hdevnames)
```

---

### PageSettings (2 methods)

Windows DEVMODE printer handle interop.

```
void CopyToHdevmode(nint hdevmode)
void SetHdevmode(nint hdevmode)
```

---

### Icon (4 methods)

Windows HICON handle interop, Win32 resource extraction, and serialization.

```
nint Handle { get; }
static Icon FromHandle(nint handle)
static Icon? ExtractAssociatedIcon(string filePath)
void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
```

---

### BufferedGraphics / BufferedGraphicsContext (2 methods)

HDC-based device context rendering.

```
void BufferedGraphics.Render(nint targetDC)
BufferedGraphics BufferedGraphicsContext.Allocate(nint targetDC, Rectangle targetRectangle)
```

---

## Cross-Platform Alternatives

For features that can't be implemented natively, these libraries may help:

### Metafile / EMF / WMF
- **[Metafiles.Net](https://www.nuget.org/packages/Metafiles.Net/)** — pure
  managed .NET library for reading EMF and WMF files cross-platform. Could
  be integrated to provide basic metafile loading and rendering to our
  Graphics surface.

### Screen Capture (CopyFromScreen)
No cross-platform .NET library exists for screen capture. Platform-specific
approaches:
- **Windows:** P/Invoke `BitBlt` or use existing `Graphics.CopyFromScreen`
- **macOS:** P/Invoke `CGDisplayCreateImage` or shell to `screencapture`
- **Linux:** P/Invoke X11/XCB `XGetImage` or shell to `import` (ImageMagick)

A future `ICaptureProvider` interface could allow platform-specific
implementations to be plugged in.

### HDC / HWND Interop
These are fundamentally Windows concepts with no cross-platform equivalent.
Applications migrating from WinForms should:
- Replace `Graphics.FromHwnd()` with `Graphics.FromImage()` + render to bitmap
- Replace `GetHdc()` / `ReleaseHdc()` with direct SkiaSharp canvas access
- Replace `FromHdc()` with bitmap-based rendering

### Printing
The current PDF-based printing implementation covers most use cases. For
physical printer access on specific platforms:
- **macOS:** `NSPrintOperation` via platform bindings
- **Linux:** CUPS via `libcups` P/Invoke
- **Windows:** The original `System.Drawing.Common` can be used directly

### Font Handles (HFONT / LOGFONT)
These are GDI-specific structures. Applications should use font family name
and style instead of GDI handles. The `Font` class fully supports creation
from family name, size, and style across all platforms.
