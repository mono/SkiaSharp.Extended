using System;

namespace SkiaSharp.Extended.Gestures;

/// <summary>
/// Base class for all gesture event arguments in the SkiaSharp gesture recognition system.
/// </summary>
/// <remarks>
/// <para>All specific gesture event argument types (such as <see cref="SKTapGestureEventArgs"/>,
/// <see cref="SKPanGestureEventArgs"/>, and <see cref="SKPinchGestureEventArgs"/>) derive from
/// this class. The <see cref="Handled"/> property allows event consumers to indicate that the
/// gesture has been processed, preventing further default handling by the
/// <see cref="SKGestureTracker"/>.</para>
/// <seealso cref="SKGestureDetector"/>
/// <seealso cref="SKGestureTracker"/>
/// </remarks>
public class SKGestureEventArgs : EventArgs
{
	/// <summary>
	/// Gets or sets a value indicating whether the event has been handled.
	/// </summary>
	/// <value>
	/// <see langword="true"/> if the event has been handled by a consumer and default processing
	/// should be skipped; otherwise, <see langword="false"/>. The default is <see langword="false"/>.
	/// </value>
	/// <remarks>
	/// Set this to <see langword="true"/> in an event handler to prevent the <see cref="SKGestureTracker"/>
	/// from applying its default transform behavior (for example, to implement custom drag handling
	/// instead of the built-in pan offset).
	/// </remarks>
	public bool Handled { get; set; }
}
