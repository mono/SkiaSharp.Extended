using System;
using System.IO;
using Xunit;

namespace SkiaSharp.Extended.Controls.Tests
{
	public class SKConfettiShapeTest
	{
		private static readonly bool SaveTestBitmaps = false;

		[Fact]
		public void EmptyDrawIsEqual()
		{
			EqualDrawing(new SKSizeI(24, 24),
				canvas => { },
				canvas => { });
		}

		[Fact]
		public void DrawsSquareCorrectly()
		{
			using var paint = new SKPaint();

			var shape = new SKConfettiSquareShape();

			EqualDrawing(new SKSizeI(24, 24),
				canvas =>
				{
					canvas.DrawRect(2, 2, 20, 20, paint);
				},
				canvas =>
				{
					canvas.Translate(12, 12);
					shape.Draw(canvas, paint, 20);
				});
		}

		[Fact]
		public void DrawsCircleCorrectly()
		{
			using var paint = new SKPaint();

			var shape = new SKConfettiCircleShape();

			EqualDrawing(new SKSizeI(24, 24),
				canvas =>
				{
					canvas.DrawOval(12, 12, 10, 10, paint);
				},
				canvas =>
				{
					canvas.Translate(12, 12);
					shape.Draw(canvas, paint, 20);
				});
		}

		[Fact]
		public void DrawsOvalCorrectly()
		{
			using var paint = new SKPaint();

			var shape = new SKConfettiOvalShape(0.5);

			EqualDrawing(new SKSizeI(24, 24),
				canvas =>
				{
					canvas.DrawOval(12, 12, 10, 5, paint);
				},
				canvas =>
				{
					canvas.Translate(12, 12);
					shape.Draw(canvas, paint, 20);
				});
		}

		[Fact]
		public void DrawsRectCorrectly()
		{
			using var paint = new SKPaint();

			var shape = new SKConfettiRectShape(0.5);

			EqualDrawing(new SKSizeI(24, 24),
				canvas =>
				{
					canvas.DrawRect(2, 7, 20, 10, paint);
				},
				canvas =>
				{
					canvas.Translate(12, 12);
					shape.Draw(canvas, paint, 20);
				});
		}

		private void EqualDrawing(SKSizeI size, Action<SKCanvas> expected, Action<SKCanvas> actual)
		{
			using var expectedBitmap = new SKBitmap(size.Width, size.Height);
			using var expectedCanvas = new SKCanvas(expectedBitmap);
			expectedCanvas.Clear(SKColors.Transparent);
			expected(expectedCanvas);
			SaveBitmap(expectedBitmap, "expected.png");

			using var actualBitmap = new SKBitmap(size.Width, size.Height);
			using var actualCanvas = new SKCanvas(actualBitmap);
			actualCanvas.Clear(SKColors.Transparent);
			actual(actualCanvas);
			SaveBitmap(actualBitmap, "actual.png");

			Assert.Equal(expectedBitmap.Pixels, actualBitmap.Pixels);
		}

		private void SaveBitmap(SKBitmap bitmap, string filename)
		{
			if (!SaveTestBitmaps)
				return;

			using var stream = File.Create(filename);
			bitmap.Encode(stream, SKEncodedImageFormat.Png, 100);
		}
	}
}
