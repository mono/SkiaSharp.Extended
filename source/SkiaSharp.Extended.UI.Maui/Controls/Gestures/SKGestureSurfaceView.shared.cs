using SkiaSharp.Extended.Gestures;
using SkiaSharp.Views.Maui;

namespace SkiaSharp.Extended.UI.Controls;

/// <summary>
/// A SkiaSharp view with built-in gesture recognition and transform tracking.
/// </summary>
/// <remarks>
/// <para>
/// This view extends <see cref="SKSurfaceView"/> to add comprehensive gesture detection including:
/// </para>
/// <list type="bullet">
///   <item><description>Single, double, and multi-tap detection</description></item>
///   <item><description>Long press detection</description></item>
///   <item><description>Pan/drag gestures with fling animation</description></item>
///   <item><description>Pinch to zoom gestures</description></item>
///   <item><description>Rotation gestures</description></item>
///   <item><description>Animated double-tap zoom and scroll zoom</description></item>
///   <item><description>Hover detection for mouse/stylus</description></item>
/// </list>
/// <para>
/// Use the <see cref="Tracker"/> property to access the transform state (<see cref="SKGestureTracker.Matrix"/>)
/// and configure gesture behavior.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;controls:SKGestureSurfaceView
///     TapDetected="OnTap"
///     PanDetected="OnPan"
///     PinchDetected="OnPinch"
///     RotateDetected="OnRotate" /&gt;
/// </code>
/// </example>
public class SKGestureSurfaceView : SKSurfaceView
{
	/// <summary>
	/// Identifies the <see cref="IsGestureEnabled"/> bindable property.
	/// </summary>
	public static readonly BindableProperty IsGestureEnabledProperty = BindableProperty.Create(
		nameof(IsGestureEnabled),
		typeof(bool),
		typeof(SKGestureSurfaceView),
		true,
		propertyChanged: OnIsGestureEnabledChanged);

	/// <summary>
	/// Identifies the <see cref="TouchSlop"/> bindable property.
	/// </summary>
	public static readonly BindableProperty TouchSlopProperty = BindableProperty.Create(
		nameof(TouchSlop),
		typeof(float),
		typeof(SKGestureSurfaceView),
		8f,
		propertyChanged: OnTouchSlopChanged);

	/// <summary>
	/// Identifies the <see cref="LongPressDuration"/> bindable property.
	/// </summary>
	public static readonly BindableProperty LongPressDurationProperty = BindableProperty.Create(
		nameof(LongPressDuration),
		typeof(int),
		typeof(SKGestureSurfaceView),
		500,
		propertyChanged: OnLongPressDurationChanged);

	private SKCanvasView? _canvasView;
	private SKGLView? _glView;

	/// <summary>
	/// Creates a new instance of <see cref="SKGestureSurfaceView"/>.
	/// </summary>
	public SKGestureSurfaceView()
	{
		ResourceLoader<Themes.SKGestureSurfaceViewResources>.EnsureRegistered(this);

		Tracker = new SKGestureTracker();

		Loaded += OnLoaded;
		Unloaded += OnUnloaded;

		DebugUtils.LogPropertyChanged(this);
	}

	/// <summary>
	/// Gets the gesture tracker that manages transform state and animations.
	/// </summary>
	public SKGestureTracker Tracker { get; }

	/// <summary>
	/// Gets or sets whether gesture detection is enabled.
	/// </summary>
	public bool IsGestureEnabled
	{
		get => (bool)GetValue(IsGestureEnabledProperty);
		set => SetValue(IsGestureEnabledProperty, value);
	}

	/// <summary>
	/// Gets or sets the touch slop (minimum movement to start a gesture).
	/// </summary>
	public float TouchSlop
	{
		get => (float)GetValue(TouchSlopProperty);
		set => SetValue(TouchSlopProperty, value);
	}

	/// <summary>
	/// Gets or sets the long press duration in milliseconds.
	/// </summary>
	public int LongPressDuration
	{
		get => (int)GetValue(LongPressDurationProperty);
		set => SetValue(LongPressDurationProperty, value);
	}

	#region Events

	/// <summary>
	/// Occurs when a tap is detected.
	/// </summary>
	public event EventHandler<SKTapEventArgs>? TapDetected;

	/// <summary>
	/// Occurs when a double tap is detected.
	/// </summary>
	public event EventHandler<SKTapEventArgs>? DoubleTapDetected;

