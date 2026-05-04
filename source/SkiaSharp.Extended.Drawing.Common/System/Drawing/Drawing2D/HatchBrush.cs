using SkiaSharp;
using System.Drawing.Internal;

namespace System.Drawing.Drawing2D;

/// <summary>
///  Defines a rectangular brush with a hatch style, a foreground color, and a background color.
///  This class cannot be inherited.
/// </summary>
public sealed partial class HatchBrush : System.Drawing.Brush
{
	private readonly HatchStyle _hatchStyle;
	private readonly Color _foreColor;
	private readonly Color _backColor;

	/// <summary>
	///  Initializes a new instance of the <see cref="HatchBrush"/> class with the specified
	///  <see cref="Drawing2D.HatchStyle"/> enumeration and foreground color.
	/// </summary>
	/// <param name="hatchstyle">One of the <see cref="Drawing2D.HatchStyle"/> values that represents the pattern drawn by this <see cref="HatchBrush"/>.</param>
	/// <param name="foreColor">The <see cref="Color"/> structure that represents the color of lines drawn by this <see cref="HatchBrush"/>.</param>
	public HatchBrush(System.Drawing.Drawing2D.HatchStyle hatchstyle, System.Drawing.Color foreColor)
		: this(hatchstyle, foreColor, Color.Black) { }

	/// <summary>
	///  Initializes a new instance of the <see cref="HatchBrush"/> class with the specified
	///  <see cref="Drawing2D.HatchStyle"/> enumeration, foreground color, and background color.
	/// </summary>
	/// <param name="hatchstyle">One of the <see cref="Drawing2D.HatchStyle"/> values that represents the pattern drawn by this <see cref="HatchBrush"/>.</param>
	/// <param name="foreColor">The <see cref="Color"/> structure that represents the color of lines drawn by this <see cref="HatchBrush"/>.</param>
	/// <param name="backColor">The <see cref="Color"/> structure that represents the color of spaces between the lines drawn by this <see cref="HatchBrush"/>.</param>
	public HatchBrush(System.Drawing.Drawing2D.HatchStyle hatchstyle, System.Drawing.Color foreColor, System.Drawing.Color backColor)
	{
		_hatchStyle = hatchstyle;
		_foreColor = foreColor;
		_backColor = backColor;
	}

	/// <summary>
	///  Gets the color of spaces between the hatch lines drawn by this <see cref="HatchBrush"/> object.
	/// </summary>
	/// <value>A <see cref="Color"/> that represents the background color for this <see cref="HatchBrush"/>.</value>
	public System.Drawing.Color BackgroundColor
	{
		get { ThrowIfDisposed(); return _backColor; }
	}

	/// <summary>
	///  Gets the color of hatch lines drawn by this <see cref="HatchBrush"/> object.
	/// </summary>
	/// <value>A <see cref="Color"/> that represents the foreground color for this <see cref="HatchBrush"/>.</value>
	public System.Drawing.Color ForegroundColor
	{
		get { ThrowIfDisposed(); return _foreColor; }
	}

	/// <summary>
	///  Gets the hatch style of this <see cref="HatchBrush"/> object.
	/// </summary>
	/// <value>One of the <see cref="Drawing2D.HatchStyle"/> values that represents the pattern of this <see cref="HatchBrush"/>.</value>
	public System.Drawing.Drawing2D.HatchStyle HatchStyle
	{
		get { ThrowIfDisposed(); return _hatchStyle; }
	}

	/// <summary>
	///  Creates an exact copy of this <see cref="HatchBrush"/> object.
	/// </summary>
	/// <returns>The <see cref="HatchBrush"/> object this method creates, cast as an <see cref="object"/>.</returns>
	public override object Clone() => new HatchBrush(_hatchStyle, _foreColor, _backColor);

	/// <summary>
	///  Creates an <see cref="SKPaint"/> configured for fill operations with this hatch brush.
	///  Generates a tiled hatch pattern bitmap for the most common hatch styles.
	/// </summary>
	/// <returns>A new <see cref="SKPaint"/> with a tiled pattern shader.</returns>
	internal override SKPaint CreatePaint()
	{
		ThrowIfDisposed();
		var paint = new SKPaint { Style = SKPaintStyle.Fill, IsAntialias = false };
		var fg = SkiaConversions.ToSKColor(_foreColor);
		var bg = SkiaConversions.ToSKColor(_backColor);

		const int tileSize = 8;
		using var tileBitmap = new SKBitmap(tileSize, tileSize);
		using var tileCanvas = new SKCanvas(tileBitmap);
		tileCanvas.Clear(bg);

		using var fgPaint = new SKPaint
		{
			Color = fg,
			IsAntialias = false,
			StrokeWidth = 1,
			Style = SKPaintStyle.Stroke,
		};

		DrawHatchPattern(tileCanvas, fgPaint, fg, bg, tileSize);

		using var image = SKImage.FromBitmap(tileBitmap);
		paint.Shader = SKShader.CreateImage(image, SKShaderTileMode.Repeat, SKShaderTileMode.Repeat);
		return paint;
	}

