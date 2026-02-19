# API Comparison: SkiaSharp.Extended.Inking vs Windows.UI.Input.Inking

This document provides a thorough comparison between our `SkiaSharp.Extended.Inking` API and Microsoft's `Windows.UI.Input.Inking` namespace, analyzing architecture, features, and recommending alignment decisions.

## Executive Summary

| Aspect | SkiaSharp.Extended.Inking | Windows.UI.Input.Inking |
|--------|---------------------------|-------------------------|
| **Target Platform** | Cross-platform (netstandard2.0, net9.0) | Windows-only (UWP/WinUI) |
| **Architecture** | Single library, minimal classes | Large namespace, many specialized classes |
| **Learning Curve** | Low | Medium-High |
| **Feature Depth** | Core inking features | Full inking ecosystem |
| **Handwriting Recognition** | ❌ Not available | ✅ Built-in |
| **Pressure Sensitivity** | ✅ Full support | ✅ Full support |
| **Tilt Support** | ❌ Not available | ✅ Available |
| **Serialization (ISF)** | ❌ Not available | ✅ Built-in |

**Recommendation**: Our API is **better suited for cross-platform scenarios** and has a **simpler API surface**. However, we could benefit from selective feature alignment (tilt, serialization) without adopting Windows' complexity.

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
    └── SKInkStrokes[] (stroke data + rendering)
            └── SKInkPoints[] (point data)

SKInkRecording / SKInkPlayer (playback support)
SKSignaturePadView (MAUI control - thin wrapper)
```

**Key Classes:**
| Class | Purpose |
|-------|---------|
| `SKInkCanvas` | Engine for managing strokes |
| `SKInkStroke` | Stroke data + path generation |
| `SKInkPoint` | Point with pressure |
| `SKInkRecording` | Recording/playback support |
| `SKInkPlayer` | Playback engine |
| `SKSignaturePadView` | MAUI control wrapper |

### Architectural Assessment

| Aspect | Our API | Windows API | Winner |
|--------|---------|-------------|--------|
| **Simplicity** | 7 classes | 15+ classes | ✅ Ours |
| **Cross-platform** | Yes | No | ✅ Ours |
| **Separation of concerns** | Engine + View separated | Tightly coupled | ✅ Ours |
| **Extensibility** | Easy to extend | Complex | ✅ Ours |
| **Feature completeness** | Basic | Comprehensive | ❌ Windows |

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
    SKInkPoint(SKPoint location, float pressure, long timestamp = 0)
    SKInkPoint(float x, float y, float pressure, long timestamp = 0)
    
    // Properties
    SKPoint Location { get; }
    float X { get; }
    float Y { get; }
    float Pressure { get; }     // 0.0 - 1.0 (clamped)
    long Timestamp { get; }     // Milliseconds
}
```

### Point Comparison Table

| Feature | Our API | Windows API | Notes |
|---------|---------|-------------|-------|
| Position | ✅ SKPoint | ✅ Point | Equivalent |
| Pressure | ✅ Clamped 0-1 | ✅ 0-1 | Ours validates |
| TiltX/TiltY | ❌ Missing | ✅ Available | Gap |
| Timestamp | ✅ Milliseconds | ✅ Microseconds | Different precision |
| Struct vs Class | ✅ Struct | ❌ Class | Ours is more efficient |
| IEquatable | ✅ Yes | ❌ No | Ours better |

**Gap Analysis**: We're missing tilt support, which is useful for calligraphy-style rendering.

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

### InkDrawingAttributes (Windows - separate class)

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
    Size Size { get; set; }           // Width x Height
}
```

### SKInkStroke (Ours)

```csharp
public class SKInkStroke : IDisposable
{
    // Constructor with all settings inline
    SKInkStroke(
        float minStrokeWidth = 1f, 
        float maxStrokeWidth = 8f,
        SKColor? color = null,
        SKStrokeCapStyle capStyle = SKStrokeCapStyle.Round,
        int smoothingFactor = 4,
        SKSmoothingAlgorithm smoothingAlgorithm = SKSmoothingAlgorithm.CatmullRom)
    
    // Properties
    float MinStrokeWidth { get; }
    float MaxStrokeWidth { get; }
    SKColor? Color { get; set; }
    SKStrokeCapStyle CapStyle { get; set; }  // Round, Flat, Tapered
    int SmoothingFactor { get; set; }        // 1-10
    SKSmoothingAlgorithm SmoothingAlgorithm { get; set; }
    IReadOnlyList<SKInkPoint> Points { get; }
    int PointCount { get; }
    bool IsEmpty { get; }
    SKPath? Path { get; }                    // Cached generated path
    SKRect Bounds { get; }
    
