using System;

namespace SkiaSharp.Extended.Gestures;

/// <summary>
/// Provides data for drag gesture lifecycle events (<see cref="SKGestureTracker.DragStarted"/>,
/// <see cref="SKGestureTracker.DragUpdated"/>, and <see cref="SKGestureTracker.DragEnded"/>).
/// </summary>
/// <remarks>
/// <para>Drag events provide a higher-level lifecycle built on top of the underlying pan gesture.
/// The lifecycle is:</para>
/// <list type="number">
/// <item><description><see cref="SKGestureTracker.DragStarted"/>: Fired once when the first pan movement
/// occurs. <see cref="StartLocation"/> and <see cref="CurrentLocation"/> define the initial positions.</description></item>
/// <item><description><see cref="SKGestureTracker.DragUpdated"/>: Fired continuously as the touch moves.
/// <see cref="Delta"/> contains the incremental displacement from the previous position.</description></item>
/// <item><description><see cref="SKGestureTracker.DragEnded"/>: Fired once when all touches are released.
/// <see cref="Delta"/> is <see cref="SKPoint.Empty"/>.</description></item>
/// </list>
/// <para>Set <see cref="Handled"/> to <see langword="true"/> during
/// <see cref="SKGestureTracker.DragStarted"/> or <see cref="SKGestureTracker.DragUpdated"/>
/// to prevent the tracker from applying its default pan offset behavior (for example, when
/// implementing custom object dragging).</para>
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
	/// <param name="startLocation">The location where the drag began, in view coordinates.</param>
	/// <param name="currentLocation">The current touch location, in view coordinates.</param>
	/// <param name="delta">The displacement from the previous touch position to <paramref name="currentLocation"/>.</param>
	public SKDragGestureEventArgs(SKPoint startLocation, SKPoint currentLocation, SKPoint delta)
	{
		StartLocation = startLocation;
		CurrentLocation = currentLocation;
		Delta = delta;
	}

	/// <summary>
	/// Gets or sets a value indicating whether the event has been handled.
	/// </summary>
	/// <value>
	/// <see langword="true"/> if the event has been handled by a consumer and default processing
	/// should be skipped; otherwise, <see langword="false"/>. The default is <see langword="false"/>.
	/// </value>
	/// <remarks>
	/// Set this to <see langword="true"/> during <see cref="SKGestureTracker.DragStarted"/> or
	/// <see cref="SKGestureTracker.DragUpdated"/> to prevent the <see cref="SKGestureTracker"/>
	/// from updating <see cref="SKGestureTracker.Offset"/> for this drag operation.
	/// </remarks>
	public bool Handled { get; set; }

	/// <summary>
	/// Gets the location where the drag began, in view coordinates.
	/// </summary>
	/// <value>An <see cref="SKPoint"/> representing the initial touch position when the drag started.</value>
	public SKPoint StartLocation { get; }

	/// <summary>
	/// Gets the current touch location in view coordinates.
	/// </summary>
	/// <value>An <see cref="SKPoint"/> representing the current position of the touch.</value>
	public SKPoint CurrentLocation { get; }

	/// <summary>
	/// Gets the displacement from the previous touch position to the current position.
	/// </summary>
	/// <value>
	/// An <see cref="SKPoint"/> where <c>X</c> and <c>Y</c> represent the incremental change in pixels.
	/// This is <see cref="SKPoint.Empty"/> for <see cref="SKGestureTracker.DragEnded"/> events.
	/// </value>
	public SKPoint Delta { get; }

}
