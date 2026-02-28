using System;
using System.Threading;

namespace SkiaSharp.Extended.Gestures;

/// <summary>
/// Tracks gesture state and maintains an absolute transform (scale, rotation, offset)
/// by consuming events from an internal <see cref="SKGestureEngine"/>.
/// </summary>
/// <remarks>
/// <para>The tracker is the primary public API for gesture handling. It accepts raw touch
/// input, detects gestures internally, and translates them into transform state changes.</para>
/// <para>Use <see cref="Matrix"/> to apply the current transform when painting.</para>
/// </remarks>
public class SKGestureTracker : IDisposable
{
	private readonly SKGestureEngine _engine;
	private SynchronizationContext? _syncContext;
	private bool _disposed;

	// Transform state
	private float _scale = 1f;
	private float _rotation;
	private SKPoint _offset = SKPoint.Empty;
	private float _viewWidth;
	private float _viewHeight;

	// Drag lifecycle state
	private bool _isDragging;
	private bool _isDragHandled;
	private SKPoint _dragStartLocation;

	// Fling animation state
	private Timer? _flingTimer;
	private int _flingToken;
	private float _flingVelocityX;
	private float _flingVelocityY;
	private bool _isFlinging;

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
	/// Creates a new gesture tracker with default options.
	/// </summary>
	public SKGestureTracker()
		: this(new SKGestureTrackerOptions())
	{
	}

	/// <summary>
	/// Creates a new gesture tracker with the specified options.
	/// </summary>
	public SKGestureTracker(SKGestureTrackerOptions options)
	{
		Options = options ?? throw new ArgumentNullException(nameof(options));
		_engine = new SKGestureEngine(options);
		SubscribeEngineEvents();
	}

	/// <summary>
	/// Gets the configuration options for this tracker.
	/// </summary>
	public SKGestureTrackerOptions Options { get; }

	#region Touch Input

	/// <summary>Processes a touch down event.</summary>
	public bool ProcessTouchDown(long id, SKPoint location, bool isMouse = false)
		=> _engine.ProcessTouchDown(id, location, isMouse);

	/// <summary>Processes a touch move event.</summary>
	public bool ProcessTouchMove(long id, SKPoint location, bool inContact = true)
		=> _engine.ProcessTouchMove(id, location, inContact);

	/// <summary>Processes a touch up event.</summary>
	public bool ProcessTouchUp(long id, SKPoint location, bool isMouse = false)
		=> _engine.ProcessTouchUp(id, location, isMouse);

	/// <summary>Processes a touch cancel event.</summary>
	public bool ProcessTouchCancel(long id)
		=> _engine.ProcessTouchCancel(id);

	/// <summary>Processes a mouse wheel event.</summary>
	public bool ProcessMouseWheel(SKPoint location, float deltaX, float deltaY)
		=> _engine.ProcessMouseWheel(location, deltaX, deltaY);

	#endregion

	#region Detection Config (forwarded to engine)

	/// <summary>Gets or sets whether gesture detection is enabled.</summary>
	public bool IsEnabled
	{
		get => _engine.IsEnabled;
		set => _engine.IsEnabled = value;
	}

	/// <summary>Gets or sets the touch slop (minimum movement to start a gesture).</summary>
	public float TouchSlop
	{
		get => Options.TouchSlop;
		set => Options.TouchSlop = value;
	}

	/// <summary>Gets or sets the double-tap slop distance.</summary>
	public float DoubleTapSlop
	{
		get => Options.DoubleTapSlop;
		set => Options.DoubleTapSlop = value;
	}

	/// <summary>Gets or sets the fling velocity detection threshold.</summary>
	public float FlingThreshold
	{
		get => Options.FlingThreshold;
		set => Options.FlingThreshold = value;
	}

	/// <summary>Gets or sets the long press duration in milliseconds.</summary>
	public int LongPressDuration
	{
		get => Options.LongPressDuration;
		set => Options.LongPressDuration = value;
	}

