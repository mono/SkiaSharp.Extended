using System;

namespace SkiaSharp.Extended.Gestures;

/// <summary>
/// Provides data for tap gesture events, including single and multi-tap interactions.
/// </summary>
/// <remarks>
/// <para>This event argument is used by both <see cref="SKGestureDetector.TapDetected"/> and
/// <see cref="SKGestureDetector.DoubleTapDetected"/> events. For single taps, <see cref="TapCount"/>
/// is <c>1</c>. For double taps, it is <c>2</c> or greater.</para>
/// <example>
/// <para>The following example shows how to handle tap events:</para>
/// <code>
/// var tracker = new SKGestureTracker();
/// tracker.TapDetected += (sender, e) =>
/// {
///     Console.WriteLine($"Tapped at ({e.Location.X}, {e.Location.Y}), count: {e.TapCount}");
/// };
/// tracker.DoubleTapDetected += (sender, e) =>
/// {
///     // Set Handled to true to prevent the default double-tap zoom behavior.
///     e.Handled = true;
///     Console.WriteLine($"Double-tapped at ({e.Location.X}, {e.Location.Y})");
/// };
/// </code>
/// </example>
/// <seealso cref="SKGestureDetector.TapDetected"/>
/// <seealso cref="SKGestureDetector.DoubleTapDetected"/>
/// </remarks>
public class SKTapGestureEventArgs : SKGestureEventArgs
{
	/// <summary>
	/// Initializes a new instance of the <see cref="SKTapGestureEventArgs"/> class.
	/// </summary>
	/// <param name="location">The location of the tap in view coordinates.</param>
	/// <param name="tapCount">The number of consecutive taps detected.</param>
	public SKTapGestureEventArgs(SKPoint location, int tapCount)
	{
		Location = location;
		TapCount = tapCount;
	}

	/// <summary>
	/// Gets the location of the tap in view coordinates.
	/// </summary>
	/// <value>An <see cref="SKPoint"/> representing the position where the tap occurred.</value>
	public SKPoint Location { get; }

	/// <summary>
	/// Gets the number of consecutive taps detected.
	/// </summary>
	/// <value>
	/// <c>1</c> for a single tap, <c>2</c> or greater for multi-tap gestures.
	/// </value>
	public int TapCount { get; }

}
