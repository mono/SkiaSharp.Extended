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

	// Wine's GPL hatch pattern data: 8 bytes per row (MSB = leftmost pixel), 9th byte = AA flag
	private static readonly byte[][] HatchData = new byte[][]
	{
		new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xff, 0 }, // 0  Horizontal
		new byte[] { 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0 }, // 1  Vertical
		new byte[] { 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80, 1 }, // 2  ForwardDiagonal
		new byte[] { 0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01, 1 }, // 3  BackwardDiagonal
		new byte[] { 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0xff, 0 }, // 4  Cross
		new byte[] { 0x81, 0x42, 0x24, 0x18, 0x18, 0x24, 0x42, 0x81, 1 }, // 5  DiagonalCross
		new byte[] { 0x00, 0x00, 0x00, 0x08, 0x00, 0x00, 0x00, 0x80, 0 }, // 6  Percent05
		new byte[] { 0x00, 0x08, 0x00, 0x80, 0x00, 0x08, 0x00, 0x80, 0 }, // 7  Percent10
		new byte[] { 0x00, 0x22, 0x00, 0x88, 0x00, 0x22, 0x00, 0x88, 0 }, // 8  Percent20
		new byte[] { 0x22, 0x88, 0x22, 0x88, 0x22, 0x88, 0x22, 0x88, 0 }, // 9  Percent25
		new byte[] { 0x11, 0xaa, 0x44, 0xaa, 0x11, 0xaa, 0x44, 0xaa, 0 }, // 10 Percent30
		new byte[] { 0x15, 0xaa, 0x55, 0xaa, 0x51, 0xaa, 0x55, 0xaa, 0 }, // 11 Percent40
		new byte[] { 0x55, 0xaa, 0x55, 0xaa, 0x55, 0xaa, 0x55, 0xaa, 0 }, // 12 Percent50
		new byte[] { 0x55, 0xbb, 0x55, 0xee, 0x55, 0xbb, 0x55, 0xee, 0 }, // 13 Percent60
		new byte[] { 0xdd, 0x77, 0xdd, 0x77, 0xdd, 0x77, 0xdd, 0x77, 0 }, // 14 Percent70
		new byte[] { 0xff, 0xdd, 0xff, 0x77, 0xff, 0xdd, 0xff, 0x77, 0 }, // 15 Percent75
		new byte[] { 0xff, 0xfe, 0xff, 0xef, 0xff, 0xfe, 0xff, 0xef, 0 }, // 16 Percent80
		new byte[] { 0x7f, 0xff, 0xff, 0xff, 0xf7, 0xff, 0xff, 0xff, 0 }, // 17 Percent90
		new byte[] { 0x11, 0x22, 0x44, 0x88, 0x11, 0x22, 0x44, 0x88, 0 }, // 18 LightDownwardDiagonal
		new byte[] { 0x88, 0x44, 0x22, 0x11, 0x88, 0x44, 0x22, 0x11, 0 }, // 19 LightUpwardDiagonal
		new byte[] { 0x99, 0x33, 0x66, 0xcc, 0x99, 0x33, 0x66, 0xcc, 0 }, // 20 DarkDownwardDiagonal
		new byte[] { 0x99, 0xcc, 0x66, 0x33, 0x99, 0xcc, 0x66, 0x33, 0 }, // 21 DarkUpwardDiagonal
		new byte[] { 0x83, 0x07, 0x0e, 0x1c, 0x38, 0x70, 0xe0, 0xc1, 0 }, // 22 WideDownwardDiagonal
		new byte[] { 0xc1, 0xe0, 0x70, 0x38, 0x1c, 0x0e, 0x07, 0x83, 0 }, // 23 WideUpwardDiagonal
		new byte[] { 0x88, 0x88, 0x88, 0x88, 0x88, 0x88, 0x88, 0x88, 0 }, // 24 LightVertical
		new byte[] { 0x00, 0x00, 0x00, 0xff, 0x00, 0x00, 0x00, 0xff, 0 }, // 25 LightHorizontal
		new byte[] { 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0 }, // 26 NarrowVertical
		new byte[] { 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0 }, // 27 NarrowHorizontal
		new byte[] { 0xcc, 0xcc, 0xcc, 0xcc, 0xcc, 0xcc, 0xcc, 0xcc, 0 }, // 28 DarkVertical
		new byte[] { 0x00, 0x00, 0xff, 0xff, 0x00, 0x00, 0xff, 0xff, 0 }, // 29 DarkHorizontal
		new byte[] { 0x00, 0x00, 0x11, 0x22, 0x44, 0x88, 0x00, 0x00, 0 }, // 30 DashedDownwardDiagonal
		new byte[] { 0x00, 0x00, 0x88, 0x44, 0x22, 0x11, 0x00, 0x00, 0 }, // 31 DashedUpwardDiagonal
		new byte[] { 0x00, 0x00, 0x00, 0x0f, 0x00, 0x00, 0x00, 0xf0, 0 }, // 32 DashedHorizontal
		new byte[] { 0x08, 0x08, 0x08, 0x08, 0x80, 0x80, 0x80, 0x80, 0 }, // 33 DashedVertical
		new byte[] { 0x04, 0x20, 0x01, 0x10, 0x02, 0x40, 0x08, 0x80, 0 }, // 34 SmallConfetti
		new byte[] { 0x8d, 0x0c, 0xc0, 0xd8, 0x1b, 0x03, 0x30, 0xb1, 0 }, // 35 LargeConfetti
		new byte[] { 0x18, 0x24, 0x42, 0x81, 0x18, 0x24, 0x42, 0x81, 0 }, // 36 ZigZag
		new byte[] { 0xc0, 0x25, 0x18, 0x00, 0xc0, 0x25, 0x18, 0x00, 0 }, // 37 Wave
		new byte[] { 0x81, 0x42, 0x24, 0x18, 0x08, 0x04, 0x02, 0x01, 0 }, // 38 DiagonalBrick
		new byte[] { 0x08, 0x08, 0x08, 0xff, 0x80, 0x80, 0x80, 0xff, 0 }, // 39 HorizontalBrick
		new byte[] { 0x51, 0x22, 0x14, 0x88, 0x45, 0x22, 0x54, 0x88, 0 }, // 40 Weave
		new byte[] { 0xf0, 0xf0, 0xf0, 0xf0, 0x55, 0xaa, 0x55, 0xaa, 0 }, // 41 Plaid
		new byte[] { 0x80, 0x01, 0x80, 0x00, 0x10, 0x08, 0x10, 0x00, 0 }, // 42 Divot
		new byte[] { 0x00, 0x80, 0x00, 0x80, 0x00, 0x80, 0x00, 0xaa, 0 }, // 43 DottedGrid
		new byte[] { 0x00, 0x22, 0x00, 0x08, 0x00, 0x22, 0x00, 0x80, 0 }, // 44 DottedDiamond
		new byte[] { 0x01, 0x01, 0x02, 0x0c, 0x30, 0x48, 0x84, 0x03, 0 }, // 45 Shingle
		new byte[] { 0x99, 0xff, 0x66, 0xff, 0x99, 0xff, 0x66, 0xff, 0 }, // 46 Trellis
		new byte[] { 0xf8, 0xf8, 0x98, 0x77, 0x8f, 0x8f, 0x89, 0x77, 0 }, // 47 Sphere
		new byte[] { 0x88, 0x88, 0x88, 0xff, 0x88, 0x88, 0x88, 0xff, 0 }, // 48 SmallGrid
		new byte[] { 0x99, 0x66, 0x66, 0x99, 0x99, 0x66, 0x66, 0x99, 0 }, // 49 SmallCheckerBoard
		new byte[] { 0x0f, 0x0f, 0x0f, 0x0f, 0xf0, 0xf0, 0xf0, 0xf0, 0 }, // 50 LargeCheckerBoard
		new byte[] { 0x01, 0x82, 0x44, 0x28, 0x10, 0x28, 0x44, 0x82, 0 }, // 51 OutlinedDiamond
		new byte[] { 0x00, 0x10, 0x38, 0x7c, 0xfe, 0x7c, 0x38, 0x10, 0 }, // 52 SolidDiamond
	};

	/// <summary>
	///  Creates an <see cref="SKPaint"/> configured for fill operations with this hatch brush.
	///  Generates a tiled hatch pattern bitmap using Wine's hatch pattern data table.
	/// </summary>
	/// <returns>A new <see cref="SKPaint"/> with a tiled pattern shader.</returns>
	internal override SKPaint CreatePaint()
	{
		ThrowIfDisposed();
		var paint = new SKPaint { Style = SKPaintStyle.Fill, IsAntialias = false };
		var fg = SkiaConversions.ToSKColor(_foreColor);
		var bg = SkiaConversions.ToSKColor(_backColor);

		int index = (int)_hatchStyle;
		if (index < 0 || index >= HatchData.Length)
			index = 0; // fallback to Horizontal

		var pattern = HatchData[index];
		const int tileSize = 8;
		using var tileBitmap = new SKBitmap(tileSize, tileSize);

		for (int y = 0; y < tileSize; y++)
		{
			byte row = pattern[y];
			for (int x = 0; x < tileSize; x++)
			{
				bool isForeground = (row & (0x80 >> x)) != 0;
				tileBitmap.SetPixel(x, y, isForeground ? fg : bg);
			}
		}

		using var image = SKImage.FromBitmap(tileBitmap);
		paint.Shader = SKShader.CreateImage(image, SKShaderTileMode.Repeat, SKShaderTileMode.Repeat);
		return paint;
	}
}