	/// <summary>
	/// Occurs when a long press is detected.
	/// </summary>
	public event EventHandler<SKTapEventArgs>? LongPressDetected;

	/// <summary>
	/// Occurs when a pan gesture is detected.
	/// </summary>
	public event EventHandler<SKPanEventArgs>? PanDetected;

	/// <summary>
	/// Occurs when a pinch (scale) gesture is detected.
	/// </summary>
	public event EventHandler<SKPinchEventArgs>? PinchDetected;

	/// <summary>
	/// Occurs when a rotation gesture is detected.
	/// </summary>
	public event EventHandler<SKRotateEventArgs>? RotateDetected;

	/// <summary>
	/// Occurs when a fling gesture is detected.
	/// </summary>
	public event EventHandler<SKFlingEventArgs>? FlingDetected;

	/// <summary>
	/// Occurs each animation frame during a fling with current velocity and per-frame delta.
	/// </summary>
	public event EventHandler<SKFlingEventArgs>? Flinging;

	/// <summary>
	/// Occurs when a fling animation completes.
	/// </summary>
	public event EventHandler? FlingCompleted;

	/// <summary>
	/// Occurs when a hover is detected.
	/// </summary>
	public event EventHandler<SKHoverEventArgs>? HoverDetected;

	/// <summary>
	/// Occurs when a mouse scroll (wheel) event is detected.
	/// </summary>
	public event EventHandler<SKScrollEventArgs>? ScrollDetected;

	/// <summary>
	/// Occurs when a gesture starts.
	/// </summary>
	public event EventHandler<SKGestureStateEventArgs>? GestureStarted;

	/// <summary>
	/// Occurs when a gesture ends.
	/// </summary>
	public event EventHandler<SKGestureStateEventArgs>? GestureEnded;

	/// <summary>
	/// Occurs when the transform state changes.
	/// </summary>
	public event EventHandler? TransformChanged;

	/// <summary>
	/// Occurs when a drag operation starts.
	/// </summary>
	public event EventHandler<SKDragEventArgs>? DragStarted;

	/// <summary>
	/// Occurs during a drag operation.
	/// </summary>
	public event EventHandler<SKDragEventArgs>? DragUpdated;

	/// <summary>
	/// Occurs when a drag operation ends.
	/// </summary>
	public event EventHandler<SKDragEventArgs>? DragEnded;

	/// <summary>
	/// Occurs when the surface needs to be painted.
	/// </summary>
	public event EventHandler<SKPaintSurfaceEventArgs>? PaintSurface;

	#endregion

	#region Event Invokers

	/// <summary>Invokes <see cref="TapDetected"/>.</summary>
	protected virtual void OnTapDetected(SKTapEventArgs e) => TapDetected?.Invoke(this, e);

	/// <summary>Invokes <see cref="DoubleTapDetected"/>.</summary>
	protected virtual void OnDoubleTapDetected(SKTapEventArgs e) => DoubleTapDetected?.Invoke(this, e);

	/// <summary>Invokes <see cref="LongPressDetected"/>.</summary>
	protected virtual void OnLongPressDetected(SKTapEventArgs e) => LongPressDetected?.Invoke(this, e);

	/// <summary>Invokes <see cref="PanDetected"/>.</summary>
	protected virtual void OnPanDetected(SKPanEventArgs e) => PanDetected?.Invoke(this, e);

	/// <summary>Invokes <see cref="PinchDetected"/>.</summary>
	protected virtual void OnPinchDetected(SKPinchEventArgs e) => PinchDetected?.Invoke(this, e);

	/// <summary>Invokes <see cref="RotateDetected"/>.</summary>
	protected virtual void OnRotateDetected(SKRotateEventArgs e) => RotateDetected?.Invoke(this, e);

	/// <summary>Invokes <see cref="FlingDetected"/>.</summary>
	protected virtual void OnFlingDetected(SKFlingEventArgs e) => FlingDetected?.Invoke(this, e);

	/// <summary>Invokes <see cref="Flinging"/>.</summary>
	protected virtual void OnFlinging(SKFlingEventArgs e) => Flinging?.Invoke(this, e);

	/// <summary>Invokes <see cref="FlingCompleted"/>.</summary>
	protected virtual void OnFlingCompleted() => FlingCompleted?.Invoke(this, EventArgs.Empty);

	/// <summary>Invokes <see cref="HoverDetected"/>.</summary>
	protected virtual void OnHoverDetected(SKHoverEventArgs e) => HoverDetected?.Invoke(this, e);

