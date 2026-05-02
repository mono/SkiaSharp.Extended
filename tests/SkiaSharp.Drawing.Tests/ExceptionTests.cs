namespace SkiaSharp.Drawing.Tests;

using System.Drawing;
using System.Drawing.Drawing2D;

/// <summary>
/// Tests that verify correct exception behavior matching System.Drawing.Common.
/// </summary>
public class ExceptionTests
{
	// === NULL ARGUMENT EXCEPTIONS ===

	[Fact]
	public void DrawLine_NullPen_Throws()
	{
		using var bmp = new Bitmap(10, 10);
		using var g = Graphics.FromImage(bmp);
		Assert.Throws<ArgumentNullException>(() => g.DrawLine(null!, 0, 0, 5, 5));
	}

	[Fact]
	public void DrawRectangle_NullPen_Throws()
	{
		using var bmp = new Bitmap(10, 10);
		using var g = Graphics.FromImage(bmp);
		Assert.Throws<ArgumentNullException>(() => g.DrawRectangle(null!, 0, 0, 5, 5));
	}

	[Fact]
	public void FillRectangle_NullBrush_Throws()
	{
		using var bmp = new Bitmap(10, 10);
		using var g = Graphics.FromImage(bmp);
		Assert.Throws<ArgumentNullException>(() => g.FillRectangle(null!, 0, 0, 5, 5));
	}

	[Fact]
	public void DrawEllipse_NullPen_Throws()
	{
		using var bmp = new Bitmap(10, 10);
		using var g = Graphics.FromImage(bmp);
		Assert.Throws<ArgumentNullException>(() => g.DrawEllipse(null!, 0, 0, 5, 5));
	}

	[Fact]
	public void FillEllipse_NullBrush_Throws()
	{
		using var bmp = new Bitmap(10, 10);
		using var g = Graphics.FromImage(bmp);
		Assert.Throws<ArgumentNullException>(() => g.FillEllipse(null!, 0, 0, 5, 5));
	}

	[Fact]
	public void DrawString_NullString_Throws()
	{
		using var bmp = new Bitmap(10, 10);
		using var g = Graphics.FromImage(bmp);
		using var font = new Font("Arial", 12);
		Assert.Throws<ArgumentNullException>(() => g.DrawString(null!, font, Brushes.Black, 0, 0));
	}

	[Fact]
	public void DrawString_NullFont_Throws()
	{
		using var bmp = new Bitmap(10, 10);
		using var g = Graphics.FromImage(bmp);
		Assert.Throws<ArgumentNullException>(() => g.DrawString("text", null!, Brushes.Black, 0, 0));
	}

	[Fact]
	public void DrawString_NullBrush_Throws()
	{
		using var bmp = new Bitmap(10, 10);
		using var g = Graphics.FromImage(bmp);
		using var font = new Font("Arial", 12);
		Assert.Throws<ArgumentNullException>(() => g.DrawString("text", font, null!, 0, 0));
	}

	[Fact]
	public void Graphics_FromImage_NullImage_Throws()
	{
		Assert.Throws<ArgumentNullException>(() => Graphics.FromImage(null!));
	}

	// === DISPOSED OBJECT EXCEPTIONS ===

	[Fact]
	public void Graphics_DrawLine_AfterDispose_Throws()
	{
		using var bmp = new Bitmap(10, 10);
		var g = Graphics.FromImage(bmp);
		g.Dispose();
		using var pen = new Pen(Color.Red);
		Assert.Throws<ObjectDisposedException>(() => g.DrawLine(pen, 0, 0, 5, 5));
	}

	[Fact]
	public void Font_Properties_AfterDispose_Throws()
	{
		var font = new Font("Arial", 12);
		font.Dispose();
		Assert.Throws<ObjectDisposedException>(() => _ = font.Name);
	}

	// === BITMAP VALIDATION ===

	[Fact]
	public void Bitmap_ZeroDimensions_Throws()
	{
		Assert.Throws<ArgumentException>(() => new Bitmap(0, 0));
	}

	[Fact]
	public void Bitmap_NegativeDimensions_Throws()
	{
		Assert.Throws<ArgumentException>(() => new Bitmap(-1, -1));
	}

	[Fact]
	public void Bitmap_GetPixel_OutOfBounds_Throws()
	{
		using var bmp = new Bitmap(10, 10);
		Assert.Throws<ArgumentException>(() => bmp.GetPixel(-1, 0));
		Assert.Throws<ArgumentException>(() => bmp.GetPixel(0, -1));
		Assert.Throws<ArgumentException>(() => bmp.GetPixel(10, 0));
		Assert.Throws<ArgumentException>(() => bmp.GetPixel(0, 10));
	}

