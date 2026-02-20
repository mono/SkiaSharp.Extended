using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SkiaSharp.Extended.Gestures;

/// <summary>
/// A platform-agnostic gesture recognition engine that can detect taps, long presses,
/// pan, pinch, rotation, and fling gestures from touch input.
/// </summary>
/// <remarks>
/// <para>This engine is designed to be testable and reusable across different platforms.
/// It processes touch events and raises events when gestures are detected.</para>
/// <para>The engine must be used on the UI thread. It captures the current 
/// <see cref="SynchronizationContext"/> when processing touch events and uses it
/// to marshal timer callbacks back to the UI thread.</para>
/// <para>Call <see cref="Dispose"/> to clean up resources when done.</para>
/// </remarks>
public class SKGestureEngine : IDisposable
{
	// Timing constants
	private const long ShortTapTicks = 125 * TimeSpan.TicksPerMillisecond;
	private const long ShortClickTicks = 250 * TimeSpan.TicksPerMillisecond;
	private const long DoubleTapDelayTicks = 300 * TimeSpan.TicksPerMillisecond;
	private const long LongPressTicks = 500 * TimeSpan.TicksPerMillisecond;

	// Distance and velocity thresholds
	private const float TouchSlopPixels = 8f;
	private const float DoubleTapSlopPixels = 40f;
	private const float FlingVelocityThreshold = 200f; // pixels per second
	private const float DefaultFlingFriction = 0.92f;
	private const float DefaultFlingMinVelocity = 5f;
	private const int DefaultFlingFrameMs = 16;

	private readonly Dictionary<long, SKTouchState> _touches = new();
	private readonly SKFlingTracker _flingTracker = new();
	private SynchronizationContext? _syncContext;
	private Timer? _longPressTimer;
	private int _longPressToken;

	private SKPoint _initialTouch = SKPoint.Empty;
	private SKPoint _lastTapLocation = SKPoint.Empty;
	private long _lastTapTicks;
	private int _tapCount;
	private SKGestureState _gestureState = SKGestureState.None;
	private SKPinchState _pinchState;
	private bool _longPressTriggered;
	private bool _dragStartedFired;
	private long _touchStartTicks;
	private bool _disposed;

	// Fling animation state
	private Timer? _flingTimer;
	private int _flingToken;
	private float _flingVelocityX;
	private float _flingVelocityY;
	private bool _isFlinging;

	/// <summary>
	/// Gets or sets the current time provider. Used for testing.
	/// </summary>
	public Func<long> TimeProvider { get; set; } = () => DateTime.Now.Ticks;

	/// <summary>
	/// Gets or sets whether the engine is enabled.
	/// </summary>
	public bool IsEnabled { get; set; } = true;

	/// <summary>
	/// Gets or sets the touch slop (minimum movement distance to start a gesture).
	/// </summary>
	public float TouchSlop { get; set; } = TouchSlopPixels;

	/// <summary>
	/// Gets or sets the maximum distance between two taps for double-tap detection.
	/// </summary>
	public float DoubleTapSlop { get; set; } = DoubleTapSlopPixels;

	/// <summary>
	/// Gets or sets the fling velocity threshold.
	/// </summary>
	public float FlingThreshold { get; set; } = FlingVelocityThreshold;

	/// <summary>
	/// Gets or sets the fling friction (deceleration factor per frame, 0-1). Higher = slower stop.
	/// </summary>
	public float FlingFriction { get; set; } = DefaultFlingFriction;

	/// <summary>
	/// Gets or sets the minimum fling velocity before the animation stops.
	/// </summary>
	public float FlingMinVelocity { get; set; } = DefaultFlingMinVelocity;

	/// <summary>
	/// Gets or sets the fling animation frame interval in milliseconds (~60 FPS = 16ms).
	/// </summary>
	public int FlingFrameInterval { get; set; } = DefaultFlingFrameMs;

	/// <summary>
	/// Gets whether a fling animation is currently running.
	/// </summary>
	public bool IsFlinging => _isFlinging;

	/// <summary>
	/// Gets or sets the long press duration in milliseconds.
	/// </summary>
	public int LongPressDuration { get; set; } = (int)(LongPressTicks / TimeSpan.TicksPerMillisecond);

	/// <summary>
	/// Gets whether a gesture is currently in progress.
	/// </summary>
	public bool IsGestureActive => _gestureState != SKGestureState.None;

	/// <summary>
	/// Gets the current gesture state.
	/// </summary>
	public SKGestureState CurrentState => _gestureState;

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
	/// Occurs when a fling gesture is detected (fires once with initial velocity).
	/// </summary>
	public event EventHandler<SKFlingEventArgs>? FlingDetected;

	/// <summary>
	/// Occurs each animation frame during a fling with current velocity and per-frame delta.
	/// </summary>
	public event EventHandler<SKFlingEventArgs>? Flinging;

	/// <summary>
	/// Occurs when a fling animation completes (velocity dropped below minimum).
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

		// Stop any active fling animation on new touch
		StopFling();

		var ticks = TimeProvider();

		_touches[id] = new SKTouchState(id, location, ticks, true);

