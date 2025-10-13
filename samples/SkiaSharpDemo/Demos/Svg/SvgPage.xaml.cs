using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharpDemo.Views;
using Svg.Skia;

namespace SkiaSharpDemo.Demos;

public partial class SvgPage : ContentPage
{
	private SKSvg? svg;

	public SvgPage()
	{
		InitializeComponent();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();

		// HACK: force a layout pass to work around an issue where the first tab is not
		//       properly laid out when the page appears
		Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(100), () =>
		{
			tabBar.Padding = new Thickness(1);
			tabBar.Padding = new Thickness(0);
		});
	}

	private async void OnSelectedTabChanged(object sender, SelectedItemChangedEventArgs e)
	{
		var tab = (BottomTab)e.SelectedItem;

		svg = null;
		svg = await LoadSvgAsync(tab.AutomationId);

		UpdateCanvasSize(tab);
	}

	private void OnTabBarSizeChanged(object sender, EventArgs e)
	{
		var view = (VisualElement)sender;
		var tab = (BottomTab)view.Parent;
		UpdateCanvasSize(tab);
	}

	private void UpdateCanvasSize(BottomTab tab)
	{
		if (!tab.IsVisible)
			return;

		var canvas = tab.GetVisualTreeDescendants()
			.OfType<SKCanvasView>()
			.FirstOrDefault();
		if (canvas is null)
			return;

		canvas.HeightRequest = svg?.Picture?.CullRect is SKRect rect && rect.Width > 0 && rect.Height > 0
			? canvas.Width * (rect.Height / rect.Width)
			: -1;

		canvas.InvalidateMeasure();

		canvas.InvalidateSurface();
	}

	private static async Task<SKSvg> LoadSvgAsync(string svgName)
	{
		// create a new SVG object
		var svg = new SKSvg();

		// load the SVG document from a stream
		using var stream = await FileSystem.OpenAppPackageFileAsync("SVG/" + svgName);
		svg.Load(stream);

		return svg;
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
		var canvasMin = Math.Min(width, height);
		var svgMax = Math.Max(svg.Picture.CullRect.Width, svg.Picture.CullRect.Height);
		var scale = canvasMin / svgMax;
		var matrix = SKMatrix.CreateScale(scale, scale);

		// draw the svg
		canvas.DrawPicture(svg.Picture, matrix);
	}
}
