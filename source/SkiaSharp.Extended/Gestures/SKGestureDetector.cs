using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SkiaSharp.Extended;

/// <summary>
/// A platform-agnostic gesture recognition engine that detects taps, long presses,
/// pan, pinch, rotation, and fling gestures from touch input.
/// </summary>
/// <remarks>
/// <para>This engine is a pure gesture detector. It processes touch events and raises
/// events when gestures are recognized. It does not maintain transform state or run
/// animations — use <see cref="SKGestureTracker"/> for that.</para>
/// <para>The engine must be used on the UI thread. It captures the current 
/// <see cref="SynchronizationContext"/> when processing touch events and uses it
/// to marshal timer callbacks back to the UI thread.</para>
/// <para>Call <see cref="Dispose"/> to clean up resources when done.</para>
/// </remarks>
public sealed class SKGestureDetector : IDisposable
{
	// Timing constants
	private const long ShortTapTicks = 125 * TimeSpan.TicksPerMillisecond;
	private const long ShortClickTicks = 250 * TimeSpan.TicksPerMillisecond;
	private const long DoubleTapDelayTicks = 300 * TimeSpan.TicksPerMillisecond;

	private readonly Dictionary<long, TouchState> _touches = new();
	private readonly SKFlingTracker _flingTracker = new();
	private SynchronizationContext? _syncContext;
	private Timer? _longPressTimer;
	private int _longPressToken;

	private SKPoint _initialTouch = SKPoint.Empty;
	private SKPoint _lastTapLocation = SKPoint.Empty;
	private long _lastTapTicks;
	private int _tapCount;
	private GestureState _gestureState = GestureState.None;
	private PinchState _pinchState;
	private bool _longPressTriggered;
	private long _touchStartTicks;
	private bool _disposed;

	/// <summary>
	/// Initializes a new instance of <see cref="SKGestureDetector"/> with default options.
	/// </summary>
	public SKGestureDetector()
		: this(new SKGestureDetectorOptions())
	{
	}

	/// <summary>
	/// Initializes a new instance of <see cref="SKGestureDetector"/> with the specified options.
	/// </summary>
	public SKGestureDetector(SKGestureDetectorOptions options)
	{
		Options = options ?? throw new ArgumentNullException(nameof(options));
	}

	/// <summary>
	/// Gets the configuration options for this detector.
	/// </summary>
	/// <value>The <see cref="SKGestureDetectorOptions"/> instance controlling detection thresholds.</value>
	public SKGestureDetectorOptions Options { get; }

	/// <summary>
	/// Gets or sets the time provider function used to obtain the current time in ticks.
	/// </summary>
	/// <value>
	/// A <see cref="Func{T}"/> that returns the current time in <see cref="DateTime.Ticks"/>.
	/// The default uses <see cref="DateTime.Now"/>.
	/// </value>
	/// <remarks>
	/// Override this for deterministic testing by supplying a custom tick source.
	/// </remarks>
	public Func<long> TimeProvider { get; set; } = () => DateTime.Now.Ticks;

	/// <summary>
	/// Gets or sets a value indicating whether the gesture detector is enabled.
	/// </summary>
	/// <value>
	/// <see langword="true"/> if the detector processes touch events; otherwise, <see langword="false"/>.
	/// The default is <see langword="true"/>. When disabled, all <c>ProcessTouch*</c> methods return <see langword="false"/>.
	/// </value>
	public bool IsEnabled { get; set; } = true;

	/// <summary>
	/// Gets a value indicating whether a gesture is currently in progress.
	/// </summary>
	/// <value>
	/// <see langword="true"/> if the detector is currently tracking an active gesture (detecting, panning,
	/// or pinching); otherwise, <see langword="false"/>.
	/// </value>
	public bool IsGestureActive => _gestureState != GestureState.None;

	/// <summary>
	/// Occurs when a single tap is detected.
	/// </summary>
	/// <remarks>
	/// A tap is recognized when a touch down and up occur within the <see cref="SKGestureDetectorOptions.TouchSlop"/>
	/// distance and within the long press duration threshold.
	/// </remarks>
	public event EventHandler<SKTapGestureEventArgs>? TapDetected;

