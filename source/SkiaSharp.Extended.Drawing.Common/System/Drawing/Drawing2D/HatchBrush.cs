using SkiaSharp;
using System.Drawing.Internal;

namespace System.Drawing.Drawing2D;

/// <summary>
///  Defines a rectangular brush with a hatch style, a foreground color, and a background color.
///  This class cannot be inherited.
/// </summary>
public sealed partial class HatchBrush : Brush
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
	public HatchBrush(HatchStyle hatchstyle, Color foreColor)
		: this(hatchstyle, foreColor, Color.Black) { }

	/// <summary>
	///  Initializes a new instance of the <see cref="HatchBrush"/> class with the specified
	///  <see cref="Drawing2D.HatchStyle"/> enumeration, foreground color, and background color.
	/// </summary>
	/// <param name="hatchstyle">One of the <see cref="Drawing2D.HatchStyle"/> values that represents the pattern drawn by this <see cref="HatchBrush"/>.</param>
	/// <param name="foreColor">The <see cref="Color"/> structure that represents the color of lines drawn by this <see cref="HatchBrush"/>.</param>
	/// <param name="backColor">The <see cref="Color"/> structure that represents the color of spaces between the lines drawn by this <see cref="HatchBrush"/>.</param>
	public HatchBrush(HatchStyle hatchstyle, Color foreColor, Color backColor)
	{
		_hatchStyle = hatchstyle;
		_foreColor = foreColor;
		_backColor = backColor;
	}

	/// <summary>
	///  Gets the color of spaces between the hatch lines drawn by this <see cref="HatchBrush"/> object.
	/// </summary>
	/// <value>A <see cref="Color"/> that represents the background color for this <see cref="HatchBrush"/>.</value>
	public Color BackgroundColor
	{
		get { ThrowIfDisposed(); return _backColor; }
	}

	/// <summary>
	///  Gets the color of hatch lines drawn by this <see cref="HatchBrush"/> object.
	/// </summary>
	/// <value>A <see cref="Color"/> that represents the foreground color for this <see cref="HatchBrush"/>.</value>
	public Color ForegroundColor
	{
		get { ThrowIfDisposed(); return _foreColor; }
	}

	/// <summary>
	///  Gets the hatch style of this <see cref="HatchBrush"/> object.
	/// </summary>
	/// <value>One of the <see cref="Drawing2D.HatchStyle"/> values that represents the pattern of this <see cref="HatchBrush"/>.</value>
	public HatchStyle HatchStyle
	{
		get { ThrowIfDisposed(); return _hatchStyle; }
	}

	/// <summary>
	///  Creates an exact copy of this <see cref="HatchBrush"/> object.
	/// </summary>
	/// <returns>The <see cref="HatchBrush"/> object this method creates, cast as an <see cref="object"/>.</returns>
	public override object Clone() => new HatchBrush(_hatchStyle, _foreColor, _backColor);

	/// <summary>
	///  Hatch pattern bitmasks: 8 bytes per pattern, each bit represents a pixel (MSB = leftmost).
	///  Indexed by (int)HatchStyle * 8. Patterns match GDI+ output on Windows.
	/// </summary>
	private static ReadOnlySpan<byte> HatchPatternData => new byte[]
	{
		// 0  Horizontal
		0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF,
		// 1  Vertical
		0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80,
		// 2  ForwardDiagonal
		0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80,
		// 3  BackwardDiagonal
		0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01,
		// 4  Cross
		0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0xFF,
		// 5  DiagonalCross
		0x81, 0x42, 0x24, 0x18, 0x18, 0x24, 0x42, 0x81,
		// 6  Percent05
		0x00, 0x00, 0x00, 0x08, 0x00, 0x00, 0x00, 0x80,
		// 7  Percent10
		0x00, 0x08, 0x00, 0x80, 0x00, 0x08, 0x00, 0x80,
		// 8  Percent20
		0x00, 0x22, 0x00, 0x88, 0x00, 0x22, 0x00, 0x88,
		// 9  Percent25
		0x22, 0x88, 0x22, 0x88, 0x22, 0x88, 0x22, 0x88,
		// 10 Percent30
		0x11, 0xAA, 0x44, 0xAA, 0x11, 0xAA, 0x44, 0xAA,
		// 11 Percent40
		0x15, 0xAA, 0x55, 0xAA, 0x51, 0xAA, 0x55, 0xAA,
		// 12 Percent50
		0x55, 0xAA, 0x55, 0xAA, 0x55, 0xAA, 0x55, 0xAA,
		// 13 Percent60
		0x55, 0xBB, 0x55, 0xEE, 0x55, 0xBB, 0x55, 0xEE,
		// 14 Percent70
		0xDD, 0x77, 0xDD, 0x77, 0xDD, 0x77, 0xDD, 0x77,
		// 15 Percent75
		0xFF, 0xDD, 0xFF, 0x77, 0xFF, 0xDD, 0xFF, 0x77,
		// 16 Percent80
		0xFF, 0xFE, 0xFF, 0xEF, 0xFF, 0xFE, 0xFF, 0xEF,
		// 17 Percent90
		0x7F, 0xFF, 0xFF, 0xFF, 0xF7, 0xFF, 0xFF, 0xFF,
		// 18 LightDownwardDiagonal
		0x11, 0x22, 0x44, 0x88, 0x11, 0x22, 0x44, 0x88,
		// 19 LightUpwardDiagonal
		0x88, 0x44, 0x22, 0x11, 0x88, 0x44, 0x22, 0x11,
		// 20 DarkDownwardDiagonal
		0x99, 0x33, 0x66, 0xCC, 0x99, 0x33, 0x66, 0xCC,
		// 21 DarkUpwardDiagonal
		0x99, 0xCC, 0x66, 0x33, 0x99, 0xCC, 0x66, 0x33,
		// 22 WideDownwardDiagonal
		0x83, 0x07, 0x0E, 0x1C, 0x38, 0x70, 0xE0, 0xC1,
		// 23 WideUpwardDiagonal
		0xC1, 0xE0, 0x70, 0x38, 0x1C, 0x0E, 0x07, 0x83,
		// 24 LightVertical
		0x88, 0x88, 0x88, 0x88, 0x88, 0x88, 0x88, 0x88,
		// 25 LightHorizontal
		0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF,
		// 26 NarrowVertical
		0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55,
		// 27 NarrowHorizontal
		0x00, 0xFF, 0x00, 0xFF, 0x00, 0xFF, 0x00, 0xFF,
		// 28 DarkVertical
		0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC,
		// 29 DarkHorizontal
		0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF,
		// 30 DashedDownwardDiagonal
		0x00, 0x00, 0x11, 0x22, 0x44, 0x88, 0x00, 0x00,
		// 31 DashedUpwardDiagonal
		0x00, 0x00, 0x88, 0x44, 0x22, 0x11, 0x00, 0x00,
		// 32 DashedHorizontal
		0x00, 0x00, 0x00, 0x0F, 0x00, 0x00, 0x00, 0xF0,
		// 33 DashedVertical
		0x08, 0x08, 0x08, 0x08, 0x80, 0x80, 0x80, 0x80,
		// 34 SmallConfetti
		0x04, 0x20, 0x01, 0x10, 0x02, 0x40, 0x08, 0x80,
		// 35 LargeConfetti
		0x8D, 0x0C, 0xC0, 0xD8, 0x1B, 0x03, 0x30, 0xB1,
		// 36 ZigZag
		0x18, 0x24, 0x42, 0x81, 0x18, 0x24, 0x42, 0x81,
		// 37 Wave
		0xC0, 0x25, 0x18, 0x00, 0xC0, 0x25, 0x18, 0x00,
		// 38 DiagonalBrick
		0x81, 0x42, 0x24, 0x18, 0x08, 0x04, 0x02, 0x01,
		// 39 HorizontalBrick
		0x08, 0x08, 0x08, 0xFF, 0x80, 0x80, 0x80, 0xFF,
		// 40 Weave
		0x51, 0x22, 0x14, 0x88, 0x45, 0x22, 0x54, 0x88,
		// 41 Plaid
		0xF0, 0xF0, 0xF0, 0xF0, 0x55, 0xAA, 0x55, 0xAA,
		// 42 Divot
		0x80, 0x01, 0x80, 0x00, 0x10, 0x08, 0x10, 0x00,
		// 43 DottedGrid
		0x00, 0x80, 0x00, 0x80, 0x00, 0x80, 0x00, 0xAA,
		// 44 DottedDiamond
		0x00, 0x22, 0x00, 0x08, 0x00, 0x22, 0x00, 0x80,
		// 45 Shingle
		0x01, 0x01, 0x02, 0x0C, 0x30, 0x48, 0x84, 0x03,
		// 46 Trellis
		0x99, 0xFF, 0x66, 0xFF, 0x99, 0xFF, 0x66, 0xFF,
		// 47 Sphere
		0xF8, 0xF8, 0x98, 0x77, 0x8F, 0x8F, 0x89, 0x77,
		// 48 SmallGrid
		0x88, 0x88, 0x88, 0xFF, 0x88, 0x88, 0x88, 0xFF,
		// 49 SmallCheckerBoard
		0x99, 0x66, 0x66, 0x99, 0x99, 0x66, 0x66, 0x99,
		// 50 LargeCheckerBoard
		0x0F, 0x0F, 0x0F, 0x0F, 0xF0, 0xF0, 0xF0, 0xF0,
		// 51 OutlinedDiamond
		0x01, 0x82, 0x44, 0x28, 0x10, 0x28, 0x44, 0x82,
		// 52 SolidDiamond
		0x00, 0x10, 0x38, 0x7C, 0xFE, 0x7C, 0x38, 0x10,
	};

	/// <summary>
	///  Creates an <see cref="SKPaint"/> configured for fill operations with this hatch brush.
	///  Generates an 8×8 tiled pattern bitmap from the hatch bitmask table.
	/// </summary>
	/// <returns>A new <see cref="SKPaint"/> with a tiled pattern shader.</returns>
	internal override SKPaint CreatePaint()
	{
		ThrowIfDisposed();
		var paint = new SKPaint { Style = SKPaintStyle.Fill, IsAntialias = false };
		uint fg = (uint)SkiaConversions.ToSKColor(_foreColor);
		uint bg = (uint)SkiaConversions.ToSKColor(_backColor);

		int index = (int)_hatchStyle;
		if (index < 0 || index > 52)
			index = 0;

		var data = HatchPatternData;
		int offset = index * 8;
		const int tileSize = 8;

		// Build 64-pixel tile in a stack-friendly buffer, then copy into SKImage
		Span<uint> pixels = stackalloc uint[tileSize * tileSize];
		for (int y = 0; y < tileSize; y++)
		{
			byte row = data[offset + y];
			int rowOffset = y * tileSize;
			for (int x = 0; x < tileSize; x++)
				pixels[rowOffset + x] = (row & (0x80 >> x)) != 0 ? fg : bg;
		}

		var info = new SKImageInfo(tileSize, tileSize, SKColorType.Bgra8888, SKAlphaType.Premul);
		unsafe
		{
			fixed (uint* ptr = pixels)
			{
				using var image = SKImage.FromPixelCopy(info, (IntPtr)ptr);
				paint.Shader = SKShader.CreateImage(image, SKShaderTileMode.Repeat, SKShaderTileMode.Repeat);
			}
		}
		return paint;
	}
}
