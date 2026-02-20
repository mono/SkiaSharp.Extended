# API Comparison: SkiaSharp.Extended.Inking vs Windows.UI.Input.Inking

This document provides a thorough comparison between our `SkiaSharp.Extended.Inking` API and Microsoft's `Windows.UI.Input.Inking` namespace, analyzing architecture, features, and alignment decisions.

## Executive Summary

| Aspect | SkiaSharp.Extended.Inking | Windows.UI.Input.Inking |
|--------|---------------------------|-------------------------|
| **Target Platform** | Cross-platform (netstandard2.0, net9.0) | Windows-only (UWP/WinUI) |
| **Architecture** | Single library, minimal classes | Large namespace, many specialized classes |
| **Learning Curve** | Low | Medium-High |
| **Feature Depth** | Core inking features | Full inking ecosystem |
| **Handwriting Recognition** | ❌ Not available | ✅ Built-in |
| **Pressure Sensitivity** | ✅ Full support | ✅ Full support |
| **Tilt Support** | ✅ Full support | ✅ Available |
| **Velocity Support** | ✅ Full support + modes | ⚠️ Limited |
| **Selection** | ✅ Rect selection | ✅ Line/polygon selection |
| **Serialization (ISF)** | ❌ Not available | ✅ Built-in |

**Conclusion**: Our API provides **feature parity** for core inking scenarios while being **simpler**, **cross-platform**, and **more flexible** with velocity-based effects.

---

## Architecture Comparison

### Windows.UI.Input.Inking Architecture

```
InkCanvas (XAML control)
    └── InkPresenter (input/rendering manager)
            └── StrokeContainer (InkStrokeContainer)
                    └── InkStrokes[] (stroke data)
                            └── InkPoints[] (point data)
                                    └── DrawingAttributes (appearance)

InkManager (alternative to InkPresenter - adds selection/recognition)
InkRecognizerContainer (handwriting recognition)
InkSynchronizer (custom rendering)
InkModelerAttributes (input prediction)
```

**Key Classes:**
| Class | Purpose |
|-------|---------|
| `InkCanvas` | XAML control for inking |
| `InkPresenter` | Manages input and rendering |
| `InkStrokeContainer` | Stores and manages strokes |
| `InkManager` | Adds selection, editing, recognition |
| `InkStroke` | Single stroke data |
| `InkPoint` | Single point with pressure/tilt |
| `InkDrawingAttributes` | Stroke appearance settings |
| `InkRecognizerContainer` | Handwriting recognition |

### SkiaSharp.Extended.Inking Architecture

```
SKInkCanvas (engine - platform independent)
    └── Brush (SKInkStrokeBrush - default settings)
    └── SKInkStrokes[] (stroke data + rendering)
            └── Brush (SKInkStrokeBrush - appearance)
            └── SKInkPoints[] (point data)

SKInkRecording / SKInkPlayer (playback support)
SKSignaturePadView (MAUI control - thin wrapper)
```

**Key Classes:**
| Class | Purpose |
|-------|---------|
| `SKInkCanvas` | Engine for managing strokes, selection, export |
| `SKInkStroke` | Stroke data + path generation |
| `SKInkStrokeBrush` | Appearance settings (color, size, cap, smoothing, velocity) |
| `SKInkPoint` | Point with pressure, tilt, velocity, timestamp |
| `SKInkRecording` | Recording/playback support |
| `SKInkPlayer` | Playback engine |
| `SKSignaturePadView` | MAUI control wrapper |

### Architectural Assessment

| Aspect | Our API | Windows API | Winner |
|--------|---------|-------------|--------|
| **Simplicity** | 8 classes | 15+ classes | ✅ Ours |
| **Cross-platform** | Yes | No | ✅ Ours |
| **Separation of concerns** | Engine + View separated | Tightly coupled | ✅ Ours |
| **Brush/Attributes design** | Separate cloneable class | Embedded in stroke | ✅ Ours |
| **Extensibility** | Easy to extend | Complex | ✅ Ours |
| **Feature completeness** | Core features | Comprehensive | ⚠️ Tie |

---

## Point Data Comparison

### InkPoint (Windows)

