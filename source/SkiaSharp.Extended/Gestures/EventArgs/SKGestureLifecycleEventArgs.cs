using System;

namespace SkiaSharp.Extended;

/// <summary>
/// Provides data for gesture lifecycle events that indicate when a gesture interaction begins or ends.
/// </summary>
/// <remarks>
/// <para>This class is used with the <see cref="SKGestureTracker.GestureStarted"/> and
/// <see cref="SKGestureTracker.GestureEnded"/> events, as well as the corresponding events on
/// <see cref="SKGestureTracker"/>.</para>
/// <para>A gesture starts when the first touch contact occurs and ends when all touches are released.
/// These events are useful for managing UI state such as cancelling inertia animations when a new
/// gesture begins, or triggering a redraw when a gesture ends.</para>
/// <seealso cref="SKGestureTracker.GestureStarted"/>
/// <seealso cref="SKGestureTracker.GestureEnded"/>
/// <seealso cref="SKGestureTracker.GestureStarted"/>
/// <seealso cref="SKGestureTracker.GestureEnded"/>
/// </remarks>
public class SKGestureLifecycleEventArgs : EventArgs
{
}