	/// <summary>
	/// Occurs when a double tap is detected.
	/// </summary>
	/// <remarks>
	/// A double tap is recognized when two taps occur within 300 ms of each other and within the
	/// <see cref="SKGestureDetectorOptions.DoubleTapSlop"/> distance.
	/// </remarks>
	public event EventHandler<SKTapGestureEventArgs>? DoubleTapDetected;

	/// <summary>
	/// Occurs when a long press is detected.
	/// </summary>
	/// <remarks>
	/// A long press is recognized when a touch is held stationary for at least
	/// <see cref="SKGestureDetectorOptions.LongPressDuration"/> milliseconds without exceeding
	/// the <see cref="SKGestureDetectorOptions.TouchSlop"/> distance.
	/// </remarks>
	public event EventHandler<SKLongPressGestureEventArgs>? LongPressDetected;

	/// <summary>
	/// Occurs when a single-finger pan (drag) gesture is detected.
	/// </summary>
	/// <remarks>
	/// Pan events fire continuously as a single touch moves beyond the
	/// <see cref="SKGestureDetectorOptions.TouchSlop"/> threshold.
	/// </remarks>
	public event EventHandler<SKPanGestureEventArgs>? PanDetected;

	/// <summary>
	/// Occurs when a two-finger pinch (scale) gesture is detected.
	/// </summary>
	/// <remarks>
	/// Pinch events fire continuously while two or more touches are active and moving.
	/// The <see cref="SKPinchGestureEventArgs.ScaleDelta"/> is a per-event relative multiplier.
	/// </remarks>
	public event EventHandler<SKPinchGestureEventArgs>? PinchDetected;

	/// <summary>
	/// Occurs when a two-finger rotation gesture is detected.
	/// </summary>
	/// <remarks>
	/// Rotation events fire simultaneously with pinch events when two or more touches are active.
	/// </remarks>
	public event EventHandler<SKRotateGestureEventArgs>? RotateDetected;

	/// <summary>
	/// Occurs when a fling gesture is detected (fired once with initial velocity upon touch release).
	/// </summary>
	/// <remarks>
	/// A fling is triggered when a single-finger pan ends with a velocity exceeding the
	/// <see cref="SKGestureDetectorOptions.FlingThreshold"/>. Flings are not triggered after
	/// multi-finger gestures (pinch/rotate).
	/// </remarks>
	public event EventHandler<SKFlingGestureEventArgs>? FlingDetected;

	/// <summary>
	/// Occurs when a mouse hover (move without contact) is detected.
	/// </summary>
	public event EventHandler<SKHoverGestureEventArgs>? HoverDetected;

	/// <summary>
	/// Occurs when a mouse scroll (wheel) event is detected.
	/// </summary>
	public event EventHandler<SKScrollGestureEventArgs>? ScrollDetected;

	/// <summary>
	/// Occurs when a touch gesture interaction begins (first finger touches the surface).
	/// </summary>
	public event EventHandler<SKGestureLifecycleEventArgs>? GestureStarted;

	/// <summary>
	/// Occurs when a touch gesture interaction ends (last finger lifts from the surface).
	/// </summary>
	public event EventHandler<SKGestureLifecycleEventArgs>? GestureEnded;

