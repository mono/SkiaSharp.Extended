using SkiaSharp.Extended.DeepZoom;
using SkiaSharp.Extended.UI.Maui.DeepZoom;

namespace SkiaSharpDemo.Demos;

public partial class DeepZoomPage : ContentPage
{
	public DeepZoomPage()
	{
		InitializeComponent();
		LoadSampleImage();
	}

	private void LoadSampleImage()
	{
		try
		{
			statusLabel.Text = "Loading sample Deep Zoom image...";

			// Use a built-in programmatic DZI for the demo
			// A real app would load from a URL or local file
			var dziXml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
				"<Image TileSize=\"256\" Overlap=\"0\" Format=\"jpg\" " +
				"xmlns=\"http://schemas.microsoft.com/deepzoom/2008\">" +
				"<Size Width=\"1024\" Height=\"768\"/></Image>";

			var tileSource = DziTileSource.Parse(dziXml, "placeholder://");

			// Load with a memory fetcher that returns colored placeholder tiles
			deepZoomView.Load(tileSource, new PlaceholderTileFetcher());

			statusLabel.Text = $"Loaded: {tileSource.ImageWidth}×{tileSource.ImageHeight} — pinch/pan/double-tap";
		}
		catch (Exception ex)
		{
			statusLabel.Text = $"Error: {ex.Message}";
		}
	}

	private void OnResetClicked(object? sender, EventArgs e)
	{
		deepZoomView.ResetView();
		UpdateZoomLabel();
	}

	private void OnZoomInClicked(object? sender, EventArgs e)
	{
		deepZoomView.ZoomAboutLogicalPoint(2.0, 0.5, 0.5 / Math.Max(deepZoomView.AspectRatio, 1));
		UpdateZoomLabel();
	}

	private void OnZoomOutClicked(object? sender, EventArgs e)
	{
		deepZoomView.ZoomAboutLogicalPoint(0.5, 0.5, 0.5 / Math.Max(deepZoomView.AspectRatio, 1));
		UpdateZoomLabel();
	}

	private void UpdateZoomLabel()
	{
		var zoom = deepZoomView.ViewportWidth > 0 ? 1.0 / deepZoomView.ViewportWidth : 1.0;
		zoomLabel.Text = $"Zoom: {zoom:F1}x";
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		deepZoomView.Dispose();
	}

	/// <summary>
	/// A tile fetcher that generates colored placeholder tiles for demo purposes.
	/// Each tile gets a unique color based on its level/col/row.
	/// </summary>
	private class PlaceholderTileFetcher : ITileFetcher
	{
		public Task<SkiaSharp.SKBitmap?> FetchTileAsync(string url, System.Threading.CancellationToken ct = default)
		{
			var bitmap = new SkiaSharp.SKBitmap(256, 256);
			using var canvas = new SkiaSharp.SKCanvas(bitmap);

			// Generate a color from the URL hash so each tile is visually distinct
			int hash = url.GetHashCode();
			byte r = (byte)((hash & 0xFF0000) >> 16 | 0x40);
			byte g = (byte)((hash & 0x00FF00) >> 8 | 0x40);
			byte b = (byte)((hash & 0x0000FF) | 0x40);

			canvas.Clear(new SkiaSharp.SKColor(r, g, b));

			// Draw a grid pattern to make zoom levels visible
			using var paint = new SkiaSharp.SKPaint
			{
				Color = new SkiaSharp.SKColor((byte)(r + 30), (byte)(g + 30), (byte)(b + 30)),
				IsStroke = true,
				StrokeWidth = 1,
			};
			for (int i = 0; i < 256; i += 32)
			{
				canvas.DrawLine(i, 0, i, 256, paint);
				canvas.DrawLine(0, i, 256, i, paint);
			}

			return Task.FromResult<SkiaSharp.SKBitmap?>(bitmap);
		}

		public void Dispose() { }
	}
}
