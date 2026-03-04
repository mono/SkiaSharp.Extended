namespace SkiaSharp.Extended.UI.Controls;

/// <summary>
/// Specifies the side of the view from which confetti particles are emitted.
/// </summary>
public enum SKConfettiEmitterSide
{
	/// <summary>
	/// Emit particles from the top edge.
	/// </summary>
	Top,
	/// <summary>
	/// Emit particles from the left edge.
	/// </summary>
	Left,
	/// <summary>
	/// Emit particles from the right edge.
	/// </summary>
	Right,
	/// <summary>
	/// Emit particles from the bottom edge.
	/// </summary>
	Bottom,

	/// <summary>
	/// Emit particles from the center of the view.
	/// </summary>
	Center,

	/// <summary>
	/// Emit particles from a custom rectangular bounds.
	/// </summary>
	Bounds,
}
