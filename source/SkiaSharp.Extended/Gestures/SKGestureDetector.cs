using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SkiaSharp.Extended.Gestures;

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
public class SKGestureDetector : IDisposable
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
	/// Gets the configuration options for this engine.
	/// </summary>
	public SKGestureDetectorOptions Options { get; }

	/// <summary>
	/// Gets or sets the current time provider. Used for testing.
	/// </summary>
	public Func<long> TimeProvider { get; set; } = () => DateTime.Now.Ticks;

	/// <summary>
	/// Gets or sets whether the engine is enabled.
	/// </summary>
	public bool IsEnabled { get; set; } = true;

	/// <summary>
	/// Gets whether a gesture is currently in progress.
	/// </summary>
	public bool IsGestureActive => _gestureState != GestureState.None;

	/// <summary>
	/// Occurs when a tap is detected.
	/// </summary>
	public event EventHandler<SKTapGestureEventArgs>? TapDetected;

	/// <summary>
	/// Occurs when a double tap is detected.
	/// </summary>
	public event EventHandler<SKTapGestureEventArgs>? DoubleTapDetected;

	/// <summary>
	/// Occurs when a long press is detected.
	/// </summary>
	public event EventHandler<SKLongPressGestureEventArgs>? LongPressDetected;

	/// <summary>
	/// Occurs when a pan gesture is detected.
	/// </summary>
	public event EventHandler<SKPanGestureEventArgs>? PanDetected;

	/// <summary>
	/// Occurs when a pinch (scale) gesture is detected.
	/// </summary>
	public event EventHandler<SKPinchGestureEventArgs>? PinchDetected;

	/// <summary>
	/// Occurs when a rotation gesture is detected.
	/// </summary>
	public event EventHandler<SKRotateGestureEventArgs>? RotateDetected;

	/// <summary>
	/// Occurs when a fling gesture is detected (fires once with initial velocity).
	/// </summary>
	public event EventHandler<SKFlingGestureEventArgs>? FlingDetected;

	/// <summary>
	/// Occurs when a hover is detected.
	/// </summary>
	public event EventHandler<SKHoverGestureEventArgs>? HoverDetected;

	/// <summary>
	/// Occurs when a mouse scroll (wheel) event is detected.
	/// </summary>
	public event EventHandler<SKScrollGestureEventArgs>? ScrollDetected;

	/// <summary>Occurs when a gesture starts.</summary>
	public event EventHandler<SKGestureLifecycleEventArgs>? GestureStarted;

	/// <summary>Occurs when a gesture ends.</summary>
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

		_touches[id] = new TouchState(id, location, ticks, true);

		// Only set initial touch state for the first finger
		if (_touches.Count == 1)
		{
			_initialTouch = location;
			_touchStartTicks = ticks;
			_longPressTriggered = false;
		}

		// Start the long press timer
		StartLongPressTimer();

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
			// Raise gesture started
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

		if (!_touches.ContainsKey(id))
			return false;

		_touches[id] = new TouchState(id, location, ticks, inContact);
		_flingTracker.AddEvent(id, location, ticks);

		var touchPoints = GetActiveTouchPoints();
		var distance = SKPoint.Distance(location, _initialTouch);

		// Start pan if moved beyond touch slop
		if (_gestureState == GestureState.Detecting && distance >= Options.TouchSlop)
		{
			StopLongPressTimer();
			_gestureState = GestureState.Panning;
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
	/// <param name="isMouse">Whether this is a mouse event.</param>
	/// <returns>True if the event was handled.</returns>
	public bool ProcessTouchUp(long id, SKPoint location, bool isMouse = false)
	{
		if (!IsEnabled || _disposed)
			return false;

		StopLongPressTimer();
		var ticks = TimeProvider();

		if (!_touches.TryGetValue(id, out var releasedTouch))
			return false;

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
			var maxTapDuration = isMouse ? ShortClickTicks : Options.LongPressDuration * TimeSpan.TicksPerMillisecond;

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
	/// Resets the gesture engine state.
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
	/// Disposes the gesture engine and releases resources.
	/// </summary>
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
		if (_syncContext != null)
		{
			_syncContext.Post(_ =>
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
		return _touches.Values
			.Where(t => t.InContact)
			.Select(t => t.Location)
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
	protected virtual void OnTapDetected(SKTapGestureEventArgs e) => TapDetected?.Invoke(this, e);
	protected virtual void OnDoubleTapDetected(SKTapGestureEventArgs e) => DoubleTapDetected?.Invoke(this, e);
	protected virtual void OnLongPressDetected(SKLongPressGestureEventArgs e) => LongPressDetected?.Invoke(this, e);
	protected virtual void OnPanDetected(SKPanGestureEventArgs e) => PanDetected?.Invoke(this, e);
	protected virtual void OnPinchDetected(SKPinchGestureEventArgs e) => PinchDetected?.Invoke(this, e);
	protected virtual void OnRotateDetected(SKRotateGestureEventArgs e) => RotateDetected?.Invoke(this, e);
	protected virtual void OnFlingDetected(SKFlingGestureEventArgs e) => FlingDetected?.Invoke(this, e);
	protected virtual void OnHoverDetected(SKHoverGestureEventArgs e) => HoverDetected?.Invoke(this, e);
	protected virtual void OnScrollDetected(SKScrollGestureEventArgs e) => ScrollDetected?.Invoke(this, e);
	protected virtual void OnGestureStarted(SKGestureLifecycleEventArgs e) => GestureStarted?.Invoke(this, e);
	protected virtual void OnGestureEnded(SKGestureLifecycleEventArgs e) => GestureEnded?.Invoke(this, e);

	private enum GestureState
	{
		None,
		Detecting,
		Panning,
		Pinching
	}

	private readonly record struct TouchState(long Id, SKPoint Location, long Ticks, bool InContact);

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
			var radius = SKPoint.Distance(center, locations[0]);
			var angle = (float)(Math.Atan2(locations[1].Y - locations[0].Y, locations[1].X - locations[0].X) * 180 / Math.PI);

			return new PinchState(center, radius, angle);
		}
	}
}
