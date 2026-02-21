using SkiaSharp.Views.Blazor;

namespace SkiaSharp.Extended.UI.Blazor.Controls;

/// <summary>
/// A SkiaSharp surface view for Blazor with built-in gesture recognition.
/// </summary>
/// <remarks>
/// <para>
/// This component extends <see cref="SKTouchCanvasView"/> to add comprehensive gesture detection including:
/// </para>
/// <list type="bullet">
///   <item><description>Single, double, and multi-tap detection</description></item>
///   <item><description>Long press detection</description></item>
///   <item><description>Pan/drag gestures</description></item>
///   <item><description>Pinch to zoom gestures</description></item>
///   <item><description>Rotation gestures</description></item>
///   <item><description>Fling (swipe) gesture detection with velocity</description></item>
///   <item><description>Hover detection for mouse/stylus</description></item>
///   <item><description>Mouse wheel scroll detection</description></item>
/// </list>
/// <para>
/// The gesture hierarchy is: <see cref="SKGestureSurfaceView"/> → <see cref="SKTouchCanvasView"/> → SkiaSharp canvas.
/// </para>
/// </remarks>
public partial class SKGestureSurfaceView : ComponentBase, IAsyncDisposable
{
    private SKTouchCanvasView? _touchView;
    private readonly SKGestureEngine _engine;

    /// <summary>Creates a new instance of <see cref="SKGestureSurfaceView"/>.</summary>
    public SKGestureSurfaceView()
    {
        _engine = new SKGestureEngine();
        SubscribeEngineEvents();
    }

    /// <summary>Gets the underlying gesture engine for advanced scenarios.</summary>
    public SKGestureEngine Engine => _engine;

    #region Parameters

    /// <summary>Gets or sets whether gesture detection is enabled.</summary>
    [Parameter]
    public bool IsGestureEnabled { get; set; } = true;

    /// <summary>Gets or sets the touch slop (minimum movement to start a gesture).</summary>
    [Parameter]
    public float TouchSlop { get; set; } = 8f;

    /// <summary>Gets or sets the long press duration in milliseconds.</summary>
    [Parameter]
    public int LongPressDuration { get; set; } = 500;

    /// <summary>Gets or sets the paint surface callback.</summary>
    [Parameter]
    public Action<SKPaintSurfaceEventArgs>? OnPaintSurface { get; set; }

    /// <summary>Gets or sets whether continuous rendering is enabled.</summary>
    [Parameter]
    public bool EnableRenderLoop { get; set; }

    /// <summary>Gets or sets whether pixel scaling is ignored.</summary>
    [Parameter]
    public bool IgnorePixelScaling { get; set; }

    /// <summary>Gets or sets the CSS style string.</summary>
    [Parameter]
    public string? Style { get; set; }

    /// <summary>Gets or sets the CSS class string.</summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>Gets or sets additional HTML attributes.</summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    #endregion

    #region Gesture Events

    /// <summary>Occurs when a tap is detected.</summary>
    [Parameter]
    public EventCallback<SKTapEventArgs> TapDetected { get; set; }

    /// <summary>Occurs when a double tap is detected.</summary>
    [Parameter]
    public EventCallback<SKTapEventArgs> DoubleTapDetected { get; set; }

    /// <summary>Occurs when a long press is detected.</summary>
    [Parameter]
    public EventCallback<SKTapEventArgs> LongPressDetected { get; set; }

    /// <summary>Occurs when a pan gesture is detected.</summary>
    [Parameter]
    public EventCallback<SKPanEventArgs> PanDetected { get; set; }

    /// <summary>Occurs when a pinch (scale) gesture is detected.</summary>
    [Parameter]
    public EventCallback<SKPinchEventArgs> PinchDetected { get; set; }

    /// <summary>Occurs when a rotation gesture is detected.</summary>
    [Parameter]
    public EventCallback<SKRotateEventArgs> RotateDetected { get; set; }

