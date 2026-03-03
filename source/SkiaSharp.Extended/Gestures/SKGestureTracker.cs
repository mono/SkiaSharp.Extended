using System;
using System.Threading;

namespace SkiaSharp.Extended;

/// <summary>
/// A high-level gesture handler that tracks touch input and maintains an absolute transform
/// (scale, rotation, and offset) by consuming events from an internal <see cref="SKGestureDetector"/>.
/// </summary>
/// <remarks>
/// <para>The tracker is the primary public API for gesture handling. It accepts raw touch input
/// via <see cref="ProcessTouchDown"/>, <see cref="ProcessTouchMove"/>, <see cref="ProcessTouchUp"/>,
/// and <see cref="ProcessMouseWheel"/>, detects gestures internally, and translates them into
/// transform state changes and higher-level events such as drag lifecycle and fling animation.</para>
/// <para>Use the <see cref="Matrix"/> property to apply the current transform when painting.
/// The matrix uses (0,0) as its origin — no view size configuration is required.</para>
/// <para>All coordinates are in view (screen) space. The tracker converts screen-space deltas
/// to content-space deltas internally when updating <see cref="Offset"/>.</para>
/// <example>
/// <para>Basic usage with an SkiaSharp canvas:</para>
/// <code>
/// var tracker = new SKGestureTracker();
///
/// // Forward touch events from your platform:
/// tracker.ProcessTouchDown(id, new SKPoint(x, y));
/// tracker.ProcessTouchMove(id, new SKPoint(x, y));
/// tracker.ProcessTouchUp(id, new SKPoint(x, y));
///
/// // In your paint handler:
/// canvas.SetMatrix(tracker.Matrix);
/// // Draw your content...
///
/// // Listen for transform changes to trigger redraws:
/// tracker.TransformChanged += (s, e) => canvas.InvalidateVisual();
/// </code>
/// </example>
/// <seealso cref="SKGestureDetector"/>
/// <seealso cref="SKGestureTrackerOptions"/>
/// </remarks>
public sealed class SKGestureTracker : IDisposable
{
	private readonly SKGestureDetector _engine;
	private SynchronizationContext? _syncContext;
	private bool _disposed;

	// Transform state
	private float _scale = 1f;
	private float _rotation;
	private SKPoint _offset = SKPoint.Empty;

	// Drag lifecycle state
	private bool _isDragging;
	private bool _isDragHandled;
	private SKPoint _lastPanLocation;
	private SKPoint _prevPanLocation;

	// Fling animation state
	private Timer? _flingTimer;
	private int _flingToken;
	private float _flingVelocityX;
	private float _flingVelocityY;
	private bool _isFlinging;
	private long _flingLastFrameTimestamp; // TimeProvider() ticks at last fling frame

	// Zoom animation state
	private Timer? _zoomTimer;
	private int _zoomToken;
	private bool _isZoomAnimating;
	private float _zoomStartScale;
	private float _zoomTargetFactor;
	private SKPoint _zoomFocalPoint;
	private long _zoomStartTicks;
	private float _zoomPrevCumulative;

	/// <summary>
	/// Initializes a new instance of the <see cref="SKGestureTracker"/> class with default options.
	/// </summary>
	public SKGestureTracker()
		: this(new SKGestureTrackerOptions())
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="SKGestureTracker"/> class with the specified options.
	/// </summary>
	/// <param name="options">The configuration options for gesture detection and tracking.</param>
	/// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
	public SKGestureTracker(SKGestureTrackerOptions options)
	{
		Options = options ?? throw new ArgumentNullException(nameof(options));
		_engine = new SKGestureDetector(options);
		SubscribeEngineEvents();
	}

	/// <summary>
	/// Gets the configuration options for this tracker.
	/// </summary>
	/// <value>The <see cref="SKGestureTrackerOptions"/> instance controlling gesture detection
	/// thresholds, transform limits, and animation parameters.</value>
	public SKGestureTrackerOptions Options { get; }

	#region Touch Input

	/// <summary>
	/// Processes a touch down event and forwards it to the internal gesture detector.
	/// </summary>
	/// <param name="id">The unique identifier for this touch pointer.</param>
	/// <param name="location">The location of the touch in view coordinates.</param>
	/// <param name="isMouse">Whether this event originates from a mouse device.</param>
	/// <returns><see langword="true"/> if the event was processed; otherwise, <see langword="false"/>.</returns>
	public bool ProcessTouchDown(long id, SKPoint location, bool isMouse = false)
		=> _engine.ProcessTouchDown(id, location, isMouse);

