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