	/// <summary>Invokes <see cref="ScrollDetected"/>.</summary>
	protected virtual void OnScrollDetected(SKScrollEventArgs e) => ScrollDetected?.Invoke(this, e);

	/// <summary>Invokes <see cref="GestureStarted"/>.</summary>
	protected virtual void OnGestureStarted(SKGestureStateEventArgs e) => GestureStarted?.Invoke(this, e);

	/// <summary>Invokes <see cref="GestureEnded"/>.</summary>
	protected virtual void OnGestureEnded(SKGestureStateEventArgs e) => GestureEnded?.Invoke(this, e);

	/// <summary>Invokes <see cref="TransformChanged"/>.</summary>
	protected virtual void OnTransformChanged() => TransformChanged?.Invoke(this, EventArgs.Empty);

	/// <summary>Invokes <see cref="DragStarted"/>.</summary>
	protected virtual void OnDragStarted(SKDragEventArgs e) => DragStarted?.Invoke(this, e);

	/// <summary>Invokes <see cref="DragUpdated"/>.</summary>
	protected virtual void OnDragUpdated(SKDragEventArgs e) => DragUpdated?.Invoke(this, e);

	/// <summary>Invokes <see cref="DragEnded"/>.</summary>
	protected virtual void OnDragEnded(SKDragEventArgs e) => DragEnded?.Invoke(this, e);

	#endregion

	/// <inheritdoc/>
	protected override void OnApplyTemplate()
	{
		// Unsubscribe from old views
		if (_canvasView is not null)
		{
			_canvasView.Touch -= OnTouch;
			_canvasView = null;
		}
		
		if (_glView is not null)
		{
			_glView.Touch -= OnTouch;
			_glView = null;
		}

		base.OnApplyTemplate();

		// Get canvas view and subscribe to touch
		var templateChild = GetTemplateChild("PART_DrawingSurface");
		if (templateChild is SKCanvasView canvasView)
		{
			_canvasView = canvasView;
			_canvasView.EnableTouchEvents = true;
			_canvasView.Touch += OnTouch;
		}
		else if (templateChild is SKGLView glView)
		{
			_glView = glView;
			_glView.EnableTouchEvents = true;
			_glView.Touch += OnTouch;
		}
	}

	private void OnLoaded(object? sender, EventArgs e)
	{
		SubscribeTrackerEvents();
	}

	private void OnUnloaded(object? sender, EventArgs e)
	{
		UnsubscribeTrackerEvents();
		Tracker.Reset();
	}

	private void SubscribeTrackerEvents()
	{
		Tracker.TapDetected += OnTrackerTapDetected;
		Tracker.DoubleTapDetected += OnTrackerDoubleTapDetected;
		Tracker.LongPressDetected += OnTrackerLongPressDetected;
		Tracker.PanDetected += OnTrackerPanDetected;
		Tracker.PinchDetected += OnTrackerPinchDetected;
		Tracker.RotateDetected += OnTrackerRotateDetected;
		Tracker.FlingDetected += OnTrackerFlingDetected;
		Tracker.Flinging += OnTrackerFlinging;
		Tracker.FlingCompleted += OnTrackerFlingCompleted;
		Tracker.HoverDetected += OnTrackerHoverDetected;
		Tracker.ScrollDetected += OnTrackerScrollDetected;
		Tracker.GestureStarted += OnTrackerGestureStarted;
		Tracker.GestureEnded += OnTrackerGestureEnded;
		Tracker.TransformChanged += OnTrackerTransformChanged;
		Tracker.DragStarted += OnTrackerDragStarted;
		Tracker.DragUpdated += OnTrackerDragUpdated;
		Tracker.DragEnded += OnTrackerDragEnded;
	}

