using System;
using System.IO;
using System.Xml.Linq;
using Xunit;

namespace SkiaSharp.Extended.Svg.Tests
{
	public class SKSvgTest : SKTest
	{
		[Fact]
		public void LoadSvgCanvasSize()
		{
			var path = Path.Combine(PathToImages, "logos.svg");

			var svg = new SKSvg();
			svg.Load(path);

			Assert.Equal(new SKSize(300, 300), svg.CanvasSize);
		}

		[Fact]
		public void LoadSvgCustomCanvasSize()
		{
			var path = Path.Combine(PathToImages, "logos.svg");

			var svg = new SKSvg(new SKSize(150, 150));
			svg.Load(path);

			Assert.Equal(new SKSize(150, 150), svg.CanvasSize);
		}

		[Fact]
		public void SvgLoadsToBitmap()
		{
			var path = Path.Combine(PathToImages, "logos.svg");
			var background = (SKColor)0xfff8f8f8;

			var bmp = LoadSvgBitmap(path);

			Assert.Equal(background, bmp.GetPixel(0, 0));
		}

		[Fact]
		public void SvgRespectsBaselineShift()
		{
			var path = Path.Combine(PathToImages, "baselines.svg");
			var background = (SKColor)0xffffffff;
			var fill = SKColors.Black;

			var bmp = LoadSvgBitmap(path, background);

			Assert.Equal(background, bmp.GetPixel(25, 25));

			// test for the explicit positioning, the others aren't supported yet
			Assert.Equal(fill, bmp.GetPixel(370, 40));
		}

		[Fact]
		public void SvgLoadsLocalEmbeddedImages()
		{
			var path = Path.Combine(PathToImages, "embedded.svg");
			var background = (SKColor)0xffffffff;
			var fill = (SKColor)0xff3498db;

			var bmp = LoadSvgBitmap(path, background);

			Assert.Equal(background, bmp.GetPixel(25, 25));
			Assert.Equal(fill, bmp.GetPixel(35, 50));
		}

		[Fact]
		public void SvgLoadsPolygon()
		{
			var path = Path.Combine(PathToImages, "sketch.svg");
			var background = (SKColor)0xfff8f8f8;
			var fill = (SKColor)0xFF4990E2;

			var bmp = LoadSvgBitmap(path, background);

			Assert.Equal(fill, bmp.GetPixel(bmp.Width / 2, bmp.Height / 2));
			Assert.Equal(background, bmp.GetPixel(5, 5));
		}

		[Fact]
		public void SvgLoadsDashes()
		{
			var path = Path.Combine(PathToImages, "dashes.svg");

			var bmp = LoadSvgBitmap(path, SKColors.White);

			Assert.Equal(SKColors.Black, bmp.GetPixel(10 + 3, 20));
			Assert.Equal(SKColors.Black, bmp.GetPixel(10 + 7, 20));
			Assert.Equal(SKColors.White, bmp.GetPixel(10 + 13, 20));

			Assert.Equal(SKColors.Black, bmp.GetPixel(10 + 3, 40));
			Assert.Equal(SKColors.White, bmp.GetPixel(10 + 7, 40));
			Assert.Equal(SKColors.Black, bmp.GetPixel(10 + 13, 40));

			Assert.Equal(SKColors.White, bmp.GetPixel(10 + 3, 60));
			Assert.Equal(SKColors.Black, bmp.GetPixel(10 + 7, 60));
			Assert.Equal(SKColors.Black, bmp.GetPixel(10 + 13, 60));

			Assert.Equal(SKColors.Black, bmp.GetPixel(10 + 3, 80));
			Assert.Equal(SKColors.Black, bmp.GetPixel(10 + 7, 80));
			Assert.Equal(SKColors.White, bmp.GetPixel(10 + 13, 80));
		}

		[Fact]
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

				Assert.Equal(ns, svg.GetDefaultNamespace());
				Assert.Equal("200", svg.Attribute("width").Value);
				Assert.Equal("150", svg.Attribute("height").Value);

