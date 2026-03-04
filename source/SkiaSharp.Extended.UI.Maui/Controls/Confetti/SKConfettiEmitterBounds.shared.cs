namespace SkiaSharp.Extended.UI.Controls;

/// <summary>
/// Defines the bounds from which a confetti emitter spawns particles.
/// </summary>
[TypeConverter(typeof(Converters.SKConfettiEmitterBoundsTypeConverter))]
public readonly struct SKConfettiEmitterBounds
{
	/// <summary>
	/// Initializes a new instance of the <see cref="SKConfettiEmitterBounds"/> struct for the specified side.
	/// </summary>
	/// <param name="side">The side of the view to emit from.</param>
	public SKConfettiEmitterBounds(SKConfettiEmitterSide side)
		: this(Rect.Zero, side)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="SKConfettiEmitterBounds"/> struct for a specific point.
	/// </summary>
	/// <param name="x">The x-coordinate of the emission point.</param>
	/// <param name="y">The y-coordinate of the emission point.</param>
	public SKConfettiEmitterBounds(double x, double y)
		: this(new Rect(x, y, 0, 0), SKConfettiEmitterSide.Bounds)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="SKConfettiEmitterBounds"/> struct for a specific point.
	/// </summary>
	/// <param name="point">The emission point.</param>
	public SKConfettiEmitterBounds(Point point)
		: this(new Rect(point, Size.Zero), SKConfettiEmitterSide.Bounds)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="SKConfettiEmitterBounds"/> struct for a specific rectangular area.
	/// </summary>
	/// <param name="x">The x-coordinate of the bounds.</param>
	/// <param name="y">The y-coordinate of the bounds.</param>
	/// <param name="width">The width of the bounds.</param>
	/// <param name="height">The height of the bounds.</param>
	public SKConfettiEmitterBounds(double x, double y, double width, double height)
		: this(new Rect(x, y, width, height), SKConfettiEmitterSide.Bounds)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="SKConfettiEmitterBounds"/> struct for a specific rectangle.
	/// </summary>
	/// <param name="rect">The emission bounds rectangle.</param>
	public SKConfettiEmitterBounds(Rect rect)
		: this(rect, SKConfettiEmitterSide.Bounds)
	{
	}

	private SKConfettiEmitterBounds(Rect rect, SKConfettiEmitterSide side)
	{
		Rect = rect;
		Side = side;
	}

	//

	/// <summary>
	/// Gets the rectangle defining the emission area.
	/// </summary>
	public Rect Rect { get; }

	/// <summary>
	/// Gets the side from which particles are emitted.
	/// </summary>
	public SKConfettiEmitterSide Side { get; }

	//

	/// <summary>
	/// Converts an <see cref="SKConfettiEmitterSide"/> to an <see cref="SKConfettiEmitterBounds"/>.
	/// </summary>
	/// <param name="side">The emitter side.</param>
	public static implicit operator SKConfettiEmitterBounds(SKConfettiEmitterSide side) =>
		new SKConfettiEmitterBounds(side);

	/// <summary>
	/// Converts a <see cref="Microsoft.Maui.Graphics.Point"/> to an <see cref="SKConfettiEmitterBounds"/>.
	/// </summary>
	/// <param name="point">The point.</param>
	public static implicit operator SKConfettiEmitterBounds(Point point) =>
		new SKConfettiEmitterBounds(point);

	/// <summary>
	/// Converts a <see cref="Microsoft.Maui.Graphics.Rect"/> to an <see cref="SKConfettiEmitterBounds"/>.
	/// </summary>
	/// <param name="rect">The rectangle.</param>
	public static implicit operator SKConfettiEmitterBounds(Rect rect) =>
		new SKConfettiEmitterBounds(rect);

	//

	/// <summary>
	/// Gets emitter bounds that emit from the top side of the view.
	/// </summary>
	public static SKConfettiEmitterBounds Top =>
		new SKConfettiEmitterBounds(SKConfettiEmitterSide.Top);

	/// <summary>
	/// Gets emitter bounds that emit from the left side of the view.
	/// </summary>
	public static SKConfettiEmitterBounds Left =>
		new SKConfettiEmitterBounds(SKConfettiEmitterSide.Left);

	/// <summary>
	/// Gets emitter bounds that emit from the right side of the view.
	/// </summary>
	public static SKConfettiEmitterBounds Right =>
		new SKConfettiEmitterBounds(SKConfettiEmitterSide.Right);

	/// <summary>
	/// Gets emitter bounds that emit from the bottom side of the view.
	/// </summary>
	public static SKConfettiEmitterBounds Bottom =>
		new SKConfettiEmitterBounds(SKConfettiEmitterSide.Bottom);

	/// <summary>
	/// Gets emitter bounds that emit from the center of the view.
	/// </summary>
	public static SKConfettiEmitterBounds Center =>
		new SKConfettiEmitterBounds(SKConfettiEmitterSide.Center);

	/// <summary>
	/// Creates emitter bounds for a specific rectangular area.
	/// </summary>
	/// <param name="x">The x-coordinate of the bounds.</param>
	/// <param name="y">The y-coordinate of the bounds.</param>
	/// <param name="width">The width of the bounds.</param>
	/// <param name="height">The height of the bounds.</param>
	/// <returns>A new <see cref="SKConfettiEmitterBounds"/> for the specified area.</returns>
	public static SKConfettiEmitterBounds Bounds(double x, double y, double width, double height) =>
		new SKConfettiEmitterBounds(x, y, width, height);

	/// <summary>
	/// Creates emitter bounds for a specific rectangle.
	/// </summary>
	/// <param name="rect">The emission bounds rectangle.</param>
	/// <returns>A new <see cref="SKConfettiEmitterBounds"/> for the specified rectangle.</returns>
	public static SKConfettiEmitterBounds Bounds(Rect rect) =>
		new SKConfettiEmitterBounds(rect);

	/// <summary>
	/// Creates emitter bounds for a specific point.
	/// </summary>
	/// <param name="x">The x-coordinate of the emission point.</param>
	/// <param name="y">The y-coordinate of the emission point.</param>
	/// <returns>A new <see cref="SKConfettiEmitterBounds"/> for the specified point.</returns>
	public static SKConfettiEmitterBounds Point(double x, double y) =>
		new SKConfettiEmitterBounds(x, y);

	/// <summary>
	/// Creates emitter bounds for a specific point.
	/// </summary>
	/// <param name="point">The emission point.</param>
	/// <returns>A new <see cref="SKConfettiEmitterBounds"/> for the specified point.</returns>
	public static SKConfettiEmitterBounds Point(Point point) =>
		new SKConfettiEmitterBounds(point);
}