    // Methods
    void AddPoint(SKInkPoint point, bool isLastPoint = false)
    void Clear()
    void Dispose()
}
```

### Stroke Comparison Table

| Feature | Our API | Windows API | Notes |
|---------|---------|-------------|-------|
| Color | ✅ Per-stroke | ✅ Via DrawingAttributes | Equivalent |
| Stroke Width | ✅ Min/Max range | ✅ Size (fixed) | Ours better for pressure |
| Cap Style | ✅ Round/Flat/Tapered | ⚠️ Via PenTip (Circle/Rectangle) | Different approach |
| Smoothing | ✅ Configurable algorithm | ✅ FitToCurve boolean | Ours more flexible |
| Highlighter Mode | ❌ Missing | ✅ DrawAsHighlighter | Gap |
| Selection | ❌ Missing | ✅ Selected property | Gap |
| Clone | ❌ Missing | ✅ Clone() method | Gap |
| Transform | ❌ Missing | ✅ PointTransform | Gap |
| Path Generation | ✅ Built-in cached | ⚠️ Via GetRenderingSegments | Different approach |
| IDisposable | ✅ Yes | ❌ No | Ours manages memory |
| Pressure Width Variation | ✅ Automatic | ⚠️ Manual via IgnorePressure | Ours better |

**Key Differences:**
1. **Width model**: Windows uses fixed `Size`, we use `MinStrokeWidth`/`MaxStrokeWidth` for pressure sensitivity
2. **Smoothing**: Windows has simple on/off, we have algorithm selection + factor
3. **Path generation**: We generate the filled polygon path internally; Windows provides segments for custom rendering

---

## Canvas/Container Comparison

### InkStrokeContainer (Windows)

```csharp
public sealed class InkStrokeContainer : IInkStrokeContainer
{
    // Properties
    Rect BoundingRect { get; }
    
    // Methods
    void AddStroke(InkStroke stroke)
    void AddStrokes(IEnumerable<InkStroke> strokes)
    bool CanPasteFromClipboard()
    InkStroke CanSelectWithLine(Point from, Point to)
    InkStroke CanSelectWithPolyLine(IEnumerable<Point> points)
    void Clear()
    void CopySelectedToClipboard()
    void DeleteSelected()
    Rect GetRecognitionResults()
    IReadOnlyList<InkStroke> GetStrokes()
    Task LoadAsync(IInputStream stream)
    Rect MoveSelected(Point translation)
    void PasteFromClipboard(Point position)
    Task<uint> SaveAsync(IOutputStream stream)
    Rect SelectWithLine(Point from, Point to)
    Rect SelectWithPolyLine(IEnumerable<Point> points)
}
```

### SKInkCanvas (Ours)

```csharp
public class SKInkCanvas : IDisposable
{
    // Properties
    float MinStrokeWidth { get; set; }
    float MaxStrokeWidth { get; set; }
    SKColor StrokeColor { get; set; }
    SKStrokeCapStyle CapStyle { get; set; }
    int SmoothingFactor { get; set; }
    SKSmoothingAlgorithm SmoothingAlgorithm { get; set; }
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
    
    // Stroke Management
    void StartStroke(SKInkPoint point)
    void StartStroke(SKInkPoint point, SKColor color, ...)
    void ContinueStroke(SKInkPoint point)
    void EndStroke(SKInkPoint point)
    void CancelStroke()
    void Clear()
    bool Undo()
    
    // Rendering & Export
    void Draw(SKCanvas canvas, SKPaint paint)
    SKPath? ToPath()
    SKRect GetBounds()
    SKImage? ToImage(int width, int height, SKColor strokeColor, ...)
    
