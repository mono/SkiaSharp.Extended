using Timer = System.Threading.Timer;

namespace SkiaSharp.Extended.UI.Controls;

/// <summary>
/// A SkiaSharp view with built-in gesture detection for pan, pinch, fling, tap, and rotate.
/// </summary>
/// <remarks>
/// <para>
/// This view extends <see cref="SKDynamicSurfaceView"/> to add comprehensive gesture detection
/// including:
/// </para>
/// <list type="bullet">
/// <item><description>Single and double tap detection</description></item>
/// <item><description>Long press detection</description></item>
/// <item><description>Pan/drag gestures</description></item>
/// <item><description>Pinch to zoom gestures</description></item>
/// <item><description>Rotation gestures</description></item>
/// <item><description>Fling (swipe) gesture detection with velocity</description></item>
/// <item><description>Hover detection for mouse/stylus</description></item>
/// </list>
/// </remarks>
public partial class SKGestureSurfaceView : SKDynamicSurfaceView
{
	// Timing constants
	private const long ShortTapTicks = 125 * TimeSpan.TicksPerMillisecond;
	private const long ShortClickTicks = 250 * TimeSpan.TicksPerMillisecond;
	private const int DelayTapMilliseconds = 200;
	private const long LongTapTicks = 500 * TimeSpan.TicksPerMillisecond;

	// Distance and velocity thresholds
	private const int TouchSlopPixels = 8;
	private const int FlingVelocityThreshold = 200; // pixels per second

	private readonly Dictionary<long, TouchEvent> touches = new();
	private readonly FlingTracker flingTracker = new();
	private SKPoint initialTouch = SKPoint.Empty;
	private Timer? multiTapTimer;
	private int tapCount = 0;
	private TouchMode touchMode = TouchMode.None;
	private PinchValue previousValues;
	private IDispatcher? dispatcher;

	/// <summary>
	/// Creates a new instance of SKGestureSurfaceView.
	/// </summary>
	public SKGestureSurfaceView()
	{
		EnableTouchEvents = true;
		Touch += OnTouchEvent;
		Loaded += OnLoaded;
	}

	/// <summary>
	/// Occurs when a gesture starts.
	/// </summary>
	public event EventHandler<SKGestureEventArgs>? GestureStarted;

	/// <summary>
	/// Occurs when a gesture ends.
	/// </summary>
	public event EventHandler<SKGestureEventArgs>? GestureEnded;

	/// <summary>
	/// Occurs when a long press is detected.
	/// </summary>
	public event EventHandler<SKTapDetectedEventArgs>? LongPressDetected;

	/// <summary>
	/// Occurs when a single tap is detected.
	/// </summary>
	public event EventHandler<SKTapDetectedEventArgs>? SingleTapDetected;

	/// <summary>
	/// Occurs when a double tap (or multi-tap) is detected.
	/// </summary>
	public event EventHandler<SKTapDetectedEventArgs>? DoubleTapDetected;

	/// <summary>
	/// Occurs when a hover is detected (mouse or stylus without contact).
	/// </summary>
	public event EventHandler<SKHoverDetectedEventArgs>? HoverDetected;

	/// <summary>
	/// Occurs when a fling gesture is detected.
	/// </summary>
	public event EventHandler<SKFlingDetectedEventArgs>? FlingDetected;

	/// <summary>
	/// Occurs when a transform gesture (pan, zoom, rotate) is detected.
	/// </summary>
	public event EventHandler<SKTransformDetectedEventArgs>? TransformDetected;

	/// <summary>
	/// Invokes the <see cref="SingleTapDetected"/> event.
	/// </summary>
	protected virtual void OnSingleTapDetected(SKTapDetectedEventArgs e) =>
		SingleTapDetected?.Invoke(this, e);

	/// <summary>
	/// Invokes the <see cref="DoubleTapDetected"/> event.
	/// </summary>
	protected virtual void OnDoubleTapDetected(SKTapDetectedEventArgs e) =>
		DoubleTapDetected?.Invoke(this, e);

	/// <summary>
	/// Invokes the <see cref="LongPressDetected"/> event.
	/// </summary>
	protected virtual void OnLongPressDetected(SKTapDetectedEventArgs e) =>
		LongPressDetected?.Invoke(this, e);

