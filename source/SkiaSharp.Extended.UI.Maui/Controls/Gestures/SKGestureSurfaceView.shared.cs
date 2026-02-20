using SkiaSharp.Extended.Gestures;
using SkiaSharp.Views.Maui;

namespace SkiaSharp.Extended.UI.Controls;

/// <summary>
/// A SkiaSharp view with built-in gesture recognition for touch interactions.
/// </summary>
/// <remarks>
/// <para>
/// This view extends <see cref="SKSurfaceView"/> to add comprehensive gesture detection including:
/// </para>
/// <list type="bullet">
///   <item><description>Single, double, and multi-tap detection</description></item>
///   <item><description>Long press detection</description></item>
///   <item><description>Pan/drag gestures</description></item>
///   <item><description>Pinch to zoom gestures</description></item>
///   <item><description>Rotation gestures</description></item>
///   <item><description>Fling (swipe) gesture detection with velocity</description></item>
///   <item><description>Hover detection for mouse/stylus</description></item>
/// </list>
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

	private readonly SKGestureEngine _engine;
	private SKCanvasView? _canvasView;
	private SKGLView? _glView;

	/// <summary>
	/// Creates a new instance of <see cref="SKGestureSurfaceView"/>.
	/// </summary>
	public SKGestureSurfaceView()
	{
		ResourceLoader<Themes.SKGestureSurfaceViewResources>.EnsureRegistered(this);

		_engine = new SKGestureEngine();
		SubscribeEngineEvents();

		Loaded += OnLoaded;
		Unloaded += OnUnloaded;

		DebugUtils.LogPropertyChanged(this);
	}

	/// <summary>
	/// Gets the underlying gesture engine for advanced scenarios.
	/// </summary>
	public SKGestureEngine Engine => _engine;

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
		SubscribeEngineEvents();
	}

	private void OnUnloaded(object? sender, EventArgs e)
	{
		UnsubscribeEngineEvents();
		_engine.Reset();
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

	private void OnEngineTapDetected(object? s, SKTapEventArgs e) => OnTapDetected(e);
	private void OnEngineDoubleTapDetected(object? s, SKTapEventArgs e) => OnDoubleTapDetected(e);
	private void OnEngineLongPressDetected(object? s, SKTapEventArgs e) => OnLongPressDetected(e);
	private void OnEnginePanDetected(object? s, SKPanEventArgs e) => OnPanDetected(e);
	private void OnEnginePinchDetected(object? s, SKPinchEventArgs e) => OnPinchDetected(e);
	private void OnEngineRotateDetected(object? s, SKRotateEventArgs e) => OnRotateDetected(e);
	private void OnEngineFlingDetected(object? s, SKFlingEventArgs e) => OnFlingDetected(e);
	private void OnEngineFlinging(object? s, SKFlingEventArgs e) => OnFlinging(e);
	private void OnEngineFlingCompleted(object? s, EventArgs e) => OnFlingCompleted();
	private void OnEngineHoverDetected(object? s, SKHoverEventArgs e) => OnHoverDetected(e);
	private void OnEngineScrollDetected(object? s, SKScrollEventArgs e) => OnScrollDetected(e);
	private void OnEngineGestureStarted(object? s, SKGestureStateEventArgs e) => OnGestureStarted(e);
	private void OnEngineGestureEnded(object? s, SKGestureStateEventArgs e) => OnGestureEnded(e);
	private void OnEngineDragStarted(object? s, SKDragEventArgs e) => OnDragStarted(e);
	private void OnEngineDragUpdated(object? s, SKDragEventArgs e) => OnDragUpdated(e);
	private void OnEngineDragEnded(object? s, SKDragEventArgs e) => OnDragEnded(e);

	private float _displayScale = GetDefaultDisplayScale();

	private void OnTouch(object? sender, SKTouchEventArgs e)
	{
		var isMouse = e.DeviceType == SKTouchDeviceType.Mouse;

		// Convert pixel coordinates to point coordinates (matching the pre-scaled canvas)
		var scale = _displayScale;
		var location = scale > 0 ? new SKPoint(e.Location.X / scale, e.Location.Y / scale) : e.Location;

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
				// Accept these to keep receiving hover/move events
				e.Handled = true;
				break;
			case SKTouchAction.WheelChanged:
				e.Handled = _engine.ProcessMouseWheel(location, 0, e.WheelDelta);
				break;
		}

		// Invalidate for visual feedback
		if (e.Handled)
			Invalidate();
	}

	/// <inheritdoc/>
	internal override void OnPaintSurfaceCore(SKSurface surface, SKSize size)
	{
		// Cache the display scale factor (pixels / points)
		if (Width > 0)
			_displayScale = size.Width / (float)Width;

		base.OnPaintSurfaceCore(surface, size);
		
		// Pass point-based dimensions (matching the pre-scaled canvas from base class)
		var scale = _displayScale;
		var pointWidth = scale > 0 ? (int)(size.Width / scale) : (int)size.Width;
		var pointHeight = scale > 0 ? (int)(size.Height / scale) : (int)size.Height;
		var info = new SKImageInfo(pointWidth, pointHeight);
		var args = new SKPaintSurfaceEventArgs(surface, info);
		PaintSurface?.Invoke(this, args);
	}

	private static void OnIsGestureEnabledChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is SKGestureSurfaceView view && newValue is bool enabled)
			view._engine.IsEnabled = enabled;
	}

	private static void OnTouchSlopChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is SKGestureSurfaceView view && newValue is float slop)
			view._engine.TouchSlop = slop;
	}

	private static void OnLongPressDurationChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is SKGestureSurfaceView view && newValue is int duration)
			view._engine.LongPressDuration = duration;
	}

	/// <inheritdoc/>
	protected override void OnHandlerChanging(HandlerChangingEventArgs args)
	{
		base.OnHandlerChanging(args);

		// Dispose engine when handler is permanently disconnected
		if (args.NewHandler == null)
		{
			UnsubscribeEngineEvents();
			_engine.Dispose();
		}
	}

	private static float GetDefaultDisplayScale()
	{
		try
		{
			return (float)DeviceDisplay.MainDisplayInfo.Density;
		}
		catch
		{
			return 1f;
		}
	}
}
