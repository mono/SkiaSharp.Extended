#if MACCATALYST
using CoreGraphics;
using SkiaSharp;
using UIKit;

namespace SkiaSharpDemo.Demos;

public partial class GesturePage
{
	private const float ScrollPointsPerNotch = 40f;

	private PointerGestureRecognizer? _pointerGestureRecognizer;
	private UIPinchGestureRecognizer? _pinchGestureRecognizer;
	private UIPanGestureRecognizer? _scrollGestureRecognizer;
	private SimultaneousGestureDelegate? _gestureDelegate;
	private UIView? _platformCanvasView;
	private SKPoint? _lastPointerLocation;
	private float _lastPinchScale = 1f;

	partial void ConnectPlatformGestures()
	{
		if (_pointerGestureRecognizer != null || canvasView.Handler?.PlatformView is not UIView platformView)
			return;

		_platformCanvasView = platformView;

		_pointerGestureRecognizer = new PointerGestureRecognizer();
		_pointerGestureRecognizer.PointerMoved += OnPlatformPointerMoved;
		canvasView.GestureRecognizers.Add(_pointerGestureRecognizer);

		_gestureDelegate = new SimultaneousGestureDelegate();

		_pinchGestureRecognizer = new UIPinchGestureRecognizer(OnPlatformPinch)
		{
			CancelsTouchesInView = false,
			Delegate = _gestureDelegate
		};
		platformView.AddGestureRecognizer(_pinchGestureRecognizer);

		_scrollGestureRecognizer = new UIPanGestureRecognizer(OnPlatformScroll)
		{
			AllowedScrollTypesMask = UIScrollTypeMask.All,
			MaximumNumberOfTouches = 0,
			CancelsTouchesInView = false,
			Delegate = _gestureDelegate
		};
		platformView.AddGestureRecognizer(_scrollGestureRecognizer);
	}

	partial void DisconnectPlatformGestures()
	{
		if (_pointerGestureRecognizer != null)
		{
			_pointerGestureRecognizer.PointerMoved -= OnPlatformPointerMoved;
			canvasView.GestureRecognizers.Remove(_pointerGestureRecognizer);
			_pointerGestureRecognizer = null;
		}

		if (_pinchGestureRecognizer != null)
		{
			_platformCanvasView?.RemoveGestureRecognizer(_pinchGestureRecognizer);
			_pinchGestureRecognizer.Dispose();
			_pinchGestureRecognizer = null;
		}

		if (_scrollGestureRecognizer != null)
		{
			_platformCanvasView?.RemoveGestureRecognizer(_scrollGestureRecognizer);
			_scrollGestureRecognizer.Dispose();
			_scrollGestureRecognizer = null;
		}

		_gestureDelegate?.Dispose();
		_gestureDelegate = null;
		_platformCanvasView = null;
		_lastPointerLocation = null;
		_lastPinchScale = 1f;
	}

	private void OnPlatformPointerMoved(object? sender, PointerEventArgs e)
	{
		var location = e.GetPosition(canvasView);
		if (location == null)
			return;

		_lastPointerLocation = ToCanvasPoint(location.Value);
		_tracker.ProcessTouchMove(0, _lastPointerLocation.Value, inContact: false);
	}

	private void OnPlatformPinch(UIPinchGestureRecognizer recognizer)
	{
		if (recognizer.State == UIGestureRecognizerState.Began)
		{
			_lastPinchScale = 1f;
			return;
		}

		if (recognizer.State != UIGestureRecognizerState.Changed)
		{
			if (recognizer.State == UIGestureRecognizerState.Ended ||
				recognizer.State == UIGestureRecognizerState.Cancelled)
				_lastPinchScale = 1f;
			return;
		}

		var currentScale = (float)recognizer.Scale;
		var scale = currentScale / _lastPinchScale;
		_lastPinchScale = currentScale;

		_tracker.SetScale(_tracker.Scale * scale, GetPointerFocalPoint());
		LogEvent($"Trackpad pinch: {scale:F2}x");
		statusLabel.Text = $"Scale: {_tracker.Scale:F2}";
	}

	private void OnPlatformScroll(UIPanGestureRecognizer recognizer)
	{
		if (recognizer.NumberOfTouches != 0 ||
			(recognizer.State != UIGestureRecognizerState.Began &&
			 recognizer.State != UIGestureRecognizerState.Changed))
			return;

		var view = _platformCanvasView;
		if (view == null)
			return;

		var translation = recognizer.TranslationInView(view);
		recognizer.SetTranslation(CGPoint.Empty, view);

		_tracker.ProcessMouseWheel(
			GetPointerFocalPoint(),
			(float)(translation.X * 120.0 / ScrollPointsPerNotch),
			(float)(-translation.Y * 120.0 / ScrollPointsPerNotch));
	}

	private SKPoint GetPointerFocalPoint()
	{
		return _lastPointerLocation ?? new SKPoint(
			canvasView.CanvasSize.Width / 2f,
			canvasView.CanvasSize.Height / 2f);
	}

	private SKPoint ToCanvasPoint(Point point)
	{
		var scaleX = canvasView.Width > 0 ? canvasView.CanvasSize.Width / (float)canvasView.Width : 1f;
		var scaleY = canvasView.Height > 0 ? canvasView.CanvasSize.Height / (float)canvasView.Height : 1f;
		return new SKPoint((float)point.X * scaleX, (float)point.Y * scaleY);
	}

	private sealed class SimultaneousGestureDelegate : UIGestureRecognizerDelegate
	{
		public override bool ShouldRecognizeSimultaneously(
			UIGestureRecognizer gestureRecognizer,
			UIGestureRecognizer otherGestureRecognizer)
		{
			return
				(gestureRecognizer is UIPinchGestureRecognizer &&
				 otherGestureRecognizer is UIPanGestureRecognizer) ||
				(gestureRecognizer is UIPanGestureRecognizer &&
				 otherGestureRecognizer is UIPinchGestureRecognizer);
		}
	}
}
#endif
