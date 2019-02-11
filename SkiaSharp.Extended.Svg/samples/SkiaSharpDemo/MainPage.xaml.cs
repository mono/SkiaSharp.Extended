using System;
using System.IO;
using System.Reflection;
using SkiaSharp;
using SkiaSharp.Extended.Svg;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace SkiaSharpDemo
{
	public partial class MainPage : TabbedPage
	{
		private SKSvg svg;

		public MainPage()
		{
			InitializeComponent();
		}

		private static Stream GetImageStream(string svgName)
		{
			var type = typeof(MainPage).GetTypeInfo();
			var assembly = type.Assembly;

			return assembly.GetManifestResourceStream($"SkiaSharpDemo.images.{svgName}");
		}

		private void LoadSvg(string svgName)
		{
			// create a new SVG object
			svg = new SKSvg();

			// load the SVG document from a stream
			using (var stream = GetImageStream(svgName))
				svg.Load(stream);
		}

		private void OnPageAppearing(object sender, EventArgs e)
		{
			svg = null;

			var page = (ContentPage)sender;
			LoadSvg(page.AutomationId);

			var canvas = (SKCanvasView)page.Content;
			canvas.InvalidateSurface();
		}

		private void OnPainting(object sender, SKPaintSurfaceEventArgs e)
		{
			var surface = e.Surface;
			var canvas = surface.Canvas;

			var width = e.Info.Width;
			var height = e.Info.Height;

			// clear the surface
			canvas.Clear(SKColors.White);

			// the page is not visible yet
			if (svg == null)
				return;

			// calculate the scaling need to fit to screen
			float canvasMin = Math.Min(width, height);
			float svgMax = Math.Max(svg.Picture.CullRect.Width, svg.Picture.CullRect.Height);
			float scale = canvasMin / svgMax;
			var matrix = SKMatrix.MakeScale(scale, scale);

			// draw the svg
			canvas.DrawPicture(svg.Picture, ref matrix);
		}
	}
}