				var rect = svg.Element(ns + "rect");
				Assert.Equal("rgb(0,0,255)", rect.Attribute("fill").Value);
				Assert.Equal("50", rect.Attribute("x").Value);
				Assert.Equal("70", rect.Attribute("y").Value);
				Assert.Equal("100", rect.Attribute("width").Value);
				Assert.Equal("30", rect.Attribute("height").Value);

				var ellipse = svg.Element(ns + "ellipse");
				Assert.Equal("rgb(255,0,0)", ellipse.Attribute("fill").Value);
				Assert.Equal("100", ellipse.Attribute("cx").Value);
				Assert.Equal("85", ellipse.Attribute("cy").Value);
				Assert.Equal("50", ellipse.Attribute("rx").Value);
				Assert.Equal("15", ellipse.Attribute("ry").Value);
			}
		}

		[Fact]
		public void SvgCanUnderstandColorNames()
		{
			var svg =
@"<svg xmlns=""http://www.w3.org/2000/svg"" version=""1.1""
    x=""0px"" y=""0px"" width=""100"" height=""100"" viewBox=""0 0 100 100"">
  <rect style=""fill:lime"" width=""100"" height=""100"" x=""0"" y=""0"" />
</svg>";

			var bmp = CreateSvgBitmap(svg);

			Assert.Equal(SKColors.Lime, bmp.GetPixel(bmp.Width / 2, bmp.Height / 2));
		}

		[Fact]
		public void SvgCanUnderstandRgbColors()
		{
			var svg =
@"<svg xmlns=""http://www.w3.org/2000/svg"" version=""1.1""
    x=""0px"" y=""0px"" width=""100"" height=""100"" viewBox=""0 0 100 100"">
  <rect style=""fill:rgb(0,255,0)"" width=""100"" height=""100"" x=""0"" y=""0"" />
</svg>";

			var bmp = CreateSvgBitmap(svg);

			Assert.Equal(SKColors.Lime, bmp.GetPixel(bmp.Width / 2, bmp.Height / 2));
		}

		[Fact]
		public void RectWithSingleCornerRadius()
		{
			var check = new Action<string>(svg =>
			{
				var bmp = CreateSvgBitmap(svg, SKColors.White);

				Assert.Equal(SKColors.White, bmp.GetPixel(3, 3));
				Assert.Equal(SKColors.White, bmp.GetPixel(97, 3));
				Assert.Equal(SKColors.White, bmp.GetPixel(3, 97));
				Assert.Equal(SKColors.White, bmp.GetPixel(97, 97));
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

		[Fact]
		public void SvgCanUnderstandPolygon()
		{
			var svg =
@"<svg xmlns=""http://www.w3.org/2000/svg"" version=""1.1""
    x=""0px"" y=""0px"" width=""100"" height=""100"" viewBox=""0 0 100 100"">
  <polygon points=""20,70 50,20 80,70"" style=""fill:white; stroke:black; stroke-width:10""/>
</svg>";

			var bmp = CreateSvgBitmap(svg);

			Assert.Equal(SKColors.Black, bmp.GetPixel(50, 70));
		}

		[Fact]
		public void SvgCanUnderstandPolyline()
		{
			var svg =
@"<svg xmlns=""http://www.w3.org/2000/svg"" version=""1.1""
    x=""0px"" y=""0px"" width=""100"" height=""100"" viewBox=""0 0 100 100"">
  <polyline points=""20,70 50,20 80,70"" style=""fill:white; stroke:black; stroke-width:10""/>
</svg>";

			var bmp = CreateSvgBitmap(svg, SKColors.Green);

			Assert.Equal(SKColors.Green, bmp.GetPixel(50, 70));
		}

		[Fact]
		public void SvgStylesAreAlsoUsed()
		{
			var path = Path.Combine(PathToImages, "issues-22.svg");

			var svg = new SKSvg();
			svg.Load(path);
			var bmp = CreateBitmap(svg, SKColors.White);

			Assert.Equal(SKColors.White, bmp.GetPixel(bmp.Width / 2, bmp.Height / 2));
		}

		[Fact]
		public void SvgCanReadFileWithNoXLinkNamespacePrefix()
		{
			var path = Path.Combine(PathToImages, "issues-8.svg");
			var background = (SKColor)0x000000;
			var fill = (SKColor)0xFFE3E6E8;

			var svg = new SKSvg();
			svg.Load(path);
			var bmp = CreateBitmap(svg, background);

			Assert.Equal(fill, bmp.GetPixel(bmp.Width / 2, bmp.Height / 2));
			Assert.Equal(background, bmp.GetPixel(5, 5));
		}

		[Fact]
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

			Assert.Equal(fill, bmp.GetPixel(bmp.Width / 2, bmp.Height / 2));
			Assert.Equal(background, bmp.GetPixel(5, 5));
		}


		[Fact]
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
					Assert.Equal(fill, bmp.GetPixel(x, y));
					Assert.Equal(background, bmp.GetPixel(x + 20, y + 20));
				}
			}
		}

		[Fact]
		public void SvgCanReadFileWithDTD()
		{
			var path = Path.Combine(PathToImages, "dtd.svg");
			var bmp = LoadSvgBitmap(path, SKColors.Red);

			Assert.Equal(SKColors.Black, bmp.GetPixel(50, 50));
		}

		[Fact]
		public void SimpleRadialGradient()
		{
			var path = Path.Combine(PathToImages, "simple-gradient.svg");
			var bmp = LoadSvgBitmap(path, SKColors.White);

			Assert.Equal(new SKColor(0xff058205), bmp.GetPixel(50, 50));
			Assert.Equal(new SKColor(0xffffffff), bmp.GetPixel(0, 0));
			Assert.Equal(new SKColor(0xff9fcf9f), bmp.GetPixel(65, 65));
		}

		[Fact]
		public void LinearGradientWithPercents()
		{
			var path = Path.Combine(PathToImages, "percents.svg");
			var bmp = LoadSvgBitmap(path, SKColors.White);

			// horizontal
			Assert.Equal(new SKColor(0xfffd0303), bmp.GetPixel(10, 60));
			Assert.Equal(new SKColor(0xfffcfcfc), bmp.GetPixel(60, 60));
			Assert.Equal(new SKColor(0xff0303fd), bmp.GetPixel(109, 60));

			// vertical
			Assert.Equal(new SKColor(0xfffd0303), bmp.GetPixel(60, 120));
			Assert.Equal(new SKColor(0xfffcfcfc), bmp.GetPixel(60, 170));
			Assert.Equal(new SKColor(0xff0303fd), bmp.GetPixel(60, 219));
		}

		[Fact]
		public void SvgReadGradientTransform()
		{
			var path = Path.Combine(PathToImages, "gradient.svg");
			var bmp = LoadSvgBitmap(path, SKColors.Green);
			SaveBitmap(bmp);

			// Radial Gradient
			Assert.Equal(new SKColor(0xfff18886), bmp.GetPixel(33, 33));
			Assert.Equal(new SKColor(0xffeb4f53), bmp.GetPixel(20, 33));
			Assert.Equal(new SKColor(0xffeb4c51), bmp.GetPixel(46, 33));

			// Linear Gradient
			Assert.Equal(new SKColor(0xfff30600), bmp.GetPixel(33, 180));
			Assert.Equal(new SKColor(0xffff0000), bmp.GetPixel(20, 180));
			Assert.Equal(new SKColor(0xffc21f00), bmp.GetPixel(46, 180));
		}

		[Fact]
		public void SvgFillsAndStrokeHaveProperInheritance()
		{
			var path = Path.Combine(PathToImages, "issues-42.svg");
			var bmp = LoadSvgBitmap(path, SKColors.Red);

			Assert.Equal(SKColors.Black, bmp.GetPixel(47, 98));
			Assert.Equal(SKColors.Black, bmp.GetPixel(149, 145));
			Assert.Equal(SKColors.White, bmp.GetPixel(166, 246));
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
