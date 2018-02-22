using System;
using System.IO;
using NUnit.Framework;
using System.Xml.Linq;

namespace SkiaSharp.Extended.Svg.Tests
{
	public class SKSvgTest : SKTest
	{
		[Test]
		public void LoadSvgCanvasSize()
		{
			var path = Path.Combine(PathToImages, "logos.svg");

			var svg = new SKSvg();
			svg.Load(path);

			Assert.AreEqual(new SKSize(300, 300), svg.CanvasSize);
		}

		[Test]
		public void LoadSvgCustomCanvasSize()
		{
			var path = Path.Combine(PathToImages, "logos.svg");

			var svg = new SKSvg(new SKSize(150, 150));
			svg.Load(path);

			Assert.AreEqual(new SKSize(150, 150), svg.CanvasSize);
		}

		[Test]
		public void SvgLoadsToBitmap()
		{
			var path = Path.Combine(PathToImages, "logos.svg");
			var background = (SKColor)0xfff8f8f8;

			var bmp = LoadSvgBitmap(path);

			Assert.AreEqual(background, bmp.GetPixel(0, 0));
		}

		[Test]
		public void SvgRespectsBaselineShift()
		{
			var path = Path.Combine(PathToImages, "baselines.svg");
			var background = (SKColor)0xffffffff;
			var fill = SKColors.Black;

			var bmp = LoadSvgBitmap(path, background);

			Assert.AreEqual(background, bmp.GetPixel(25, 25));

			// test for the explicit positioning, the others aren't supported yet
			Assert.AreEqual(fill, bmp.GetPixel(370, 40));
		}

		[Test]
		public void SvgLoadsLocalEmbeddedImages()
		{
			var path = Path.Combine(PathToImages, "embedded.svg");
			var background = (SKColor)0xffffffff;
			var fill = (SKColor)0xff3498db;

			var bmp = LoadSvgBitmap(path, background);

			Assert.AreEqual(background, bmp.GetPixel(25, 25));
			Assert.AreEqual(fill, bmp.GetPixel(35, 50));
		}

		[Test]
		public void SvgLoadsPolygon()
		{
			var path = Path.Combine(PathToImages, "sketch.svg");
			var background = (SKColor)0xfff8f8f8;
			var fill = (SKColor)0xFF4990E2;

			var bmp = LoadSvgBitmap(path, background);

			Assert.AreEqual(fill, bmp.GetPixel(bmp.Width / 2, bmp.Height / 2));
			Assert.AreEqual(background, bmp.GetPixel(5, 5));
		}

		[Test]
		public void SvgLoadsDashes()
		{
			var path = Path.Combine(PathToImages, "dashes.svg");

			var bmp = LoadSvgBitmap(path, SKColors.White);

			Assert.AreEqual(SKColors.Black, bmp.GetPixel(10 + 3, 20));
			Assert.AreEqual(SKColors.Black, bmp.GetPixel(10 + 7, 20));
			Assert.AreEqual(SKColors.White, bmp.GetPixel(10 + 13, 20));

			Assert.AreEqual(SKColors.Black, bmp.GetPixel(10 + 3, 40));
			Assert.AreEqual(SKColors.White, bmp.GetPixel(10 + 7, 40));
			Assert.AreEqual(SKColors.Black, bmp.GetPixel(10 + 13, 40));

			Assert.AreEqual(SKColors.White, bmp.GetPixel(10 + 3, 60));
			Assert.AreEqual(SKColors.Black, bmp.GetPixel(10 + 7, 60));
			Assert.AreEqual(SKColors.Black, bmp.GetPixel(10 + 13, 60));

			Assert.AreEqual(SKColors.Black, bmp.GetPixel(10 + 3, 80));
			Assert.AreEqual(SKColors.Black, bmp.GetPixel(10 + 7, 80));
			Assert.AreEqual(SKColors.White, bmp.GetPixel(10 + 13, 80));
		}

