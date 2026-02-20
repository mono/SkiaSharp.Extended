using System;

namespace SkiaSharp.Extended.Gestures;

/// <summary>
/// Event arguments for gesture state changes.
/// </summary>
public class SKGestureStateEventArgs : EventArgs
{
/// <summary>
/// Creates a new instance.
/// </summary>
public SKGestureStateEventArgs(SKPoint[] touchPoints, SKGestureState state)
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
public SKGestureState State { get; }

/// <summary>
/// Gets or sets whether the event was handled.
/// </summary>
public bool Handled { get; set; }
}