	/// <summary>
	/// Processes a touch move event and forwards it to the internal gesture detector.
	/// </summary>
	/// <param name="id">The unique identifier for this touch pointer.</param>
	/// <param name="location">The new location of the touch in view coordinates.</param>
	/// <param name="inContact">
	/// <see langword="true"/> if the touch is in contact with the surface; <see langword="false"/>
	/// for hover (mouse move without button pressed).
	/// </param>
	/// <returns><see langword="true"/> if the event was processed; otherwise, <see langword="false"/>.</returns>
	public bool ProcessTouchMove(long id, SKPoint location, bool inContact = true)
		=> _engine.ProcessTouchMove(id, location, inContact);

	/// <summary>
	/// Processes a touch up event and forwards it to the internal gesture detector.
	/// </summary>
	/// <param name="id">The unique identifier for this touch pointer.</param>
	/// <param name="location">The final location of the touch in view coordinates.</param>
	/// <param name="isMouse">Whether this event originates from a mouse device.</param>
	/// <returns><see langword="true"/> if the event was processed; otherwise, <see langword="false"/>.</returns>
	public bool ProcessTouchUp(long id, SKPoint location, bool isMouse = false)
		=> _engine.ProcessTouchUp(id, location, isMouse);

	/// <summary>
	/// Processes a touch cancel event and forwards it to the internal gesture detector.
	/// </summary>
	/// <param name="id">The unique identifier for the cancelled touch pointer.</param>
	/// <returns><see langword="true"/> if the event was processed; otherwise, <see langword="false"/>.</returns>
	public bool ProcessTouchCancel(long id)
		=> _engine.ProcessTouchCancel(id);

	/// <summary>
	/// Processes a mouse wheel (scroll) event and forwards it to the internal gesture detector.
	/// </summary>
	/// <param name="location">The position of the mouse cursor in view coordinates.</param>
	/// <param name="deltaX">The horizontal scroll delta.</param>
	/// <param name="deltaY">The vertical scroll delta.</param>
	/// <returns><see langword="true"/> if the event was processed; otherwise, <see langword="false"/>.</returns>
	public bool ProcessMouseWheel(SKPoint location, float deltaX, float deltaY)
		=> _engine.ProcessMouseWheel(location, deltaX, deltaY);

	#endregion

	#region Detection Config (forwarded to engine)

	/// <summary>
	/// Gets or sets a value indicating whether gesture detection is enabled.
	/// </summary>
	/// <value>
	/// <see langword="true"/> if the tracker processes touch events; otherwise, <see langword="false"/>.
	/// The default is <see langword="true"/>.
	/// </value>
	public bool IsEnabled
	{
		get => _engine.IsEnabled;
		set => _engine.IsEnabled = value;
	}

	/// <summary>
	/// Gets or sets the time provider function used to obtain the current time in ticks.
	/// </summary>
	/// <value>
	/// A <see cref="Func{T}"/> that returns the current time in <see cref="DateTime.Ticks"/>.
	/// Override this for deterministic testing.
	/// </value>
	public Func<long> TimeProvider
	{
		get => _engine.TimeProvider;
		set => _engine.TimeProvider = value;
	}

	#endregion

	#region Transform State (read-only)

	/// <summary>
	/// Gets the current zoom scale factor.
	/// </summary>
	/// <value>
	/// The absolute scale factor. A value of <c>1.0</c> represents the original (unscaled) view.
	/// Values greater than <c>1.0</c> are zoomed in; values less than <c>1.0</c> are zoomed out.
	/// The value is clamped between <see cref="SKGestureTrackerOptions.MinScale"/> and
	/// <see cref="SKGestureTrackerOptions.MaxScale"/>.
	/// </value>
	public float Scale => _scale;

	/// <summary>
	/// Gets the current rotation angle in degrees.
	/// </summary>
	/// <value>The cumulative rotation in degrees. Positive values are clockwise.</value>
	public float Rotation => _rotation;

	/// <summary>
	/// Gets the current pan offset in content coordinates.
	/// </summary>
	/// <value>An <see cref="SKPoint"/> representing the translation offset applied after scale and rotation.</value>
	/// <remarks>
	/// The offset is in content (post-scale, post-rotation) coordinate space, not screen space.
	/// Screen-space deltas are converted to content-space internally by accounting for the
	/// current <see cref="Scale"/> and <see cref="Rotation"/>.
	/// </remarks>
	public SKPoint Offset => _offset;

	/// <summary>
	/// Gets the composite transform matrix that combines scale, rotation, and offset,
	/// using (0,0) as the transform origin.
	/// </summary>
	/// <value>
	/// An <see cref="SKMatrix"/> that can be applied to an <see cref="SKCanvas"/> to render content
	/// with the current gesture transform. The matrix applies transformations in the order:
	/// scale, rotate, translate.
	/// </value>
	public SKMatrix Matrix
	{
		get
		{
			var m = SKMatrix.CreateScale(_scale, _scale);
			m = m.PreConcat(SKMatrix.CreateRotationDegrees(_rotation));
			m = m.PreConcat(SKMatrix.CreateTranslation(_offset.X, _offset.Y));
			return m;
		}
	}