		[Test]
		public void SvgCanvasCreatesValidDrawing()
		{
			using (var stream = new MemoryStream())
			{
				// draw the SVG
				using (var skStream = new SKManagedWStream(stream, false))
				using (var writer = new SKXmlStreamWriter(skStream))
				using (var canvas = SKSvgCanvas.Create(SKRect.Create(200, 150), writer))
				{
					var rectPaint = new SKPaint { Color = SKColors.Blue, Style = SKPaintStyle.Fill };
					canvas.DrawRect(SKRect.Create(50, 70, 100, 30), rectPaint);

					var circlePaint = new SKPaint { Color = SKColors.Red, Style = SKPaintStyle.Fill };
					canvas.DrawOval(SKRect.Create(50, 70, 100, 30), circlePaint);

					skStream.Flush();
				}

				// reset the sream
				stream.Position = 0;

				// read the SVG
				var xdoc = XDocument.Load(stream);
				var svg = xdoc.Root;

				var ns = (XNamespace)"http://www.w3.org/2000/svg";

				Assert.AreEqual(ns, svg.GetDefaultNamespace());
				Assert.AreEqual("200", svg.Attribute("width").Value);
				Assert.AreEqual("150", svg.Attribute("height").Value);

				var rect = svg.Element(ns + "rect");
				Assert.AreEqual("rgb(0,0,255)", rect.Attribute("fill").Value);
				Assert.AreEqual("50", rect.Attribute("x").Value);
				Assert.AreEqual("70", rect.Attribute("y").Value);
				Assert.AreEqual("100", rect.Attribute("width").Value);
				Assert.AreEqual("30", rect.Attribute("height").Value);

				var ellipse = svg.Element(ns + "ellipse");
				Assert.AreEqual("rgb(255,0,0)", ellipse.Attribute("fill").Value);
				Assert.AreEqual("100", ellipse.Attribute("cx").Value);
				Assert.AreEqual("85", ellipse.Attribute("cy").Value);
				Assert.AreEqual("50", ellipse.Attribute("rx").Value);
				Assert.AreEqual("15", ellipse.Attribute("ry").Value);
			}
		}

		[Test]
		public void SvgCanUnderstandColorNames()
		{
			var svg =
@"<svg xmlns=""http://www.w3.org/2000/svg"" version=""1.1""
    x=""0px"" y=""0px"" width=""100"" height=""100"" viewBox=""0 0 100 100"">
  <rect style=""fill:lime"" width=""100"" height=""100"" x=""0"" y=""0"" />
</svg>";

			var bmp = CreateSvgBitmap(svg);

			Assert.AreEqual(SKColors.Lime, bmp.GetPixel(bmp.Width / 2, bmp.Height / 2));
		}

		[Test]
		public void SvgCanUnderstandRgbColors()
		{
			var svg =
@"<svg xmlns=""http://www.w3.org/2000/svg"" version=""1.1""
    x=""0px"" y=""0px"" width=""100"" height=""100"" viewBox=""0 0 100 100"">
  <rect style=""fill:rgb(0,255,0)"" width=""100"" height=""100"" x=""0"" y=""0"" />
</svg>";

			var bmp = CreateSvgBitmap(svg);

			Assert.AreEqual(SKColors.Lime, bmp.GetPixel(bmp.Width / 2, bmp.Height / 2));
		}