```csharp
public sealed class InkPoint
{
    // Constructors
    InkPoint(Point position, float pressure)
    InkPoint(Point position, float pressure, float tiltX, float tiltY, ulong timestamp)
    
    // Properties
    Point Position { get; }
    float Pressure { get; }     // 0.0 - 1.0
    float TiltX { get; }        // -90 to +90 degrees
    float TiltY { get; }        // -90 to +90 degrees
    ulong Timestamp { get; }    // Microseconds
}
```

### SKInkPoint (Ours)

```csharp
public readonly struct SKInkPoint : IEquatable<SKInkPoint>
{
    // Constructors
    SKInkPoint(SKPoint location, float pressure, long timestampMicroseconds = 0)
    SKInkPoint(float x, float y, float pressure, float tiltX, float tiltY, long timestampMicroseconds)
    
    // Properties
    SKPoint Location { get; }
    float X { get; }
    float Y { get; }
    float Pressure { get; }              // 0.0 - 1.0 (clamped)
    float TiltX { get; }                 // -90 to +90 degrees
    float TiltY { get; }                 // -90 to +90 degrees
    float Velocity { get; }              // Calculated px/ms (our addition)
    long TimestampMicroseconds { get; }  // Microseconds (aligned with Windows)
    
    // Methods
    static float CalculateVelocity(SKInkPoint from, SKInkPoint to)
    SKInkPoint WithVelocity(float velocity)
}
```

### Point Comparison Table

| Feature | Our API | Windows API | Notes |
|---------|---------|-------------|-------|
| Position | ✅ SKPoint | ✅ Point | Equivalent |
| Pressure | ✅ Clamped 0-1 | ✅ 0-1 | Ours validates |
| TiltX/TiltY | ✅ Full support | ✅ Available | ✅ Aligned |
| Velocity | ✅ Calculated | ❌ Not available | Ours better |
| Timestamp | ✅ Microseconds | ✅ Microseconds | ✅ Aligned |
| Struct vs Class | ✅ Struct | ❌ Class | Ours more efficient |
| IEquatable | ✅ Yes | ❌ No | Ours better |

**Our additions beyond Windows:**
- `Velocity` property for velocity-based stroke effects
- `CalculateVelocity()` static helper
- `WithVelocity()` immutable builder

---

## Stroke Appearance Comparison

### InkDrawingAttributes (Windows)

```csharp
public sealed class InkDrawingAttributes
{
    Color Color { get; set; }
    bool DrawAsHighlighter { get; set; }
    bool FitToCurve { get; set; }
    bool IgnorePressure { get; set; }
    bool IgnoreTilt { get; set; }
    PenTipShape PenTip { get; set; }  // Circle, Rectangle
    Matrix3x2 PenTipTransform { get; set; }
    Size Size { get; set; }           // Width x Height (fixed)
}
```

### SKInkStrokeBrush (Ours)

```csharp
public class SKInkStrokeBrush
{
    // Appearance
    SKColor Color { get; set; }
    SKSize MinSize { get; set; }           // Width at pressure 0
    SKSize MaxSize { get; set; }           // Width at pressure 1
    SKStrokeCapStyle CapStyle { get; set; } // Round, Flat, Tapered
    
    // Smoothing
    SKSmoothingAlgorithm SmoothingAlgorithm { get; set; } // CatmullRom, QuadraticBezier
    int SmoothingFactor { get; set; }      // 1-10
    
    // Velocity (unique to us)
    SKVelocityMode VelocityMode { get; set; }  // None, BallpointPen, Pencil
    float VelocityScale { get; set; }          // 0.0-1.0 effect strength
    
    // Methods
    SKInkStrokeBrush Clone()
    float GetWidthForPressure(float pressure)
    float GetWidthForPressureAndVelocity(float pressure, float velocity)
    SKColor GetColorForVelocity(float velocity)  // Alpha adjustment for Pencil mode
}
```

### Appearance Comparison Table

