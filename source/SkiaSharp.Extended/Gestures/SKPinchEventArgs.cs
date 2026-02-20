using System;

namespace SkiaSharp.Extended.Gestures;

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