	// === PEN VALIDATION ===

	[Fact]
	public void Pen_NegativeWidth_Throws()
	{
		Assert.Throws<ArgumentException>(() => new Pen(Color.Red, -1));
	}

	[Fact]
	public void SystemPen_Mutate_Throws()
	{
		var pen = Pens.Red;
		Assert.Throws<ArgumentException>(() => pen.Color = Color.Blue);
	}

	[Fact]
	public void SystemBrush_Mutate_Throws()
	{
		var brush = (SolidBrush)Brushes.Red;
		Assert.Throws<ArgumentException>(() => brush.Color = Color.Blue);
	}

	// === EMPTY ARRAY EXCEPTIONS ===

	[Fact]
	public void DrawLines_EmptyArray_Throws()
	{
		using var bmp = new Bitmap(10, 10);
		using var g = Graphics.FromImage(bmp);
		using var pen = new Pen(Color.Red);
		Assert.Throws<ArgumentException>(() => g.DrawLines(pen, Array.Empty<PointF>()));
	}

	[Fact]
	public void DrawPolygon_EmptyArray_Throws()
	{
		using var bmp = new Bitmap(10, 10);
		using var g = Graphics.FromImage(bmp);
		using var pen = new Pen(Color.Red);
		Assert.Throws<ArgumentException>(() => g.DrawPolygon(pen, Array.Empty<PointF>()));
	}

	// === BITMAP CONSTRUCTOR NULL CHECK ===

	[Fact]
	public void Bitmap_WithNullGraphics_Throws()
	{
		Assert.Throws<ArgumentNullException>(() => new Bitmap(10, 10, (Graphics)null!));
	}

	// === DRAWSTRING OVERLOAD WITH RECTANGLE ===

	[Fact]
	public void DrawString_Rectangle_NullString_Throws()
	{
		using var bmp = new Bitmap(10, 10);
		using var g = Graphics.FromImage(bmp);
		using var font = new Font("Arial", 12);
		Assert.Throws<ArgumentNullException>(() => g.DrawString(null!, font, Brushes.Black, new RectangleF(0, 0, 10, 10)));
	}

	[Fact]
	public void DrawString_Rectangle_NullFont_Throws()
	{
		using var bmp = new Bitmap(10, 10);
		using var g = Graphics.FromImage(bmp);
		Assert.Throws<ArgumentNullException>(() => g.DrawString("text", null!, Brushes.Black, new RectangleF(0, 0, 10, 10)));
	}

	[Fact]
	public void DrawString_Rectangle_NullBrush_Throws()
	{
		using var bmp = new Bitmap(10, 10);
		using var g = Graphics.FromImage(bmp);
		using var font = new Font("Arial", 12);
		Assert.Throws<ArgumentNullException>(() => g.DrawString("text", font, null!, new RectangleF(0, 0, 10, 10)));
	}

	// === PEN IMMUTABLE PROPERTY CHECKS ===

	[Fact]
	public void SystemPen_Width_Throws()
	{
		var pen = Pens.Blue;
		Assert.Throws<ArgumentException>(() => pen.Width = 2);
	}

	[Fact]
	public void SystemPen_DashStyle_Throws()
	{
		var pen = Pens.Blue;
		Assert.Throws<ArgumentException>(() => pen.DashStyle = DashStyle.Dot);
	}

	// === PEN NEGATIVE WIDTH SETTER ===

	[Fact]
	public void Pen_SetNegativeWidth_Throws()
	{
		using var pen = new Pen(Color.Red, 1);
		Assert.Throws<ArgumentException>(() => pen.Width = -5);
	}

	// === FONT DISPOSED CHECKS ===

	[Fact]
	public void Font_Height_AfterDispose_Throws()
	{
		var font = new Font("Arial", 12);
		font.Dispose();
		Assert.Throws<ObjectDisposedException>(() => _ = font.Height);
	}

	[Fact]
	public void Font_GetHeight_AfterDispose_Throws()
	{
		var font = new Font("Arial", 12);
		font.Dispose();
		Assert.Throws<ObjectDisposedException>(() => font.GetHeight());
	}

	[Fact]
	public void Font_Clone_AfterDispose_Throws()
	{
		var font = new Font("Arial", 12);
		font.Dispose();
		Assert.Throws<ObjectDisposedException>(() => font.Clone());
	}

	[Fact]
	public void Font_ToString_AfterDispose_Throws()
	{
		var font = new Font("Arial", 12);
		font.Dispose();
		Assert.Throws<ObjectDisposedException>(() => font.ToString());
	}
}
