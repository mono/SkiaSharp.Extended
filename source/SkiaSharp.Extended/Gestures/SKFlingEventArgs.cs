using System;

namespace SkiaSharp.Extended.Gestures;

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