	private void UnsubscribeTrackerEvents()
	{
		Tracker.TapDetected -= OnTrackerTapDetected;
		Tracker.DoubleTapDetected -= OnTrackerDoubleTapDetected;
		Tracker.LongPressDetected -= OnTrackerLongPressDetected;
		Tracker.PanDetected -= OnTrackerPanDetected;
		Tracker.PinchDetected -= OnTrackerPinchDetected;
		Tracker.RotateDetected -= OnTrackerRotateDetected;
		Tracker.FlingDetected -= OnTrackerFlingDetected;
		Tracker.Flinging -= OnTrackerFlinging;
		Tracker.FlingCompleted -= OnTrackerFlingCompleted;
		Tracker.HoverDetected -= OnTrackerHoverDetected;
		Tracker.ScrollDetected -= OnTrackerScrollDetected;
		Tracker.GestureStarted -= OnTrackerGestureStarted;
		Tracker.GestureEnded -= OnTrackerGestureEnded;
		Tracker.TransformChanged -= OnTrackerTransformChanged;
		Tracker.DragStarted -= OnTrackerDragStarted;
		Tracker.DragUpdated -= OnTrackerDragUpdated;
		Tracker.DragEnded -= OnTrackerDragEnded;
	}

	private void OnTrackerTapDetected(object? s, SKTapEventArgs e) => OnTapDetected(e);
	private void OnTrackerDoubleTapDetected(object? s, SKTapEventArgs e) => OnDoubleTapDetected(e);
	private void OnTrackerLongPressDetected(object? s, SKTapEventArgs e) => OnLongPressDetected(e);
	private void OnTrackerPanDetected(object? s, SKPanEventArgs e) => OnPanDetected(e);
	private void OnTrackerPinchDetected(object? s, SKPinchEventArgs e) => OnPinchDetected(e);
	private void OnTrackerRotateDetected(object? s, SKRotateEventArgs e) => OnRotateDetected(e);
	private void OnTrackerFlingDetected(object? s, SKFlingEventArgs e) => OnFlingDetected(e);
	private void OnTrackerFlinging(object? s, SKFlingEventArgs e) { OnFlinging(e); Invalidate(); }
	private void OnTrackerFlingCompleted(object? s, EventArgs e) { OnFlingCompleted(); Invalidate(); }
	private void OnTrackerHoverDetected(object? s, SKHoverEventArgs e) => OnHoverDetected(e);
	private void OnTrackerScrollDetected(object? s, SKScrollEventArgs e) => OnScrollDetected(e);
	private void OnTrackerGestureStarted(object? s, SKGestureStateEventArgs e) => OnGestureStarted(e);
	private void OnTrackerGestureEnded(object? s, SKGestureStateEventArgs e) => OnGestureEnded(e);
	private void OnTrackerTransformChanged(object? s, EventArgs e) { OnTransformChanged(); Invalidate(); }
	private void OnTrackerDragStarted(object? s, SKDragEventArgs e) => OnDragStarted(e);
	private void OnTrackerDragUpdated(object? s, SKDragEventArgs e) => OnDragUpdated(e);
	private void OnTrackerDragEnded(object? s, SKDragEventArgs e) => OnDragEnded(e);

	private void OnTouch(object? sender, SKTouchEventArgs e)
	{
		e.Handled = Tracker.ProcessTouch(e);

		if (e.Handled)
			Invalidate();
	}

	/// <inheritdoc/>
	internal override void OnPaintSurfaceCore(SKSurface surface, SKSize size)
	{
		// Update display scale (pixels / points)
		if (Width > 0)
			Tracker.DisplayScale = size.Width / (float)Width;

		base.OnPaintSurfaceCore(surface, size);
		
		// Update view size in point coordinates
		var scale = Tracker.DisplayScale;
		var pointWidth = scale > 0 ? (int)(size.Width / scale) : (int)size.Width;
		var pointHeight = scale > 0 ? (int)(size.Height / scale) : (int)size.Height;
		Tracker.SetViewSize(pointWidth, pointHeight);

		var info = new SKImageInfo(pointWidth, pointHeight);
		var args = new SKPaintSurfaceEventArgs(surface, info);
		PaintSurface?.Invoke(this, args);
	}

	private static void OnIsGestureEnabledChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is SKGestureSurfaceView view && newValue is bool enabled)
			view.Tracker.IsEnabled = enabled;
	}

	private static void OnTouchSlopChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is SKGestureSurfaceView view && newValue is float slop)
			view.Tracker.TouchSlop = slop;
	}

	private static void OnLongPressDurationChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is SKGestureSurfaceView view && newValue is int duration)
			view.Tracker.LongPressDuration = duration;
	}

	/// <inheritdoc/>
	protected override void OnHandlerChanging(HandlerChangingEventArgs args)
	{
		base.OnHandlerChanging(args);

		// Dispose tracker when handler is permanently disconnected
		if (args.NewHandler == null)
		{
			UnsubscribeTrackerEvents();
			Tracker.Dispose();
		}
	}
}
