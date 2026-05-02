namespace SkiaSharp.Drawing.Tests;

using System.Drawing;
using System.Drawing.Drawing2D;

/// <summary>
/// Tests that verify actual RENDERING results by checking pixel values after drawing.
/// These are the highest-value tests — they catch rendering bugs that property tests miss.
/// </summary>
public class DrawingResultTests
{
	private static (Bitmap bmp, Graphics g) CreateCanvas(int w = 100, int h = 100)
	{
		var bmp = new Bitmap(w, h);
		var g = Graphics.FromImage(bmp);
		g.SmoothingMode = SmoothingMode.None;
		g.Clear(Color.White);
		return (bmp, g);
	}

	private static void AssertPixelIs(Bitmap bmp, int x, int y, Color expected, int tolerance = 5)
	{
		var actual = bmp.GetPixel(x, y);
		Assert.True(
			Math.Abs(actual.R - expected.R) <= tolerance &&
			Math.Abs(actual.G - expected.G) <= tolerance &&
			Math.Abs(actual.B - expected.B) <= tolerance &&
			Math.Abs(actual.A - expected.A) <= tolerance,
			$"Pixel ({x},{y}): expected ({expected.R},{expected.G},{expected.B},{expected.A}), got ({actual.R},{actual.G},{actual.B},{actual.A})");
	}

	// === FILL COVERAGE ===

	[Fact]
	public void FillRectangle_AllInteriorPixelsFilled()
	{
		var (bmp, g) = CreateCanvas(50, 50);
		using (g) g.FillRectangle(Brushes.Red, 10, 10, 30, 30);
		for (int y = 10; y < 40; y++)
			for (int x = 10; x < 40; x++)
				AssertPixelIs(bmp, x, y, Color.Red);
		AssertPixelIs(bmp, 5, 5, Color.White);
		AssertPixelIs(bmp, 45, 45, Color.White);
		bmp.Dispose();
	}

	[Fact]
	public void FillRectangle_ZeroSize_NoPixelsModified()
	{
		var (bmp, g) = CreateCanvas(20, 20);
		using (g) g.FillRectangle(Brushes.Red, 5, 5, 0, 0);
		AssertPixelIs(bmp, 5, 5, Color.White);
		bmp.Dispose();
	}

	[Fact]
	public void FillEllipse_CenterIsFilled_CornersAreNot()
	{
		var (bmp, g) = CreateCanvas(100, 100);
		using (g) g.FillEllipse(Brushes.Blue, 10, 10, 80, 80);
		AssertPixelIs(bmp, 50, 50, Color.Blue);
		AssertPixelIs(bmp, 11, 11, Color.White);
		AssertPixelIs(bmp, 89, 89, Color.White);
		bmp.Dispose();
	}

	// === LINE RENDERING ===

	[Fact]
	public void DrawLine_ThickPen_StrokeWidthCorrect()
	{
		var (bmp, g) = CreateCanvas();
		using var pen = new Pen(Color.Black, 5);
		using (g) g.DrawLine(pen, 10, 50, 90, 50);
		AssertPixelIs(bmp, 50, 50, Color.Black);
		AssertPixelIs(bmp, 50, 48, Color.Black);
		AssertPixelIs(bmp, 50, 52, Color.Black);
		AssertPixelIs(bmp, 50, 46, Color.White);
		AssertPixelIs(bmp, 50, 54, Color.White);
		bmp.Dispose();
	}

	[Fact]
	public void DrawLine_DashedPen_HasGaps()
	{
		var (bmp, g) = CreateCanvas();
		using var pen = new Pen(Color.Black, 1) { DashStyle = DashStyle.Dash };
		using (g) g.DrawLine(pen, 0, 50, 99, 50);
		int colored = 0, uncolored = 0;
		for (int x = 0; x < 100; x++)
		{
			var p = bmp.GetPixel(x, 50);
			if (p.R < 128) colored++; else uncolored++;
		}
		Assert.True(colored > 10, $"Expected some colored pixels, got {colored}");
		Assert.True(uncolored > 5, $"Expected some gaps in dash, got {uncolored} uncolored");
		bmp.Dispose();
	}

	// === TRANSFORMS ===

	[Fact]
	public void TranslateTransform_ShiftsDrawing()
	{
		var (bmp, g) = CreateCanvas();
		using (g)
		{
			g.TranslateTransform(20, 20);
			g.FillRectangle(Brushes.Red, 0, 0, 10, 10);
		}
		AssertPixelIs(bmp, 0, 0, Color.White);
		AssertPixelIs(bmp, 25, 25, Color.Red);
		bmp.Dispose();
	}

	[Fact]
	public void ScaleTransform_ScalesDrawing()
	{
		var (bmp, g) = CreateCanvas();
		using (g)
		{
			g.ScaleTransform(2, 2);
			g.FillRectangle(Brushes.Green, 5, 5, 10, 10);
		}
		AssertPixelIs(bmp, 10, 10, Color.Green);
		AssertPixelIs(bmp, 29, 29, Color.Green);
		AssertPixelIs(bmp, 31, 31, Color.White);
		bmp.Dispose();
	}

	[Fact]
	public void SaveRestore_TransformIsRestored()
	{
		var (bmp, g) = CreateCanvas();
		using (g)
		{
			var state = g.Save();
			g.TranslateTransform(50, 50);
			g.Restore(state);
			g.FillRectangle(Brushes.Red, 0, 0, 5, 5);
		}
		AssertPixelIs(bmp, 2, 2, Color.Red);
		AssertPixelIs(bmp, 52, 52, Color.White);
		bmp.Dispose();
	}