	/// <summary>Gets or sets the time provider (for testing).</summary>
	public Func<long> TimeProvider
	{
		get => _engine.TimeProvider;
		set => _engine.TimeProvider = value;
	}

	#endregion

	#region Transform State (read-only)

	/// <summary>Gets the current zoom scale.</summary>
	public float Scale => _scale;

	/// <summary>Gets the current rotation in degrees.</summary>
	public float Rotation => _rotation;

	/// <summary>Gets the current pan offset.</summary>
	public SKPoint Offset => _offset;

	/// <summary>Gets the composite transform matrix.</summary>
	public SKMatrix Matrix
	{
		get
		{
			var w2 = _viewWidth / 2f;
			var h2 = _viewHeight / 2f;
			var m = SKMatrix.CreateTranslation(w2, h2);
			m = m.PreConcat(SKMatrix.CreateScale(_scale, _scale));
			m = m.PreConcat(SKMatrix.CreateRotationDegrees(_rotation));
			m = m.PreConcat(SKMatrix.CreateTranslation(_offset.X, _offset.Y));
			m = m.PreConcat(SKMatrix.CreateTranslation(-w2, -h2));
			return m;
		}
	}

	/// <summary>Sets the view dimensions (needed for pivot and matrix calculations).</summary>
	public void SetViewSize(float width, float height)
	{
		_viewWidth = width;
		_viewHeight = height;
	}

	#endregion

	#region Transform Config

	/// <summary>Gets or sets the minimum allowed scale.</summary>
	public float MinScale
	{
		get => Options.MinScale;
		set => Options.MinScale = value;
	}

	/// <summary>Gets or sets the maximum allowed scale.</summary>
	public float MaxScale
	{
		get => Options.MaxScale;
		set => Options.MaxScale = value;
	}

	/// <summary>Gets or sets the zoom factor applied per double-tap.</summary>
	public float DoubleTapZoomFactor
	{
		get => Options.DoubleTapZoomFactor;
		set => Options.DoubleTapZoomFactor = value;
	}

	/// <summary>Gets or sets the zoom animation duration in milliseconds.</summary>
	public int ZoomAnimationDuration
	{
		get => Options.ZoomAnimationDuration;
		set => Options.ZoomAnimationDuration = value;
	}

	/// <summary>Gets or sets how much each scroll tick changes scale.</summary>
	public float ScrollZoomFactor
	{
		get => Options.ScrollZoomFactor;
		set => Options.ScrollZoomFactor = value;
	}

	/// <summary>
	/// Gets or sets the fling friction (0 = no friction / infinite fling, 1 = full friction / no fling).
	/// Default is 0.08.
	/// </summary>
	public float FlingFriction
	{
		get => Options.FlingFriction;
		set => Options.FlingFriction = value;
	}

	/// <summary>Gets or sets the minimum fling velocity before the animation stops.</summary>
	public float FlingMinVelocity
	{
		get => Options.FlingMinVelocity;
		set => Options.FlingMinVelocity = value;
	}

	/// <summary>Gets or sets the fling animation frame interval in milliseconds.</summary>
	public int FlingFrameInterval
	{
		get => Options.FlingFrameInterval;
		set => Options.FlingFrameInterval = value;
	}

	#endregion

	#region Feature Toggles

	/// <summary>Gets or sets whether pan is enabled.</summary>
	public bool IsPanEnabled { get; set; } = true;

	/// <summary>Gets or sets whether pinch-to-zoom is enabled.</summary>
	public bool IsPinchEnabled { get; set; } = true;

	/// <summary>Gets or sets whether rotation is enabled.</summary>
	public bool IsRotateEnabled { get; set; } = true;

	/// <summary>Gets or sets whether fling animation is enabled.</summary>
	public bool IsFlingEnabled { get; set; } = true;

	/// <summary>Gets or sets whether double-tap zoom is enabled.</summary>
	public bool IsDoubleTapZoomEnabled { get; set; } = true;

	/// <summary>Gets or sets whether scroll-wheel zoom is enabled.</summary>
	public bool IsScrollZoomEnabled { get; set; } = true;

	#endregion