		// Only set initial touch state for the first finger
		if (_touches.Count == 1)
		{
			_initialTouch = location;
			_touchStartTicks = ticks;
			_longPressTriggered = false;
			_dragStartedFired = false;
		}

		// Start the long press timer
		StartLongPressTimer();

		// Check for double tap using the last completed tap location
		if (_touches.Count == 1 &&
			ticks - _lastTapTicks < DoubleTapDelayTicks &&
			SKPoint.Distance(location, _lastTapLocation) < DoubleTapSlop)
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
			OnGestureStarted(new SKGestureStateEventArgs(touchPoints, SKGestureState.Detecting));

			if (touchPoints.Length >= 2)
			{
				_pinchState = SKPinchState.FromLocations(touchPoints);
				_gestureState = SKGestureState.Pinching;
			}
			else
			{
				_pinchState = new SKPinchState(touchPoints[0], 0, 0);
				_gestureState = SKGestureState.Detecting;
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
			OnHoverDetected(new SKHoverEventArgs(location));
			return true;
		}

		if (!_touches.ContainsKey(id))
			return false;

		_touches[id] = new SKTouchState(id, location, ticks, inContact);
		_flingTracker.AddEvent(id, location, ticks);

		var touchPoints = GetActiveTouchPoints();
		var distance = SKPoint.Distance(location, _initialTouch);

		// Start pan/drag if moved beyond touch slop
		if (_gestureState == SKGestureState.Detecting && distance >= TouchSlop)
		{
			StopLongPressTimer();
			_gestureState = SKGestureState.Panning;
			_dragStartedFired = true;
			OnDragStarted(new SKDragEventArgs(_initialTouch, location, location - _initialTouch));
		}

