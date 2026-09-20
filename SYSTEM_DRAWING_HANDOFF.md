# System.Drawing compatibility work: implementation and handoff

This document is the continuation guide for the
`SkiaSharp.Extended.Drawing.Common` work on pull request
[mono/SkiaSharp.Extended#388](https://github.com/mono/SkiaSharp.Extended/pull/388).
It records what was built, why the current design was chosen, how correctness
is measured, the important discoveries made during implementation, and what
remains for the next developer or agent.

The project is intentionally not described as complete. The public API surface
is present and much of the common drawing path is implemented, but a meaningful
set of pixel comparisons still fails. Those failures are retained as evidence
of incomplete compatibility rather than hidden with broad tolerances or
hardcoded output.

## Current snapshot

At the time this handoff was written:

| Item | State |
| --- | --- |
| Pull request | [#388](https://github.com/mono/SkiaSharp.Extended/pull/388), open as a draft |
| Feature branch | `mattleibow/system-drawing-wrapper` |
| Base branch | `main` |
| Last implementation commit before this document | `e4e2a3452` |
| Remote synchronization | Clean and synchronized with `origin/mattleibow/system-drawing-wrapper` before this document |
| Base branch drift | The feature branch was 3 commits behind `origin/main` when this document was written |
| Main library source | 159 C# files, approximately 16,825 lines |
| Shared drawing scenarios | 406 `[Fact]` scenarios in 43 feature files |
| Checked-in reference images | 816 PNG files |
| API compatibility | Strict ApiCompat validation against the official `netstandard2.0` reference assembly |
| Remaining unsupported API entries | 130 documented `PlatformNotSupportedException` stubs |

The latest locally recorded full pixel comparison passed 258 scenarios and
failed 148 scenarios. Run the commands in [Resume and verify](#resume-and-verify)
before treating those numbers as current; the checked-in image tree should also
be audited because it contains more image pairs than shared scenario methods.

The latest implementation fixes were:

- `CompositingMode.SourceCopy` and `SourceOver` mapping, including image draws.
- `PixelOffsetMode.Half` and `HighQuality` using a half-pixel canvas
  translation.
- Save/restore behavior for replacement clips.
- Partial pen transform, inset alignment, compound-array, and differing-cap
  behavior.
- A broad magic-number cleanup and expanded limitation documentation.

## Goal and definition of success

The goal is a cross-platform, SkiaSharp-backed implementation of the
`System.Drawing.Common` contract that lets existing drawing code migrate with
minimal source changes and produces output that is measurably close to real
GDI+.

There are two independent compatibility goals:

1. **API compatibility**: existing code should compile and link against the
   same public types and signatures.
2. **Rendering compatibility**: common drawing operations should produce
   effectively the same pixels as real `System.Drawing.Common` on Windows.

The user initially described the desired result as 99.9999% reproducible and
less than 1% pixel variation. The practical test policy distinguishes exact
solid output, non-antialiased geometry, and antialiased geometry because GDI+
and Skia use different rasterizers:

| Category | Current maximum differing-pixel ratio |
| --- | ---: |
| Solid fills and colors | 0.1% |
| Non-AA strokes and geometry | 0.5% |
| Explicit AA scenario categories | 5% |

Pixel comparisons additionally allow a total RGB-channel distance of 3 per
pixel, which represents the common case of a one-byte interpolation rounding
difference in each channel. This per-pixel threshold is separate from the
overall differing-pixel ratio.

These tolerances are not targets to weaken. They are diagnostic boundaries.
Do not raise them without explicit human approval after testing multiple
implementation approaches and documenting why a closer match is not feasible.

## Non-negotiable engineering rules

These rules were established during the work and should be preserved:

- Add a focused drawing scenario for every newly discovered rendering
  discrepancy.
- Cover complete finite enum sets, not representative samples. This is why all
  53 `HatchStyle` values and the relevant dash, cap, join, and rotate/flip
  values have scenarios.
- Add parameter combinations, transforms, widths, clipping, smoothing modes,
  and scale variants when they expose materially different behavior.
- Keep scenario files organized by feature. Do not create catch-all
  `Advanced`, `Extended`, or multi-thousand-line scenario files.
- Prefer a real implementation over making a test pass. A failing test with an
  honest limitation is better than a special-case output hack.
- Do not replace rendering with hardcoded expected pixels without explicit
  human approval.
- Do not relax category tolerances without explicit human approval.
- Do not copy GPL implementation code or data from Mono/libgdiplus, Wine, or
  other GPL projects. They may be studied to understand externally observable
  behavior, but their implementation cannot be incorporated.
- Hatch tiles must not be generated with repeated `SetPixel` calls. Use compact
  immutable pattern data and bulk pixel construction.
- Hatch foreground and background colors must remain dynamic.
- Use enums and named constants instead of unexplained numeric values.
- Update `source/SkiaSharp.Extended.Drawing.Common/KNOWN-LIMITATIONS.md` in the
  same change whenever a partial implementation is added, fixed, or discovered.
- Keep errors visible. Unsupported members throw a descriptive
  `PlatformNotSupportedException`; partial behavior must not silently pretend to
  be complete.
- Preserve the distinction between a rasterizer difference and an incomplete
  implementation. Document both, but prioritize fixing the latter.

## Package and assembly identity

The main project is:

```text
source/SkiaSharp.Extended.Drawing.Common/
```

Its package and normal assembly identity are both:

```text
SkiaSharp.Extended.Drawing.Common
```

Its root namespace is:

```text
System.Drawing
```

The project targets:

```text
netstandard2.0;net9.0;net10.0
```

The distinct assembly name is deliberate. Loading the official
`System.Drawing.Common` and the replacement under the same assembly identity
caused runtime and benchmark reference unification problems. ApiCompat instead
builds the replacement with:

```bash
-p:AssemblyName=System.Drawing.Common
```

for the compatibility comparison only.

This gives the project a normal package identity that can coexist with the
official assembly in validation and benchmark processes while preserving the
`System.Drawing` source namespace and contract.

Modern .NET supplies primitive types such as `Color`, `Point`, `PointF`,
`Rectangle`, `RectangleF`, `Size`, and `SizeF` through
`System.Drawing.Primitives`. Do not duplicate these types for current target
frameworks. In benchmark code they are intentionally used without an extern
alias, while assembly-specific types such as `Graphics`, `Bitmap`, and `Pen`
use the `Gdi` or `Skia` aliases.

## Repository layout

| Path | Purpose |
| --- | --- |
| `source/SkiaSharp.Extended.Drawing.Common/` | SkiaSharp-backed implementation |
| `source/SkiaSharp.Extended.Drawing.Common/KNOWN-LIMITATIONS.md` | Source of truth for unsupported and partial behavior |
| `tests/SkiaSharp.Extended.Drawing.Common.Tests/` | API, behavior, exception, and focused rendering tests |
| `tests/SkiaSharp.Extended.Drawing.Common.Scenarios/` | Drawing scenarios compiled against both implementations |
| `tests/SkiaSharp.Extended.Drawing.Common.ReferenceGenerator/` | Windows-only real GDI+ scenario generator |
| `tests/SkiaSharp.Extended.Drawing.Common.SkiaGenerator/` | Cross-platform Skia-backed scenario generator |
| `tests/SkiaSharp.Extended.Drawing.Common.PixelDiff/` | Decoded-pixel comparison and diff artifact generation |
| `tests/SkiaSharp.Extended.Drawing.Common.Scenarios.ReferenceImages/` | Checked-in `.gdi.png` and `.skia.png` images |
| `benchmarks/SkiaSharp.Extended.Drawing.Common.Benchmarks/` | BenchmarkDotNet comparison of GDI+ and Skia |
| `tools/api-baseline/` | Official API reference assembly and ApiCompat suppressions |
| `azure-pipelines-public.yml` | Main and Drawing.Common CI stages |
| `scripts/azure-pipelines-steps-prepare.yml` | Shared preparation with optional workload installation |

## Implementation architecture

### Core rendering

`Graphics` wraps an `SKCanvas` and is the central translation layer from
System.Drawing operations to Skia operations. It owns drawing state such as:

- world transforms;
- clipping;
- compositing mode;
- interpolation mode;
- smoothing mode;
- pixel offset mode;
- text rendering behavior;
- save/restore snapshots.

All drawing paths, including `DrawImage`, must apply the common graphics state.
Historically, image drawing bypassed part of this state, which made
`CompositingMode.SourceCopy` appear implemented for shapes but not images.

### Images and bitmaps

`Image` and `Bitmap` are backed by Skia images and bitmaps. Implemented behavior
includes common constructors, file and stream loading, PNG/JPEG/BMP/GIF/WEBP
encoding, pixel access, 32-bit `LockBits`, frame selection, cloning, and image
drawing.

Important boundaries:

- Direct `GetPixel`/`SetPixel` correctness must be tested separately from
  scaled-image sampling.
- Sub-byte `LockBits` formats are unsupported.
- EXIF/property-item behavior is not implemented.
- Encoder continuation APIs such as `SaveAdd` are unsupported.
- GDI handle conversion remains unsupported.

### Pens and strokes

`Pen` translates width, brush/color, dash style, join, alignment, caps,
compound arrays, and transforms into Skia paint or path operations.

The remaining pen work is significant because GDI+ exposes behavior that
`SKPaint` does not directly model:

- different start and end caps;
- anchor caps rendered as endpoint geometry;
- separate dash caps;
- full affine pen transforms;
- exact inset alignment;
- exact compound stroke bands.

Avoid treating these as simple enum mappings. Several require drawing the line
body and endpoint geometry separately or converting strokes to fill paths.

### Brushes

Implemented brushes include:

- `SolidBrush`;
- `TextureBrush`;
- `LinearGradientBrush`;
- `PathGradientBrush`;
- `HatchBrush`;
- named `Brushes` and system brushes.

`PathGradientBrush` currently uses a clipped `SKVertices` triangle fan. It
supports a center color and boundary colors, including concave paths after
flattening, but the interpolation model is not equivalent to GDI+.

`HatchBrush` supports all 53 standard styles with repeating 8x8 tiles and
dynamic foreground/background colors. Diagonal patterns remain an important
rasterization mismatch because GDI+ antialiases diagonal tile strokes while
the current tiles are binary.

### Paths, regions, and transforms

`GraphicsPath` wraps `SKPath` and implements common line, rectangle, ellipse,
arc, pie, polygon, Bezier, curve, text, flatten, widen, outline visibility, and
path-data operations.

`Region` uses Skia path operations for union, intersection, exclusion,
complement, and XOR. Region serialization and scan decomposition are not
implemented.

`Matrix` wraps `SKMatrix`, including `MatrixOrder` behavior.

### Fonts and text

Fonts are backed by Skia typefaces and fonts. Drawing, measurement, word
wrapping, and text-to-path behavior are implemented. Exact glyph rasterization
cannot match Windows GDI+ because the font engines differ. Character-level
ellipsis trimming is incomplete.

### Printing

The `System.Drawing.Printing` event model is implemented with PDF output through
`SKDocument`. It is not a physical printer spooler:

- page settings, margins, landscape, and multi-page PDF output work;
- installed-printer discovery, HDEVMODE/HDNAMES, collation, duplex control, and
  native spooler integration do not.

## API generation and compatibility validation

The original public surface was generated from the official
`System.Drawing.Common` reference assembly using
`Microsoft.DotNet.GenAPI.Tool`. The generated output was split into one source
file per type, repaired for nullable metadata and generated signature issues,
and then implemented incrementally.

Strict compatibility is checked with:

```bash
dotnet tool restore
dotnet build \
  source/SkiaSharp.Extended.Drawing.Common/SkiaSharp.Extended.Drawing.Common.csproj \
  -c Release \
  -p:AssemblyName=System.Drawing.Common

dotnet apicompat \
  --left tools/api-baseline/netstandard2.0/System.Drawing.Common.dll \
  --right source/SkiaSharp.Extended.Drawing.Common/bin/Release/netstandard2.0/System.Drawing.Common.dll \
  --strict-mode \
  --suppression-file tools/api-baseline/api-compat-suppressions.xml
```

The baseline DLL is checked in under `tools/api-baseline/netstandard2.0/`.
The suppression file is for intentional assembly identity/public-key
differences, not missing API.

API compatibility means every public member exists with the correct signature.
It does not mean every member is implemented. Unsupported members are listed in
`KNOWN-LIMITATIONS.md`.

## Pixel validation architecture

The most important test design decision was to compile the exact same scenario
source twice instead of creating a custom drawing abstraction:

1. `ReferenceGenerator` compiles the scenario files against the official
   `System.Drawing.Common` package and runs on Windows.
2. `SkiaGenerator` compiles the same files against
   `SkiaSharp.Extended.Drawing.Common`.
3. Each scenario writes a PNG named `{scenario}.gdi.png` or
   `{scenario}.skia.png`.
4. `PixelDiff` decodes both files and compares pixels.
5. On failure it saves the GDI image, Skia image, and a red difference mask.

The generator projects deliberately do not compare PNG bytes. Different PNG
encoders may produce different byte streams for identical pixels.

The shared scenario base uses:

- `SCENARIO_OUTPUT_PATH` to select generated output;
- `REFERENCE_IMAGES_PATH` to locate checked-in or downloaded images;
- the consuming project's partial `ScenarioConfig` to select the `gdi` or
  `skia` suffix.

### Adding a scenario

1. Add a focused `[Fact]` to the appropriate category file under
   `tests/SkiaSharp.Extended.Drawing.Common.Scenarios/`.
2. If the category is already large, create a precise feature category rather
   than an `Advanced` or `Extended` catch-all.
3. Generate the Skia image locally.
4. Let Windows CI generate the GDI image from the same source.
5. Download the latest artifacts and update both checked-in images.
6. Run `PixelDiff`.
7. If the test fails, keep it failing until the implementation is corrected or
   the limitation is explicitly accepted and documented.

For complete enums, add the complete set. For geometry, include meaningful
integer and fractional coordinates, widths, smoothing modes, transforms, and
the existing 0.5x/1x/2x scale structure where relevant.

## CI and artifacts

`azure-pipelines-public.yml` has two independent stages:

### Main stage

- build;
- pack;
- normal tests.

### Drawing.Common stage

- strict API compatibility;
- Windows GDI+ image generation;
- Skia image generation;
- merged pixel comparison;
- GDI+ versus Skia BenchmarkDotNet runs.

Drawing.Common jobs pass `installWorkloads: false` to the shared preparation
template. They do not need MAUI, WASM, Java, or mobile workloads.

The important published artifacts are:

| Artifact | Contents |
| --- | --- |
| `gdi-images` | Fresh Windows GDI+ scenario images |
| `skia-images` | Fresh images from the replacement |
| `diff-images` | Difference masks and failing input pairs |
| `benchmarks` | JSON, CSV, and Markdown BenchmarkDotNet reports |

### Updating checked-in reference images

1. Use artifacts from the same successful pipeline run.
2. Download both `gdi-images` and `skia-images`.
3. Preserve each file's category-relative path.
4. Replace the corresponding files under
   `tests/SkiaSharp.Extended.Drawing.Common.Scenarios.ReferenceImages/`.
5. Do not copy an artifact root directory as another nested folder.
6. Check for stale categories and orphaned image pairs before committing.
7. Run both the Skia generator and PixelDiff locally.
8. Review unexpected image changes visually; do not blindly accept all
   generated output.

The reference image tree should currently be audited because 816 PNGs imply
408 pairs while the shared source contains 406 scenario methods.

## Benchmark architecture

The benchmark project references:

- the official `System.Drawing.Common` package with the `Gdi` alias;
- the replacement project with the `Skia` alias;
- shared framework primitives such as `Color` and `Rectangle` without aliases.

It covers fills, strokes, paths, text, and images. Run:

```bash
dotnet run -c Release \
  --project benchmarks/SkiaSharp.Extended.Drawing.Common.Benchmarks/ \
  -- --filter "*" --exporters json csv markdown --job short
```

The authoritative numbers are the `benchmarks` artifact from a current Windows
CI run. No benchmark result is embedded in this handoff because the last
verified numeric report was not preserved in the repository. Do not quote old
numbers from conversation memory; download the current artifact and update the
pull request description with the exact environment and table.

## Important compatibility discoveries

### GDI+ curve and polygon pixel centers

GDI+ curve and polygon rasterization frequently behaves as if coordinates are
shifted by `+0.5` pixels relative to the equivalent Skia operation.
Brute-force testing offsets from -1 to +1 found that this change reduced
several non-AA geometry errors from roughly 2-4% to about 0.08%.

The implementation uses `GdiCurveRect()` and `GdiPolygonPath()` helpers.

This is not a global coordinate rule. Ordinary lines and rectangles do not
uniformly need the shift. Test each primitive and smoothing mode rather than
adding a canvas-wide offset.

### PixelOffsetMode uses the opposite canvas translation

`PixelOffsetMode.Half` and `HighQuality` are represented by a `-0.5, -0.5`
canvas translation. This state must survive save/restore exactly once; do not
double-apply it while restoring logical and native state.

### Save/restore needs logical and native clip boundaries

Skia canvas save/restore handles native clip state, but the wrapper also tracks
clip scopes. A replacement clip previously restored through the user-visible
`Graphics.Save()` boundary. Keep the native canvas stack and wrapper state
stack aligned.

### SourceCopy must reach every rendering path

`CompositingMode.SourceCopy` maps to `SKBlendMode.Src`;
`CompositingMode.SourceOver` maps to `SKBlendMode.SrcOver`. Every shape, text,
path, and image operation must use the shared graphics state.

### Gradient rounding and interpolation are separate problems

Linear gradients often differ by one byte per channel because GDI+ and Skia
round interpolated floating-point colors differently. The per-pixel RGB
tolerance handles this narrow case.

Path gradients are not a rounding problem. GDI+ interpolates outward along
rays from the center; an `SKVertices` triangle fan performs barycentric
interpolation inside each triangle. Multi-color and irregular paths can differ
substantially even when every input color and boundary point is correct.

### Image interpolation differs at sample boundaries

Image drawing uses `SKSamplingOptions`. GDI+ and Skia still choose different
sample footprints at boundaries, especially when enlarging tiny,
high-frequency checkerboards. Large mismatch percentages in those tests do not
necessarily indicate incorrect source pixels. Keep raw pixel access and scaled
image tests separate.

### Cardinal spline tension is empirical

The current cardinal spline conversion uses `tension * 0.3`, matching measured
GDI+-compatible behavior also observed in compatibility implementations, rather
than the mathematically conventional `tension / 3`.

## What is implemented

The following areas have substantial working implementations:

- image and bitmap creation, decoding, encoding, cloning, pixels, and 32-bit
  locking;
- graphics creation from images;
- clear, line, rectangle, ellipse, arc, pie, polygon, Bezier, spline, and path
  drawing;
- solid, texture, linear-gradient, path-gradient, and hatch brushes;
- all 53 hatch styles;
- pen widths, colors/brushes, common dashes, common caps, joins, alignment,
  compound arrays, and partial transforms;
- transforms and matrix order;
- all six clip combine modes;
- graphics state save/restore;
- compositing and pixel-offset modes;
- image drawing and color matrices;
- fonts, text drawing, measurement, wrapping, and text paths;
- graphics paths, flattening, widening, visibility, and path data;
- regions and Boolean operations;
- icons with limited format support;
- system colors, brushes, pens, and fonts;
- PDF-backed printing and print preview;
- exception behavior for many historically unusual GDI+ cases;
- type converters and data/property classes sufficient for API presence, with
  several designer conversions still unsupported.

## Known incomplete areas

`KNOWN-LIMITATIONS.md` is the detailed source of truth. The highest-impact
remaining areas are:

1. **Image interpolation**
   - Nearest-neighbor and bicubic behavior do not yet match GDI+ sampling.
   - High-frequency upscaling exposes large boundary differences.

2. **Pen semantics**
   - Full affine pen transforms are not reproduced.
   - Differing start/end caps, anchor caps, and dash caps need explicit
     endpoint or segment geometry.
   - Inset and compound strokes are approximations.

3. **PathGradientBrush**
   - The triangle fan is barycentric rather than ray-based.
   - `Blend`, `InterpolationColors`, `FocusScales`, and `WrapMode` are not
     fully applied.

4. **Hatch antialiasing**
   - Diagonal binary tiles do not reproduce GDI+'s antialiased diagonal hatch
     pixels.

5. **Text**
   - Font engines, hinting, and ClearType differ.
   - Character-level ellipsis trimming is incomplete.

6. **ImageAttributes**
   - Color matrices are applied.
   - Gamma, threshold, and color-key values are stored but not applied.

7. **GraphicsPath and Region**
   - `GraphicsPath.Warp`, region serialization, and scan decomposition are
     unsupported.

8. **Metafiles and Windows handles**
   - EMF/WMF, HDC/HWND/HBITMAP/HICON/HFONT operations, native screen capture,
     and native printing handles are unsupported.

9. **Physical printing**
   - PDF output works; platform spoolers do not.

The 130 documented unsupported entries are primarily Windows-handle,
metafile, printing-handle, screen-capture, and serialization APIs. Do not
replace those exceptions with no-op implementations.

## Last recorded pixel failures

The latest full local comparison recorded before this handoff had:

```text
Passed: 258
Failed: 148
Total:  406
```

Notable failures observed near the end of the work included:

- `InterpolationMode_NearestNeighbor` around 17%;
- `InterpolationMode_Bicubic` around 53%;
- `SaveRestore_SmoothingMode` around 1.36%;
- path-gradient multi-color and irregular-path scenarios around 25-65%;
- diagonal hatch styles around 37-63%;
- some combined-operation scenarios around 1-8%;
- scaled scenarios that repeat the underlying unscaled mismatch.

These values are diagnostic snapshots, not approved exceptions. Re-run the
suite and use the generated masks before working from them.

Recent fixes moved these scenarios from failing to passing:

- `Compositing_SourceCopy`, previously around 25%;
- `PixelOffset_Half`, previously around 2.2%;
- `PixelOffset_HighQuality`, previously around 2.2%;
- `SaveRestore_Clip`, previously around 9%.

## Recommended implementation priorities

Work in this order unless new evidence changes the impact:

1. **Image sampling**
   - Create tiny source images with diagnostic color grids.
   - Test source rectangles, destination rectangles, wrap behavior, and pixel
     offsets independently.
   - Map each `InterpolationMode` explicitly instead of grouping modes by
     quality name.

2. **Pen endpoints and dashes**
   - Separate the stroked line body from start/end cap rendering.
   - Add geometry helpers for each anchor cap.
   - Test horizontal, vertical, diagonal, transformed, and reversed paths.
   - Implement dash caps without changing ordinary line caps.

3. **Pen transforms**
   - Test anisotropic scale, rotation, shear, and matrix order.
   - Prefer stroke-to-fill geometry when Skia paint state cannot represent the
     GDI+ transformed pen.

4. **Save/restore smoothing behavior**
   - Reduce the existing focused failure before touching tolerances.

5. **Path gradients**
   - Prototype center-to-boundary ray parameterization or concentric rings.
   - Include blend positions and interpolation colors in the geometry.
   - Validate triangles first, then rectangles, concave paths, ellipses, and
     curves.

6. **Hatch diagonals**
   - Investigate antialiased tile construction that remains data-driven,
     efficient, and dynamically colored.
   - Do not copy GPL pattern data and do not special-case expected images.

7. **Remaining composites and scaled scenarios**
   - Fix the base primitive first when a scaled test inherits its discrepancy.

After structural rendering work, implement the stored-but-unused
`ImageAttributes` behaviors and character trimming.

## Approaches tried and lessons learned

### Rejected: a custom drawing abstraction for scenarios

An early scenario system wrapped both implementations behind a custom
`IDrawingSurface`. It was rejected because it tested the abstraction rather
than the actual System.Drawing API. The shared-source, dual-compilation design
is the correct architecture.

### Rejected: PNG byte comparison

PNG encoders differ. Decode images and compare pixels.

### Rejected: runtime hatch generation with SetPixel

Repeated `SetPixel` calls are slow and obscure the intended pattern data.
Use compact immutable patterns and bulk pixel copies.

### Rejected: copying compatibility implementation tables

Wine/Mono/libgdiplus can be useful behavioral references, but GPL code or data
cannot be incorporated. One intermediate hatch implementation was replaced
with independently measured GDI+ output data to remove this concern.

### Rejected: broad tolerance increases

Large errors often exposed real missing behavior: half-pixel placement,
compositing, clip restoration, sampling, caps, or interpolation. Tolerance
changes are not an implementation.

### Rejected: assuming one universal half-pixel rule

Curves and polygons benefited from `+0.5`; lines and rectangles did not
uniformly do so. Apply empirical rules narrowly.

### Rejected: identical assembly identities in one benchmark process

Referencing two assemblies both named `System.Drawing.Common` caused identity
and type-forwarding problems. The normal replacement assembly now has its own
identity, and ApiCompat overrides it only for validation.

## Conversation and implementation history

The work proceeded through these major phases:

1. **Planning and prior-art research**
   - Defined API and pixel compatibility as separate goals.
   - Reviewed existing wrappers and compatibility implementations.
   - Chose infrastructure-first implementation rather than manually adding
     APIs as needed.

2. **API surface generation**
   - Generated approximately 3,041 lines across 148 public types with GenAPI.
   - Split generated declarations into per-type files.
   - Repaired nullable metadata, delegates, constructors, and interfaces.
   - Reached strict ApiCompat parity apart from intentional identity details.

3. **Core Skia implementation**
   - Implemented bitmap, image, graphics, pen, brush, font, matrix, path,
     region, clipping, text, image, and printing foundations.
   - Added named colors, pens, brushes, fonts, image attributes, icons, and
     supporting property types.

4. **Behavior and exception compatibility**
   - Matched unusual GDI+ exception behavior for invalid bitmap dimensions,
     invalid images, pixel bounds, negative pen widths, null text, immutable
     system objects, and empty point arrays.
   - Added focused unit and rendering-result tests.

5. **Shared scenario infrastructure**
   - Replaced the rejected abstraction design with shared C# scenario files.
   - Converted scenarios to xUnit `[Fact]` methods.
   - Added independent GDI and Skia generators plus decoded-pixel comparison.

6. **Coordinate compatibility**
   - Used brute-force offset experiments to discover GDI+'s curve/polygon
     half-pixel behavior.
   - Added narrow geometry helpers and reduced several large errors to
     near-exact matches.

7. **CI and benchmark integration**
   - Split normal and Drawing.Common validation into parallel stages.
   - Added API validation, generators, diff artifacts, and benchmarks.
   - Skipped unrelated workloads for Drawing.Common jobs.
   - Solved benchmark assembly identity and primitive type-forwarding issues.

8. **Coverage expansion**
   - Grew the shared scenario suite to 406 scenarios.
   - Added full hatch, dash, cap, join, and rotate/flip enum coverage.
   - Added transform, scale, stroke-width, clipping, gradient, image, text, and
     combined-operation coverage.
   - Consolidated temporary catch-all scenario files into feature categories.

9. **Hatch and gradient investigation**
   - Replaced slow hatch construction with compact data and bulk image
     creation.
   - Explored Skia sampling options, floating-point color paths, and gradient
     color spaces.
   - Replaced a radial path-gradient approximation with a clipped triangle
     fan, improving shape/color behavior while exposing the interpolation-model
     gap.

10. **Pen, magic-number, and limitation review**
    - Audited the codebase for unexplained numeric values.
    - Added partial transform, alignment, cap, and compound-stroke support.
    - Expanded the known-limitations document rather than describing
      approximations as complete.

11. **Graphics state fixes**
    - Corrected source-copy compositing.
    - Applied graphics state to images.
    - Implemented half-pixel pixel-offset modes.
    - Fixed clip scope restoration.

## Important commits

This list identifies useful investigation points rather than every branch
commit:

| Commit | Change |
| --- | --- |
| `e5dc72fd4` | Initial GenAPI-generated project |
| `2650016d6` | Build fixes and initial zero-diff ApiCompat |
| `af3ea425b` | Image and Bitmap implementation |
| `3ce970af6` | Initial Graphics implementation |
| `c022023ae` | GDI+ curve half-pixel discovery/fix |
| `ad2d67544` | Polygon vertex half-pixel fix |
| `dde322e13` | Three-step pixel validation pipeline |
| `e4346dc16` | Fixes from independent code reviews |
| `459b7e5c0` | Exception behavior fixes and tests |
| `83a04e05d` | Path, hatch, and clipping expansion |
| `0e71835e7` | Rename to SkiaSharp.Extended.Drawing.Common |
| `4e96e3dc5` | Distinct normal assembly identity |
| `655605380` | ApiCompat assembly-name override |
| `aec0a653f` | Large scenario coverage expansion |
| `535de3a7b` | Complete enum scenario coverage |
| `545f70daf` | Per-pixel interpolation rounding tolerance |
| `51ed4fc3f` | Triangle-fan PathGradientBrush |
| `24d107294` | RotateFlip decoding and axis fix |
| `4b89c0cc7` | Magic-number cleanup |
| `226b89003` | Pen transform/alignment/compound/cap work |
| `d75144525` | Full compromise documentation |
| `e4e2a3452` | Compositing, PixelOffsetMode, and clip restore fixes |

Use `git log origin/main..HEAD` for the complete chronological history.

## Resume and verify

Start by confirming the worktree and remotes:

```bash
git fetch origin --prune
git status --short --branch
git rev-list --left-right --count \
  origin/mattleibow/system-drawing-wrapper...HEAD
git rev-list --left-right --count origin/main...HEAD
```

Restore repository tools:

```bash
dotnet tool restore
```

Build the library:

```bash
dotnet build \
  source/SkiaSharp.Extended.Drawing.Common/SkiaSharp.Extended.Drawing.Common.csproj \
  -c Release
```

Run focused unit tests:

```bash
dotnet test tests/SkiaSharp.Extended.Drawing.Common.Tests/
```

Generate Skia scenario images:

```bash
dotnet test tests/SkiaSharp.Extended.Drawing.Common.SkiaGenerator/
```

Compare checked-in GDI and Skia images:

```bash
dotnet test \
  tests/SkiaSharp.Extended.Drawing.Common.PixelDiff/ \
  --filter "FullyQualifiedName~PixelDiff"
```

Run strict API validation:

```bash
dotnet build \
  source/SkiaSharp.Extended.Drawing.Common/SkiaSharp.Extended.Drawing.Common.csproj \
  -c Release \
  -p:AssemblyName=System.Drawing.Common

dotnet apicompat \
  --left tools/api-baseline/netstandard2.0/System.Drawing.Common.dll \
  --right source/SkiaSharp.Extended.Drawing.Common/bin/Release/netstandard2.0/System.Drawing.Common.dll \
  --strict-mode \
  --suppression-file tools/api-baseline/api-compat-suppressions.xml
```

For the repository-level CI equivalent:

```bash
dotnet cake
```

The ReferenceGenerator requires Windows and should normally be run through CI.

## Before the next implementation change

1. Read the relevant scenario file and `KNOWN-LIMITATIONS.md`.
2. Inspect the GDI image, Skia image, and red diff mask.
3. Reduce the case to a small focused scenario if the current one combines
   multiple behaviors.
4. Determine whether the mismatch is geometry, state, sampling, color
   interpolation, antialiasing, or an unsupported semantic.
5. Try at least two plausible implementation approaches when the mismatch is
   architectural.
6. Add or retain tests before editing tolerances.
7. Update limitations in the same commit.
8. Run the smallest relevant unit/scenario selection, then the full Skia
   generator and PixelDiff suite.
9. Download new GDI artifacts when scenario source changes.
10. Review generated image changes before committing.

## Pull request completion checklist

Before taking the pull request out of draft:

- Rebase or merge the current `main` branch and resolve any project/CI changes.
- Run the full local build and non-Windows validation.
- Run a fresh Windows Drawing.Common pipeline.
- Download and commit current GDI and Skia artifacts.
- Remove orphan reference image pairs.
- Record the fresh passed/failed pixel totals and category breakdown.
- Download current benchmark reports and add exact numbers to the PR body.
- Ensure ApiCompat is strict and has only intentional suppressions.
- Ensure `KNOWN-LIMITATIONS.md` matches the implementation.
- Review every tolerance change in branch history and confirm it is still
  justified.
- Keep the PR draft while structural pixel failures remain unless the
  maintainer explicitly chooses an incremental release strategy.

## Final orientation

The repository now contains a broad System.Drawing-compatible API, a substantial
Skia-backed implementation, and the validation infrastructure needed to improve
it methodically. The key asset is not only the current implementation: it is the
ability to compile the same drawing programs against real GDI+ and the
replacement, preserve both outputs, and turn every mismatch into a focused,
repeatable engineering problem.

Continue from evidence. Keep failures visible, improve the implementation
before tolerances, and document every unavoidable compromise.