	[Fact]
	public void SaveRestore_SmoothingModeIsRestored()
	{
		var (bmp, g) = CreateCanvas();
		using (g)
		{
			g.SmoothingMode = SmoothingMode.None;
			var state = g.Save();
			g.SmoothingMode = SmoothingMode.AntiAlias;
			Assert.Equal(SmoothingMode.AntiAlias, g.SmoothingMode);
			g.Restore(state);
			Assert.Equal(SmoothingMode.None, g.SmoothingMode);
		}
		bmp.Dispose();
	}

	// === CLIPPING ===

	[Fact]
	public void SetClip_Rectangle_ClipsDrawing()
	{
		var (bmp, g) = CreateCanvas();
		using (g)
		{
			g.SetClip(new Rectangle(20, 20, 60, 60));
			g.FillRectangle(Brushes.Red, 0, 0, 100, 100);
		}
		AssertPixelIs(bmp, 50, 50, Color.Red);
		AssertPixelIs(bmp, 5, 5, Color.White);
		AssertPixelIs(bmp, 95, 95, Color.White);
		bmp.Dispose();
	}

	// === COMPOSITING ===

	[Fact]
	public void CompositingMode_SourceOver_BlendsAlpha()
	{
		var (bmp, g) = CreateCanvas();
		using (g)
		{
			g.CompositingMode = CompositingMode.SourceOver;
			g.FillRectangle(Brushes.Red, 0, 0, 100, 100);
			using var semiBrush = new SolidBrush(Color.FromArgb(128, 0, 0, 255));
			g.FillRectangle(semiBrush, 0, 0, 100, 100);
		}
		var p = bmp.GetPixel(50, 50);
		Assert.True(p.R > 50 && p.R < 200, $"Expected blended red, got R={p.R}");
		Assert.True(p.B > 50 && p.B < 200, $"Expected blended blue, got B={p.B}");
		bmp.Dispose();
	}

	// === GRADIENT ===

	[Fact]
	public void LinearGradientBrush_StartsAndEndsWithCorrectColors()
	{
		var (bmp, g) = CreateCanvas();
		using var brush = new LinearGradientBrush(
			new Point(0, 0), new Point(99, 0), Color.Red, Color.Blue);
		using (g) g.FillRectangle(brush, 0, 0, 100, 100);
		var left = bmp.GetPixel(2, 50);
		Assert.True(left.R > 200, $"Left should be red, got R={left.R}");
		var right = bmp.GetPixel(97, 50);
		Assert.True(right.B > 200, $"Right should be blue, got B={right.B}");
		var mid = bmp.GetPixel(50, 50);
		Assert.True(mid.R > 50 && mid.B > 50, $"Mid should be blended: R={mid.R} B={mid.B}");
		bmp.Dispose();
	}

	// === DRAWSTRING ===

	[Fact]
	public void DrawString_RendersVisiblePixels()
	{
		var (bmp, g) = CreateCanvas();
		using var font = new Font("Arial", 20);
		using (g) g.DrawString("X", font, Brushes.Black, 10, 10);
		bool anyNonWhite = false;
		for (int y = 10; y < 40 && !anyNonWhite; y++)
			for (int x = 10; x < 40 && !anyNonWhite; x++)
			{
				var p = bmp.GetPixel(x, y);
				if (p.R < 200) { anyNonWhite = true; }
			}
		Assert.True(anyNonWhite, "DrawString should produce visible pixels");
		bmp.Dispose();
	}

	// === DRAWIMAGE ===

	[Fact]
	public void DrawImage_CopiesPixels()
	{
		using var src = new Bitmap(10, 10);
		src.SetPixel(5, 5, Color.Red);
		var (bmp, g) = CreateCanvas(50, 50);
		using (g) g.DrawImage(src, 20, 20);
		AssertPixelIs(bmp, 25, 25, Color.Red);
		AssertPixelIs(bmp, 0, 0, Color.White);
		bmp.Dispose();
	}

	[Fact]
	public void DrawImage_WithScale_ChangesSize()
	{
		using var src = new Bitmap(10, 10);
		using (var sg = Graphics.FromImage(src)) sg.Clear(Color.Green);
		var (bmp, g) = CreateCanvas(50, 50);
		using (g) g.DrawImage(src, new Rectangle(0, 0, 30, 30));
		AssertPixelIs(bmp, 15, 15, Color.Green);
		AssertPixelIs(bmp, 35, 35, Color.White);
		bmp.Dispose();
	}

	// === GRAPHICSPATH ===

	[Fact]
	public void FillPath_Rectangle_MatchesFillRectangle()
	{
		var (bmp1, g1) = CreateCanvas(50, 50);
		using (g1) g1.FillRectangle(Brushes.Red, 10, 10, 30, 30);

		var (bmp2, g2) = CreateCanvas(50, 50);
		using var path = new GraphicsPath();
		path.AddRectangle(new RectangleF(10, 10, 30, 30));
		using (g2) g2.FillPath(Brushes.Red, path);

		for (int y = 0; y < 50; y++)
			for (int x = 0; x < 50; x++)
			{
				var p1 = bmp1.GetPixel(x, y);
				var p2 = bmp2.GetPixel(x, y);
				Assert.True(p1.ToArgb() == p2.ToArgb(),
					$"Pixel ({x},{y}) differs: FillRectangle=({p1.R},{p1.G},{p1.B}) vs FillPath=({p2.R},{p2.G},{p2.B})");
			}
		bmp1.Dispose(); bmp2.Dispose();
	}
}