| Feature | Our API | Windows API | Notes |
|---------|---------|-------------|-------|
| Color | ✅ SKColor | ✅ Color | Equivalent |
| Stroke Width | ✅ MinSize/MaxSize range | ⚠️ Fixed Size | Ours better for pressure |
| Cap Style | ✅ Round/Flat/Tapered | ⚠️ PenTip shape | Tapered is unique |
| Smoothing | ✅ Algorithm + Factor | ⚠️ FitToCurve bool | Ours more flexible |
| Highlighter Mode | ❌ Missing | ✅ DrawAsHighlighter | Gap |
| Velocity Mode | ✅ None/BallpointPen/Pencil | ❌ Not available | Ours better |
| Velocity Scale | ✅ 0-1 effect strength | ❌ Not available | Ours better |
| Clone | ✅ Clone() | ⚠️ Manual | Ours simpler |
| Ignore Pressure | ⚠️ MinSize = MaxSize | ✅ IgnorePressure | Different approach |
| Pen Transform | ❌ Missing | ✅ Matrix3x2 | Gap |

### Velocity Modes (Our Unique Feature)

```csharp
public enum SKVelocityMode
{
    None,         // Velocity has no effect (pressure-only)
    BallpointPen, // Faster = thinner stroke (simulates ink flow)
    Pencil        // Faster = thinner AND lighter (simulates graphite)
}
```

This matches the Windows Ink "ballpoint pen" and "pencil" brush behaviors but exposes them as configurable options rather than separate brush types.

---

## Stroke Comparison

### InkStroke (Windows)

```csharp
public sealed class InkStroke
{
    // Properties
    Rect BoundingRect { get; }
    InkDrawingAttributes DrawingAttributes { get; set; }
    uint Id { get; }
    Matrix3x2 PointTransform { get; set; }
    bool Recognized { get; }
    bool Selected { get; set; }
    uint StrokeDuration { get; set; }
    DateTime StrokeStartedTime { get; set; }
    
    // Methods
    InkStroke Clone()
    IReadOnlyList<InkPoint> GetInkPoints()
    IReadOnlyList<InkStrokeRenderingSegment> GetRenderingSegments()
}
```

### SKInkStroke (Ours)

```csharp
public class SKInkStroke : IDisposable
{
    // Properties
    SKInkStrokeBrush Brush { get; }
    IReadOnlyList<SKInkPoint> Points { get; }
    int PointCount { get; }
    bool IsEmpty { get; }
    SKPath? Path { get; }           // Cached variable-width path
    SKRect Bounds { get; }
    bool IsSelected { get; set; }   // Selection support
    
    // Methods
    void AddPoint(SKInkPoint point, bool isLastPoint = false)
    void Clear()
    void Dispose()
}
```

### Stroke Comparison Table

| Feature | Our API | Windows API | Notes |
|---------|---------|-------------|-------|
| Appearance | ✅ Via Brush | ✅ Via DrawingAttributes | Equivalent |
| Points | ✅ ReadOnlyList | ✅ ReadOnlyList | Equivalent |
| Bounds | ✅ SKRect | ✅ Rect | Equivalent |
| Selection | ✅ IsSelected | ✅ Selected | ✅ Aligned |
| Clone | ❌ Not yet | ✅ Clone() | Gap |
| Transform | ❌ Missing | ✅ PointTransform | Gap |
| Path Generation | ✅ Automatic + cached | ⚠️ Via GetRenderingSegments | Ours simpler |
| Variable Width | ✅ Automatic | ⚠️ Manual | Ours better |
| IDisposable | ✅ Yes | ❌ No | Ours manages memory |
| Timestamp | ✅ Via Points | ✅ StrokeDuration/StartedTime | Different approach |

---

## Canvas/Container Comparison

### InkStrokeContainer (Windows)

```csharp
public sealed class InkStrokeContainer : IInkStrokeContainer
{
    // Properties
    Rect BoundingRect { get; }
    
    // Stroke Management
    void AddStroke(InkStroke stroke)
    void AddStrokes(IEnumerable<InkStroke> strokes)
    void Clear()
    IReadOnlyList<InkStroke> GetStrokes()
    
    // Selection
    InkStroke CanSelectWithLine(Point from, Point to)
    InkStroke CanSelectWithPolyLine(IEnumerable<Point> points)
    void DeleteSelected()
    Rect SelectWithLine(Point from, Point to)
    Rect SelectWithPolyLine(IEnumerable<Point> points)
    Rect MoveSelected(Point translation)
    
    // Clipboard
    bool CanPasteFromClipboard()
    void CopySelectedToClipboard()
    void PasteFromClipboard(Point position)
    
    // Serialization
    Task LoadAsync(IInputStream stream)
    Task<uint> SaveAsync(IOutputStream stream)
}
```

