using SkiaSharp.Extended.DeepZoom;
using SkiaSharp.Extended.UI.Maui.DeepZoom;

namespace SkiaSharpDemo.Demos;

public partial class DeepZoomPage : ContentPage
{
	// OpenSeadragon's publicly hosted sample DZI
	private const string SampleDziUrl = "https://openseadragon.github.io/example-images/highsmith/highsmith.dzi";

	public DeepZoomPage()
	{
		InitializeComponent();
		LoadSampleImage();
	}

	private async void LoadSampleImage()
	{
		try
		{
			statusLabel.Text = "Loading Deep Zoom image...";

			// Download and parse the DZI descriptor
			using var httpClient = new HttpClient();
			var dziXml = await httpClient.GetStringAsync(SampleDziUrl);

			var baseUrl = SampleDziUrl.Replace(".dzi", "_files/");
			var tileSource = DziTileSource.Parse(dziXml, baseUrl);

			// Load into the view with HTTP tile fetcher
			deepZoomView.Load(tileSource, new HttpTileFetcher());

			deepZoomView.ImageOpenSucceeded += (s, e) =>
			{
				MainThread.BeginInvokeOnMainThread(() =>
				{
					statusLabel.Text = $"Loaded: {tileSource.ImageWidth}×{tileSource.ImageHeight} — pinch/pan/double-tap";
				});
			};

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
}
