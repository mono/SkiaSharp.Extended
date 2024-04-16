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

	private async Task<SKSvg> LoadSvgAsync(string svgName)
	{
		// create a new SVG object
		var svg = new SKSvg();

		// load the SVG document from a stream
		using var stream = await FileSystem.OpenAppPackageFileAsync("SVG/" + svgName);
		svg.Load(stream);

		return svg;
	}

	private async void OnPageAppearing(object sender, EventArgs e)
	{
		var page = (ContentPage)sender;
		var scrollView = (ScrollView)page.Content;
		var canvas = (SKCanvasView)scrollView.Content;

		svg = null;
		svg = await LoadSvgAsync(page.AutomationId);

		UpdateCanvasSize(page);

		canvas.InvalidateSurface();
	}

	private void UpdateCanvasSize(ContentPage page)
	{
		var scrollView = (ScrollView)page.Content;
		var canvas = (SKCanvasView)scrollView.Content;

		canvas.HeightRequest = svg?.Picture?.CullRect is SKRect rect && rect.Width > 0 && rect.Height > 0
			? canvas.Width * (rect.Height / rect.Width)
			: -1;
	}

	private void OnPageSizeChanged(object sender, EventArgs e)
	{
		var page = (ContentPage)sender;
		UpdateCanvasSize(page);
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