    void Dispose()
}
```

### Canvas Comparison Table

| Feature | Our API | Windows API | Notes |
|---------|---------|-------------|-------|
| Add/Remove Strokes | ✅ Via Start/End/Cancel | ✅ AddStroke/Clear | Different approach |
| Default Settings | ✅ On canvas | ❌ On InkPresenter | Ours simpler |
| Real-time Input | ✅ Start/Continue/End | ⚠️ Via InkPresenter | Different architecture |
| Undo | ✅ Built-in | ❌ Manual | Ours better |
| Selection | ❌ Missing | ✅ SelectWith* methods | Gap |
| Clipboard | ❌ Missing | ✅ Copy/Paste | Gap |
| Serialization | ❌ Missing | ✅ Save/LoadAsync (ISF) | Gap |
| Recognition Results | ❌ Missing | ✅ GetRecognitionResults | Gap |
| Rendering | ✅ Draw() method | ⚠️ Via InkPresenter/InkSynchronizer | Ours simpler |
| Export to Image | ✅ ToImage() | ❌ Manual | Ours better |
| Export to Path | ✅ ToPath() | ❌ Not available | Ours better |
| Events | ✅ Invalidated/StrokeCompleted | ⚠️ Via InkPresenter events | Similar |
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
| **Recording/Playback** | SKInkRecording + SKInkPlayer | Signature animation demos |
| **ToImage() Export** | Direct export to SKImage with scaling | Convenient export |
| **ToPath() Export** | Get combined SKPath of all strokes | Path manipulation |
| **Cross-platform** | Works on Windows, macOS, Linux, iOS, Android | Wide reach |
| **Platform Independence** | Engine separate from UI | Testable, reusable |

### Features Only Windows Has

| Feature | Description | Value |
|---------|-------------|-------|
| **Tilt Support** | TiltX/TiltY for stylus angle | Calligraphy effects |
| **Handwriting Recognition** | Convert ink to text | Text input from ink |
| **ISF Serialization** | Standard ink format save/load | File persistence |
| **Selection** | Select strokes by line/polygon | Editing workflows |
| **Clipboard** | Copy/paste ink | Standard editing |
| **Highlighter Mode** | Transparent overlay rendering | Highlighting text |
| **Pen Tip Shape** | Circle vs Rectangle tips | Different stroke styles |
| **Pen Tip Transform** | Matrix transform for custom shapes | Advanced styling |
| **InkModelerAttributes** | Input prediction/smoothing | Reduced latency feel |
| **InkAnalyzer** | Ink analysis (shapes, text) | Intelligent ink processing |

---

## Recommendations

### Keep Our Current Approach (Don't Align)

1. **Architecture**: Our separation of engine (SKInkCanvas) from UI (SKSignaturePadView) is superior for testability and reuse.

2. **Min/Max Width Model**: Our pressure-sensitive width range is more intuitive than Windows' fixed size + IgnorePressure flag.

3. **Smoothing Algorithm Choice**: Offering Catmull-Rom and Quadratic Bezier with a factor is more powerful than Windows' simple FitToCurve boolean.

4. **Cap Styles**: Our Round/Flat/Tapered is better suited for handwriting than Windows' Circle/Rectangle pen tips.

### Consider Aligning (Add Missing Features)

| Feature | Priority | Effort | Recommendation |
|---------|----------|--------|----------------|
| **Tilt Support** | Medium | Low | Add TiltX/TiltY to SKInkPoint |
| **ISF Serialization** | Low | High | Skip - use platform-specific solutions |
| **Selection** | Medium | Medium | Add Selected property + selection methods |
| **Clone** | Low | Low | Add Clone() to SKInkStroke |
| **Highlighter Mode** | Low | Medium | Add DrawAsHighlighter flag |
| **Transform** | Low | Medium | Add Transform property |
| **Recognition** | Low | N/A | Skip - platform-specific only |

### Recommended Changes

```csharp
// 1. Add tilt to SKInkPoint (LOW EFFORT)
public readonly struct SKInkPoint
{
    public float TiltX { get; }  // -90 to +90 degrees
    public float TiltY { get; }  // -90 to +90 degrees
    
    public SKInkPoint(SKPoint location, float pressure, 
        float tiltX = 0, float tiltY = 0, long timestamp = 0)
}

// 2. Add selection to SKInkStroke (MEDIUM EFFORT)
public class SKInkStroke
{
    public bool Selected { get; set; }
    public SKInkStroke Clone()
}

// 3. Add selection methods to SKInkCanvas (MEDIUM EFFORT)
public class SKInkCanvas
{
    public IReadOnlyList<SKInkStroke> GetSelectedStrokes()
    public void SelectAll()
    public void DeselectAll()
    public void DeleteSelected()
    public SKRect SelectWithRect(SKRect rect)
}
```

---

## API Naming Comparison

| Concept | Our Naming | Windows Naming | Assessment |
|---------|------------|----------------|------------|
| Point | SKInkPoint | InkPoint | ✅ Aligned with SK prefix |
| Stroke | SKInkStroke | InkStroke | ✅ Aligned with SK prefix |
| Canvas | SKInkCanvas | InkStrokeContainer | ⚠️ Different concept |
| Color | Color property | DrawingAttributes.Color | ✅ Simpler |
| Width | Min/MaxStrokeWidth | Size | ✅ More descriptive |
| Smoothing | SmoothingAlgorithm | FitToCurve | ✅ More flexible |

Our naming follows SkiaSharp conventions (SK prefix) and is generally more descriptive.

---

## Conclusion

### Our API is Better For:
- ✅ Cross-platform applications
- ✅ Simple signature/inking needs
- ✅ Pressure-sensitive handwriting
- ✅ Animation and playback
- ✅ Export to images
- ✅ Testability

### Windows API is Better For:
- ✅ Windows-only applications
- ✅ Handwriting-to-text conversion
- ✅ Full editing workflows (select/cut/copy/paste)
- ✅ Enterprise ink scenarios
- ✅ ISF format compatibility

### Final Recommendation

**Do not fully align** with Windows.UI.Input.Inking. Our API is simpler, cross-platform, and better suited for signature/inking use cases. However, consider adding:

1. **TiltX/TiltY** to SKInkPoint (low effort, enables calligraphy)
2. **Clone()** method to SKInkStroke (low effort)
3. **Selection support** if editing workflows are needed (medium effort)

The current architecture is sound and should be preserved. Our focus on pressure sensitivity, smooth curves, and cross-platform compatibility addresses a gap that Windows.UI.Input.Inking doesn't fill.
