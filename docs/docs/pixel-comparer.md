# Pixel Comparer

> [!TIP]
> **[Try it live →](../sample/pixelcomparer)** — Interactive Pixel Comparer demo in the Blazor sample app.

Pixel Comparer lets you compare two images pixel by pixel and quantify their differences. It's ideal for visual regression testing, screenshot comparison in CI pipelines, and verifying that rendering output matches expected results.

## Quick Start

### Compare two images

```csharp
using SkiaSharp;
using SkiaSharp.Extended;

var result = SKPixelComparer.Compare("expected.png", "actual.png");

Console.WriteLine($"Total pixels: {result.TotalPixels}");
Console.WriteLine($"Error pixels: {result.ErrorPixelCount}");
Console.WriteLine($"Error percentage: {result.ErrorPixelPercentage:P2}");
Console.WriteLine($"Absolute error: {result.AbsoluteError}");
```

### Generate a difference mask

```csharp
using var mask = SKPixelComparer.GenerateDifferenceMask("expected.png", "actual.png");

// Encode and save the mask
using var data = mask.Encode(SKEncodedImageFormat.Png, 100);
File.WriteAllBytes("diff-mask.png", data.ToArray());
```

The mask is a black-and-white image where **white pixels** indicate differences and **black pixels** indicate matching areas.

## How It Works

The comparer normalizes both images to BGRA8888 format, then walks through every pixel and sums the per-channel (red, green, blue) absolute differences:

1. For each pixel, compute `|R₁ − R₂| + |G₁ − G₂| + |B₁ − B₂|`
2. If the sum is greater than zero, that pixel is counted as an error
3. The total per-pixel sums are accumulated into `AbsoluteError`

Both images must have the same dimensions; otherwise an `InvalidOperationException` is thrown.

## Comparison Results

The [`SKPixelComparisonResult`](xref:SkiaSharp.Extended.SKPixelComparisonResult) contains:

| Property | Type | Description |
| :------- | :--- | :---------- |
| `TotalPixels` | `int` | Width × Height of the images |
| `ErrorPixelCount` | `int` | Number of pixels with any difference |
| `ErrorPixelPercentage` | `double` | `ErrorPixelCount / TotalPixels` |
| `AbsoluteError` | `int` | Sum of all per-channel differences |
| `SumSquaredError` | `long` | Sum of all per-channel squared differences |
| `MeanAbsoluteError` | `double` | Average absolute error per channel (range: 0–255) |
| `MeanSquaredError` | `double` | Average squared error per channel (range: 0–65025) |
| `RootMeanSquaredError` | `double` | Square root of MSE (range: 0–255) |
| `NormalizedRootMeanSquaredError` | `double` | RMSE divided by 255 (range: 0–1) |
| `PeakSignalToNoiseRatio` | `double` | PSNR in dB (∞ for identical images) |

The **RMSE** metric is commonly used in visual testing tools (such as .NET MAUI's `VisualTestUtils.MagickNet`) to quantify image differences as a single number. The **NormalizedRootMeanSquaredError** maps this to a 0–1 range, where 0 means identical and 1 means maximum difference—useful for threshold-based pass/fail testing.

## Mask-Based Comparison

When comparing images that have expected minor differences (e.g., anti-aliasing, compression artifacts), you can supply a tolerance mask. The mask image uses per-channel thresholds—a difference is only counted if it exceeds the corresponding channel value in the mask pixel:

```csharp
using var expected = SKImage.FromEncodedData("expected.png");
using var actual = SKImage.FromEncodedData("actual.png");
using var mask = SKImage.FromEncodedData("tolerance-mask.png");

var result = SKPixelComparer.Compare(expected, actual, mask);
```

For example, if a mask pixel has RGB values of `(10, 10, 10)`, differences of up to 10 per channel at that location are ignored. This lets you define region-specific tolerances.

## Tolerance-Based Comparison

For a simpler approach than mask-based comparison, you can specify a uniform per-pixel tolerance threshold (similar to ImageMagick's "fuzz" parameter). The tolerance is the maximum allowed sum of per-channel differences (`|ΔR| + |ΔG| + |ΔB|`) per pixel:

```csharp
// Ignore pixels where the total RGB difference is 10 or less
var result = SKPixelComparer.Compare(expected, actual, tolerance: 10);

Console.WriteLine($"Pixels exceeding tolerance: {result.ErrorPixelCount}");
```

A tolerance of `0` is equivalent to the standard comparison. The maximum possible per-pixel difference is 765 (255 × 3 channels).

## Input Overloads

All comparison methods accept multiple input types for convenience:

| Input Type | Example |
| :--------- | :------ |
| File paths | `Compare("a.png", "b.png")` |
| `SKImage` | `Compare(imageA, imageB)` |
| `SKBitmap` | `Compare(bitmapA, bitmapB)` |
| `SKPixmap` | `Compare(pixmapA, pixmapB)` |

The same overloads are available for `GenerateDifferenceMask` (without mask support) and for the three-argument masked `Compare`.

### Generate a difference image

Unlike the binary black-and-white mask, `GenerateDifferenceImage` produces a full-color visualization of per-channel differences:

```csharp
using var diff = SKPixelComparer.GenerateDifferenceImage("expected.png", "actual.png");

// Each pixel's R, G, B values represent the absolute channel differences
using var data = diff.Encode(SKEncodedImageFormat.Png, 100);
File.WriteAllBytes("diff-image.png", data.ToArray());
```

This is similar to the difference visualization produced by ImageMagick's `compare` command and is useful for understanding the magnitude and distribution of differences across channels.

## Usage Patterns

### Visual regression testing

Use the comparer in your test suite to catch unintended rendering changes:

```csharp
[Fact]
public void ChartRendering_MatchesBaseline()
{
    using var actual = RenderChart(testData);
    using var expected = SKImage.FromEncodedData("baselines/chart.png");

    var result = SKPixelComparer.Compare(expected, actual);

    Assert.Equal(0, result.ErrorPixelCount);
}
```

### CI screenshot comparison with tolerance

For tests that may have minor platform-specific differences:

```csharp
[Fact]
public void UI_MatchesBaseline_WithTolerance()
{
    using var actual = CaptureScreenshot();
    using var expected = SKImage.FromEncodedData("baselines/screen.png");

    // Option 1: Use per-pixel tolerance to ignore minor differences
    var result = SKPixelComparer.Compare(expected, actual, tolerance: 10);
    Assert.Equal(0, result.ErrorPixelCount);

    // Option 2: Use RMSE threshold (like .NET MAUI VisualTestUtils)
    var result2 = SKPixelComparer.Compare(expected, actual);
    Assert.True(result2.NormalizedRootMeanSquaredError < 0.005,
        $"NRMSE too high: {result2.NormalizedRootMeanSquaredError:F4}");
}
```

### Saving a difference mask for debugging

When a comparison fails, generate and save the mask to help diagnose what changed:

```csharp
if (result.ErrorPixelCount > 0)
{
    using var diffMask = SKPixelComparer.GenerateDifferenceMask(expected, actual);
    using var encoded = diffMask.Encode(SKEncodedImageFormat.Png, 100);
    File.WriteAllBytes("test-output/diff-mask.png", encoded.ToArray());
}
```

## Learn More

- [API Reference — SKPixelComparer](xref:SkiaSharp.Extended.SKPixelComparer) — Full method documentation
- [API Reference — SKPixelComparisonResult](xref:SkiaSharp.Extended.SKPixelComparisonResult) — Result class documentation
