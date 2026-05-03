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
- ❌ Multi-frame images (animated GIF) — `SelectActiveFrame` decodes individual frames; `ImageAnimator` basic stub only
- ❌ Indexed pixel formats (8bpp, 4bpp, 1bpp) — converted to 32bpp on load

### Partial Implementations

These features have basic implementations with known gaps:

**HatchBrush:**
Renders all 53 standard hatch patterns as tiled 8×8 pixel bitmaps using
`SKShader.CreateImage()` with `SKShaderTileMode.Repeat`. Patterns include
lines, diagonals, cross-hatches, percent fills, checkerboards, bricks,
and more. Unrecognized styles fall back to a cross pattern.

**PathGradientBrush:**
Approximated using `SKShader.CreateRadialGradient()`. Does not perfectly
match GDI+'s path gradient algorithm for non-circular paths.

**GraphicsPath:**
- ✅ AddLine, AddRectangle, AddEllipse, AddArc, AddBezier, AddPolygon, AddPath, AddPie
- ✅ `AddCurve` / `AddClosedCurve` — cardinal spline curves on path
- ✅ `AddString` — text outlines via `SKFont.GetTextPath()`
- ✅ `Flatten()` — De Casteljau subdivision of curves to line segments
- ✅ `Widen(Pen)` — stroke to fill conversion via `SKPaint.GetFillPath()`
- ✅ `IsOutlineVisible()` — hit testing on widened stroke path
- ✅ `PathData` — returns points and types arrays
- ❌ `Warp()` — perspective/bilinear warp (complex, no SkiaSharp equivalent)

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

`SetClip()` supports all six combine modes:
- ✅ `Replace`, `Intersect`, `Exclude` — direct SKCanvas clip operations
- ✅ `Union`, `Xor`, `Complement` — computed via `SKPath.Op()`, then re-applied as a clip path

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

## Remaining Unimplemented APIs (130 stubs)

All remaining `PlatformNotSupportedException` stubs fall into these categories:

| Category | Count | Reason |
|----------|-------|--------|
| EnumerateMetafile | 36 | EMF/WMF record playback |
| Metafile class | 46 | EMF/WMF format construction |
| HDC/HWND (Graphics) | 10 | Windows device context handles |
| CopyFromScreen | 4 | Platform-specific screen capture |
| Font GDI handles | 8 | FromHfont/ToHfont/ToLogFont/FromHdc |
| Bitmap GDI handles | 5 | GetHbitmap/GetHicon/FromHicon |
| Printing GDI handles | 7 | GetHdevmode/SetHdevmode |
| Image | 6 | FromHbitmap, serialization, encoder params |
| Icon handles | 4 | Handle, FromHandle, ExtractAssociatedIcon |
| BufferedGraphics | 2 | HDC-based device context rendering |
| Other | 2 | GetHalftonePalette, GetContextInfo |

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