		switch (_gestureState)
		{
			case SKGestureState.Panning:
				if (touchPoints.Length == 1)
				{
					var delta = location - _pinchState.Center;
					OnPanDetected(new SKPanEventArgs(location, _pinchState.Center, delta));
					if (_dragStartedFired)
						OnDragUpdated(new SKDragEventArgs(_initialTouch, location, delta));
					_pinchState = new SKPinchState(location, 0, 0);
				}
				break;

			case SKGestureState.Pinching:
				if (touchPoints.Length >= 2)
				{
					var newPinch = SKPinchState.FromLocations(touchPoints);

					// Calculate scale
					var scaleDelta = _pinchState.Radius > 0 ? newPinch.Radius / _pinchState.Radius : 1f;
					OnPinchDetected(new SKPinchEventArgs(newPinch.Center, _pinchState.Center, scaleDelta));

					// Calculate rotation
					var rotationDelta = newPinch.Angle - _pinchState.Angle;
					rotationDelta = NormalizeAngle(rotationDelta);
					OnRotateDetected(new SKRotateEventArgs(newPinch.Center, _pinchState.Center, rotationDelta));

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

		// Check for fling
		if (touchPoints.Length == 0)
		{
			var velocity = _flingTracker.CalculateVelocity(id, ticks);
			var velocityMagnitude = (float)Math.Sqrt(velocity.X * velocity.X + velocity.Y * velocity.Y);

			if (velocityMagnitude > FlingThreshold)
			{
				OnFlingDetected(new SKFlingEventArgs(velocity.X, velocity.Y));
				StartFlingAnimation(velocity.X, velocity.Y);
				handled = true;
			}

			// Check for tap — only if we haven't transitioned to panning/pinching
			if (_gestureState == SKGestureState.Detecting)
			{
				var distance = SKPoint.Distance(location, _initialTouch);
				var duration = ticks - _touchStartTicks;
				var maxTapDuration = isMouse ? ShortClickTicks : LongPressTicks;

				if (distance < TouchSlop && duration < maxTapDuration && !_longPressTriggered)
				{
					_lastTapTicks = ticks;
					_lastTapLocation = location;

					if (_tapCount > 1)
					{
						OnDoubleTapDetected(new SKTapEventArgs(location, _tapCount));
						_tapCount = 0;
					}
					else
					{
						OnTapDetected(new SKTapEventArgs(location, 1));
					}
					handled = true;
				}
			}
		}

		_flingTracker.RemoveId(id);

		// Handle end of drag/pan
		if (_gestureState == SKGestureState.Panning && _dragStartedFired)
		{
			OnDragEnded(new SKDragEventArgs(_initialTouch, location, location - _initialTouch));
		}

		// Transition gesture state
		if (touchPoints.Length == 0)
		{
			if (_gestureState != SKGestureState.None)
			{
				OnGestureEnded(new SKGestureStateEventArgs(Array.Empty<SKPoint>(), _gestureState));
				_gestureState = SKGestureState.None;
			}
			_dragStartedFired = false;
		}
		else if (touchPoints.Length == 1)
		{
			// Transition from pinch to pan — update origin and fire DragStarted
			if (_gestureState == SKGestureState.Pinching)
			{
				_initialTouch = touchPoints[0];
				if (!_dragStartedFired)
				{
					_dragStartedFired = true;
					OnDragStarted(new SKDragEventArgs(touchPoints[0], touchPoints[0], SKPoint.Empty));
				}
			}
			_gestureState = SKGestureState.Panning;
			_pinchState = new SKPinchState(touchPoints[0], 0, 0);
		}
		else if (touchPoints.Length >= 2)
		{
			// Recalculate pinch state for remaining fingers to avoid jumps
			_pinchState = SKPinchState.FromLocations(touchPoints);
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
			if (_gestureState == SKGestureState.Panning && _dragStartedFired)
			{
				OnDragEnded(new SKDragEventArgs(_initialTouch, _initialTouch, SKPoint.Empty));
			}
			if (_gestureState != SKGestureState.None)
			{
				OnGestureEnded(new SKGestureStateEventArgs(Array.Empty<SKPoint>(), _gestureState));
				_gestureState = SKGestureState.None;
			}
			_dragStartedFired = false;
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

		OnScrollDetected(new SKScrollEventArgs(location, deltaX, deltaY));
		return true;
	}

	/// <summary>
	/// Resets the gesture engine state.
	/// </summary>
	public void Reset()
	{
		StopLongPressTimer();
		StopFling();
		_touches.Clear();
		_flingTracker.Clear();
		_gestureState = SKGestureState.None;
		_tapCount = 0;
		_lastTapTicks = 0;
		_lastTapLocation = SKPoint.Empty;
		_longPressTriggered = false;
		_dragStartedFired = false;
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
		StopFling();
		Reset();
	}

	private void StartLongPressTimer()
	{
		StopLongPressTimer();
		var token = Interlocked.Increment(ref _longPressToken);
		var timer = new Timer(OnLongPressTimerTick, token, LongPressDuration, Timeout.Infinite);
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
		if (_disposed || !IsEnabled || _longPressTriggered || _gestureState != SKGestureState.Detecting)
			return;

		var touchPoints = GetActiveTouchPoints();

		if (touchPoints.Length == 1)
		{
			var distance = SKPoint.Distance(touchPoints[0], _initialTouch);
			if (distance < TouchSlop)
			{
				_longPressTriggered = true;
				StopLongPressTimer();
				OnLongPressDetected(new SKTapEventArgs(touchPoints[0], 1));
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

	/// <summary>
	/// Stops any active fling animation.
	/// </summary>
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
		OnFlingCompleted();
	}

	private void StartFlingAnimation(float velocityX, float velocityY)
	{
		// Stop any existing fling timer without firing completed
		var oldTimer = _flingTimer;
		oldTimer?.Change(Timeout.Infinite, Timeout.Infinite);
		oldTimer?.Dispose();

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

		// Calculate per-frame displacement
		var dt = FlingFrameInterval / 1000f;
		var deltaX = _flingVelocityX * dt;
		var deltaY = _flingVelocityY * dt;

		// Fire the per-frame event
		OnFlinging(new SKFlingEventArgs(_flingVelocityX, _flingVelocityY, deltaX, deltaY));

		// Apply friction
		_flingVelocityX *= FlingFriction;
		_flingVelocityY *= FlingFriction;

		// Check if velocity is below minimum
		var speed = (float)Math.Sqrt(_flingVelocityX * _flingVelocityX + _flingVelocityY * _flingVelocityY);
		if (speed < FlingMinVelocity)
		{
			StopFling();
		}
	}

	// Event invokers
	protected virtual void OnTapDetected(SKTapEventArgs e) => TapDetected?.Invoke(this, e);
	protected virtual void OnDoubleTapDetected(SKTapEventArgs e) => DoubleTapDetected?.Invoke(this, e);
	protected virtual void OnLongPressDetected(SKTapEventArgs e) => LongPressDetected?.Invoke(this, e);
	protected virtual void OnPanDetected(SKPanEventArgs e) => PanDetected?.Invoke(this, e);
	protected virtual void OnPinchDetected(SKPinchEventArgs e) => PinchDetected?.Invoke(this, e);
	protected virtual void OnRotateDetected(SKRotateEventArgs e) => RotateDetected?.Invoke(this, e);
	protected virtual void OnFlingDetected(SKFlingEventArgs e) => FlingDetected?.Invoke(this, e);
	protected virtual void OnFlinging(SKFlingEventArgs e) => Flinging?.Invoke(this, e);
	protected virtual void OnFlingCompleted() => FlingCompleted?.Invoke(this, EventArgs.Empty);
	protected virtual void OnHoverDetected(SKHoverEventArgs e) => HoverDetected?.Invoke(this, e);
	protected virtual void OnScrollDetected(SKScrollEventArgs e) => ScrollDetected?.Invoke(this, e);
	protected virtual void OnGestureStarted(SKGestureStateEventArgs e) => GestureStarted?.Invoke(this, e);
	protected virtual void OnGestureEnded(SKGestureStateEventArgs e) => GestureEnded?.Invoke(this, e);
	protected virtual void OnDragStarted(SKDragEventArgs e) => DragStarted?.Invoke(this, e);
	protected virtual void OnDragUpdated(SKDragEventArgs e) => DragUpdated?.Invoke(this, e);
	protected virtual void OnDragEnded(SKDragEventArgs e) => DragEnded?.Invoke(this, e);
}