	#endregion

	#region Feature Toggles

	/// <summary>Gets or sets a value indicating whether tap detection is enabled.</summary>
	/// <value><see langword="true"/> to detect single taps; otherwise, <see langword="false"/>. The default is <see langword="true"/>.</value>
	public bool IsTapEnabled { get => Options.IsTapEnabled; set => Options.IsTapEnabled = value; }

	/// <summary>Gets or sets a value indicating whether double-tap detection is enabled.</summary>
	/// <value><see langword="true"/> to detect double taps; otherwise, <see langword="false"/>. The default is <see langword="true"/>.</value>
	public bool IsDoubleTapEnabled { get => Options.IsDoubleTapEnabled; set => Options.IsDoubleTapEnabled = value; }

	/// <summary>Gets or sets a value indicating whether long press detection is enabled.</summary>
	/// <value><see langword="true"/> to detect long presses; otherwise, <see langword="false"/>. The default is <see langword="true"/>.</value>
	public bool IsLongPressEnabled { get => Options.IsLongPressEnabled; set => Options.IsLongPressEnabled = value; }

	/// <summary>Gets or sets a value indicating whether pan gestures update the <see cref="Offset"/>.</summary>
	/// <value><see langword="true"/> to apply pan deltas to the offset; otherwise, <see langword="false"/>. The default is <see langword="true"/>.</value>
	public bool IsPanEnabled { get => Options.IsPanEnabled; set => Options.IsPanEnabled = value; }

	/// <summary>Gets or sets a value indicating whether pinch-to-zoom gestures update the <see cref="Scale"/>.</summary>
	/// <value><see langword="true"/> to apply pinch scale changes; otherwise, <see langword="false"/>. The default is <see langword="true"/>.</value>
	public bool IsPinchEnabled { get => Options.IsPinchEnabled; set => Options.IsPinchEnabled = value; }

	/// <summary>Gets or sets a value indicating whether rotation gestures update the <see cref="Rotation"/>.</summary>
	/// <value><see langword="true"/> to apply rotation changes; otherwise, <see langword="false"/>. The default is <see langword="true"/>.</value>
	public bool IsRotateEnabled { get => Options.IsRotateEnabled; set => Options.IsRotateEnabled = value; }

	/// <summary>Gets or sets a value indicating whether fling (inertia) animation is enabled after a pan gesture.</summary>
	/// <value><see langword="true"/> to run fling animations; otherwise, <see langword="false"/>. The default is <see langword="true"/>.</value>
	public bool IsFlingEnabled { get => Options.IsFlingEnabled; set => Options.IsFlingEnabled = value; }

	/// <summary>Gets or sets a value indicating whether double-tap triggers an animated zoom.</summary>
	/// <value><see langword="true"/> to enable double-tap zoom; otherwise, <see langword="false"/>. The default is <see langword="true"/>.</value>
	public bool IsDoubleTapZoomEnabled { get => Options.IsDoubleTapZoomEnabled; set => Options.IsDoubleTapZoomEnabled = value; }

	/// <summary>Gets or sets a value indicating whether scroll-wheel events trigger zoom.</summary>
	/// <value><see langword="true"/> to enable scroll-wheel zoom; otherwise, <see langword="false"/>. The default is <see langword="true"/>.</value>
	public bool IsScrollZoomEnabled { get => Options.IsScrollZoomEnabled; set => Options.IsScrollZoomEnabled = value; }

	/// <summary>Gets or sets a value indicating whether hover (mouse move without contact) detection is enabled.</summary>
	/// <value><see langword="true"/> to detect hover events; otherwise, <see langword="false"/>. The default is <see langword="true"/>.</value>
	public bool IsHoverEnabled { get => Options.IsHoverEnabled; set => Options.IsHoverEnabled = value; }

	#endregion

	#region Animation State

	/// <summary>Gets a value indicating whether an animated zoom (from double-tap or <see cref="ZoomTo"/>) is in progress.</summary>
	/// <value><see langword="true"/> if a zoom animation is running; otherwise, <see langword="false"/>.</value>
	public bool IsZoomAnimating => _isZoomAnimating;

	/// <summary>Gets a value indicating whether a fling (inertia) animation is in progress.</summary>
	/// <value><see langword="true"/> if a fling animation is running; otherwise, <see langword="false"/>.</value>
	public bool IsFlinging => _isFlinging;

	/// <summary>Gets a value indicating whether any gesture is currently active (touch contact in progress).</summary>
	/// <value><see langword="true"/> if the internal detector is tracking an active gesture; otherwise, <see langword="false"/>.</value>
	public bool IsGestureActive => _engine.IsGestureActive;