	/// <summary>
	/// Processes a touch down event.
	/// </summary>
	/// <param name="id">The unique identifier for this touch.</param>
	/// <param name="location">The location of the touch.</param>
	/// <param name="isMouse">Whether this is a mouse event.</param>
	/// <returns>True if the event was handled.</returns>
	public bool ProcessTouchDown(long id, SKPoint location, bool isMouse = false)
	{
		if (!IsEnabled || _disposed)
			return false;

		// Capture the synchronization context on first touch (UI thread)
		_syncContext ??= SynchronizationContext.Current;

		var ticks = TimeProvider();

		_touches[id] = new TouchState(id, location, ticks, true, isMouse);

		// Only set initial touch state for the first finger
		if (_touches.Count == 1)
		{
			_initialTouch = location;
			_touchStartTicks = ticks;
			_longPressTriggered = false;
			// Start the long press timer only on the first finger (not on 2nd+ during pinch)
			StartLongPressTimer();
		}

		// Check for double tap using the last completed tap location
		if (_touches.Count == 1 &&
			ticks - _lastTapTicks < DoubleTapDelayTicks &&
			SKPoint.Distance(location, _lastTapLocation) < Options.DoubleTapSlop)
		{
			_tapCount++;
		}
		else if (_touches.Count == 1)
		{
			_tapCount = 1;
		}

		var touchPoints = GetActiveTouchPoints();

		if (touchPoints.Length > 0)
		{
			// Only raise GestureStarted for the first touch
			if (_touches.Count == 1)
				OnGestureStarted(new SKGestureLifecycleEventArgs());

			if (touchPoints.Length >= 2)
			{
				_pinchState = PinchState.FromLocations(touchPoints);
				_gestureState = GestureState.Pinching;
			}
			else
			{
				_pinchState = new PinchState(touchPoints[0], 0, 0);
				_gestureState = GestureState.Detecting;
			}

			return true;
		}

		return false;
	}

	/// <summary>
	/// Processes a touch move event.
	/// </summary>
	/// <param name="id">The unique identifier for this touch.</param>
	/// <param name="location">The new location of the touch.</param>
	/// <param name="inContact">Whether the touch is in contact with the surface.</param>
	/// <returns>True if the event was handled.</returns>
	public bool ProcessTouchMove(long id, SKPoint location, bool inContact = true)
	{
		if (!IsEnabled || _disposed)
			return false;

		var ticks = TimeProvider();

		// Handle hover (mouse without contact) — no prior touch down required
		if (!inContact)
		{
			OnHoverDetected(new SKHoverGestureEventArgs(location));
			return true;
		}

		if (!_touches.TryGetValue(id, out var existingTouch))
			return false;

		_touches[id] = new TouchState(id, location, ticks, inContact, existingTouch.IsMouse);
		_flingTracker.AddEvent(id, location, ticks);

		var touchPoints = GetActiveTouchPoints();
		var distance = SKPoint.Distance(location, _initialTouch);

		// Start pan if moved beyond touch slop
		if (_gestureState == GestureState.Detecting && distance >= Options.TouchSlop)
		{
			StopLongPressTimer();
			_gestureState = GestureState.Panning;
			// Invalidate double-tap counter — this touch became a pan, not a tap
			_tapCount = 0;
			_lastTapTicks = 0;
		}

		switch (_gestureState)
		{
			case GestureState.Panning:
				if (touchPoints.Length == 1)
				{
					var delta = location - _pinchState.Center;
					var velocity = _flingTracker.CalculateVelocity(id, ticks);
					OnPanDetected(new SKPanGestureEventArgs(location, _pinchState.Center, delta, velocity));
					_pinchState = new PinchState(location, 0, 0);
				}
				break;

			case GestureState.Pinching:
				if (touchPoints.Length >= 2)
				{
					var newPinch = PinchState.FromLocations(touchPoints);

					// Calculate scale
					var scaleDelta = _pinchState.Radius > 0 ? newPinch.Radius / _pinchState.Radius : 1f;
					OnPinchDetected(new SKPinchGestureEventArgs(newPinch.Center, _pinchState.Center, scaleDelta));

					// Calculate rotation
					var rotationDelta = newPinch.Angle - _pinchState.Angle;
					rotationDelta = NormalizeAngle(rotationDelta);
					OnRotateDetected(new SKRotateGestureEventArgs(newPinch.Center, _pinchState.Center, rotationDelta));

					_pinchState = newPinch;
				}
				break;
		}

		return true;
	}