	/// <summary>
	/// Invokes the <see cref="HoverDetected"/> event.
	/// </summary>
	protected virtual void OnHoverDetected(SKHoverDetectedEventArgs e) =>
		HoverDetected?.Invoke(this, e);

	/// <summary>
	/// Invokes the <see cref="FlingDetected"/> event.
	/// </summary>
	protected virtual void OnFlingDetected(SKFlingDetectedEventArgs e) =>
		FlingDetected?.Invoke(this, e);

	/// <summary>
	/// Invokes the <see cref="TransformDetected"/> event.
	/// </summary>
	protected virtual void OnTransformDetected(SKTransformDetectedEventArgs e) =>
		TransformDetected?.Invoke(this, e);

	/// <summary>
	/// Invokes the <see cref="GestureStarted"/> event.
	/// </summary>
	protected virtual void OnGestureStarted(SKGestureEventArgs e) =>
		GestureStarted?.Invoke(this, e);

	/// <summary>
	/// Invokes the <see cref="GestureEnded"/> event.
	/// </summary>
	protected virtual void OnGestureEnded(SKGestureEventArgs e) =>
		GestureEnded?.Invoke(this, e);

	private void OnLoaded(object? sender, EventArgs e)
	{
		dispatcher = Dispatcher;
	}

	private void OnTouchEvent(object? sender, SKTouchEventArgs e)
	{
		switch (e.ActionType)
		{
			case SKTouchAction.Pressed:
				e.Handled = OnTouchPressed(e);
				break;
			case SKTouchAction.Moved:
				e.Handled = OnTouchMoved(e);
				break;
			case SKTouchAction.Released:
				e.Handled = OnTouchReleased(e);
				break;
			case SKTouchAction.Cancelled:
				e.Handled = OnTouchCancelled(e);
				break;
		}
	}

	private bool OnTouchPressed(SKTouchEventArgs e)
	{
		var ticks = DateTime.Now.Ticks;
		var location = e.Location;

		initialTouch = location;
		touches[e.Id] = new TouchEvent(e.Id, location, ticks, e.InContact);

		// Update the fling tracker
		flingTracker.Clear();

		// If we are in the middle of a multi-tap, then restart with more taps
		if (multiTapTimer is not null)
		{
			multiTapTimer.Dispose();
			multiTapTimer = null;
			tapCount++;
		}
		else
		{
			tapCount = 1;
		}

		var handled = false;

		// Start detecting once a finger is on the screen
		var touchPoints = GetInContactTouchPoints();
		if (touchPoints.Length > 0)
		{
			// Try start a gesture
			var args = new SKGestureEventArgs(touchPoints);
			OnGestureStarted(args);
			handled = args.Handled;

			// If no gesture was detected, then we will handle it
			if (!handled)
			{
				if (touchPoints.Length == 2)
				{
					previousValues = PinchValue.FromLocations(touchPoints);
					touchMode = TouchMode.Multiple;
				}
				else
				{
					previousValues.Center = touchPoints[0];
					touchMode = TouchMode.Single;
				}
				handled = true;
			}
		}

		return handled;
	}

	private bool OnTouchMoved(SKTouchEventArgs e)
	{
		var ticks = DateTime.Now.Ticks;
		var location = e.Location;

		touches[e.Id] = new TouchEvent(e.Id, location, ticks, e.InContact);

		// Update the fling tracker
		if (e.InContact)
			flingTracker.AddEvent(e.Id, location, ticks);

		// If this is a mouse or pen hover, then raise an event
		if (!e.InContact)
		{
			var args = new SKHoverDetectedEventArgs(e.Location);
			OnHoverDetected(args);
			return args.Handled;
		}

		var touchPoints = GetInContactTouchPoints();

		switch (touchMode)
		{
			case TouchMode.Single:
				{
					if (touchPoints.Length != 1)
						return false;

					var touchPosition = touchPoints[0];

					if (!previousValues.Center.IsEmpty)
					{
						var args = new SKTransformDetectedEventArgs(touchPosition, previousValues.Center);
						OnTransformDetected(args);
					}

					previousValues.Center = touchPosition;
				}
				break;

			case TouchMode.Multiple:
				{
					if (touchPoints.Length != 2)
						return false;

					var prevVals = previousValues;
					var pinchValue = PinchValue.FromLocations(touchPoints);

					var rotationDelta = pinchValue.Angle - prevVals.Angle;
					rotationDelta %= 360;

					if (rotationDelta > 180)
						rotationDelta -= 360;
					else if (rotationDelta < -180)
						rotationDelta += 360;

					var scaleDelta = prevVals.Radius > 0 ? pinchValue.Radius / prevVals.Radius : 1f;

					var args = new SKTransformDetectedEventArgs(
						pinchValue.Center,
						prevVals.Center,
						scaleDelta,
						rotationDelta);
					OnTransformDetected(args);

					previousValues = pinchValue;
				}
				break;
		}

		return true;
	}

