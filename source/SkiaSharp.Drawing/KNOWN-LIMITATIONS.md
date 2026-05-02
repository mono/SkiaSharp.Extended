# SkiaSharp.Drawing — Known Limitations

This document describes the known differences and limitations between
`SkiaSharp.Drawing` and the official `System.Drawing.Common` (GDI+).

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

### Half-Pixel Coordinate Offset

GDI+ treats integer coordinates with a +0.5 pixel center offset for
curve rasterization (ellipses, arcs, pies). SkiaSharp does not.

**Mitigation:** SkiaSharp.Drawing applies the +0.5 offset automatically
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
  SkiaSharp.Drawing replicates this, but exact widths may differ slightly
- Line spacing calculations may produce different `Height` values
- ClearType subpixel rendering is not available in SkiaSharp

**Recommendation:** For text-heavy pixel comparison tests, use a higher
tolerance (2-5%) or compare at larger font sizes where rasterizer
differences are proportionally smaller.

### Gradient Interpolation

GDI+ interpolates gradients in sRGB color space. SkiaSharp can
interpolate in either sRGB or linear space. The default behavior may
produce slightly different color values in gradient midpoints.

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
- ❌ Multi-frame images (animated GIF) — `ImageAnimator` not implemented
- ❌ Indexed pixel formats (8bpp, 4bpp, 1bpp) — converted to 32bpp on load

### Partial Implementations

These features have basic implementations with known gaps:

**HatchBrush:**
Currently renders as a solid fill using the foreground color.
The 53 standard hatch patterns are not yet rendered as tiled bitmaps.

**PathGradientBrush:**
Approximated using `SKShader.CreateRadialGradient()`. Does not perfectly
match GDI+'s path gradient algorithm for non-circular paths.

**GraphicsPath:**
- ✅ AddLine, AddRectangle, AddEllipse, AddArc, AddBezier, AddPolygon, AddPath, AddPie
- ❌ `AddCurve` / `AddClosedCurve` — cardinal splines on path (use `Graphics.DrawCurve` instead)
- ❌ `AddString` — text outlines (needs font-to-path conversion)
- ❌ `Flatten()` — convert curves to line segments
- ❌ `Widen(Pen)` — stroke to fill conversion
- ❌ `Warp()` — perspective/bilinear warp
- ❌ `IsOutlineVisible()` — hit testing on stroke

**Region:**
- ✅ Boolean operations (Union, Intersect, Exclude, Complement, Xor)
- ✅ Visibility testing, bounds, transform
- ❌ `GetRegionData()` — serialization to byte array
- ❌ `GetRegionScans(Matrix)` — decomposition to rectangles

**Font:**
- ✅ All constructors, style mapping, size conversion
- ✅ `Name`, `Size`, `Style`, `Bold`, `Italic`, `Height`
- ✅ `GetHeight(Graphics)` — DPI-aware height calculation
- ❌ `ToLogFont()` — Windows LOGFONT structure
- ❌ `GdiCharSet`, `GdiVerticalFont` — GDI-specific properties

**TypeConverters:**
`FontConverter`, `ImageConverter`, `ImageFormatConverter`, `ColorConverter`,
`MarginsConverter`, `IconConverter` — most conversion methods throw PNSE.
These are primarily used by the Windows Forms designer and property grid.

### Image Processing

**ImageAttributes:**
Properties are stored and `SetColorMatrix()` is applied during rendering
via `SKColorFilter.CreateColorMatrix()`. `SetGamma()`, `SetThreshold()`,
`SetColorKey()` store values but are **not yet applied** during drawing.

**ColorMatrix:**
Stored as a 5×5 float matrix and applied to image rendering when set via
`ImageAttributes.SetColorMatrix()`.

### Clip Region CombineModes

`SetClip()` supports `Replace`, `Intersect`, and `Exclude` combine modes.
`Union`, `Xor`, and `Complement` modes throw `NotSupportedException` because
SKCanvas does not natively support these clip operations.

## API Compatibility

The assembly has **100% ABI compatibility** with `System.Drawing.Common`
validated by `dotnet apicompat --strict-mode`. Every public type, method,
property, and event exists with the correct signature.

Unimplemented methods throw `PlatformNotSupportedException` with a
descriptive message. This allows code that references System.Drawing APIs
to compile and link against SkiaSharp.Drawing, failing gracefully at
runtime only when unimplemented features are actually called.

## Performance Notes

- `GetPixel()` / `SetPixel()` are implemented but slow for bulk operations.
  Use `LockBits()` for direct pixel buffer access instead.
- `Graphics.FromImage()` creates an `SKCanvas` per call. Reuse the
  Graphics object when drawing multiple operations on the same image.
- `Brushes.*` and `Pens.*` factories use lazy initialization with caching.
  They are thread-safe for read access.
