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

		DrawHatchPattern(tileCanvas, fg, bg, tileSize);

		using var image = SKImage.FromBitmap(tileBitmap);
		paint.Shader = SKShader.CreateImage(image, SKShaderTileMode.Repeat, SKShaderTileMode.Repeat);
		return paint;
	}

	private void DrawHatchPattern(SKCanvas canvas, SKColor fg, SKColor bg, int size)
	{
		switch (_hatchStyle)
		{
			case HatchStyle.Horizontal:
			case HatchStyle.LightHorizontal:
				for (int x = 0; x < size; x++)
					SetPixel(canvas, x, 0, fg);
				break;
			case HatchStyle.Vertical:
			case HatchStyle.LightVertical:
				for (int y = 0; y < size; y++)
					SetPixel(canvas, 0, y, fg);
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
				for (int x = 0; x < size; x++)
					SetPixel(canvas, x, 0, fg);
				for (int y = 1; y < size; y++)
					SetPixel(canvas, 0, y, fg);
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
				for (int x = 0; x < size; x++)
				{
					SetPixel(canvas, x, 0, fg);
					SetPixel(canvas, x, 1, fg);
					SetPixel(canvas, x, 4, fg);
					SetPixel(canvas, x, 5, fg);
				}
				break;
			case HatchStyle.DarkVertical:
				for (int y = 0; y < size; y++)
				{
					SetPixel(canvas, 0, y, fg);
					SetPixel(canvas, 1, y, fg);
					SetPixel(canvas, 4, y, fg);
					SetPixel(canvas, 5, y, fg);
				}
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
					for (int y = 0; y < size; y++)
						SetPixel(canvas, x, y, fg);
				break;
			case HatchStyle.NarrowHorizontal:
				for (int y = 0; y < size; y += 2)
					for (int x = 0; x < size; x++)
						SetPixel(canvas, x, y, fg);
				break;
			case HatchStyle.DashedHorizontal:
				for (int x = 0; x < 4; x++)
					SetPixel(canvas, x, 0, fg);
				for (int x = 4; x < 8; x++)
					SetPixel(canvas, x, 4, fg);
				break;
			case HatchStyle.DashedVertical:
				for (int y = 0; y < 4; y++)
					SetPixel(canvas, 0, y, fg);
				for (int y = 4; y < 8; y++)
					SetPixel(canvas, 4, y, fg);
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
				for (int y = 0; y < 4; y++)
					for (int x = 4; x < 8; x++)
						SetPixel(canvas, x, y, fg);
				for (int y = 4; y < 8; y++)
					for (int x = 0; x < 4; x++)
						SetPixel(canvas, x, y, fg);
				break;
			case HatchStyle.Sphere:
				// Circle-ish dot pattern
				SetPixel(canvas, 2, 0, fg); SetPixel(canvas, 3, 0, fg);
				SetPixel(canvas, 1, 1, fg); SetPixel(canvas, 4, 1, fg);
				SetPixel(canvas, 1, 2, fg); SetPixel(canvas, 4, 2, fg);
				SetPixel(canvas, 2, 3, fg); SetPixel(canvas, 3, 3, fg);
				SetPixel(canvas, 6, 4, fg); SetPixel(canvas, 7, 4, fg);
				SetPixel(canvas, 5, 5, fg); SetPixel(canvas, 0, 5, fg);
				SetPixel(canvas, 5, 6, fg); SetPixel(canvas, 0, 6, fg);
				SetPixel(canvas, 6, 7, fg); SetPixel(canvas, 7, 7, fg);
				break;
			case HatchStyle.Trellis:
				// Alternating rows of checks
				for (int y = 0; y < size; y++)
					for (int x = 0; x < size; x++)
						SetPixel(canvas, x, y, ((x + y) % 2 == 0) ? fg : bg);
				break;
			case HatchStyle.Weave:
				// Downward diagonal (0,0)→(4,4), upward (4,4)→(7,1), vertical (4,4)→(4,7)
				SetPixel(canvas, 0, 0, fg);
				SetPixel(canvas, 1, 1, fg);
				SetPixel(canvas, 2, 2, fg);
				SetPixel(canvas, 3, 3, fg);
				SetPixel(canvas, 4, 4, fg);
				SetPixel(canvas, 5, 3, fg);
				SetPixel(canvas, 6, 2, fg);
				SetPixel(canvas, 7, 1, fg);
				SetPixel(canvas, 4, 5, fg);
				SetPixel(canvas, 4, 6, fg);
				SetPixel(canvas, 4, 7, fg);
				break;
			case HatchStyle.HorizontalBrick:
				// Horizontal line at y=4, vertical mortar at x=0 (top) and x=4 (bottom)
				for (int x = 0; x < size; x++)
					SetPixel(canvas, x, 4, fg);
				for (int y = 0; y < 4; y++)
					SetPixel(canvas, 0, y, fg);
				for (int y = 5; y < size; y++)
					SetPixel(canvas, 4, y, fg);
				break;
			case HatchStyle.DiagonalBrick:
				// Main diagonal + counter diagonal from bottom-left to center
				for (int i = 0; i < size; i++)
					SetPixel(canvas, i, i, fg);
				for (int i = 1; i < size / 2; i++)
					SetPixel(canvas, i, size - i, fg);
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
				for (int dy = 0; dy < 2; dy++)
					for (int dx = 0; dx < 2; dx++)
					{
						SetPixel(canvas, dx, dy, fg);
						SetPixel(canvas, 4 + dx, 4 + dy, fg);
					}
				break;
			case HatchStyle.ZigZag:
				// V shape: up from (0,4) to (4,0) then down to (7,3)
				for (int i = 0; i <= 4; i++)
					SetPixel(canvas, i, 4 - i, fg);
				for (int i = 1; i < 4; i++)
					SetPixel(canvas, 4 + i, i, fg);
				break;
			case HatchStyle.Wave:
				// Sine-like wave: down-up-down-up
				SetPixel(canvas, 0, 4, fg);
				SetPixel(canvas, 1, 3, fg);
				SetPixel(canvas, 2, 2, fg);
				SetPixel(canvas, 3, 3, fg);
				SetPixel(canvas, 4, 4, fg);
				SetPixel(canvas, 5, 3, fg);
				SetPixel(canvas, 6, 2, fg);
				SetPixel(canvas, 7, 3, fg);
				break;
			case HatchStyle.Shingle:
				// Main diagonal + counter line from bottom-left to center
				for (int i = 0; i < size; i++)
					SetPixel(canvas, i, i, fg);
				for (int i = 1; i < size / 2; i++)
					SetPixel(canvas, i, size - i, fg);
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
				for (int y = 0; y < size; y++)
					for (int x = 0; x < size; x++)
						if (((x / 2) + (y / 2)) % 2 == 0)
							SetPixel(canvas, x, y, fg);
				break;
			case HatchStyle.LargeCheckerBoard:
				for (int y = 0; y < size; y++)
					for (int x = 0; x < size; x++)
						if (((x / 4) + (y / 4)) % 2 == 0)
							SetPixel(canvas, x, y, fg);
				break;
			default:
				// Fallback: cross pattern
				for (int x = 0; x < size; x++)
					SetPixel(canvas, x, 4, fg);
				for (int y = 0; y < size; y++)
					SetPixel(canvas, 4, y, fg);
				break;
		}
	}

	private static void SetPixel(SKCanvas canvas, int x, int y, SKColor color)
	{
		using var paint = new SKPaint { Color = color, IsAntialias = false, Style = SKPaintStyle.Fill };
		canvas.DrawRect(x, y, 1, 1, paint);
	}
}