	#endregion

	#region Gesture Events (forwarded from engine)

	/// <summary>Occurs when a single tap is detected. Forwarded from the internal <see cref="SKGestureDetector"/>.</summary>
	public event EventHandler<SKTapGestureEventArgs>? TapDetected;

	/// <summary>Occurs when a double tap is detected. Forwarded from the internal <see cref="SKGestureDetector"/>.</summary>
	public event EventHandler<SKTapGestureEventArgs>? DoubleTapDetected;

	/// <summary>Occurs when a long press is detected. Forwarded from the internal <see cref="SKGestureDetector"/>.</summary>
	public event EventHandler<SKLongPressGestureEventArgs>? LongPressDetected;

	/// <summary>Occurs when a pan gesture is detected. Forwarded from the internal <see cref="SKGestureDetector"/>.</summary>
	public event EventHandler<SKPanGestureEventArgs>? PanDetected;

	/// <summary>Occurs when a pinch (scale) gesture is detected. Forwarded from the internal <see cref="SKGestureDetector"/>.</summary>
	public event EventHandler<SKPinchGestureEventArgs>? PinchDetected;

	/// <summary>Occurs when a rotation gesture is detected. Forwarded from the internal <see cref="SKGestureDetector"/>.</summary>
	public event EventHandler<SKRotateGestureEventArgs>? RotateDetected;

	/// <summary>Occurs when a fling gesture is detected (once, with initial velocity). Forwarded from the internal <see cref="SKGestureDetector"/>.</summary>
	public event EventHandler<SKFlingGestureEventArgs>? FlingDetected;

	/// <summary>Occurs when a hover is detected. Forwarded from the internal <see cref="SKGestureDetector"/>.</summary>
	public event EventHandler<SKHoverGestureEventArgs>? HoverDetected;

	/// <summary>Occurs when a scroll (mouse wheel) event is detected. Forwarded from the internal <see cref="SKGestureDetector"/>.</summary>
	public event EventHandler<SKScrollGestureEventArgs>? ScrollDetected;

	/// <summary>Occurs when a gesture interaction begins (first touch contact). Forwarded from the internal <see cref="SKGestureDetector"/>.</summary>
	public event EventHandler<SKGestureLifecycleEventArgs>? GestureStarted;

	/// <summary>Occurs when a gesture interaction ends (last touch released). Forwarded from the internal <see cref="SKGestureDetector"/>.</summary>
	public event EventHandler<SKGestureLifecycleEventArgs>? GestureEnded;

	#endregion

	#region Tracker Events

	/// <summary>
	/// Occurs when the transform state (<see cref="Scale"/>, <see cref="Rotation"/>,
	/// <see cref="Offset"/>, or <see cref="Matrix"/>) changes.
	/// </summary>
	/// <remarks>
	/// Subscribe to this event to trigger canvas redraws when the user interacts with the view.
	/// This event fires for pan, pinch, rotation, fling animation frames, zoom animation frames,
	/// and programmatic transform changes via <see cref="SetTransform"/>, <see cref="SetScale"/>,
	/// <see cref="SetRotation"/>, <see cref="SetOffset"/>, or <see cref="Reset"/>.
	/// </remarks>
	public event EventHandler? TransformChanged;

	/// <summary>
	/// Occurs when a drag operation starts (first pan movement after touch down).
	/// </summary>
	/// <remarks>
	/// Set <see cref="SKDragGestureEventArgs.Handled"/> to <see langword="true"/> to prevent the
	/// tracker from updating <see cref="Offset"/> for this drag (useful for custom object dragging).
	/// </remarks>
	public event EventHandler<SKDragGestureEventArgs>? DragStarted;

	/// <summary>
	/// Occurs on each movement during a drag operation.
	/// </summary>
	public event EventHandler<SKDragGestureEventArgs>? DragUpdated;

	/// <summary>
	/// Occurs when a drag operation ends (all touches released).
	/// </summary>
	public event EventHandler<SKDragGestureEventArgs>? DragEnded;

	/// <summary>
	/// Occurs each animation frame during a fling deceleration.
	/// </summary>
	/// <remarks>
	/// The <see cref="SKFlingGestureEventArgs.Delta"/>
	/// properties contain the per-frame displacement. The velocity decays each frame according to
	/// <see cref="SKGestureTrackerOptions.FlingFriction"/>.
	/// </remarks>
	public event EventHandler<SKFlingGestureEventArgs>? FlingUpdated;

	/// <summary>
	/// Occurs when a fling animation completes (velocity drops below
	/// <see cref="SKGestureTrackerOptions.FlingMinVelocity"/>).
	/// </summary>
	public event EventHandler? FlingCompleted;

