namespace SkiaSharp.Extended.UI.Controls;

/// <summary>
/// Abstract base class for confetti particle shapes.
/// </summary>
public abstract class SKConfettiShape : BindableObject
{
	/// <summary>
	/// Draws the shape on the specified canvas.
	/// </summary>
	/// <param name="canvas">The canvas to draw on.</param>
	/// <param name="paint">The paint to use for drawing.</param>
	/// <param name="size">The size of the shape.</param>
	public void Draw(SKCanvas canvas, SKPaint paint, float size)
	{
		OnDraw(canvas, paint, size);
	}

	/// <summary>
	/// When overridden in a derived class, draws the shape on the specified canvas.
	/// </summary>
	/// <param name="canvas">The canvas to draw on.</param>
	/// <param name="paint">The paint to use for drawing.</param>
	/// <param name="size">The size of the shape.</param>
	protected abstract void OnDraw(SKCanvas canvas, SKPaint paint, float size);
}

/// <summary>
/// A confetti shape that draws a square.
/// </summary>
public class SKConfettiSquareShape : SKConfettiShape
{
	/// <inheritdoc/>
	protected override void OnDraw(SKCanvas canvas, SKPaint paint, float size)
	{
		var offset = -size / 2f;
		var rect = SKRect.Create(offset, offset, size, size);
		canvas.DrawRect(rect, paint);
	}
}

/// <summary>
/// A confetti shape that draws a circle.
/// </summary>
public class SKConfettiCircleShape : SKConfettiShape
{
	/// <inheritdoc/>
	protected override void OnDraw(SKCanvas canvas, SKPaint paint, float size)
	{
		canvas.DrawCircle(0, 0, size / 2f, paint);
	}
}

/// <summary>
/// A confetti shape that draws a rectangle with a configurable height ratio.
/// </summary>
public class SKConfettiRectShape : SKConfettiShape
{
	/// <summary>
	/// Identifies the <see cref="HeightRatio"/> bindable property.
	/// </summary>
	public static readonly BindableProperty HeightRatioProperty = BindableProperty.Create(
		nameof(HeightRatio),
		typeof(double),
		typeof(SKConfettiRectShape),
		0.5,
		coerceValue: OnCoerceHeightRatio);

	/// <summary>
	/// Initializes a new instance of the <see cref="SKConfettiRectShape"/> class.
	/// </summary>
	public SKConfettiRectShape()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="SKConfettiRectShape"/> class with the specified height ratio.
	/// </summary>
	/// <param name="heightRatio">The ratio of height to width, clamped between 0 and 1.</param>
	public SKConfettiRectShape(double heightRatio)
	{
		HeightRatio = heightRatio;
	}

	/// <summary>
	/// Gets or sets the ratio of height to width, clamped between 0 and 1.
	/// </summary>
	public double HeightRatio
	{
		get => (double)GetValue(HeightRatioProperty);
		set => SetValue(HeightRatioProperty, value);
	}

	/// <inheritdoc/>
	protected override void OnDraw(SKCanvas canvas, SKPaint paint, float size)
	{
		var height = size * (float)HeightRatio;
		if (size <= 0 || height <= 0)
			return;

		var rect = SKRect.Create(-size / 2f, -height / 2f, size, height);
		canvas.DrawRect(rect, paint);
	}

	private static object OnCoerceHeightRatio(BindableObject bindable, object value) =>
		value is double ratio
			? Math.Max(0, Math.Min(ratio, 1.0))
			: (object)1.0;
}

/// <summary>
/// A confetti shape that draws an oval with a configurable height ratio.
/// </summary>
public class SKConfettiOvalShape : SKConfettiShape
{
	/// <summary>
	/// Identifies the <see cref="HeightRatio"/> bindable property.
	/// </summary>
	public static readonly BindableProperty HeightRatioProperty = BindableProperty.Create(
		nameof(HeightRatio),
		typeof(double),
		typeof(SKConfettiOvalShape),
		0.5,
		coerceValue: OnCoerceHeightRatio);

	/// <summary>
	/// Initializes a new instance of the <see cref="SKConfettiOvalShape"/> class.
	/// </summary>
	public SKConfettiOvalShape()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="SKConfettiOvalShape"/> class with the specified height ratio.
	/// </summary>
	/// <param name="heightRatio">The ratio of height to width, clamped between 0 and 1.</param>
	public SKConfettiOvalShape(double heightRatio)
	{
		HeightRatio = heightRatio;
	}

	/// <summary>
	/// Gets or sets the ratio of height to width, clamped between 0 and 1.
	/// </summary>
	public double HeightRatio
	{
		get => (double)GetValue(HeightRatioProperty);
		set => SetValue(HeightRatioProperty, value);
	}

	/// <inheritdoc/>
	protected override void OnDraw(SKCanvas canvas, SKPaint paint, float size)
	{
		var height = size * (float)HeightRatio;
		if (size <= 0 || height <= 0)
			return;

		var rect = SKRect.Create(-size / 2f, -height / 2f, size, height);
		canvas.DrawOval(rect, paint);
	}

	private static object OnCoerceHeightRatio(BindableObject bindable, object value) =>
		value is double ratio
			? Math.Max(0, Math.Min(ratio, 1.0))
			: (object)1.0;
}

/// <summary>
/// A confetti shape that draws a custom <see cref="SKPath"/>.
/// </summary>
public class SKConfettiPathShape : SKConfettiShape
{
	/// <summary>
	/// Identifies the <see cref="Path"/> bindable property.
	/// </summary>
	public static readonly BindableProperty PathProperty = BindableProperty.Create(
		nameof(Path),
		typeof(SKPath),
		typeof(SKConfettiPathShape),
		null,
		propertyChanged: OnPathChanged);

	private SKSize baseSize;

	/// <summary>
	/// Initializes a new instance of the <see cref="SKConfettiPathShape"/> class.
	/// </summary>
	public SKConfettiPathShape()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="SKConfettiPathShape"/> class with the specified path.
	/// </summary>
	/// <param name="path">The path that defines the shape.</param>
	public SKConfettiPathShape(SKPath path)
	{
		Path = path ?? throw new ArgumentNullException(nameof(path));
	}

	/// <summary>
	/// Gets or sets the path that defines the shape.
	/// </summary>
	public SKPath? Path
	{
		get => (SKPath?)GetValue(PathProperty);
		set => SetValue(PathProperty, value);
	}

	/// <inheritdoc/>
	protected override void OnDraw(SKCanvas canvas, SKPaint paint, float size)
	{
		if (baseSize.Width <= 0 || baseSize.Height <= 0 || Path == null)
			return;

		canvas.Save();
		canvas.Scale(size / baseSize.Width, size / baseSize.Height);

		canvas.DrawPath(Path, paint);

		canvas.Restore();
	}

	private static void OnPathChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is SKConfettiPathShape shape && newValue is SKPath path)
			shape.baseSize = path.TightBounds.Size;
	}
}
