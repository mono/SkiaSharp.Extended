using System;

namespace SkiaSharp.Extended.Gestures;

/// <summary>
/// Event arguments for a tap gesture.
/// </summary>
public class SKTapEventArgs : EventArgs
{
	/// <summary>
	/// Creates a new instance.
	/// </summary>
	public SKTapEventArgs(SKPoint location, int tapCount)
	{
		Location = location;
		TapCount = tapCount;
	}

	/// <summary>
	/// Gets the location of the tap.
	/// </summary>
	public SKPoint Location { get; }

	/// <summary>
	/// Gets the number of taps (1 for single, 2+ for multi-tap).
	/// </summary>
	public int TapCount { get; }

	/// <summary>
	/// Gets or sets whether the event was handled.
	/// </summary>
	public bool Handled { get; set; }
}

/// <summary>
/// Event arguments for a pan gesture.
/// </summary>
public class SKPanEventArgs : EventArgs
{
	/// <summary>
	/// Creates a new instance.
	/// </summary>
	public SKPanEventArgs(SKPoint location, SKPoint previousLocation, SKPoint delta)
	{
		Location = location;
		PreviousLocation = previousLocation;
		Delta = delta;
	}

	/// <summary>
	/// Gets the current location.
	/// </summary>
	public SKPoint Location { get; }

	/// <summary>
	/// Gets the previous location.
	/// </summary>
	public SKPoint PreviousLocation { get; }

	/// <summary>
	/// Gets the delta movement.
	/// </summary>
	public SKPoint Delta { get; }

	/// <summary>
	/// Gets or sets whether the event was handled.
	/// </summary>
	public bool Handled { get; set; }
}

/// <summary>
/// Event arguments for a pinch (scale) gesture.
/// </summary>
public class SKPinchEventArgs : EventArgs
{
	/// <summary>
	/// Creates a new instance.
	/// </summary>
	public SKPinchEventArgs(SKPoint center, SKPoint previousCenter, float scale)
	{
		Center = center;
		PreviousCenter = previousCenter;
		Scale = scale;
	}

	/// <summary>
	/// Gets the center point of the pinch.
	/// </summary>
	public SKPoint Center { get; }

	/// <summary>
	/// Gets the previous center point.
	/// </summary>
	public SKPoint PreviousCenter { get; }

	/// <summary>
	/// Gets the scale factor (1.0 = no change).
	/// </summary>
	public float Scale { get; }

	/// <summary>
	/// Gets or sets whether the event was handled.
	/// </summary>
	public bool Handled { get; set; }
}

/// <summary>
/// Event arguments for a rotation gesture.
/// </summary>
public class SKRotateEventArgs : EventArgs
{
	/// <summary>
	/// Creates a new instance.
	/// </summary>
	public SKRotateEventArgs(SKPoint center, float rotationDelta)
	{
		Center = center;
		RotationDelta = rotationDelta;
	}

	/// <summary>
	/// Gets the center point of rotation.
	/// </summary>
	public SKPoint Center { get; }

	/// <summary>
	/// Gets the rotation delta in degrees.
	/// </summary>
	public float RotationDelta { get; }

	/// <summary>
	/// Gets or sets whether the event was handled.
	/// </summary>
	public bool Handled { get; set; }
}

/// <summary>
/// Event arguments for a fling gesture.
/// </summary>
public class SKFlingEventArgs : EventArgs
{
	/// <summary>
	/// Creates a new instance.
	/// </summary>
	public SKFlingEventArgs(float velocityX, float velocityY)
	{
		VelocityX = velocityX;
		VelocityY = velocityY;
	}

	/// <summary>
	/// Gets the X velocity in pixels per second.
	/// </summary>
	public float VelocityX { get; }

	/// <summary>
	/// Gets the Y velocity in pixels per second.
	/// </summary>
	public float VelocityY { get; }

	/// <summary>
	/// Gets or sets whether the event was handled.
	/// </summary>
	public bool Handled { get; set; }
}

/// <summary>
/// Event arguments for a hover event.
/// </summary>
public class SKHoverEventArgs : EventArgs
{
	/// <summary>
	/// Creates a new instance.
	/// </summary>
	public SKHoverEventArgs(SKPoint location)
	{
		Location = location;
	}

	/// <summary>
	/// Gets the hover location.
	/// </summary>
	public SKPoint Location { get; }

	/// <summary>
	/// Gets or sets whether the event was handled.
	/// </summary>
	public bool Handled { get; set; }
}

/// <summary>
/// Event arguments for gesture state changes.
/// </summary>
public class SKGestureStateEventArgs : EventArgs
{
	/// <summary>
	/// Creates a new instance.
	/// </summary>
	public SKGestureStateEventArgs(SKPoint[] touchPoints, GestureState state)
	{
		TouchPoints = touchPoints;
		State = state;
	}

	/// <summary>
	/// Gets the current touch points.
	/// </summary>
	public SKPoint[] TouchPoints { get; }

	/// <summary>
	/// Gets the gesture state.
	/// </summary>
	public GestureState State { get; }

	/// <summary>
	/// Gets or sets whether the event was handled.
	/// </summary>
	public bool Handled { get; set; }
}

/// <summary>
/// Event arguments for selection changes.
/// </summary>
public class SKSelectionChangedEventArgs : EventArgs
{
	/// <summary>
	/// Creates a new instance.
	/// </summary>
	public SKSelectionChangedEventArgs(long? selectedItemId)
	{
		SelectedItemId = selectedItemId;
	}

	/// <summary>
	/// Gets the selected item ID, or null if nothing is selected.
	/// </summary>
	public long? SelectedItemId { get; }
}

/// <summary>
/// Event arguments for a drag operation.
/// </summary>
public class SKDragEventArgs : EventArgs
{
	/// <summary>
	/// Creates a new instance.
	/// </summary>
	public SKDragEventArgs(SKPoint startLocation, SKPoint currentLocation, SKPoint delta)
	{
		StartLocation = startLocation;
		CurrentLocation = currentLocation;
		Delta = delta;
	}

	/// <summary>
	/// Gets the starting location of the drag.
	/// </summary>
	public SKPoint StartLocation { get; }

	/// <summary>
	/// Gets the current location.
	/// </summary>
	public SKPoint CurrentLocation { get; }

	/// <summary>
	/// Gets the delta from the previous position.
	/// </summary>
	public SKPoint Delta { get; }

	/// <summary>
	/// Gets or sets whether the event was handled.
	/// </summary>
	public bool Handled { get; set; }
}