	/// <summary>
	/// Processes a touch up event.
	/// </summary>
	/// <param name="id">The unique identifier for this touch.</param>
	/// <param name="location">The final location of the touch.</param>
	/// <param name="isMouse">Whether this is a mouse event (kept for backward compatibility; the stored value from touch-down is used internally).</param>
	/// <returns>True if the event was handled.</returns>
	public bool ProcessTouchUp(long id, SKPoint location, bool isMouse = false)
	{
		if (!IsEnabled || _disposed)
			return false;

		StopLongPressTimer();
		var ticks = TimeProvider();

		if (!_touches.TryGetValue(id, out var releasedTouch))
			return false;

		// Use the stored IsMouse value from touch-down (more reliable than caller-supplied value)
		var storedIsMouse = releasedTouch.IsMouse;

		_touches.Remove(id);

		var touchPoints = GetActiveTouchPoints();
		var handled = false;

		// Check for fling — only after a single-finger pan, not after pinch/rotate
		if (touchPoints.Length == 0 && _gestureState == GestureState.Panning)
		{
			var velocity = _flingTracker.CalculateVelocity(id, ticks);
			var velocityMagnitude = (float)Math.Sqrt(velocity.X * velocity.X + velocity.Y * velocity.Y);

			if (velocityMagnitude > Options.FlingThreshold)
			{
				OnFlingDetected(new SKFlingGestureEventArgs(velocity.X, velocity.Y));
				handled = true;
			}
		}

		// Check for tap — only if we haven't transitioned to panning/pinching
		if (touchPoints.Length == 0 && _gestureState == GestureState.Detecting)
		{
			var distance = SKPoint.Distance(location, _initialTouch);
			var duration = ticks - _touchStartTicks;
			var maxTapDuration = storedIsMouse ? ShortClickTicks : Options.LongPressDuration * TimeSpan.TicksPerMillisecond;

			if (distance < Options.TouchSlop && duration < maxTapDuration && !_longPressTriggered)
			{
				_lastTapTicks = ticks;
				_lastTapLocation = location;

				if (_tapCount > 1)
				{
					OnDoubleTapDetected(new SKTapGestureEventArgs(location, _tapCount));
					_tapCount = 0;
				}
				else
				{
					OnTapDetected(new SKTapGestureEventArgs(location, 1));
				}
				handled = true;
			}
			else
			{
				// Touch ended but failed tap validation (moved too far or held too long).
				// Reset the counter so the next touch-down is not misidentified as a double-tap.
				_tapCount = 0;
				_lastTapTicks = 0;
			}
		}

		_flingTracker.RemoveId(id);

		// Transition gesture state
		if (touchPoints.Length == 0)
		{
			if (_gestureState != GestureState.None)
			{
				OnGestureEnded(new SKGestureLifecycleEventArgs());
				_gestureState = GestureState.None;
			}
		}
		else if (touchPoints.Length == 1)
		{
			// Transition from pinch to pan
			if (_gestureState == GestureState.Pinching)
			{
				_initialTouch = touchPoints[0];
				// Clear velocity history so rotation movement doesn't cause a fling
				_flingTracker.Clear();
			}
			_gestureState = GestureState.Panning;
			_pinchState = new PinchState(touchPoints[0], 0, 0);
		}
		else if (touchPoints.Length >= 2)
		{
			// Recalculate pinch state for remaining fingers to avoid jumps
			_pinchState = PinchState.FromLocations(touchPoints);
		}

		return handled;
	}

	/// <summary>
	/// Processes a touch cancel event.
	/// </summary>
	/// <param name="id">The unique identifier for this touch.</param>
	/// <returns>True if the event was handled.</returns>
	public bool ProcessTouchCancel(long id)
	{
		if (!IsEnabled || _disposed)
			return false;

		StopLongPressTimer();
		_touches.Remove(id);
		_flingTracker.RemoveId(id);

		var touchPoints = GetActiveTouchPoints();
		if (touchPoints.Length == 0)
		{
			if (_gestureState != GestureState.None)
			{
				OnGestureEnded(new SKGestureLifecycleEventArgs());
				_gestureState = GestureState.None;
			}
		}
		else if (touchPoints.Length == 1)
		{
			// Transition from pinch to pan when one finger is cancelled
			if (_gestureState == GestureState.Pinching)
			{
				_initialTouch = touchPoints[0];
				// Clear velocity history so rotation movement doesn't cause a fling
				_flingTracker.Clear();
			}
			_gestureState = GestureState.Panning;
			_pinchState = new PinchState(touchPoints[0], 0, 0);
		}
		else if (touchPoints.Length >= 2)
		{
			// Recalculate pinch state for remaining fingers to avoid jumps
			_pinchState = PinchState.FromLocations(touchPoints);
		}

		return true;
	}

