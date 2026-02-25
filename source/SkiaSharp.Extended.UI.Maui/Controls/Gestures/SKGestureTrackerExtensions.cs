using SkiaSharp.Extended.Gestures;
using SkiaSharp.Views.Maui;

namespace SkiaSharp.Extended.UI.Controls;

/// <summary>
/// Extension methods for <see cref="SKGestureTracker"/> to work with MAUI touch events.
/// </summary>
public static class SKGestureTrackerExtensions
{
	/// <summary>
	/// Processes a MAUI <see cref="SKTouchEventArgs"/> through the gesture tracker.
	/// Converts pixel coordinates to point coordinates using the tracker's <see cref="SKGestureTracker.DisplayScale"/>.
	/// </summary>
	/// <param name="tracker">The gesture tracker.</param>
	/// <param name="e">The touch event args from MAUI.</param>
	/// <returns><c>true</c> if the event was handled.</returns>
	public static bool ProcessTouch(this SKGestureTracker tracker, SKTouchEventArgs e)
	{
		var isMouse = e.DeviceType == SKTouchDeviceType.Mouse;
		var scale = tracker.DisplayScale;
		var location = scale > 0
			? new SKPoint(e.Location.X / scale, e.Location.Y / scale)
			: e.Location;

		return e.ActionType switch
		{
			SKTouchAction.Pressed => tracker.ProcessTouchDown(e.Id, location, isMouse),
			SKTouchAction.Moved => tracker.ProcessTouchMove(e.Id, location, e.InContact),
			SKTouchAction.Released => tracker.ProcessTouchUp(e.Id, location, isMouse),
			SKTouchAction.Cancelled => tracker.ProcessTouchCancel(e.Id),
			SKTouchAction.WheelChanged => tracker.ProcessMouseWheel(location, 0, e.WheelDelta),
			_ => true, // Entered/Exited — accept to keep receiving events
		};
	}
}
