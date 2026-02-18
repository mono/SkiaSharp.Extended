# SkiaSharp.Extended.Inking

A platform-independent digital inking engine for SkiaSharp that provides smooth, pressure-sensitive stroke rendering.

## Overview

The inking engine consists of three main components:

| Component | Description |
|:----------|:------------|
| **SKInkPoint** | A point with pressure and timestamp data |
| **SKInkStroke** | A single stroke with variable-width rendering based on pressure |
| **SKInkCanvas** | An engine for managing multiple strokes |

## Installation

```
dotnet add package SkiaSharp.Extended.Inking
```

## Basic Usage

### Creating an Ink Canvas

```csharp
using SkiaSharp.Extended.Inking;

// Create an ink canvas with stroke width range
var inkCanvas = new SKInkCanvas(minStrokeWidth: 2f, maxStrokeWidth: 10f);

// Subscribe to events
inkCanvas.Invalidated += (s, e) => Redraw();
inkCanvas.StrokeCompleted += (s, e) => Console.WriteLine($"Stroke {e.StrokeCount} completed");
```

### Drawing Strokes

```csharp
// Start a new stroke with pressure
inkCanvas.StartStroke(new SKPoint(100, 100), pressure: 0.5f);

// Continue the stroke as the user moves
inkCanvas.ContinueStroke(new SKPoint(150, 120), pressure: 0.7f);
inkCanvas.ContinueStroke(new SKPoint(200, 110), pressure: 0.4f);

// End the stroke
inkCanvas.EndStroke(new SKPoint(250, 130), pressure: 0.3f);
```

### Rendering

```csharp
// In your SkiaSharp paint handler
void OnPaintSurface(SKCanvas canvas)
{
    canvas.Clear(SKColors.White);
    
    using var paint = new SKPaint
    {
        Color = SKColors.DarkBlue,
        Style = SKPaintStyle.Fill,
        IsAntialias = true
    };
    
    inkCanvas.Draw(canvas, paint);
}
```

## SKInkPoint

Represents a single point in a stroke with pressure and timing information.

```csharp
// Create a point with location and pressure
var point = new SKInkPoint(100f, 200f, pressure: 0.6f);

// Create a point with timestamp (for recording/playback)
var timedPoint = new SKInkPoint(100f, 200f, pressure: 0.6f, timestamp: 1234567890);

// Access properties
float x = point.X;
float y = point.Y;
SKPoint location = point.Location;
float pressure = point.Pressure;  // 0.0 to 1.0
```

## SKInkStroke

A single stroke with pressure-sensitive variable-width rendering.

### Properties

| Property | Type | Description |
|:---------|:-----|:------------|
| **Points** | `IReadOnlyList<SKInkPoint>` | The points in the stroke |
| **PointCount** | `int` | Number of points |
| **IsEmpty** | `bool` | Whether the stroke has no points |
| **Path** | `SKPath?` | The rendered path (cached) |
| **Bounds** | `SKRect` | Bounding rectangle of the stroke |
| **MinStrokeWidth** | `float` | Width at zero pressure |
| **MaxStrokeWidth** | `float` | Width at full pressure |

### Rendering Algorithm

The stroke uses a sophisticated algorithm for smooth, variable-width rendering:

1. **Point Collection**: Touch points with pressure values
2. **Distance Filtering**: Points too close are filtered to reduce noise
3. **Bézier Smoothing**: Quadratic Bézier interpolation for smooth curves
4. **Variable Width**: Width calculated from pressure: `width = min + (max - min) * pressure`
5. **Polygon Rendering**: Filled polygon with offset curves on each side
6. **Rounded Caps**: Semicircular caps at start and end

![Pressure Sensitivity](../images/inking/pressure-demo.png)

## SKInkCanvas

Manages multiple strokes and provides undo/clear functionality.

### Properties

| Property | Type | Description |
|:---------|:-----|:------------|
| **Strokes** | `IReadOnlyList<SKInkStroke>` | Completed strokes |
| **CurrentStroke** | `SKInkStroke?` | Stroke being drawn |
| **StrokeCount** | `int` | Number of completed strokes |
| **IsBlank** | `bool` | Whether canvas has no strokes |
| **IsDrawing** | `bool` | Whether a stroke is in progress |
| **MinStrokeWidth** | `float` | Minimum stroke width |
| **MaxStrokeWidth** | `float` | Maximum stroke width |