	#region Animation State

	/// <summary>Gets whether a zoom animation is currently running.</summary>
	public bool IsZoomAnimating => _isZoomAnimating;

	/// <summary>Gets whether a fling animation is currently running.</summary>
	public bool IsFlinging => _isFlinging;

	/// <summary>Gets whether any gesture is currently active.</summary>
	public bool IsGestureActive => _engine.IsGestureActive;

	#endregion

	#region Gesture Events (forwarded from engine)

	/// <summary>Occurs when a tap is detected.</summary>
	public event EventHandler<SKTapEventArgs>? TapDetected;

	/// <summary>Occurs when a double tap is detected.</summary>
	public event EventHandler<SKTapEventArgs>? DoubleTapDetected;

	/// <summary>Occurs when a long press is detected.</summary>
	public event EventHandler<SKTapEventArgs>? LongPressDetected;

	/// <summary>Occurs when a pan gesture is detected.</summary>
	public event EventHandler<SKPanEventArgs>? PanDetected;

	/// <summary>Occurs when a pinch gesture is detected.</summary>
	public event EventHandler<SKPinchEventArgs>? PinchDetected;

	/// <summary>Occurs when a rotation gesture is detected.</summary>
	public event EventHandler<SKRotateEventArgs>? RotateDetected;

	/// <summary>Occurs when a fling gesture is detected (once, with velocity).</summary>
	public event EventHandler<SKFlingEventArgs>? FlingDetected;

	/// <summary>Occurs when a hover is detected.</summary>
	public event EventHandler<SKHoverEventArgs>? HoverDetected;

	/// <summary>Occurs when a scroll event is detected.</summary>
	public event EventHandler<SKScrollEventArgs>? ScrollDetected;

	/// <summary>Occurs when a gesture starts.</summary>
	public event EventHandler? GestureStarted;

	/// <summary>Occurs when a gesture ends.</summary>
	public event EventHandler? GestureEnded;

	#endregion

	#region Tracker Events

	/// <summary>Occurs when the transform (Scale, Rotation, Offset, Matrix) changes.</summary>
	public event EventHandler? TransformChanged;

	/// <summary>Occurs when a drag operation starts.</summary>
	public event EventHandler<SKDragEventArgs>? DragStarted;

	/// <summary>Occurs during a drag operation.</summary>
	public event EventHandler<SKDragEventArgs>? DragUpdated;

	/// <summary>Occurs when a drag operation ends.</summary>
	public event EventHandler<SKDragEventArgs>? DragEnded;

	/// <summary>Occurs each animation frame during a fling.</summary>
	public event EventHandler<SKFlingEventArgs>? Flinging;

	/// <summary>Occurs when a fling animation completes.</summary>
	public event EventHandler? FlingCompleted;

	#endregion

	#region Public Methods

