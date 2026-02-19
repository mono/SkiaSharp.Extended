using System;
using System.Collections.Generic;
using System.Linq;

namespace SkiaSharp.Extended.Gestures;

/// <summary>
/// A platform-agnostic gesture recognition engine that can detect taps, long presses,
/// pan, pinch, rotation, and fling gestures from touch input.
/// </summary>
/// <remarks>
/// <para>This engine is designed to be testable and reusable across different platforms.
/// It processes touch events and raises events when gestures are detected.</para>
/// <para>Selection modes:</para>
/// <list type="bullet">
///   <item><description><see cref="SKGestureSelectionMode.TapToSelect"/> - Tap to select, then drag</description></item>
///   <item><description><see cref="SKGestureSelectionMode.LongPressToSelect"/> - Long press to select, then drag</description></item>
///   <item><description><see cref="SKGestureSelectionMode.Immediate"/> - Start dragging immediately on touch</description></item>
/// </list>
/// </remarks>
public class SKGestureEngine
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
	
	private SKPoint _initialTouch = SKPoint.Empty;
	private long _lastTapTicks;
	private int _tapCount;
	private GestureState _gestureState = GestureState.None;
	private PinchState _pinchState;
	private long? _selectedItemId;
	private bool _longPressTriggered;
	private long _touchStartTicks;

	/// <summary>
	/// Gets or sets the current time provider. Used for testing.
	/// </summary>
	public Func<long> TimeProvider { get; set; } = () => DateTime.Now.Ticks;

	/// <summary>
	/// Gets or sets the selection mode for the gesture engine.
	/// </summary>
	public SKGestureSelectionMode SelectionMode { get; set; } = SKGestureSelectionMode.Immediate;

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
	/// Gets or sets the currently selected item ID.
	/// </summary>
	public long? SelectedItemId
	{
		get => _selectedItemId;
		set
		{
			if (_selectedItemId != value)
			{
				_selectedItemId = value;
				SelectionChanged?.Invoke(this, new SKSelectionChangedEventArgs(value));
			}
		}
	}

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
	/// Occurs when selection changes.
	/// </summary>
	public event EventHandler<SKSelectionChangedEventArgs>? SelectionChanged;

	/// <summary>
	/// Occurs when a drag operation starts (after selection criteria is met).
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
		if (!IsEnabled)
			return false;

		var ticks = TimeProvider();
		
		_touches[id] = new TouchState(id, location, ticks, true);
		_initialTouch = location;
		_touchStartTicks = ticks;
		_longPressTriggered = false;
		_flingTracker.Clear();

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
		if (!IsEnabled)
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

		// Check for long press
		if (!_longPressTriggered && 
			touchPoints.Length == 1 && 
			distance < TouchSlop &&
			(ticks - _touchStartTicks) >= LongPressTicks * TimeSpan.TicksPerMillisecond / 1000)
		{
			_longPressTriggered = true;
			
			// In LongPressToSelect mode, selecting happens on long press
			if (SelectionMode == SKGestureSelectionMode.LongPressToSelect)
			{
				OnLongPressDetected(new SKTapEventArgs(location, 1));
				OnDragStarted(new SKDragEventArgs(location, location, SKPoint.Empty));
				_gestureState = GestureState.Dragging;
			}
			else
			{
				OnLongPressDetected(new SKTapEventArgs(location, 1));
			}
			
			return true;
		}

		// Start pan/drag if moved beyond touch slop
		if (_gestureState == GestureState.Detecting && distance >= TouchSlop)
		{
			if (SelectionMode == SKGestureSelectionMode.Immediate)
			{
				_gestureState = GestureState.Panning;
				OnDragStarted(new SKDragEventArgs(_initialTouch, location, location - _initialTouch));
			}
			else if (SelectionMode == SKGestureSelectionMode.TapToSelect && _selectedItemId.HasValue)
			{
				_gestureState = GestureState.Dragging;
				OnDragStarted(new SKDragEventArgs(_initialTouch, location, location - _initialTouch));
			}
			else
			{
				_gestureState = GestureState.Panning;
			}
		}

		switch (_gestureState)
		{
			case GestureState.Panning:
				if (touchPoints.Length == 1)
				{
					var delta = location - _pinchState.Center;
					OnPanDetected(new SKPanEventArgs(location, _pinchState.Center, delta));
					_pinchState = new PinchState(location, 0, 0);
				}
				break;

			case GestureState.Dragging:
				if (touchPoints.Length == 1)
				{
					var delta = location - _pinchState.Center;
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
		if (!IsEnabled)
			return false;

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
					
					// In TapToSelect mode, tapping selects
					if (SelectionMode == SKGestureSelectionMode.TapToSelect)
					{
						// Selection would be handled by consumer based on tap location
					}
				}
				handled = true;
			}
		}

		_flingTracker.RemoveId(id);

		// Handle end of drag
		if (_gestureState == GestureState.Dragging)
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
	/// Checks if long press should be triggered (called from timer).
	/// </summary>
	public void CheckLongPress()
	{
		if (!IsEnabled || _longPressTriggered || _gestureState != GestureState.Detecting)
			return;

		var ticks = TimeProvider();
		var duration = ticks - _touchStartTicks;
		var touchPoints = GetActiveTouchPoints();

		if (touchPoints.Length == 1 && duration >= LongPressDuration * TimeSpan.TicksPerMillisecond)
		{
			var distance = SKPoint.Distance(touchPoints[0], _initialTouch);
			if (distance < TouchSlop)
			{
				_longPressTriggered = true;
				OnLongPressDetected(new SKTapEventArgs(touchPoints[0], 1));

				if (SelectionMode == SKGestureSelectionMode.LongPressToSelect)
				{
					_gestureState = GestureState.Dragging;
					OnDragStarted(new SKDragEventArgs(touchPoints[0], touchPoints[0], SKPoint.Empty));
				}
			}
		}
	}

	/// <summary>
	/// Resets the gesture engine state.
	/// </summary>
	public void Reset()
	{
		_touches.Clear();
		_flingTracker.Clear();
		_gestureState = GestureState.None;
		_tapCount = 0;
		_lastTapTicks = 0;
		_longPressTriggered = false;
		_selectedItemId = null;
	}

	private SKPoint[] GetActiveTouchPoints() =>
		_touches.Values
			.Where(t => t.InContact)
			.Select(t => t.Location)
			.ToArray();

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
/// The selection mode for the gesture engine.
/// </summary>
public enum SKGestureSelectionMode
{
	/// <summary>
	/// Start dragging immediately on touch (no selection required).
	/// </summary>
	Immediate,

	/// <summary>
	/// Tap to select, then drag the selected item.
	/// </summary>
	TapToSelect,

	/// <summary>
	/// Long press to select and start dragging.
	/// </summary>
	LongPressToSelect
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
	Pinching,

	/// <summary>
	/// A drag operation is in progress (after selection).
	/// </summary>
	Dragging
}
