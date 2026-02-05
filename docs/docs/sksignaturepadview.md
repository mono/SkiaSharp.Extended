# SKSignaturePadView

The signature pad view provides fluid, pressure-sensitive ink rendering for capturing signatures or handwritten input. It supports stylus/pen pressure for variable stroke width (harder pressure = thicker lines) and uses quadratic Bezier curves to smooth jagged input from finger or pen movement.

## Properties

The main properties of a signature pad view are:

| Property               | Type      | Description |
| :--------------------- | :-------- | :---------- |
| **StrokeColor**        | `Color`   | The color of the ink strokes. Default is `Black`. |
| **MinStrokeWidth**     | `float`   | The minimum stroke width at zero pressure. Default is `1`. |
| **MaxStrokeWidth**     | `float`   | The maximum stroke width at full pressure. Default is `8`. |
| **PadBackgroundColor** | `Color`   | The background color of the signature pad. Default is `White`. |
| **IsBlank**            | `bool`    | A read-only value indicating whether the pad has no strokes. |
| **StrokeCount**        | `int`     | The number of strokes on the signature pad. |

## Methods

The signature pad provides several methods for manipulating and exporting signatures:

| Method       | Return Type | Description |
| :----------- | :---------- | :---------- |
| **Clear()**  | `void`      | Clears all strokes from the signature pad. |
| **Undo()**   | `bool`      | Removes the last stroke. Returns `true` if a stroke was removed. |
| **ToPath()** | `SKPath?`   | Gets a combined path of all strokes. Returns `null` if blank. |
| **ToImage(int width, int height, SKColor? backgroundColor)** | `SKImage?` | Renders the signature to an image with the specified dimensions. |
| **GetStrokeBounds()** | `SKRect` | Gets the bounding rectangle of all strokes. |

## Events

| Event               | Type                                          | Description |
| :------------------ | :-------------------------------------------- | :---------- |
| **Cleared**         | `EventHandler`                                | Raised when all strokes are cleared. |
| **StrokeCompleted** | `EventHandler<SKSignatureStrokeCompletedEventArgs>` | Raised when a stroke is completed. |

## Commands

| Command                       | Description |
| :---------------------------- | :---------- |
| **ClearedCommand**            | Executed when strokes are cleared. |
| **ClearedCommandParameter**   | Parameter passed to the ClearedCommand. |
| **StrokeCompletedCommand**    | Executed when a stroke is completed. |
| **StrokeCompletedCommandParameter** | Parameter passed to the StrokeCompletedCommand. |

## Parts

The default template uses an SKCanvasView for rendering:

```xaml
<ControlTemplate x:Key="SKSignaturePadViewControlTemplate">
    <skia:SKCanvasView x:Name="PART_DrawingSurface" />
</ControlTemplate>
```

| Part                     | Description |
| :----------------------- | :---------- |
| **PART_DrawingSurface**  | This part can either be a `SKCanvasView` or a `SKGLView` and describes the actual rendering surface for the signature. |

## Usage

### Basic Usage

```xaml
<skia:SKSignaturePadView
    x:Name="signaturePad"
    HeightRequest="200"
    StrokeColor="DarkBlue"
    MinStrokeWidth="1"
    MaxStrokeWidth="10"
    PadBackgroundColor="Ivory" />
```

### With Commands

```xaml
<skia:SKSignaturePadView
    x:Name="signaturePad"
    HeightRequest="200"
    StrokeCompletedCommand="{Binding OnStrokeCompleted}"
    ClearedCommand="{Binding OnCleared}" />

<Button Text="Clear" Command="{Binding ClearCommand}" />
```

### Exporting Signature

```csharp
// Get the signature as an image
var image = signaturePad.ToImage(400, 200, SKColors.White);

// Get the signature as a path for vector export
var path = signaturePad.ToPath();

// Check if signature exists
if (!signaturePad.IsBlank)
{
    // Process signature
}
```

### Undo Support

```csharp
// Undo the last stroke
if (signaturePad.Undo())
{
    // Stroke was removed
}
```

## Pressure Sensitivity

The signature pad automatically uses pressure data from stylus/pen input when available:

- **No pressure data (finger touch)**: Uses a default pressure of 0.5, resulting in medium stroke width
- **Pressure data available**: Maps pressure (0.0-1.0) to stroke width between `MinStrokeWidth` and `MaxStrokeWidth`
- **Harder pressure**: Produces thicker lines
- **Lighter pressure**: Produces thinner lines

## Ink Rendering Algorithm

The signature pad uses a sophisticated algorithm for fluid ink rendering:

1. **Point Collection**: Touch points are collected with associated pressure values
2. **Minimum Distance Filtering**: Points too close together are filtered out to reduce noise
3. **Quadratic Bezier Smoothing**: Points are interpolated using quadratic Bezier curves for smooth paths
4. **Variable Width Calculation**: Stroke width is calculated at each point based on pressure
5. **Polygon Generation**: The stroke is rendered as a filled polygon with offset curves on each side
6. **Rounded Caps**: Start and end caps are added for natural ink appearance

This approach mimics the fluid ink rendering found in Windows Ink and other professional digital ink systems.
