using System.IO;

namespace SkiaSharp.Extended.Tests
{
	public abstract class SKPixelComparerTestBase
	{
		protected const string jpgFirst = "First.jpg";
		protected const string pngFirst = "First.png";
		protected const string jpgSecond = "Second.jpg";
		protected const string pngSecond = "Second.png";
		protected const string jpgMask = "MaskJpg.png";
		protected const string pngMask = "MaskPng.png";

		public static readonly string BaseImages = Path.Combine("images", "Comparer");

		public static string GetImagePath(string filename) =>
			Path.Combine(BaseImages, filename);

		protected static SKImage CreateTestImage(SKColor color, int width = 5, int height = 5)
		{
			using var surface = SKSurface.Create(new SKImageInfo(width, height));
			surface.Canvas.Clear(color);
			return surface.Snapshot();
		}

		protected static SKBitmap CreateTestBitmap(SKColor color, int width = 5, int height = 5)
		{
			var bmp = new SKBitmap(new SKImageInfo(width, height));
			bmp.Erase(color);
			return bmp;
		}

		protected static void SaveOutputDiff(SKImage first, SKImage second)
		{
			using var img = SKPixelComparer.GenerateDifferenceMask(first, second);
			using var data = img.Encode(SKEncodedImageFormat.Png, 100);
			using (var str = File.Create(GetImagePath("diff.png")))
				data.SaveTo(str);
		}

		protected static SKBitmap GetNormalizedBitmap(SKImage image)
		{
			var bitmap = new SKBitmap(new SKImageInfo(image.Width, image.Height, SKColorType.Bgra8888));
			using (var canvas = new SKCanvas(bitmap))
				canvas.DrawImage(image, 0, 0);
			return bitmap;
		}
	}
}