	/// <summary>
	/// Starts an animated zoom by the given multiplicative factor around a focal point.
	/// </summary>
	public void ZoomTo(float factor, SKPoint focalPoint)
	{
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
			FlingFrameInterval,
			FlingFrameInterval);
	}

	/// <summary>Stops any active zoom animation.</summary>
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

	/// <summary>Stops any active fling animation.</summary>
	public void StopFling()
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
		FlingCompleted?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>Resets the tracker to identity transform.</summary>
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

	/// <summary>Disposes the tracker and its internal engine.</summary>
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

	private void OnEngineTapDetected(object? s, SKTapEventArgs e)
		=> TapDetected?.Invoke(this, e);

	private void OnEngineDoubleTapDetected(object? s, SKTapEventArgs e)
	{
		DoubleTapDetected?.Invoke(this, e);

		// If the consumer handled the event (e.g. sticker selection), skip zoom
		if (e.Handled)
			return;

		if (!IsDoubleTapZoomEnabled)
			return;

		if (_scale >= MaxScale - 0.01f)
		{
			// At max zoom — animate reset to 1.0
			ZoomTo(1f / _scale, e.Location);
		}
		else
		{
			var factor = Math.Min(DoubleTapZoomFactor, MaxScale / _scale);
			ZoomTo(factor, e.Location);
		}
	}

	private void OnEngineLongPressDetected(object? s, SKTapEventArgs e)
		=> LongPressDetected?.Invoke(this, e);

	private void OnEnginePanDetected(object? s, SKPanEventArgs e)
	{
		PanDetected?.Invoke(this, e);

		if (!IsPanEnabled)
			return;

		// Derive drag lifecycle
		SKDragEventArgs? dragArgs = null;
		if (!_isDragging)
		{
			_isDragging = true;
			_isDragHandled = false;
			_dragStartLocation = e.PreviousLocation;
			dragArgs = new SKDragEventArgs(_dragStartLocation, e.Location, e.Delta);
			DragStarted?.Invoke(this, dragArgs);
		}
		else
		{
			dragArgs = new SKDragEventArgs(_dragStartLocation, e.Location, e.Delta);
			DragUpdated?.Invoke(this, dragArgs);
		}

		// Track whether the consumer is handling this drag (e.g. sticker drag)
		if (dragArgs?.Handled ?? false)
			_isDragHandled = true;

		// Skip offset update if consumer handled the pan or drag (e.g. sticker drag)
		if (e.Handled || _isDragHandled)
			return;

		// Update offset
		var d = ScreenToContentDelta(e.Delta.X, e.Delta.Y);
		_offset = new SKPoint(_offset.X + d.X, _offset.Y + d.Y);
		TransformChanged?.Invoke(this, EventArgs.Empty);
	}

	private void OnEnginePinchDetected(object? s, SKPinchEventArgs e)
	{
		PinchDetected?.Invoke(this, e);

		// Apply center movement as pan
		if (IsPanEnabled)
		{
			var panDelta = ScreenToContentDelta(
				e.Center.X - e.PreviousCenter.X,
				e.Center.Y - e.PreviousCenter.Y);
			_offset = new SKPoint(_offset.X + panDelta.X, _offset.Y + panDelta.Y);
		}

		if (IsPinchEnabled)
		{
			var newScale = Clamp(_scale * e.Scale, MinScale, MaxScale);
			AdjustOffsetForPivot(e.Center, _scale, newScale, _rotation, _rotation);
			_scale = newScale;
		}

		TransformChanged?.Invoke(this, EventArgs.Empty);
	}

	private void OnEngineRotateDetected(object? s, SKRotateEventArgs e)
	{
		RotateDetected?.Invoke(this, e);

		if (!IsRotateEnabled)
			return;

		var newRotation = _rotation + e.RotationDelta;
		AdjustOffsetForPivot(e.Center, _scale, _scale, _rotation, newRotation);
		_rotation = newRotation;
		TransformChanged?.Invoke(this, EventArgs.Empty);
	}

	private void OnEngineFlingDetected(object? s, SKFlingEventArgs e)
	{
		FlingDetected?.Invoke(this, e);

		// Don't fling if the drag was handled by the consumer (e.g. sticker drag)
		if (!IsFlingEnabled || _isDragHandled)
			return;

		StartFlingAnimation(e.VelocityX, e.VelocityY);
	}

	private void OnEngineHoverDetected(object? s, SKHoverEventArgs e)
		=> HoverDetected?.Invoke(this, e);

	private void OnEngineScrollDetected(object? s, SKScrollEventArgs e)
	{
		ScrollDetected?.Invoke(this, e);

		if (!IsScrollZoomEnabled || e.DeltaY == 0)
			return;

		var scaleDelta = 1f + e.DeltaY * ScrollZoomFactor;
		var newScale = Clamp(_scale * scaleDelta, MinScale, MaxScale);
		AdjustOffsetForPivot(e.Location, _scale, newScale, _rotation, _rotation);
		_scale = newScale;
		TransformChanged?.Invoke(this, EventArgs.Empty);
	}

	private void OnEngineGestureStarted(object? s, EventArgs e)
	{
		_syncContext ??= SynchronizationContext.Current;
		StopFling();
		StopZoomAnimation();
		GestureStarted?.Invoke(this, EventArgs.Empty);
	}

	private void OnEngineGestureEnded(object? s, EventArgs e)
	{
		if (_isDragging)
		{
			_isDragging = false;
			_isDragHandled = false;
			DragEnded?.Invoke(this, new SKDragEventArgs(_dragStartLocation, _dragStartLocation, SKPoint.Empty));
		}
		GestureEnded?.Invoke(this, EventArgs.Empty);
	}

	#endregion

	#region Transform Helpers

	private SKPoint ScreenToContentDelta(float dx, float dy)
	{
		var inv = SKMatrix.CreateRotationDegrees(-_rotation);
		var mapped = inv.MapVector(dx, dy);
		return new SKPoint(mapped.X / _scale, mapped.Y / _scale);
	}

	private void AdjustOffsetForPivot(SKPoint screenPivot, float oldScale, float newScale, float oldRotDeg, float newRotDeg)
	{
		var w2 = _viewWidth / 2f;
		var h2 = _viewHeight / 2f;
		var d = new SKPoint(screenPivot.X - w2, screenPivot.Y - h2);

		var rotOld = SKMatrix.CreateRotationDegrees(-oldRotDeg);
		var qOld = rotOld.MapVector(d.X, d.Y);
		qOld = new SKPoint(qOld.X / oldScale, qOld.Y / oldScale);

		var rotNew = SKMatrix.CreateRotationDegrees(-newRotDeg);
		var qNew = rotNew.MapVector(d.X, d.Y);
		qNew = new SKPoint(qNew.X / newScale, qNew.Y / newScale);

		_offset = new SKPoint(
			_offset.X + qNew.X - qOld.X,
			_offset.Y + qNew.Y - qOld.Y);
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

		var token = Interlocked.Increment(ref _flingToken);
		_flingTimer = new Timer(
			OnFlingTimerTick,
			token,
			FlingFrameInterval,
			FlingFrameInterval);
	}

	private void OnFlingTimerTick(object? state)
	{
		if (state is not int token || token != Volatile.Read(ref _flingToken))
			return;

		if (!_isFlinging || _disposed)
			return;

		if (_syncContext != null)
		{
			_syncContext.Post(_ =>
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

		var dt = FlingFrameInterval / 1000f;
		var deltaX = _flingVelocityX * dt;
		var deltaY = _flingVelocityY * dt;

		Flinging?.Invoke(this, new SKFlingEventArgs(_flingVelocityX, _flingVelocityY, deltaX, deltaY));

		// Apply as pan offset
		var d = ScreenToContentDelta(deltaX, deltaY);
		_offset = new SKPoint(_offset.X + d.X, _offset.Y + d.Y);
		TransformChanged?.Invoke(this, EventArgs.Empty);

		// Apply friction (FlingFriction: 0 = no friction, 1 = full friction)
		var decay = 1f - FlingFriction;
		_flingVelocityX *= decay;
		_flingVelocityY *= decay;

		var speed = (float)Math.Sqrt(_flingVelocityX * _flingVelocityX + _flingVelocityY * _flingVelocityY);
		if (speed < FlingMinVelocity)
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

		if (_syncContext != null)
		{
			_syncContext.Post(_ =>
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
		var duration = ZoomAnimationDuration * TimeSpan.TicksPerMillisecond;
		var t = duration > 0 ? Math.Min(1.0, (double)elapsed / duration) : 1.0;

		// CubicOut easing: 1 - (1 - t)^3
		var eased = 1.0 - Math.Pow(1.0 - t, 3);

		// Log-space interpolation: cumulative = factor^eased(t)
		var cumulative = (float)Math.Pow(_zoomTargetFactor, eased);

		// Per-frame scale delta
		var frameDelta = _zoomPrevCumulative > 0 ? cumulative / _zoomPrevCumulative : 1f;
		_zoomPrevCumulative = cumulative;

		// Apply scale change
		var oldScale = _scale;
		var newScale = Clamp(_zoomStartScale * cumulative, MinScale, MaxScale);
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
