using System;

namespace SkiaSharp.Extended.Gestures;

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