	/// <summary>
	/// Processes a mouse wheel (scroll) event.
	/// </summary>
	/// <param name="location">The location of the mouse pointer.</param>
	/// <param name="deltaX">The horizontal scroll delta.</param>
	/// <param name="deltaY">The vertical scroll delta.</param>
	/// <returns>True if the event was handled.</returns>
	public bool ProcessMouseWheel(SKPoint location, float deltaX, float deltaY)
	{
		if (!IsEnabled || _disposed)
			return false;

		OnScrollDetected(new SKScrollGestureEventArgs(location, deltaX, deltaY));
		return true;
	}

	/// <summary>
	/// Resets the gesture detector to its initial state, clearing all active touches and
	/// cancelling any pending timers.
	/// </summary>
	public void Reset()
	{
		StopLongPressTimer();
		_touches.Clear();
		_flingTracker.Clear();
		_gestureState = GestureState.None;
		_tapCount = 0;
		_lastTapTicks = 0;
		_lastTapLocation = SKPoint.Empty;
		_longPressTriggered = false;
	}

	/// <summary>
	/// Releases all resources used by this <see cref="SKGestureDetector"/> instance.
	/// </summary>
	/// <remarks>
	/// Stops any active long press timer and resets all internal state. After disposal,
	/// all <c>ProcessTouch*</c> methods return <see langword="false"/>.
	/// </remarks>
	public void Dispose()
	{
		if (_disposed)
			return;

		_disposed = true;
		StopLongPressTimer();
		Reset();
	}

	private void StartLongPressTimer()
	{
		StopLongPressTimer();
		var token = Interlocked.Increment(ref _longPressToken);
		var timer = new Timer(OnLongPressTimerTick, token, Options.LongPressDuration, Timeout.Infinite);
		_longPressTimer = timer;
	}

	private void StopLongPressTimer()
	{
		Interlocked.Increment(ref _longPressToken);
		var timer = _longPressTimer;
		_longPressTimer = null;
		timer?.Change(Timeout.Infinite, Timeout.Infinite);
		timer?.Dispose();
	}

	private void OnLongPressTimerTick(object? state)
	{
		// Verify this callback is for the current timer (not a stale one)
		if (state is not int token || token != Volatile.Read(ref _longPressToken))
			return;

		// Marshal to UI thread if we have a sync context
		var ctx = _syncContext;
		if (ctx != null)
		{
			ctx.Post(_ =>
			{
				if (token == Volatile.Read(ref _longPressToken))
					HandleLongPress();
			}, null);
		}
		else
		{
			// No sync context (testing or console app) - run directly
			HandleLongPress();
		}
	}

	private void HandleLongPress()
	{
		if (_disposed || !IsEnabled || _longPressTriggered || _gestureState != GestureState.Detecting)
			return;

		var touchPoints = GetActiveTouchPoints();

		if (touchPoints.Length == 1)
		{
			var distance = SKPoint.Distance(touchPoints[0], _initialTouch);
			if (distance < Options.TouchSlop)
			{
				_longPressTriggered = true;
				StopLongPressTimer();
				var duration = TimeSpan.FromTicks(TimeProvider() - _touchStartTicks);
				OnLongPressDetected(new SKLongPressGestureEventArgs(touchPoints[0], duration));
			}
		}
	}

	private SKPoint[] GetActiveTouchPoints()
	{
		// Sort by touch ID for stable ordering — prevents angle jumps when fingers
		// are added/removed and Dictionary iteration order changes.
		return _touches
			.Where(kv => kv.Value.InContact)
			.OrderBy(kv => kv.Key)
			.Select(kv => kv.Value.Location)
			.ToArray();
	}