### SKInkCanvas (Ours)

```csharp
public class SKInkCanvas : IDisposable
{
    // Default Settings
    SKInkStrokeBrush Brush { get; set; }   // Cloned for new strokes
    
    // Strokes
    IReadOnlyList<SKInkStroke> Strokes { get; }
    SKInkStroke? CurrentStroke { get; }
    int StrokeCount { get; }
    bool IsBlank { get; }
    bool IsDrawing { get; }
    
    // Events
    event EventHandler Invalidated
    event EventHandler Cleared
    event EventHandler<SKInkStrokeCompletedEventArgs> StrokeCompleted
    event EventHandler StrokeStarted
    event EventHandler SelectionChanged
    
    // Stroke Input
    void StartStroke(SKInkPoint point)
    void StartStroke(SKInkPoint point, SKInkStrokeBrush brush)
    void ContinueStroke(SKInkPoint point)
    void EndStroke(SKInkPoint point)
    void CancelStroke()
    
    // Stroke Management
    void Clear()
    bool Undo()
    
    // Selection
    IReadOnlyList<SKInkStroke> SelectedStrokes { get; }
    void SelectStroke(SKInkStroke stroke)
    void DeselectStroke(SKInkStroke stroke)
    void SelectStrokesInRect(SKRect rect)
    void DeselectAll()
    void DeleteSelected()
    
    // Rendering & Export
    void Draw(SKCanvas canvas, SKPaint paint)
    SKPath? ToPath()
    SKRect GetBounds()
    SKImage? ToImage(int width, int height, SKColor backgroundColor)
    
    void Dispose()
}
```

### Canvas Comparison Table

| Feature | Our API | Windows API | Notes |
|---------|---------|-------------|-------|
| Add/Remove Strokes | ✅ Via Start/End/Cancel | ✅ AddStroke/Clear | Different but complete |
| Default Settings | ✅ Brush property | ❌ On InkPresenter | Ours simpler |
| Undo | ✅ Built-in | ❌ Manual | Ours better |
| Selection | ✅ Rect selection | ✅ Line/polygon selection | Windows more flexible |
| Delete Selected | ✅ DeleteSelected() | ✅ DeleteSelected() | ✅ Aligned |
| Selection Event | ✅ SelectionChanged | ⚠️ Via InkPresenter | Equivalent |
| Clipboard | ❌ Missing | ✅ Copy/Paste | Gap |
| Serialization | ❌ Missing | ✅ Save/LoadAsync (ISF) | Gap |
| Rendering | ✅ Draw() method | ⚠️ Via InkPresenter | Ours simpler |
| Export to Image | ✅ ToImage() | ❌ Manual | Ours better |
| Export to Path | ✅ ToPath() | ❌ Not available | Ours better |
| Events | ✅ Rich event set | ⚠️ Via InkPresenter | Equivalent |
| IDisposable | ✅ Yes | ❌ No | Ours manages memory |

---

## Unique Features Comparison

### Features Only We Have

| Feature | Description | Value |
|---------|-------------|-------|
| **Catmull-Rom Smoothing** | Algorithm passes through all control points | Better handwriting accuracy |
| **Configurable Smoothing Factor** | 1-10 scale for curve smoothness | Fine-tuned control |
| **Tapered Cap Style** | Narrows to a point like pen lift | Natural ink appearance |
| **Min/Max Width Range** | Automatic pressure-to-width mapping | Easier pressure sensitivity |
| **Velocity Modes** | BallpointPen (thinner) + Pencil (lighter) | Windows Ink behavior as options |
| **Recording/Playback** | SKInkRecording + SKInkPlayer | Signature animation demos |
| **ToImage() Export** | Direct export to SKImage with scaling | Convenient export |
| **ToPath() Export** | Get combined SKPath of all strokes | Path manipulation |
| **Cross-platform** | Works on Windows, macOS, Linux, iOS, Android | Wide reach |
| **Platform Independence** | Engine separate from UI | Testable, reusable |
| **Brush Cloning** | Clone() for isolated stroke appearance | Clean stroke isolation |