	private void DrawHatchPattern(SKCanvas canvas, SKPaint fgPaint, SKColor fg, SKColor bg, int size)
	{
		switch (_hatchStyle)
		{
			case HatchStyle.Horizontal:
			case HatchStyle.LightHorizontal:
				canvas.DrawLine(0, 0, size, 0, fgPaint);
				break;
			case HatchStyle.Vertical:
			case HatchStyle.LightVertical:
				canvas.DrawLine(0, 0, 0, size, fgPaint);
				break;
			case HatchStyle.ForwardDiagonal:
			case HatchStyle.LightDownwardDiagonal:
				for (int i = 0; i < size; i++)
					SetPixel(canvas, i, i, fg);
				break;
			case HatchStyle.BackwardDiagonal:
			case HatchStyle.LightUpwardDiagonal:
				for (int i = 0; i < size; i++)
					SetPixel(canvas, size - 1 - i, i, fg);
				break;
			case HatchStyle.Cross:
			case HatchStyle.SmallGrid:
				canvas.DrawLine(0, 0, size, 0, fgPaint);
				canvas.DrawLine(0, 0, 0, size, fgPaint);
				break;
			case HatchStyle.DiagonalCross:
			case HatchStyle.OutlinedDiamond:
				for (int i = 0; i < size; i++)
				{
					SetPixel(canvas, i, i, fg);
					SetPixel(canvas, size - 1 - i, i, fg);
				}
				break;
			case HatchStyle.DarkHorizontal:
				fgPaint.StrokeWidth = 2;
				canvas.DrawLine(0, 2, size, 2, fgPaint);
				canvas.DrawLine(0, 6, size, 6, fgPaint);
				break;
			case HatchStyle.DarkVertical:
				fgPaint.StrokeWidth = 2;
				canvas.DrawLine(2, 0, 2, size, fgPaint);
				canvas.DrawLine(6, 0, 6, size, fgPaint);
				break;
			case HatchStyle.DarkDownwardDiagonal:
				for (int i = 0; i < size; i++)
				{
					SetPixel(canvas, i, i, fg);
					SetPixel(canvas, (i + 1) % size, i, fg);
				}
				break;
			case HatchStyle.DarkUpwardDiagonal:
				for (int i = 0; i < size; i++)
				{
					SetPixel(canvas, size - 1 - i, i, fg);
					SetPixel(canvas, (size - 2 - i + size) % size, i, fg);
				}
				break;
			case HatchStyle.WideDownwardDiagonal:
				for (int i = 0; i < size; i++)
				{
					SetPixel(canvas, (i - 1 + size) % size, i, fg);
					SetPixel(canvas, i, i, fg);
					SetPixel(canvas, (i + 1) % size, i, fg);
				}
				break;
			case HatchStyle.WideUpwardDiagonal:
				for (int i = 0; i < size; i++)
				{
					SetPixel(canvas, (size - i) % size, i, fg);
					SetPixel(canvas, size - 1 - i, i, fg);
					SetPixel(canvas, (size - 2 - i + size) % size, i, fg);
				}
				break;
			case HatchStyle.NarrowVertical:
				for (int x = 0; x < size; x += 2)
					canvas.DrawLine(x, 0, x, size, fgPaint);
				break;
			case HatchStyle.NarrowHorizontal:
				for (int y = 0; y < size; y += 2)
					canvas.DrawLine(0, y, size, y, fgPaint);
				break;
			case HatchStyle.DashedHorizontal:
				canvas.DrawLine(0, 0, 4, 0, fgPaint);
				canvas.DrawLine(4, 4, 8, 4, fgPaint);
				break;
			case HatchStyle.DashedVertical:
				canvas.DrawLine(0, 0, 0, 4, fgPaint);
				canvas.DrawLine(4, 4, 4, 8, fgPaint);
				break;
			case HatchStyle.DashedDownwardDiagonal:
				for (int i = 0; i < 4; i++)
					SetPixel(canvas, i, i, fg);
				for (int i = 4; i < 8; i++)
					SetPixel(canvas, i, i, fg);
				break;
			case HatchStyle.DashedUpwardDiagonal:
				for (int i = 0; i < 4; i++)
					SetPixel(canvas, 4 - 1 - i, i, fg);
				for (int i = 0; i < 4; i++)
					SetPixel(canvas, 8 - 1 - i, 4 + i, fg);
				break;
			case HatchStyle.Percent05:
				SetPixel(canvas, 0, 0, fg);
				break;
			case HatchStyle.Percent10:
				SetPixel(canvas, 0, 0, fg);
				SetPixel(canvas, 4, 4, fg);
				break;
			case HatchStyle.Percent20:
				SetPixel(canvas, 0, 0, fg);
				SetPixel(canvas, 4, 2, fg);
				SetPixel(canvas, 2, 4, fg);
				SetPixel(canvas, 6, 6, fg);
				break;
			case HatchStyle.Percent25:
				SetPixel(canvas, 0, 0, fg); SetPixel(canvas, 4, 0, fg);
				SetPixel(canvas, 2, 2, fg); SetPixel(canvas, 6, 2, fg);
				SetPixel(canvas, 0, 4, fg); SetPixel(canvas, 4, 4, fg);
				SetPixel(canvas, 2, 6, fg); SetPixel(canvas, 6, 6, fg);
				break;
			case HatchStyle.Percent30:
				for (int y = 0; y < size; y++)
					for (int x = (y % 2 == 0) ? 0 : 1; x < size; x += 3)
						SetPixel(canvas, x, y, fg);
				break;
			case HatchStyle.Percent40:
				for (int y = 0; y < size; y++)
					for (int x = (y % 2 == 0) ? 0 : 1; x < size; x += 2)
						SetPixel(canvas, x, y, fg);
				break;
			case HatchStyle.Percent50:
				for (int y = 0; y < size; y++)
					for (int x = (y % 2); x < size; x += 2)
						SetPixel(canvas, x, y, fg);
				break;
			case HatchStyle.Percent60:
				canvas.Clear(fg);
				for (int y = 0; y < size; y++)
					for (int x = (y % 2); x < size; x += 2)
						SetPixel(canvas, x, y, bg);
				break;
			case HatchStyle.Percent70:
				canvas.Clear(fg);
				for (int y = 0; y < size; y++)
					for (int x = (y % 2 == 0) ? 0 : 1; x < size; x += 2)
						SetPixel(canvas, x, y, bg);
				break;
			case HatchStyle.Percent75:
				canvas.Clear(fg);
				SetPixel(canvas, 0, 0, bg); SetPixel(canvas, 4, 0, bg);
				SetPixel(canvas, 2, 2, bg); SetPixel(canvas, 6, 2, bg);
				SetPixel(canvas, 0, 4, bg); SetPixel(canvas, 4, 4, bg);
				SetPixel(canvas, 2, 6, bg); SetPixel(canvas, 6, 6, bg);
				break;
			case HatchStyle.Percent80:
				canvas.Clear(fg);
				SetPixel(canvas, 0, 0, bg);
				SetPixel(canvas, 4, 2, bg);
				SetPixel(canvas, 2, 4, bg);
				SetPixel(canvas, 6, 6, bg);
				break;
			case HatchStyle.Percent90:
				canvas.Clear(fg);
				SetPixel(canvas, 0, 0, bg);
				SetPixel(canvas, 4, 4, bg);
				break;
			case HatchStyle.Plaid:
				// Top-left 4x4 checkerboard, bottom-right 4x4 solid
				for (int y = 0; y < 4; y++)
					for (int x = 0; x < 4; x++)
						SetPixel(canvas, x, y, ((x + y) % 2 == 0) ? fg : bg);
				using (var fillPaint = new SKPaint { Color = fg, IsAntialias = false })
					canvas.DrawRect(4, 0, 4, 4, fillPaint);
				for (int y = 4; y < 8; y++)
					for (int x = 0; x < 4; x++)
						SetPixel(canvas, x, y, fg);
				break;
			case HatchStyle.Sphere:
				// Circle-ish dot pattern
				using (var fillPaint = new SKPaint { Color = fg, IsAntialias = false, Style = SKPaintStyle.Fill })
				{
					canvas.DrawOval(new SKRect(1, 1, 5, 5), fillPaint);
				}
				break;
			case HatchStyle.Trellis:
				// Alternating rows of checks
				for (int y = 0; y < size; y++)
					for (int x = 0; x < size; x++)
						SetPixel(canvas, x, y, ((x + y) % 2 == 0) ? fg : bg);
				break;
			case HatchStyle.Weave:
				canvas.DrawLine(0, 0, 4, 4, fgPaint);
				canvas.DrawLine(4, 4, 8, 0, fgPaint);
				canvas.DrawLine(4, 4, 4, 8, fgPaint);
				break;
			case HatchStyle.HorizontalBrick:
				canvas.DrawLine(0, 4, size, 4, fgPaint);
				canvas.DrawLine(0, 0, 0, 4, fgPaint);
				canvas.DrawLine(4, 4, 4, 8, fgPaint);
				break;
			case HatchStyle.DiagonalBrick:
				canvas.DrawLine(0, 0, size, size, fgPaint);
				canvas.DrawLine(0, size, size / 2, size / 2, fgPaint);
				break;
			case HatchStyle.DottedGrid:
				for (int i = 0; i < size; i += 2)
				{
					SetPixel(canvas, i, 0, fg);
					SetPixel(canvas, 0, i, fg);
				}
				break;
			case HatchStyle.DottedDiamond:
				SetPixel(canvas, 0, 0, fg);
				SetPixel(canvas, 1, 1, fg);
				SetPixel(canvas, 2, 2, fg);
				SetPixel(canvas, 3, 3, fg);
				SetPixel(canvas, 4, 4, fg);
				SetPixel(canvas, 3, 5, fg);
				SetPixel(canvas, 2, 6, fg);
				SetPixel(canvas, 1, 7, fg);
				break;
			case HatchStyle.SmallConfetti:
				SetPixel(canvas, 1, 0, fg);
				SetPixel(canvas, 5, 1, fg);
				SetPixel(canvas, 3, 2, fg);
				SetPixel(canvas, 7, 3, fg);
				SetPixel(canvas, 0, 4, fg);
				SetPixel(canvas, 4, 5, fg);
				SetPixel(canvas, 2, 6, fg);
				SetPixel(canvas, 6, 7, fg);
				break;
			case HatchStyle.LargeConfetti:
				using (var fillPaint = new SKPaint { Color = fg, IsAntialias = false, Style = SKPaintStyle.Fill })
				{
					canvas.DrawRect(0, 0, 2, 2, fillPaint);
					canvas.DrawRect(4, 4, 2, 2, fillPaint);
				}
				break;
			case HatchStyle.ZigZag:
				canvas.DrawLine(0, 4, 4, 0, fgPaint);
				canvas.DrawLine(4, 0, 8, 4, fgPaint);
				break;
			case HatchStyle.Wave:
				canvas.DrawLine(0, 4, 2, 2, fgPaint);
				canvas.DrawLine(2, 2, 4, 4, fgPaint);
				canvas.DrawLine(4, 4, 6, 2, fgPaint);
				canvas.DrawLine(6, 2, 8, 4, fgPaint);
				break;
			case HatchStyle.Shingle:
				canvas.DrawLine(0, 0, 8, 8, fgPaint);
				canvas.DrawLine(0, 8, 4, 4, fgPaint);
				break;
			case HatchStyle.Divot:
				SetPixel(canvas, 2, 1, fg);
				SetPixel(canvas, 3, 2, fg);
				SetPixel(canvas, 6, 5, fg);
				SetPixel(canvas, 5, 6, fg);
				break;
			case HatchStyle.SolidDiamond:
				using (var fillPaint = new SKPaint { Color = fg, IsAntialias = false, Style = SKPaintStyle.Fill })
				{
					var diamondPath = new SKPath();
					diamondPath.MoveTo(4, 0);
					diamondPath.LineTo(8, 4);
					diamondPath.LineTo(4, 8);
					diamondPath.LineTo(0, 4);
					diamondPath.Close();
					canvas.DrawPath(diamondPath, fillPaint);
					diamondPath.Dispose();
				}
				break;
			case HatchStyle.SmallCheckerBoard:
				using (var fillPaint = new SKPaint { Color = fg, IsAntialias = false, Style = SKPaintStyle.Fill })
				{
					canvas.DrawRect(0, 0, 2, 2, fillPaint);
					canvas.DrawRect(4, 0, 2, 2, fillPaint);
					canvas.DrawRect(2, 2, 2, 2, fillPaint);
					canvas.DrawRect(6, 2, 2, 2, fillPaint);
					canvas.DrawRect(0, 4, 2, 2, fillPaint);
					canvas.DrawRect(4, 4, 2, 2, fillPaint);
					canvas.DrawRect(2, 6, 2, 2, fillPaint);
					canvas.DrawRect(6, 6, 2, 2, fillPaint);
				}
				break;
			case HatchStyle.LargeCheckerBoard:
				using (var fillPaint = new SKPaint { Color = fg, IsAntialias = false, Style = SKPaintStyle.Fill })
				{
					canvas.DrawRect(0, 0, 4, 4, fillPaint);
					canvas.DrawRect(4, 4, 4, 4, fillPaint);
				}
				break;
			default:
				// Fallback: cross pattern
				canvas.DrawLine(0, 4, size, 4, fgPaint);
				canvas.DrawLine(4, 0, 4, size, fgPaint);
				break;
		}
	}

	private static void SetPixel(SKCanvas canvas, int x, int y, SKColor color)
	{
		using var paint = new SKPaint { Color = color, IsAntialias = false, Style = SKPaintStyle.Fill };
		canvas.DrawRect(x, y, 1, 1, paint);
	}
}
