namespace SkiaSharp.Extended.UI.Controls;

/// <summary>
/// Event arguments for when a fling gesture is detected.
/// </summary>
public class SKFlingDetectedEventArgs : EventArgs
{
	/// <summary>
	/// Creates a new instance with the specified velocities.
	/// </summary>
	/// <param name="velocityX">The velocity in the X direction in pixels per second.</param>
	/// <param name="velocityY">The velocity in the Y direction in pixels per second.</param>
	public SKFlingDetectedEventArgs(float velocityX, float velocityY)
	{
		VelocityX = velocityX;
		VelocityY = velocityY;
	}

	/// <summary>
	/// Gets the velocity in the X direction in pixels per second.
	/// </summary>
	public float VelocityX { get; }

	/// <summary>
	/// Gets the velocity in the Y direction in pixels per second.
	/// </summary>
	public float VelocityY { get; }

	/// <summary>
	/// Gets or sets whether the event has been handled.
	/// </summary>
	public bool Handled { get; set; }
}