	#endregion

	#region Public Methods

	/// <summary>
	/// Sets the transform to the specified values, clamping scale to
	/// <see cref="SKGestureTrackerOptions.MinScale"/>/<see cref="SKGestureTrackerOptions.MaxScale"/>.
	/// </summary>
	/// <param name="scale">The desired zoom scale factor.</param>
	/// <param name="rotation">The desired rotation angle in degrees.</param>
	/// <param name="offset">The desired pan offset in content coordinates.</param>
	public void SetTransform(float scale, float rotation, SKPoint offset)
	{
		_scale = Clamp(scale, Options.MinScale, Options.MaxScale);
		_rotation = rotation;
		_offset = offset;
		TransformChanged?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>
	/// Sets the zoom scale, clamping to <see cref="SKGestureTrackerOptions.MinScale"/>/<see cref="SKGestureTrackerOptions.MaxScale"/>,
	/// and raises <see cref="TransformChanged"/>.
	/// </summary>
	/// <param name="scale">The desired zoom scale factor.</param>
	/// <param name="pivot">Optional pivot point in view coordinates. When provided, the offset is
	/// adjusted so the pivot point remains stationary after the scale change.</param>
	public void SetScale(float scale, SKPoint? pivot = null)
	{
		var newScale = Clamp(scale, Options.MinScale, Options.MaxScale);
		if (pivot.HasValue)
			AdjustOffsetForPivot(pivot.Value, _scale, newScale, _rotation, _rotation);
		_scale = newScale;
		TransformChanged?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>
	/// Sets the rotation angle and raises <see cref="TransformChanged"/>.
	/// </summary>
	/// <param name="rotation">The desired rotation angle in degrees.</param>
	/// <param name="pivot">Optional pivot point in view coordinates. When provided, the offset is
	/// adjusted so the pivot point remains stationary after the rotation change.</param>
	public void SetRotation(float rotation, SKPoint? pivot = null)
	{
		if (pivot.HasValue)
			AdjustOffsetForPivot(pivot.Value, _scale, _scale, _rotation, rotation);
		_rotation = rotation;
		TransformChanged?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>
	/// Sets the pan offset and raises <see cref="TransformChanged"/>.
	/// </summary>
	/// <param name="offset">The desired pan offset in content coordinates.</param>
	public void SetOffset(SKPoint offset)
	{
		_offset = offset;
		TransformChanged?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>
	/// Starts an animated zoom by the given multiplicative factor around a focal point.
	/// </summary>
	/// <param name="factor">The scale multiplier to apply (e.g., <c>2.0</c> to double the current zoom).</param>
	/// <param name="focalPoint">The point in view coordinates to zoom towards.</param>
	/// <remarks>
	/// The animation uses a cubic-out easing curve and runs for
	/// <see cref="SKGestureTrackerOptions.ZoomAnimationDuration"/> milliseconds. Any previously
	/// running zoom animation is stopped before the new one begins.
	/// </remarks>
	public void ZoomTo(float factor, SKPoint focalPoint)
	{
		if (factor <= 0 || float.IsNaN(factor) || float.IsInfinity(factor))
			throw new ArgumentOutOfRangeException(nameof(factor), factor, "Factor must be a positive finite number.");

		StopZoomAnimation();
		_syncContext ??= SynchronizationContext.Current;

		_zoomStartScale = _scale;
		_zoomTargetFactor = factor;
		_zoomFocalPoint = focalPoint;
		_zoomStartTicks = TimeProvider();
		_zoomPrevCumulative = 1f;
		_isZoomAnimating = true;

		var token = Interlocked.Increment(ref _zoomToken);
		_zoomTimer = new Timer(
			OnZoomTimerTick,
			token,
			Options.ZoomAnimationInterval,
			Options.ZoomAnimationInterval);
	}

	/// <summary>Stops any active zoom animation immediately.</summary>
	public void StopZoomAnimation()
	{
		if (!_isZoomAnimating)
			return;

		_isZoomAnimating = false;
		Interlocked.Increment(ref _zoomToken);
		var timer = _zoomTimer;
		_zoomTimer = null;
		timer?.Change(Timeout.Infinite, Timeout.Infinite);
		timer?.Dispose();
	}

	/// <summary>
	/// Stops any active fling animation and raises <see cref="FlingCompleted"/>.
	/// </summary>
	public void StopFling()
	{
		if (!_isFlinging)
			return;

		CancelFlingInternal();
		FlingCompleted?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>Cancels any active fling animation without raising <see cref="FlingCompleted"/>.</summary>
	private void CancelFlingInternal()
	{
		if (!_isFlinging)
			return;

		_isFlinging = false;
		_flingVelocityX = 0;
		_flingVelocityY = 0;
		Interlocked.Increment(ref _flingToken);
		var timer = _flingTimer;
		_flingTimer = null;
		timer?.Change(Timeout.Infinite, Timeout.Infinite);
		timer?.Dispose();
	}

	/// <summary>
	/// Resets the tracker to an identity transform (scale 1, rotation 0, offset zero), stops all
	/// animations, and raises <see cref="TransformChanged"/>.
	/// </summary>
	public void Reset()
	{
		StopFling();
		StopZoomAnimation();
		_scale = 1f;
		_rotation = 0f;
		_offset = SKPoint.Empty;
		_isDragging = false;
		_engine.Reset();
		TransformChanged?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>
	/// Releases all resources used by this <see cref="SKGestureTracker"/> instance, including
	/// stopping all animations and disposing the internal <see cref="SKGestureDetector"/>.
	/// </summary>
	public void Dispose()
	{
		if (_disposed)
			return;

		_disposed = true;
		StopFling();
		StopZoomAnimation();
		UnsubscribeEngineEvents();
		_engine.Dispose();
	}

	#endregion

	#region Engine Event Subscriptions

	private void SubscribeEngineEvents()
	{
		_engine.TapDetected += OnEngineTapDetected;
		_engine.DoubleTapDetected += OnEngineDoubleTapDetected;
		_engine.LongPressDetected += OnEngineLongPressDetected;
		_engine.PanDetected += OnEnginePanDetected;
		_engine.PinchDetected += OnEnginePinchDetected;
		_engine.RotateDetected += OnEngineRotateDetected;
		_engine.FlingDetected += OnEngineFlingDetected;
		_engine.HoverDetected += OnEngineHoverDetected;
		_engine.ScrollDetected += OnEngineScrollDetected;
		_engine.GestureStarted += OnEngineGestureStarted;
		_engine.GestureEnded += OnEngineGestureEnded;
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
		_engine.HoverDetected -= OnEngineHoverDetected;
		_engine.ScrollDetected -= OnEngineScrollDetected;
		_engine.GestureStarted -= OnEngineGestureStarted;
		_engine.GestureEnded -= OnEngineGestureEnded;
	}

	#endregion

	#region Engine Event Handlers

	private void OnEngineTapDetected(object? s, SKTapGestureEventArgs e)
	{
		if (!IsTapEnabled)
			return;
		TapDetected?.Invoke(this, e);
	}

	private void OnEngineDoubleTapDetected(object? s, SKTapGestureEventArgs e)
	{
		if (!IsDoubleTapEnabled)
			return;

		DoubleTapDetected?.Invoke(this, e);

		// If the consumer handled the event (e.g. sticker selection), skip zoom
		if (e.Handled)
			return;

		if (!IsDoubleTapZoomEnabled)
			return;

		if (_scale >= Options.MaxScale - 0.01f)
		{
			// At max zoom — animate reset to 1.0
			ZoomTo(1f / _scale, e.Location);
		}
		else
		{
			var factor = Math.Min(Options.DoubleTapZoomFactor, Options.MaxScale / _scale);
			ZoomTo(factor, e.Location);
		}
	}

	private void OnEngineLongPressDetected(object? s, SKLongPressGestureEventArgs e)
	{
		if (!IsLongPressEnabled)
			return;
		LongPressDetected?.Invoke(this, e);
	}

	private void OnEnginePanDetected(object? s, SKPanGestureEventArgs e)
	{
		if (IsPanEnabled)
			PanDetected?.Invoke(this, e);

		// Track last pan position for DragEnded
		_prevPanLocation = _lastPanLocation;
		_lastPanLocation = e.Location;

		// Derive drag lifecycle
		SKDragGestureEventArgs? dragArgs = null;
		if (!_isDragging)
		{
			_isDragging = true;
			_isDragHandled = false;
			_lastPanLocation = e.Location;
			dragArgs = new SKDragGestureEventArgs(e.Location, e.PrevLocation);
			DragStarted?.Invoke(this, dragArgs);
		}
		else
		{
			dragArgs = new SKDragGestureEventArgs(e.Location, e.PrevLocation);
			DragUpdated?.Invoke(this, dragArgs);
		}

		// Track whether the consumer is handling this drag (e.g. sticker drag)
		if (dragArgs?.Handled ?? false)
			_isDragHandled = true;

		// Skip offset update if consumer handled the pan or drag (e.g. sticker drag)
		if (!IsPanEnabled || e.Handled || _isDragHandled)
			return;

		// Update offset
		var d = ScreenToContentDelta(e.Delta.X, e.Delta.Y);
		_offset = new SKPoint(_offset.X + d.X, _offset.Y + d.Y);
		TransformChanged?.Invoke(this, EventArgs.Empty);
	}

	private void OnEnginePinchDetected(object? s, SKPinchGestureEventArgs e)
	{
		PinchDetected?.Invoke(this, e);

		// Apply center movement as pan
		if (IsPanEnabled)
		{
			var panDelta = ScreenToContentDelta(
				e.FocalPoint.X - e.PreviousFocalPoint.X,
				e.FocalPoint.Y - e.PreviousFocalPoint.Y);
			_offset = new SKPoint(_offset.X + panDelta.X, _offset.Y + panDelta.Y);
		}

		if (IsPinchEnabled)
		{
			var newScale = Clamp(_scale * e.ScaleDelta, Options.MinScale, Options.MaxScale);
			AdjustOffsetForPivot(e.FocalPoint, _scale, newScale, _rotation, _rotation);
			_scale = newScale;
		}

		// TransformChanged is deferred to OnEngineRotateDetected, which always fires
		// immediately after this handler for the same gesture frame, to avoid two
		// notifications per two-finger move frame.
	}

	private void OnEngineRotateDetected(object? s, SKRotateGestureEventArgs e)
	{
		RotateDetected?.Invoke(this, e);

		if (IsRotateEnabled)
		{
			var newRotation = _rotation + e.RotationDelta;
			AdjustOffsetForPivot(e.FocalPoint, _scale, _scale, _rotation, newRotation);
			_rotation = newRotation;
		}

		// Fire TransformChanged once per two-finger frame (batched with pinch changes above)
		TransformChanged?.Invoke(this, EventArgs.Empty);
	}

	private void OnEngineFlingDetected(object? s, SKFlingGestureEventArgs e)
	{
		FlingDetected?.Invoke(this, e);

		// Don't fling if the drag was handled by the consumer (e.g. sticker drag)
		if (!IsFlingEnabled || _isDragHandled)
			return;

		StartFlingAnimation(e.Velocity.X, e.Velocity.Y);
	}

	private void OnEngineHoverDetected(object? s, SKHoverGestureEventArgs e)
	{
		if (!IsHoverEnabled)
			return;
		HoverDetected?.Invoke(this, e);
	}

	private void OnEngineScrollDetected(object? s, SKScrollGestureEventArgs e)
	{
		ScrollDetected?.Invoke(this, e);

		if (!IsScrollZoomEnabled || e.Delta.Y == 0)
			return;

		var scaleDelta = 1f + e.Delta.Y * Options.ScrollZoomFactor;
		var newScale = Clamp(_scale * scaleDelta, Options.MinScale, Options.MaxScale);
		AdjustOffsetForPivot(e.Location, _scale, newScale, _rotation, _rotation);
		_scale = newScale;
		TransformChanged?.Invoke(this, EventArgs.Empty);
	}

	private void OnEngineGestureStarted(object? s, SKGestureLifecycleEventArgs e)
	{
		_syncContext ??= SynchronizationContext.Current;
		CancelFlingInternal(); // Don't fire FlingCompleted — fling was interrupted by new touch
		StopZoomAnimation();
		GestureStarted?.Invoke(this, new SKGestureLifecycleEventArgs());
	}

	private void OnEngineGestureEnded(object? s, SKGestureLifecycleEventArgs e)
	{
		if (_isDragging)
		{
			_isDragging = false;
			_isDragHandled = false;
			DragEnded?.Invoke(this, new SKDragGestureEventArgs(_lastPanLocation, _prevPanLocation));
		}
		GestureEnded?.Invoke(this, new SKGestureLifecycleEventArgs());
	}

	#endregion

	#region Transform Helpers

	private SKPoint ScreenToContentDelta(float dx, float dy)
	{
		var inv = SKMatrix.CreateRotationDegrees(-_rotation);
		var mapped = inv.MapVector(dx, dy);
		return new SKPoint(mapped.X / _scale, mapped.Y / _scale);
	}

	private void AdjustOffsetForPivot(SKPoint pivot, float oldScale, float newScale, float oldRotDeg, float newRotDeg)
	{
		// Matrix model: P_screen = S * R * (P_content + offset)
		// To keep the same content point at screen position 'pivot':
		//   new_offset = R(-newRot).MapVector(pivot / newScale)
		//              - R(-oldRot).MapVector(pivot / oldScale)
		//              + old_offset
		var rotOld = SKMatrix.CreateRotationDegrees(-oldRotDeg);
		var rotNew = SKMatrix.CreateRotationDegrees(-newRotDeg);
		var oldMapped = rotOld.MapVector(pivot.X / oldScale, pivot.Y / oldScale);
		var newMapped = rotNew.MapVector(pivot.X / newScale, pivot.Y / newScale);
		_offset = new SKPoint(newMapped.X - oldMapped.X + _offset.X, newMapped.Y - oldMapped.Y + _offset.Y);
	}

	private static float Clamp(float value, float min, float max)
		=> value < min ? min : value > max ? max : value;

	#endregion

	#region Fling Animation

	private void StartFlingAnimation(float velocityX, float velocityY)
	{
		StopFling();
		_syncContext ??= SynchronizationContext.Current;

		_flingVelocityX = velocityX;
		_flingVelocityY = velocityY;
		_isFlinging = true;
		_flingLastFrameTimestamp = TimeProvider();

		var token = Interlocked.Increment(ref _flingToken);
		_flingTimer = new Timer(
			OnFlingTimerTick,
			token,
			Options.FlingFrameInterval,
			Options.FlingFrameInterval);
	}

	private void OnFlingTimerTick(object? state)
	{
		if (state is not int token || token != Volatile.Read(ref _flingToken))
			return;

		if (!_isFlinging || _disposed)
			return;

		var ctx = _syncContext;
		if (ctx != null)
		{
			ctx.Post(_ =>
			{
				if (token == Volatile.Read(ref _flingToken))
					HandleFlingFrame();
			}, null);
		}
		else
		{
			HandleFlingFrame();
		}
	}

	private void HandleFlingFrame()
	{
		if (!_isFlinging || _disposed)
			return;

		// Use actual elapsed time for frame-rate-independent deceleration
		var now = TimeProvider();
		var actualDtMs = Math.Max(1f, (float)((now - _flingLastFrameTimestamp) / (double)TimeSpan.TicksPerMillisecond));
		_flingLastFrameTimestamp = now;

		var dt = actualDtMs / 1000f;
		var deltaX = _flingVelocityX * dt;
		var deltaY = _flingVelocityY * dt;

		FlingUpdated?.Invoke(this, new SKFlingGestureEventArgs(new SKPoint(_flingVelocityX, _flingVelocityY), new SKPoint(deltaX, deltaY)));

		// Apply as pan offset
		var d = ScreenToContentDelta(deltaX, deltaY);
		_offset = new SKPoint(_offset.X + d.X, _offset.Y + d.Y);
		TransformChanged?.Invoke(this, EventArgs.Empty);

		// Apply time-scaled friction so deceleration is consistent regardless of frame rate
		var nominalDtMs = (float)Options.FlingFrameInterval;
		var decay = nominalDtMs > 0
			? (float)Math.Pow(1.0 - Options.FlingFriction, actualDtMs / nominalDtMs)
			: 1f - Options.FlingFriction;
		_flingVelocityX *= decay;
		_flingVelocityY *= decay;

		var speed = (float)Math.Sqrt(_flingVelocityX * _flingVelocityX + _flingVelocityY * _flingVelocityY);
		if (speed < Options.FlingMinVelocity)
		{
			StopFling();
		}
	}

	#endregion

	#region Zoom Animation

	private void OnZoomTimerTick(object? state)
	{
		if (state is not int token || token != Volatile.Read(ref _zoomToken))
			return;

		if (!_isZoomAnimating || _disposed)
			return;

		var ctx = _syncContext;
		if (ctx != null)
		{
			ctx.Post(_ =>
			{
				if (token == Volatile.Read(ref _zoomToken))
					HandleZoomFrame();
			}, null);
		}
		else
		{
			HandleZoomFrame();
		}
	}

	private void HandleZoomFrame()
	{
		if (!_isZoomAnimating || _disposed)
			return;

		var elapsed = TimeProvider() - _zoomStartTicks;
		var duration = Options.ZoomAnimationDuration * TimeSpan.TicksPerMillisecond;
		var t = duration > 0 ? Math.Min(1.0, (double)elapsed / duration) : 1.0;

		// CubicOut easing: 1 - (1 - t)^3
		var eased = 1.0 - Math.Pow(1.0 - t, 3);

		// Log-space interpolation: cumulative = factor^eased(t)
		var cumulative = (float)Math.Pow(_zoomTargetFactor, eased);

		_zoomPrevCumulative = cumulative;

		// Apply scale change
		var oldScale = _scale;
		var newScale = Clamp(_zoomStartScale * cumulative, Options.MinScale, Options.MaxScale);
		AdjustOffsetForPivot(_zoomFocalPoint, oldScale, newScale, _rotation, _rotation);
		_scale = newScale;
		TransformChanged?.Invoke(this, EventArgs.Empty);

		if (t >= 1.0)
		{
			_isZoomAnimating = false;
			Interlocked.Increment(ref _zoomToken);
			var timer = _zoomTimer;
			_zoomTimer = null;
			timer?.Change(Timeout.Infinite, Timeout.Infinite);
			timer?.Dispose();
		}
	}

	#endregion
}