    /// <summary>Occurs when a fling gesture is detected.</summary>
    [Parameter]
    public EventCallback<SKFlingEventArgs> FlingDetected { get; set; }

    /// <summary>Occurs each animation frame during a fling with current velocity and per-frame delta.</summary>
    [Parameter]
    public EventCallback<SKFlingEventArgs> Flinging { get; set; }

    /// <summary>Occurs when a fling animation completes.</summary>
    [Parameter]
    public EventCallback FlingCompleted { get; set; }

    /// <summary>Occurs when hover is detected.</summary>
    [Parameter]
    public EventCallback<SKHoverEventArgs> HoverDetected { get; set; }

    /// <summary>Occurs when a mouse scroll (wheel) event is detected.</summary>
    [Parameter]
    public EventCallback<SKScrollEventArgs> ScrollDetected { get; set; }

    /// <summary>Occurs when a gesture starts.</summary>
    [Parameter]
    public EventCallback<SKGestureStateEventArgs> GestureStarted { get; set; }

    /// <summary>Occurs when a gesture ends.</summary>
    [Parameter]
    public EventCallback<SKGestureStateEventArgs> GestureEnded { get; set; }

    /// <summary>Occurs when a drag operation starts.</summary>
    [Parameter]
    public EventCallback<SKDragEventArgs> DragStarted { get; set; }

    /// <summary>Occurs during a drag operation.</summary>
    [Parameter]
    public EventCallback<SKDragEventArgs> DragUpdated { get; set; }

    /// <summary>Occurs when a drag operation ends.</summary>
    [Parameter]
    public EventCallback<SKDragEventArgs> DragEnded { get; set; }

    #endregion

    /// <summary>Gets the current DPI of the display.</summary>
    public float Dpi => _touchView?.Dpi ?? 1f;

    /// <summary>Requests a redraw of the canvas.</summary>
    public void Invalidate() => _touchView?.Invalidate();