### Features Only Windows Has

| Feature | Description | Value |
|---------|-------------|-------|
| **Handwriting Recognition** | Convert ink to text | Text input from ink |
| **ISF Serialization** | Standard ink format save/load | File persistence |
| **Line/Polygon Selection** | Select strokes by line/polygon | Flexible selection |
| **Clipboard** | Copy/paste ink | Standard editing |
| **Highlighter Mode** | Transparent overlay rendering | Highlighting text |
| **Pen Tip Transform** | Matrix transform for custom shapes | Advanced styling |
| **InkModelerAttributes** | Input prediction/smoothing | Reduced latency feel |
| **InkAnalyzer** | Ink analysis (shapes, text) | Intelligent ink processing |

---

## Feature Alignment Status

| Feature | Windows | Ours | Status |
|---------|---------|------|--------|
| Pressure sensitivity | ✅ | ✅ | ✅ Aligned |
| Tilt support | ✅ | ✅ | ✅ Aligned |
| Microsecond timestamps | ✅ | ✅ | ✅ Aligned |
| Velocity effects | ⚠️ Limited | ✅ Full | ✅ We're better |
| Selection support | ✅ | ✅ | ✅ Aligned |
| Variable-width strokes | ⚠️ Manual | ✅ Automatic | ✅ We're better |
| Brush/attributes class | ✅ | ✅ | ✅ Aligned (SKInkStrokeBrush) |
| Handwriting recognition | ✅ | ❌ | ❌ Not planned (platform-specific) |
| ISF serialization | ✅ | ❌ | ❌ Low priority |
| Clipboard | ✅ | ❌ | ❌ Low priority |

---

## API Naming Comparison

| Concept | Our Naming | Windows Naming | Assessment |
|---------|------------|----------------|------------|
| Point | SKInkPoint | InkPoint | ✅ Aligned with SK prefix |
| Stroke | SKInkStroke | InkStroke | ✅ Aligned with SK prefix |
| Canvas | SKInkCanvas | InkStrokeContainer | ⚠️ Different concept |
| Appearance | SKInkStrokeBrush | InkDrawingAttributes | ✅ Brush is more intuitive |
| Color | Brush.Color | DrawingAttributes.Color | ✅ Similar |
| Width | Brush.MinSize/MaxSize | DrawingAttributes.Size | ✅ Ours better for pressure |
| Smoothing | SmoothingAlgorithm | FitToCurve | ✅ Ours more flexible |
| Velocity | VelocityMode | N/A | ✅ Unique to us |

---

## Conclusion

### Our API is Better For:
- ✅ Cross-platform applications
- ✅ Signature capture and inking
- ✅ Pressure-sensitive handwriting
- ✅ Velocity-based pen effects
- ✅ Animation and playback
- ✅ Export to images/paths
- ✅ Testability
- ✅ Simple API surface

### Windows API is Better For:
- ✅ Windows-only applications
- ✅ Handwriting-to-text conversion
- ✅ Complex selection workflows (line/polygon)
- ✅ Enterprise ink scenarios
- ✅ ISF format compatibility
- ✅ Clipboard integration

### Summary

We have achieved **feature parity** with Windows.UI.Input.Inking for core inking scenarios:
- ✅ Pressure sensitivity
- ✅ Tilt support
- ✅ Microsecond timestamps
- ✅ Selection support
- ✅ Brush/attributes class

We **exceed** Windows in:
- ✅ Velocity-based stroke effects (BallpointPen/Pencil modes)
- ✅ Smoothing algorithm flexibility (Catmull-Rom + QuadraticBezier)
- ✅ Variable-width rendering (automatic)
- ✅ Cross-platform support
- ✅ Export capabilities (ToImage, ToPath)
- ✅ Playback support

Features we **intentionally don't provide** (platform-specific):
- ❌ Handwriting recognition
- ❌ ISF serialization
- ❌ Clipboard integration
- ❌ InkAnalyzer

Our API is **production-ready** for signature capture, digital inking, and cross-platform drawing applications.
