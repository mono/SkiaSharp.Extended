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
	private const float FlingVelocityThreshold = 200f; // pixels per second

	private readonly Dictionary<long, TouchState> _touches = new();
	private readonly FlingTracker _flingTracker = new();
	private SynchronizationContext? _syncContext;
	private Timer? _longPressTimer;
	
	private SKPoint _initialTouch = SKPoint.Empty;
	private long _lastTapTicks;
	private int _tapCount;
	private GestureState _gestureState = GestureState.None;
	private PinchState _pinchState;
	private bool _longPressTriggered;
	private long _touchStartTicks;
	private bool _disposed;

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
	/// Gets or sets the fling velocity threshold.
	/// </summary>
	public float FlingThreshold { get; set; } = FlingVelocityThreshold;

	/// <summary>
	/// Gets or sets the long press duration in milliseconds.
	/// </summary>
	public int LongPressDuration { get; set; } = (int)(LongPressTicks / TimeSpan.TicksPerMillisecond);

	/// <summary>
	/// Gets whether a gesture is currently in progress.
	/// </summary>
	public bool IsGestureActive => _gestureState != GestureState.None;

	/// <summary>
	/// Gets the current gesture state.
	/// </summary>
	public GestureState CurrentState => _gestureState;

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

		var ticks = TimeProvider();
		
		_touches[id] = new TouchState(id, location, ticks, true);
		
		_initialTouch = location;
		_touchStartTicks = ticks;
		_longPressTriggered = false;
		_flingTracker.Clear();

		// Start the long press timer
		StartLongPressTimer();

		// Check for double tap
		if (ticks - _lastTapTicks < DoubleTapDelayTicks && 
			SKPoint.Distance(location, _initialTouch) < TouchSlop)
		{
			_tapCount++;
		}
		else
		{
			_tapCount = 1;
		}

		var touchPoints = GetActiveTouchPoints();
		
		if (touchPoints.Length > 0)
		{
			// Raise gesture started
			OnGestureStarted(new SKGestureStateEventArgs(touchPoints, GestureState.Detecting));
			
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
		
		if (!_touches.ContainsKey(id))
			return false;

		_touches[id] = new TouchState(id, location, ticks, inContact);

		if (inContact)
			_flingTracker.AddEvent(id, location, ticks);

		// Handle hover (mouse without contact)
		if (!inContact)
		{
			OnHoverDetected(new SKHoverEventArgs(location));
			return true;
		}

		var touchPoints = GetActiveTouchPoints();
		var distance = SKPoint.Distance(location, _initialTouch);

		// Start pan/drag if moved beyond touch slop
		if (_gestureState == GestureState.Detecting && distance >= TouchSlop)
		{
			StopLongPressTimer();
			_gestureState = GestureState.Panning;
			OnDragStarted(new SKDragEventArgs(_initialTouch, location, location - _initialTouch));
		}

		switch (_gestureState)
		{
			case GestureState.Panning:
				if (touchPoints.Length == 1)
				{
					var delta = location - _pinchState.Center;
					OnPanDetected(new SKPanEventArgs(location, _pinchState.Center, delta));
					OnDragUpdated(new SKDragEventArgs(_initialTouch, location, delta));
					_pinchState = new PinchState(location, 0, 0);
				}
				break;

			case GestureState.Pinching:
				if (touchPoints.Length >= 2)
				{
					var newPinch = PinchState.FromLocations(touchPoints);
					
					// Calculate scale
					var scaleDelta = _pinchState.Radius > 0 ? newPinch.Radius / _pinchState.Radius : 1f;
					OnPinchDetected(new SKPinchEventArgs(newPinch.Center, _pinchState.Center, scaleDelta));

					// Calculate rotation
					var rotationDelta = newPinch.Angle - _pinchState.Angle;
					rotationDelta = NormalizeAngle(rotationDelta);
					OnRotateDetected(new SKRotateEventArgs(newPinch.Center, rotationDelta));

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
				handled = true;
			}

			// Check for tap
			var distance = SKPoint.Distance(releasedTouch.Location, _initialTouch);
			var duration = ticks - releasedTouch.Ticks;
			var maxTapDuration = isMouse ? ShortClickTicks : LongPressTicks;

			if (distance < TouchSlop && duration < maxTapDuration && !_longPressTriggered)
			{
				_lastTapTicks = ticks;
				
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

		_flingTracker.RemoveId(id);

		// Handle end of drag/pan
		if (_gestureState == GestureState.Panning)
		{
			OnDragEnded(new SKDragEventArgs(_initialTouch, location, location - _initialTouch));
		}

		// Transition gesture state
		if (touchPoints.Length == 0)
		{
			if (_gestureState != GestureState.None)
			{
				OnGestureEnded(new SKGestureStateEventArgs(Array.Empty<SKPoint>(), _gestureState));
				_gestureState = GestureState.None;
			}
		}
		else if (touchPoints.Length == 1)
		{
			_gestureState = GestureState.Panning;
			_pinchState = new PinchState(touchPoints[0], 0, 0);
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
		if (!IsEnabled)
			return false;

		_touches.Remove(id);
		_flingTracker.RemoveId(id);

		var touchPoints = GetActiveTouchPoints();
		if (touchPoints.Length == 0 && _gestureState != GestureState.None)
		{
			OnGestureEnded(new SKGestureStateEventArgs(Array.Empty<SKPoint>(), _gestureState));
			_gestureState = GestureState.None;
		}

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
		_longPressTimer = new Timer(OnLongPressTimerTick, null, LongPressDuration, Timeout.Infinite);
	}

	private void StopLongPressTimer()
	{
		_longPressTimer?.Dispose();
		_longPressTimer = null;
	}

	private void OnLongPressTimerTick(object? state)
	{
		// Marshal to UI thread if we have a sync context
		if (_syncContext != null)
		{
			_syncContext.Post(_ => HandleLongPress(), null);
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
		if (angle > 180f) angle -= 360f;
		if (angle < -180f) angle += 360f;
		return angle;
	}

	// Event invokers
	protected virtual void OnTapDetected(SKTapEventArgs e) => TapDetected?.Invoke(this, e);
	protected virtual void OnDoubleTapDetected(SKTapEventArgs e) => DoubleTapDetected?.Invoke(this, e);
	protected virtual void OnLongPressDetected(SKTapEventArgs e) => LongPressDetected?.Invoke(this, e);
	protected virtual void OnPanDetected(SKPanEventArgs e) => PanDetected?.Invoke(this, e);
	protected virtual void OnPinchDetected(SKPinchEventArgs e) => PinchDetected?.Invoke(this, e);
	protected virtual void OnRotateDetected(SKRotateEventArgs e) => RotateDetected?.Invoke(this, e);
	protected virtual void OnFlingDetected(SKFlingEventArgs e) => FlingDetected?.Invoke(this, e);
	protected virtual void OnHoverDetected(SKHoverEventArgs e) => HoverDetected?.Invoke(this, e);
	protected virtual void OnGestureStarted(SKGestureStateEventArgs e) => GestureStarted?.Invoke(this, e);
	protected virtual void OnGestureEnded(SKGestureStateEventArgs e) => GestureEnded?.Invoke(this, e);
	protected virtual void OnDragStarted(SKDragEventArgs e) => DragStarted?.Invoke(this, e);
	protected virtual void OnDragUpdated(SKDragEventArgs e) => DragUpdated?.Invoke(this, e);
	protected virtual void OnDragEnded(SKDragEventArgs e) => DragEnded?.Invoke(this, e);
}

/// <summary>
/// The current state of a gesture.
/// </summary>
public enum GestureState
{
	/// <summary>
	/// No gesture is active.
	/// </summary>
	None,

	/// <summary>
	/// A gesture is being detected.
	/// </summary>
	Detecting,

	/// <summary>
	/// A pan gesture is in progress.
	/// </summary>
	Panning,

	/// <summary>
	/// A pinch/zoom gesture is in progress.
	/// </summary>
	Pinching
}
