using System;

namespace SkiaSharp.Extended.Gestures;

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
