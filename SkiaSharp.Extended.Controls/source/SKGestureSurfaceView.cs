using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace SkiaSharp.Extended.Controls
{
	public partial class SKGestureSurfaceView : SKDynamicSurfaceView
	{

		public static readonly BindableProperty EnableTouchEventsProperty = BindableProperty.Create(
			nameof(EnableTouchEvents),
			typeof(bool),
			typeof(SKDynamicSurfaceView),
			false,
			propertyChanged: OnEnableTouchEventsChanged);

		public bool EnableTouchEvents
		{
			get => (bool)GetValue(EnableTouchEventsProperty);
			set => SetValue(EnableTouchEventsProperty, value);
		}




		private readonly Dictionary<long, TouchEvent> touches = new Dictionary<long, TouchEvent>();
		private readonly FlingTracker flingTracker = new FlingTracker();
		private SKPoint initialTouch = SKPoint.Empty;
		private System.Threading.Timer doubleTapTimer;
		private int tapCount = 0;
		private TouchMode touchMode = TouchMode.None;
		private PinchValue previousValues;

		private const int shortTap = 125;
		private const int shortClick = 250;
		private const int delayTap = 200;
		private const int longTap = 500;
		private const int touchSlop = 8;
		private const int flingVelocity = 200;

		public SKGestureSurfaceView()
		{
			EnableTouchEvents = true;
			Touch += OnTouch;
		}

		public event EventHandler<SKGestureEventArgs> GestureStarted;

		public event EventHandler<SKGestureEventArgs> GestureEnded;

		public event EventHandler<SKTapDetectedEventArgs> LongPressDetected;

		public event EventHandler<SKTapDetectedEventArgs> SingleTapDetected;

		public event EventHandler<SKTapDetectedEventArgs> DoubleTapDetected;

		public event EventHandler<SKHoverDetectedEventArgs> HoverDetected;

		public event EventHandler<SKFlingDetectedEventArgs> FlingDetected;

		public event EventHandler<SKTransformDetectedEventArgs> TransformDetected;

		protected virtual void OnSingleTapDetected(SKTapDetectedEventArgs e) =>
			SingleTapDetected?.Invoke(this, e);

		protected virtual void OnDoubleTapDetected(SKTapDetectedEventArgs e) =>
			DoubleTapDetected?.Invoke(this, e);

		protected virtual void OnLongPressDetected(SKTapDetectedEventArgs e) =>
			LongPressDetected?.Invoke(this, e);

		protected virtual void OnHoverDetected(SKHoverDetectedEventArgs e) =>
			HoverDetected?.Invoke(this, e);

		protected virtual void OnFlingDetected(SKFlingDetectedEventArgs e) =>
			FlingDetected?.Invoke(this, e);

		private void OnTransformDetected(SKTransformDetectedEventArgs e) =>
			TransformDetected?.Invoke(this, e);

		protected virtual void OnGestureStarted(SKGestureEventArgs e) =>
			GestureStarted?.Invoke(this, e);

		protected virtual void OnGestureEnded(SKGestureEventArgs e) =>
			GestureEnded?.Invoke(this, e);

		private void OnTouch(object sender, SKTouchEventArgs e)
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
			flingTracker.Clear();

			// Do we have a doubleTapTestTimer running?
			// If yes, stop it and increment _numOfTaps
			if (doubleTapTimer != null)
			{
				doubleTapTimer.Dispose();
				doubleTapTimer = null;
				tapCount++;
			}
			else
			{
				tapCount = 1;
			}

			var handled = false;

			var touchPoints = GetTouchPoints();
			if (touchPoints.Length > 0)
			{
				var args = new SKGestureEventArgs(touchPoints);
				OnGestureStarted(args);
				handled = args.Handled;

				if (!handled)
				{
					if (touchPoints.Length == 2)
					{
						previousValues = PinchValue.FromLocations(touchPoints);
						touchMode = TouchMode.Multiple;
					}
					else
					{
						previousValues.Center = touchPoints.First();
						touchMode = TouchMode.Single;
					}
					handled = true;
				}
			}

			return handled;
		}

		private SKPoint[] GetTouchPoints() =>
			touches.Values.Where(t => t.InContact).Select(t => t.Location).ToArray();

		private bool OnTouchMoved(SKTouchEventArgs e)
		{
			var ticks = DateTime.Now.Ticks;
			var location = e.Location;

			touches[e.Id] = new TouchEvent(e.Id, location, ticks, e.InContact);

			if (e.InContact)
				flingTracker.AddEvent(e.Id, location, ticks);

			if (!e.InContact)
			{
				var args = new SKHoverDetectedEventArgs(e.Location);
				OnHoverDetected(args);
				return args.Handled;
			}

			var touchPoints = GetTouchPoints();

			// TODO: potentially handle move events before gestures

			switch (touchMode)
			{
				case TouchMode.Single:
					{
						if (touchPoints.Length != 1)
							return false;

						var touchPosition = touchPoints.First();

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

						var args = new SKTransformDetectedEventArgs(pinchValue.Center, prevVals.Center, pinchValue.Radius / prevVals.Radius, rotationDelta);
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
			var releasedTouch = touches[e.Id];

			touches.Remove(e.Id);

			// Is this a fling or swipe?
			if (touches.Count == 0)
			{
				var velocity = flingTracker.CalculateVelocity(e.Id, ticks);

				// This was the last finger on screen, so this is a fling
				if (Math.Abs(velocity.X) > flingVelocity || Math.Abs(velocity.Y) > flingVelocity)
				{
					var args = new SKFlingDetectedEventArgs(velocity.X, velocity.Y);
					OnFlingDetected(args);
					handled = args.Handled;
				}

				// While tapping on screen, there could be a small movement of the finger
				// (especially on Samsung). So check, if touch start location isn't more 
				// than a number of pixels away from touch end location.
				var isAround = SKPoint.Distance(releasedTouch.Location, initialTouch) < touchSlop;

				// If touch start and end is in the same area and the touch time is shorter
				// than longTap, than we have a tap.
				if (isAround && (ticks - releasedTouch.Tick) < (e.DeviceType == SKTouchDeviceType.Mouse ? shortClick : longTap) * 10000)
				{
					// Start a timer with timeout delayTap ms. If than isn't arrived another tap, than it is a single
					void TimerHandler(object state)
					{
						var l = (SKPoint)state;
						if (!handled)
						{
							if (tapCount > 1)
							{
								var args = new SKTapDetectedEventArgs(location, tapCount);
								OnDoubleTapDetected(args);
								handled = args.Handled;
							}
							else
							{
								var args = new SKTapDetectedEventArgs(l);
								OnSingleTapDetected(args);
								handled = args.Handled;
							}
						}
						tapCount = 1;
						doubleTapTimer?.Dispose();
						doubleTapTimer = null;
					};
					doubleTapTimer = new System.Threading.Timer(TimerHandler, location, delayTap, -1);
				}
				else if (isAround && (ticks - releasedTouch.Tick) >= longTap * 10000)
				{
					if (!handled)
					{
						var args = new SKTapDetectedEventArgs(location);
						OnLongPressDetected(args);
						handled = args.Handled;
					}
				}
			}

			flingTracker.RemoveId(e.Id);

			var points = GetTouchPoints();

			if (points.Length == 1)
			{
				var args = new SKGestureEventArgs(points);
				OnGestureStarted(args);
				handled = args.Handled;

				if (!handled)
				{
					touchMode = TouchMode.Single;
					previousValues.Center = points[0];
					handled = true;
				}
			}

			if (!handled)
			{
				var args = new SKGestureEventArgs(points);
				OnGestureEnded(args);
				handled = args.Handled;

				if (points.Length == 0)
					touchMode = TouchMode.None;
			}

			return handled;
		}

		private bool OnTouchCancelled(SKTouchEventArgs e)
		{
			touches.Remove(e.Id);
			return false;
		}
	}
}