		[Test]
		public void RectWithSingleCornerRadius()
		{
			var check = new Action<string>(svg =>
			{
				var bmp = CreateSvgBitmap(svg, SKColors.White);

				Assert.AreEqual(SKColors.White, bmp.GetPixel(3, 3));
				Assert.AreEqual(SKColors.White, bmp.GetPixel(97, 3));
				Assert.AreEqual(SKColors.White, bmp.GetPixel(3, 97));
				Assert.AreEqual(SKColors.White, bmp.GetPixel(97, 97));
			});

			check(
@"<svg xmlns=""http://www.w3.org/2000/svg"" version=""1.1""
    x=""0px"" y=""0px"" width=""100"" height=""100"" viewBox=""0 0 100 100"">
  <rect style=""fill:red"" width=""100"" height=""100"" x=""0"" y=""0"" rx=""20"" ry=""20"" />
</svg>");

			check(
@"<svg xmlns=""http://www.w3.org/2000/svg"" version=""1.1""
    x=""0px"" y=""0px"" width=""100"" height=""100"" viewBox=""0 0 100 100"">
  <rect style=""fill:red"" width=""100"" height=""100"" x=""0"" y=""0"" rx=""20"" />
</svg>");

			check(
@"<svg xmlns=""http://www.w3.org/2000/svg"" version=""1.1""
    x=""0px"" y=""0px"" width=""100"" height=""100"" viewBox=""0 0 100 100"">
  <rect style=""fill:red"" width=""100"" height=""100"" x=""0"" y=""0"" ry=""20"" />
</svg>");
		}

		[Test]
		public void SvgCanUnderstandPolygon()
		{
			var svg =
@"<svg xmlns=""http://www.w3.org/2000/svg"" version=""1.1""
    x=""0px"" y=""0px"" width=""100"" height=""100"" viewBox=""0 0 100 100"">
  <polygon points=""20,70 50,20 80,70"" style=""fill:white; stroke:black; stroke-width:10""/>
</svg>";

			var bmp = CreateSvgBitmap(svg);

			Assert.AreEqual(SKColors.Black, bmp.GetPixel(50, 70));
		}

		[Test]
		public void SvgCanUnderstandPolyline()
		{
			var svg =
@"<svg xmlns=""http://www.w3.org/2000/svg"" version=""1.1""
    x=""0px"" y=""0px"" width=""100"" height=""100"" viewBox=""0 0 100 100"">
  <polyline points=""20,70 50,20 80,70"" style=""fill:white; stroke:black; stroke-width:10""/>
</svg>";

			var bmp = CreateSvgBitmap(svg, SKColors.Green);

			Assert.AreEqual(SKColors.Green, bmp.GetPixel(50, 70));
		}

		[Test]
		public void SvgStylesAreAlsoUsed()
		{
			var path = Path.Combine(PathToImages, "issues-22.svg");

			var svg = new SKSvg();
			svg.Load(path);
			var bmp = CreateBitmap(svg, SKColors.White);

			Assert.AreEqual(SKColors.White, bmp.GetPixel(bmp.Width / 2, bmp.Height / 2));
		}

		[Test]
		public void SvgCanReadFileWithNoXLinkNamespacePrefix()
		{
			var path = Path.Combine(PathToImages, "issues-8.svg");
			var background = (SKColor)0x000000;
			var fill = (SKColor)0xFFE3E6E8;

			var svg = new SKSvg();
			svg.Load(path);
			var bmp = CreateBitmap(svg, background);

			Assert.AreEqual(fill, bmp.GetPixel(bmp.Width / 2, bmp.Height / 2));
			Assert.AreEqual(background, bmp.GetPixel(5, 5));
		}

		[Test]
		public void SvgCanReadFileWithNoXLinkNamespacePrefixFromStreams()
		{
			var path = Path.Combine(PathToImages, "issues-8.svg");
			var background = (SKColor)0x000000;
			var fill = (SKColor)0xFFE3E6E8;

			var svg = new SKSvg();
			using (var stream = File.OpenRead(path))
			{
				svg.Load(stream);
			}

			var bmp = CreateBitmap(svg, background);

			Assert.AreEqual(fill, bmp.GetPixel(bmp.Width / 2, bmp.Height / 2));
			Assert.AreEqual(background, bmp.GetPixel(5, 5));
		}


		[Test]
		public void SvgRespectsClipPath()
		{
			var path = Path.Combine(PathToImages, "clipping.svg");
			var background = (SKColor)0xffffffff;
			var fill = (SKColor)0xff000000;

			var svg = new SKSvg();
			svg.Load(path);

			var bmp = new SKBitmap((int)svg.CanvasSize.Width, (int)svg.CanvasSize.Height);
			var canvas = new SKCanvas(bmp);
			canvas.Clear(background);

			canvas.DrawPicture(svg.Picture);
			canvas.Flush();

			for (int x = 1; x < 20; x++)
			{
				for (int y = 1; y < 20; y++)
				{
					Assert.AreEqual(fill, bmp.GetPixel(x, y));
					Assert.AreEqual(background, bmp.GetPixel(x + 20, y + 20));
				}
			}
		}

		[Test]
		public void SvgCanReadFileWithDTD()
		{
			var path = Path.Combine(PathToImages, "dtd.svg");
			var bmp = LoadSvgBitmap(path, SKColors.Red);

			Assert.AreEqual(SKColors.Black, bmp.GetPixel(50, 50));
		}

		private static SKBitmap LoadSvgBitmap(string svgPath, SKColor? background = null)
		{
			// open the SVG
			var svg = new SKSvg();
			svg.Load(svgPath);

			return CreateBitmap(svg, background);
		}

		private static SKBitmap CreateSvgBitmap(string svgData, SKColor? background = null)
		{
			// open the SVG
			var svg = new SKSvg();
			using (var stream = new MemoryStream())
			using (var writer = new StreamWriter(stream))
			{
				writer.Write(svgData);
				writer.Flush();
				stream.Position = 0;

				svg.Load(stream);
			}

			return CreateBitmap(svg, background);
		}

		private static SKBitmap CreateBitmap(SKSvg svg, SKColor? background = null)
		{
			// create and draw the bitmap
			var bmp = new SKBitmap((int)svg.CanvasSize.Width, (int)svg.CanvasSize.Height);
			using (var canvas = new SKCanvas(bmp))
			{
				canvas.Clear(background ?? SKColors.Transparent);
				canvas.DrawPicture(svg.Picture);
				canvas.Flush();
			}

			return bmp;
		}

		private static void SaveBitmap(SKBitmap bitmap, string path = "output.png")
		{
			using (var file = File.OpenWrite(Path.Combine(PathToImages, path)))
			using (var stream = new SKManagedWStream(file))
			{
				bitmap.Encode(stream, SKEncodedImageFormat.Png, 100);
			}
		}
	}
}
