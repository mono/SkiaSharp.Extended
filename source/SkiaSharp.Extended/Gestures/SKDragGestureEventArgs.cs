using System;

namespace SkiaSharp.Extended;

/// <summary>
/// Provides data for drag gesture lifecycle events (<see cref="SKGestureTracker.DragStarted"/>,
/// <see cref="SKGestureTracker.DragUpdated"/>, and <see cref="SKGestureTracker.DragEnded"/>).
/// </summary>
/// <remarks>
/// <para>Drag events provide a higher-level lifecycle built on top of the underlying pan gesture.
/// The lifecycle is:</para>
/// <list type="number">
/// <item><description><see cref="SKGestureTracker.DragStarted"/>: Fired once when the first pan movement
/// occurs.</description></item>
/// <item><description><see cref="SKGestureTracker.DragUpdated"/>: Fired continuously as the touch moves.
/// <see cref="Delta"/> contains the incremental displacement from the previous position.</description></item>
/// <item><description><see cref="SKGestureTracker.DragEnded"/>: Fired once when all touches are released.</description></item>
/// </list>
/// <para>Set <see cref="Handled"/> to <see langword="true"/> during
/// <see cref="SKGestureTracker.DragStarted"/> or <see cref="SKGestureTracker.DragUpdated"/>
/// to prevent the tracker from applying its default pan offset behavior.</para>
/// <seealso cref="SKGestureTracker.DragStarted"/>
/// <seealso cref="SKGestureTracker.DragUpdated"/>
/// <seealso cref="SKGestureTracker.DragEnded"/>
/// <seealso cref="SKPanGestureEventArgs"/>
/// </remarks>
public class SKDragGestureEventArgs : EventArgs
{
	/// <summary>
	/// Initializes a new instance of the <see cref="SKDragGestureEventArgs"/> class.
	/// </summary>
	/// <param name="location">The current touch location, in view coordinates.</param>
	/// <param name="prevLocation">The previous touch location, in view coordinates.</param>
	public SKDragGestureEventArgs(SKPoint location, SKPoint prevLocation)
	{
		Location = location;
		PrevLocation = prevLocation;
	}

	/// <summary>
	/// Gets or sets a value indicating whether the event has been handled.
	/// </summary>
	/// <value>
	/// <see langword="true"/> if the event has been handled by a consumer and default processing
	/// should be skipped; otherwise, <see langword="false"/>. The default is <see langword="false"/>.
	/// </value>
	public bool Handled { get; set; }

	/// <summary>
	/// Gets the current touch location in view coordinates.
	/// </summary>
	/// <value>An <see cref="SKPoint"/> representing the current position of the touch.</value>
	public SKPoint Location { get; }

	/// <summary>
	/// Gets the previous touch location in view coordinates.
	/// </summary>
	/// <value>An <see cref="SKPoint"/> representing the previous position of the touch.</value>
	public SKPoint PrevLocation { get; }

	/// <summary>
	/// Gets the displacement from <see cref="PrevLocation"/> to <see cref="Location"/>.
	/// </summary>
	/// <value>
	/// An <see cref="SKPoint"/> where <c>X</c> and <c>Y</c> represent the incremental change in pixels.
	/// </value>
	/// <remarks>Calculated as <c>Location - PrevLocation</c>.</remarks>
	public SKPoint Delta => new SKPoint(Location.X - PrevLocation.X, Location.Y - PrevLocation.Y);
}