	private static float NormalizeAngle(float angle)
	{
		angle %= 360f;
		if (angle > 180f)
			angle -= 360f;
		if (angle < -180f)
			angle += 360f;
		return angle;
	}

	// Event invokers

	/// <summary>Raises the <see cref="TapDetected"/> event.</summary>
	/// <param name="e">The event data.</param>
	private void OnTapDetected(SKTapGestureEventArgs e) => TapDetected?.Invoke(this, e);

	/// <summary>Raises the <see cref="DoubleTapDetected"/> event.</summary>
	/// <param name="e">The event data.</param>
	private void OnDoubleTapDetected(SKTapGestureEventArgs e) => DoubleTapDetected?.Invoke(this, e);

	/// <summary>Raises the <see cref="LongPressDetected"/> event.</summary>
	/// <param name="e">The event data.</param>
	private void OnLongPressDetected(SKLongPressGestureEventArgs e) => LongPressDetected?.Invoke(this, e);

	/// <summary>Raises the <see cref="PanDetected"/> event.</summary>
	/// <param name="e">The event data.</param>
	private void OnPanDetected(SKPanGestureEventArgs e) => PanDetected?.Invoke(this, e);

	/// <summary>Raises the <see cref="PinchDetected"/> event.</summary>
	/// <param name="e">The event data.</param>
	private void OnPinchDetected(SKPinchGestureEventArgs e) => PinchDetected?.Invoke(this, e);

	/// <summary>Raises the <see cref="RotateDetected"/> event.</summary>
	/// <param name="e">The event data.</param>
	private void OnRotateDetected(SKRotateGestureEventArgs e) => RotateDetected?.Invoke(this, e);

	/// <summary>Raises the <see cref="FlingDetected"/> event.</summary>
	/// <param name="e">The event data.</param>
	private void OnFlingDetected(SKFlingGestureEventArgs e) => FlingDetected?.Invoke(this, e);

	/// <summary>Raises the <see cref="HoverDetected"/> event.</summary>
	/// <param name="e">The event data.</param>
	private void OnHoverDetected(SKHoverGestureEventArgs e) => HoverDetected?.Invoke(this, e);

	/// <summary>Raises the <see cref="ScrollDetected"/> event.</summary>
	/// <param name="e">The event data.</param>
	private void OnScrollDetected(SKScrollGestureEventArgs e) => ScrollDetected?.Invoke(this, e);

	/// <summary>Raises the <see cref="GestureStarted"/> event.</summary>
	/// <param name="e">The event data.</param>
	private void OnGestureStarted(SKGestureLifecycleEventArgs e) => GestureStarted?.Invoke(this, e);

	/// <summary>Raises the <see cref="GestureEnded"/> event.</summary>
	/// <param name="e">The event data.</param>
	private void OnGestureEnded(SKGestureLifecycleEventArgs e) => GestureEnded?.Invoke(this, e);

	private enum GestureState
	{
		None,
		Detecting,
		Panning,
		Pinching
	}

	private readonly record struct TouchState(long Id, SKPoint Location, long Ticks, bool InContact, bool IsMouse);

	private readonly record struct PinchState(SKPoint Center, float Radius, float Angle)
	{
		public static PinchState FromLocations(SKPoint[] locations)
		{
			if (locations == null || locations.Length < 2)
				return new PinchState(locations?.Length > 0 ? locations[0] : SKPoint.Empty, 0, 0);

			var centerX = 0f;
			var centerY = 0f;
			foreach (var loc in locations)
			{
				centerX += loc.X;
				centerY += loc.Y;
			}
			centerX /= locations.Length;
			centerY /= locations.Length;

			var center = new SKPoint(centerX, centerY);
			var radius = 0f;
			foreach (var loc in locations)
				radius += SKPoint.Distance(center, loc);
			radius /= locations.Length;
			var angle = (float)(Math.Atan2(locations[1].Y - locations[0].Y, locations[1].X - locations[0].X) * 180 / Math.PI);

			return new PinchState(center, radius, angle);
		}
	}
}
