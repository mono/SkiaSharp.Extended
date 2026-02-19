using SkiaSharp.Extended.Gestures;

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
/// <para>Selection modes for sticker/image manipulation:</para>
/// <list type="bullet">
///   <item><description><see cref="Gestures.SKGestureSelectionMode.Immediate"/> - Start dragging immediately</description></item>
///   <item><description><see cref="Gestures.SKGestureSelectionMode.TapToSelect"/> - Tap to select, then drag</description></item>
///   <item><description><see cref="Gestures.SKGestureSelectionMode.LongPressToSelect"/> - Long press to select and drag</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// &lt;controls:SKGestureSurfaceView
///     SKGestureSelectionMode="TapToSelect"
///     TapDetected="OnTap"
///     PanDetected="OnPan"
///     PinchDetected="OnPinch"
///     RotateDetected="OnRotate" /&gt;
/// </code>
/// </example>
public class SKGestureSurfaceView : SKSurfaceView
{
	/// <summary>
	/// Identifies the <see cref="SKGestureSelectionMode"/> bindable property.
	/// </summary>
	public static readonly BindableProperty SKGestureSelectionModeProperty = BindableProperty.Create(
		nameof(GestureSelectionMode),
		typeof(SKGestureSelectionMode),
		typeof(SKGestureSurfaceView),
		SKGestureSelectionMode.Immediate,
		propertyChanged: OnGestureSelectionModeChanged);

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
	private IDispatcher? _dispatcher;
	private IDispatcherTimer? _longPressTimer;

	/// <summary>
	/// Creates a new instance of <see cref="SKGestureSurfaceView"/>.
	/// </summary>
	public SKGestureSurfaceView()
	{
		ResourceLoader<Themes.SKGestureSurfaceViewResources>.EnsureRegistered(this);

		_engine = new SKGestureEngine();
		
		// Wire up engine events
		_engine.TapDetected += (s, e) => OnTapDetected(e);
		_engine.DoubleTapDetected += (s, e) => OnDoubleTapDetected(e);
		_engine.LongPressDetected += (s, e) => OnLongPressDetected(e);
		_engine.PanDetected += (s, e) => OnPanDetected(e);
		_engine.PinchDetected += (s, e) => OnPinchDetected(e);
		_engine.RotateDetected += (s, e) => OnRotateDetected(e);
		_engine.FlingDetected += (s, e) => OnFlingDetected(e);
		_engine.HoverDetected += (s, e) => OnHoverDetected(e);
		_engine.GestureStarted += (s, e) => OnGestureStarted(e);
		_engine.GestureEnded += (s, e) => OnGestureEnded(e);
		_engine.SelectionChanged += (s, e) => OnSelectionChanged(e);
		_engine.DragStarted += (s, e) => OnDragStarted(e);
		_engine.DragUpdated += (s, e) => OnDragUpdated(e);
		_engine.DragEnded += (s, e) => OnDragEnded(e);

		Loaded += OnLoaded;
		Unloaded += OnUnloaded;

		DebugUtils.LogPropertyChanged(this);
	}

	/// <summary>
	/// Gets the underlying gesture engine for advanced scenarios.
	/// </summary>
	public SKGestureEngine Engine => _engine;

	/// <summary>
	/// Gets or sets the selection mode for gesture handling.
	/// </summary>
	public SKGestureSelectionMode GestureSelectionMode
	{
		get => (SKGestureSelectionMode)GetValue(SKGestureSelectionModeProperty);
		set => SetValue(SKGestureSelectionModeProperty, value);
	}

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

	/// <summary>
	/// Gets or sets the currently selected item ID.
	/// </summary>
	public long? SelectedItemId
	{
		get => _engine.SelectedItemId;
		set => _engine.SelectedItemId = value;
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
	/// Occurs when a hover is detected.
	/// </summary>
	public event EventHandler<SKHoverEventArgs>? HoverDetected;

	/// <summary>
	/// Occurs when a gesture starts.
	/// </summary>
	public event EventHandler<SKGestureStateEventArgs>? GestureStarted;

	/// <summary>
	/// Occurs when a gesture ends.
	/// </summary>
	public event EventHandler<SKGestureStateEventArgs>? GestureEnded;

	/// <summary>
	/// Occurs when selection changes.
	/// </summary>
	public event EventHandler<SKSelectionChangedEventArgs>? SelectionChanged;

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

	/// <summary>Invokes <see cref="HoverDetected"/>.</summary>
	protected virtual void OnHoverDetected(SKHoverEventArgs e) => HoverDetected?.Invoke(this, e);

	/// <summary>Invokes <see cref="GestureStarted"/>.</summary>
	protected virtual void OnGestureStarted(SKGestureStateEventArgs e) => GestureStarted?.Invoke(this, e);

	/// <summary>Invokes <see cref="GestureEnded"/>.</summary>
	protected virtual void OnGestureEnded(SKGestureStateEventArgs e) => GestureEnded?.Invoke(this, e);

	/// <summary>Invokes <see cref="SelectionChanged"/>.</summary>
	protected virtual void OnSelectionChanged(SKSelectionChangedEventArgs e) => SelectionChanged?.Invoke(this, e);

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
		// Unsubscribe from old view
		if (_canvasView is not null)
		{
			_canvasView.Touch -= OnTouch;
			_canvasView = null;
		}

		base.OnApplyTemplate();

		// Get canvas view and subscribe to touch
		var templateChild = GetTemplateChild("PART_DrawingSurface");
		if (templateChild is SKCanvasView view)
		{
			_canvasView = view;
			_canvasView.EnableTouchEvents = true;
			_canvasView.Touch += OnTouch;
		}
	}

	private void OnLoaded(object? sender, EventArgs e)
	{
		_dispatcher = Dispatcher;
		StartLongPressTimer();
	}

	private void OnUnloaded(object? sender, EventArgs e)
	{
		StopLongPressTimer();
		_engine.Reset();
	}

	private void OnTouch(object? sender, SKTouchEventArgs e)
	{
		var isMouse = e.DeviceType == SKTouchDeviceType.Mouse;

		switch (e.ActionType)
		{
			case SKTouchAction.Pressed:
				e.Handled = _engine.ProcessTouchDown(e.Id, e.Location, isMouse);
				break;
			case SKTouchAction.Moved:
				e.Handled = _engine.ProcessTouchMove(e.Id, e.Location, e.InContact);
				break;
			case SKTouchAction.Released:
				e.Handled = _engine.ProcessTouchUp(e.Id, e.Location, isMouse);
				break;
			case SKTouchAction.Cancelled:
				e.Handled = _engine.ProcessTouchCancel(e.Id);
				break;
		}

		// Invalidate for visual feedback
		if (e.Handled)
			Invalidate();
	}

	private void StartLongPressTimer()
	{
		if (_dispatcher is null)
			return;

		_longPressTimer = _dispatcher.CreateTimer();
		_longPressTimer.Interval = TimeSpan.FromMilliseconds(100);
		_longPressTimer.Tick += (s, e) => _engine.CheckLongPress();
		_longPressTimer.Start();
	}

	private void StopLongPressTimer()
	{
		_longPressTimer?.Stop();
		_longPressTimer = null;
	}

	private static void OnGestureSelectionModeChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is SKGestureSurfaceView view && newValue is SKGestureSelectionMode mode)
			view._engine.SelectionMode = mode;
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
}
