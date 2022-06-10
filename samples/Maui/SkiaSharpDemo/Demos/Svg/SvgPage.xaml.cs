using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using Svg.Skia;

namespace SkiaSharpDemo.Demos;

public partial class SvgPage : TabbedPage
{
	private SKSvg? svg;

	public SvgPage()
	{
		InitializeComponent();
	}

	private async Task LoadSvgAsync(string svgName)
	{
		// create a new SVG object
		svg = new SKSvg();

		// load the SVG document from a stream
		using var stream = await FileSystem.OpenAppPackageFileAsync(svgName);
		svg.Load(stream);
	}

	private async void OnPageAppearing(object sender, EventArgs e)
	{
		svg = null;

		var page = (ContentPage)sender;
		await LoadSvgAsync(page.AutomationId);

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
		if (svg?.Picture is null)
			return;

		// calculate the scaling need to fit to screen
		float canvasMin = Math.Min(width, height);
		float svgMax = Math.Max(svg.Picture.CullRect.Width, svg.Picture.CullRect.Height);
		float scale = canvasMin / svgMax;
		var matrix = SKMatrix.CreateScale(scale, scale);

		// draw the svg
		canvas.DrawPicture(svg.Picture, ref matrix);
	}
}