	private bool OnTouchReleased(SKTouchEventArgs e)
	{
		var handled = false;

		var ticks = DateTime.Now.Ticks;
		var location = e.Location;

		if (!touches.TryGetValue(e.Id, out var releasedTouch))
			return false;

		touches.Remove(e.Id);

		var points = GetInContactTouchPoints();

		// No more fingers on the screen
		if (points.Length == 0)
		{
			// Check to see if it was a fling
			var velocity = flingTracker.CalculateVelocity(e.Id, ticks);
			if (Math.Abs(velocity.X * velocity.Y) > (FlingVelocityThreshold * FlingVelocityThreshold))
			{
				var args = new SKFlingDetectedEventArgs(velocity.X, velocity.Y);
				OnFlingDetected(args);
				handled = args.Handled;
			}

			// When tapping, the finger never goes to exactly the same location
			var isAround = SKPoint.Distance(releasedTouch.Location, initialTouch) < TouchSlopPixels;
			var touchDuration = ticks - releasedTouch.Tick;

			if (isAround && touchDuration < (e.DeviceType == SKTouchDeviceType.Mouse ? ShortClickTicks : LongTapTicks))
			{
				// Add a timer to detect the type of tap (single or multi)
				var tapLocation = location;
				var currentTapCount = tapCount;

				multiTapTimer = new Timer(
					_ => DispatchTapHandler(tapLocation, currentTapCount, ref handled),
					null,
					DelayTapMilliseconds,
					Timeout.Infinite);
			}
			else if (isAround && touchDuration >= LongTapTicks)
			{
				// If the finger was down for a long time, then it is a long tap
				if (!handled)
				{
					var args = new SKTapDetectedEventArgs(location);
					OnLongPressDetected(args);
					handled = args.Handled;
				}
			}
		}

		// Update the fling tracker
		flingTracker.RemoveId(e.Id);

		if (points.Length == 1)
		{
			// If there is still 1 finger on the screen, then try start a new gesture
			var args = new SKGestureEventArgs(points);
			OnGestureStarted(args);
			handled = args.Handled;

			// If no gesture was started, then we will handle it
			if (!handled)
			{
				touchMode = TouchMode.Single;
				previousValues.Center = points[0];
				handled = true;
			}
		}

		if (!handled)
		{
			// The gesture was not handled, so end it
			var args = new SKGestureEventArgs(points);
			OnGestureEnded(args);
			handled = args.Handled;

			if (points.Length == 0)
				touchMode = TouchMode.None;
		}

		return handled;
	}

	private void DispatchTapHandler(SKPoint location, int currentTapCount, ref bool handled)
	{
		// Dispatch to UI thread
		var currentDispatcher = dispatcher;
		if (currentDispatcher is null)
		{
			HandleTap(location, currentTapCount, ref handled);
			return;
		}

		var localHandled = handled;
		currentDispatcher.Dispatch(() =>
		{
			HandleTap(location, currentTapCount, ref localHandled);
		});
	}

	private void HandleTap(SKPoint location, int currentTapCount, ref bool handled)
	{
		if (!handled)
		{
			if (currentTapCount > 1)
			{
				var args = new SKTapDetectedEventArgs(location, currentTapCount);
				OnDoubleTapDetected(args);
				handled = args.Handled;
			}
			else
			{
				var args = new SKTapDetectedEventArgs(location);
				OnSingleTapDetected(args);
				handled = args.Handled;
			}
		}

		tapCount = 1;
		multiTapTimer?.Dispose();
		multiTapTimer = null;
	}

	private bool OnTouchCancelled(SKTouchEventArgs e)
	{
		touches.Remove(e.Id);
		flingTracker.RemoveId(e.Id);
		return false;
	}

	private SKPoint[] GetInContactTouchPoints() =>
		touches.Values
			.Where(t => t.InContact)
			.Select(t => t.Location)
			.ToArray();
}