### Methods

| Method | Description |
|:-------|:------------|
| **StartStroke(point)** | Begins a new stroke |
| **ContinueStroke(point)** | Adds a point to the current stroke |
| **EndStroke(point)** | Completes the current stroke |
| **CancelStroke()** | Cancels without saving |
| **Undo()** | Removes the last stroke |
| **Clear()** | Removes all strokes |
| **ToPath()** | Gets combined path of all strokes |
| **ToImage(w, h, color)** | Renders to an image |
| **GetBounds()** | Gets bounding rectangle |
| **Draw(canvas, paint)** | Draws all strokes |

### Events

| Event | Description |
|:------|:------------|
| **StrokeStarted** | Raised when a stroke begins |
| **StrokeCompleted** | Raised when a stroke is finished |
| **Cleared** | Raised when canvas is cleared |
| **Invalidated** | Raised when redraw is needed |

## Recording and Playback

The inking engine supports recording strokes and playing them back, which is useful for:
- Demonstrating signatures
- Tutorial animations
- Replaying user input

### Recording

```csharp
// Record from an existing canvas
var recording = SKInkRecording.FromCanvas(inkCanvas);

// Or create a sample signature
var sampleSignature = SKInkRecording.CreateSampleSignature(width: 400, height: 200);
```

### Playback

```csharp
// Create a player
var player = new SKInkPlayer();
player.PlaybackSpeed = 1.5f;  // 1.5x speed
player.Load(recording, targetCanvas);

// Start playback
player.Play();

// In your animation loop
while (player.IsPlaying)
{
    player.Update();
    Invalidate();
    await Task.Delay(16);  // ~60 FPS
}

// Or play instantly
player.PlayInstant();
```

## Integration with MAUI

For .NET MAUI applications, use the `SKSignaturePadView` control which wraps the inking engine:

```xaml
<skia:SKSignaturePadView
    x:Name="signaturePad"
    StrokeColor="DarkBlue"
    MinStrokeWidth="2"
    MaxStrokeWidth="10"
    PadBackgroundColor="Ivory" />
```

Access the underlying ink canvas:

```csharp
SKInkCanvas inkCanvas = signaturePad.InkCanvas;
```

See [SKSignaturePadView documentation](sksignaturepadview.md) for more details.

## Example: Custom Ink View

Here's an example of creating a custom ink view in SkiaSharp without MAUI:

```csharp
public class CustomInkView : SKCanvasView
{
    private readonly SKInkCanvas inkCanvas = new SKInkCanvas(2f, 10f);
    private readonly SKPaint inkPaint = new SKPaint
    {
        Color = SKColors.Black,
        Style = SKPaintStyle.Fill,
        IsAntialias = true
    };

    public CustomInkView()
    {
        inkCanvas.Invalidated += (s, e) => InvalidateSurface();
        EnableTouchEvents = true;
    }

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.White);
        inkCanvas.Draw(canvas, inkPaint);
    }

    protected override void OnTouch(SKTouchEventArgs e)
    {
        var point = e.Location;
        var pressure = e.Pressure > 0 ? e.Pressure : 0.5f;

        switch (e.ActionType)
        {
            case SKTouchAction.Pressed:
                inkCanvas.StartStroke(point, pressure);
                break;
            case SKTouchAction.Moved:
                inkCanvas.ContinueStroke(point, pressure);
                break;
            case SKTouchAction.Released:
                inkCanvas.EndStroke(point, pressure);
                break;
        }

        e.Handled = true;
    }
}
```

## Performance Considerations

- **Path Caching**: Stroke paths are cached and only regenerated when points change
- **Point Filtering**: Points too close together are filtered to reduce rendering load
- **Dispose**: Always dispose strokes and canvases when done to free SKPath resources

```csharp
// Proper disposal
using (var inkCanvas = new SKInkCanvas())
{
    // Use the canvas
} // Automatically disposed
```