    /// <inheritdoc/>
    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        _engine.IsEnabled = IsGestureEnabled;
        _engine.TouchSlop = TouchSlop;
        _engine.LongPressDuration = LongPressDuration;
    }

    private async Task OnTouchInternal(SKTouchEventArgs e)
    {
        var isMouse = e.DeviceType == SKTouchDeviceType.Mouse;
        var location = e.Location;

        switch (e.ActionType)
        {
            case SKTouchAction.Pressed:
                e.Handled = _engine.ProcessTouchDown(e.Id, location, isMouse);
                break;
            case SKTouchAction.Moved:
                e.Handled = _engine.ProcessTouchMove(e.Id, location, e.InContact);
                break;
            case SKTouchAction.Released:
                e.Handled = _engine.ProcessTouchUp(e.Id, location, isMouse);
                break;
            case SKTouchAction.Cancelled:
                e.Handled = _engine.ProcessTouchCancel(e.Id);
                break;
            case SKTouchAction.Entered:
            case SKTouchAction.Exited:
                e.Handled = true;
                break;
            case SKTouchAction.WheelChanged:
                e.Handled = _engine.ProcessMouseWheel(location, 0, e.WheelDelta);
                break;
        }

        if (e.Handled)
            Invalidate();

        await Task.CompletedTask;
    }

    private void OnPaintSurfaceInternal(SKPaintSurfaceEventArgs e)
    {
        OnPaintSurface?.Invoke(e);
    }

    private void SubscribeEngineEvents()
    {
        _engine.TapDetected += OnEngineTapDetected;
        _engine.DoubleTapDetected += OnEngineDoubleTapDetected;
        _engine.LongPressDetected += OnEngineLongPressDetected;
        _engine.PanDetected += OnEnginePanDetected;
        _engine.PinchDetected += OnEnginePinchDetected;
        _engine.RotateDetected += OnEngineRotateDetected;
        _engine.FlingDetected += OnEngineFlingDetected;
        _engine.Flinging += OnEngineFlinging;
        _engine.FlingCompleted += OnEngineFlingCompleted;
        _engine.HoverDetected += OnEngineHoverDetected;
        _engine.ScrollDetected += OnEngineScrollDetected;
        _engine.GestureStarted += OnEngineGestureStarted;
        _engine.GestureEnded += OnEngineGestureEnded;
        _engine.DragStarted += OnEngineDragStarted;
        _engine.DragUpdated += OnEngineDragUpdated;
        _engine.DragEnded += OnEngineDragEnded;
    }

    private void UnsubscribeEngineEvents()
    {
        _engine.TapDetected -= OnEngineTapDetected;
        _engine.DoubleTapDetected -= OnEngineDoubleTapDetected;
        _engine.LongPressDetected -= OnEngineLongPressDetected;
        _engine.PanDetected -= OnEnginePanDetected;
        _engine.PinchDetected -= OnEnginePinchDetected;
        _engine.RotateDetected -= OnEngineRotateDetected;
        _engine.FlingDetected -= OnEngineFlingDetected;
        _engine.Flinging -= OnEngineFlinging;
        _engine.FlingCompleted -= OnEngineFlingCompleted;
        _engine.HoverDetected -= OnEngineHoverDetected;
        _engine.ScrollDetected -= OnEngineScrollDetected;
        _engine.GestureStarted -= OnEngineGestureStarted;
        _engine.GestureEnded -= OnEngineGestureEnded;
        _engine.DragStarted -= OnEngineDragStarted;
        _engine.DragUpdated -= OnEngineDragUpdated;
        _engine.DragEnded -= OnEngineDragEnded;
    }

    private async void OnEngineTapDetected(object? s, SKTapEventArgs e) => await TapDetected.InvokeAsync(e);
    private async void OnEngineDoubleTapDetected(object? s, SKTapEventArgs e) => await DoubleTapDetected.InvokeAsync(e);
    private async void OnEngineLongPressDetected(object? s, SKTapEventArgs e) => await LongPressDetected.InvokeAsync(e);
    private async void OnEnginePanDetected(object? s, SKPanEventArgs e) => await PanDetected.InvokeAsync(e);
    private async void OnEnginePinchDetected(object? s, SKPinchEventArgs e) => await PinchDetected.InvokeAsync(e);
    private async void OnEngineRotateDetected(object? s, SKRotateEventArgs e) => await RotateDetected.InvokeAsync(e);
    private async void OnEngineFlingDetected(object? s, SKFlingEventArgs e) => await FlingDetected.InvokeAsync(e);
    private async void OnEngineFlinging(object? s, SKFlingEventArgs e) => await Flinging.InvokeAsync(e);
    private async void OnEngineFlingCompleted(object? s, EventArgs e) => await FlingCompleted.InvokeAsync();
    private async void OnEngineHoverDetected(object? s, SKHoverEventArgs e) => await HoverDetected.InvokeAsync(e);
    private async void OnEngineScrollDetected(object? s, SKScrollEventArgs e) => await ScrollDetected.InvokeAsync(e);
    private async void OnEngineGestureStarted(object? s, SKGestureStateEventArgs e) => await GestureStarted.InvokeAsync(e);
    private async void OnEngineGestureEnded(object? s, SKGestureStateEventArgs e) => await GestureEnded.InvokeAsync(e);
    private async void OnEngineDragStarted(object? s, SKDragEventArgs e) => await DragStarted.InvokeAsync(e);
    private async void OnEngineDragUpdated(object? s, SKDragEventArgs e) => await DragUpdated.InvokeAsync(e);
    private async void OnEngineDragEnded(object? s, SKDragEventArgs e) => await DragEnded.InvokeAsync(e);

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        UnsubscribeEngineEvents();
        _engine.Dispose();
        if (_touchView is not null)
            await _touchView.DisposeAsync();
    }
}
