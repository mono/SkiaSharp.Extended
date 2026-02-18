using SkiaSharp;
using SkiaSharp.Extended.DeepZoom;
using SkiaSharp.Extended.UI.Maui.DeepZoom;

namespace SkiaSharpDemo.Demos;

public partial class DeepZoomPage : ContentPage
{
	public DeepZoomPage()
	{
		InitializeComponent();
		LoadConceptCarDzi();
	}

	private async void LoadConceptCarDzi()
	{
		try
		{
			statusLabel.Text = "Loading Deep Zoom image...";

			// Parse the DZC to find a sub-image with its DZI source
			using var dzcStream = await FileSystem.OpenAppPackageFileAsync(
				"Collections/conceptcars/deepzoom/conceptcars.dzc");
			var dzc = DzcTileSource.Parse(dzcStream);

			// Pick the first sub-image and load its DZI
			var sub = dzc.Items[0];
			var dziPath = $"Collections/conceptcars/deepzoom/{sub.Source}";
			using var dziStream = await FileSystem.OpenAppPackageFileAsync(dziPath);
			using var dziReader = new StreamReader(dziStream);
			var dziXml = await dziReader.ReadToEndAsync();

			// Base URL for tiles: DZI path with .dzi replaced by _files/
			var tileBase = $"asset://Collections/conceptcars/deepzoom/{sub.Source!.Replace(".dzi", "_files/")}";
			var tileSource = DziTileSource.Parse(dziXml, tileBase);

			deepZoomView.Load(tileSource, new AppPackageTileFetcher());

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
	/// Loads tiles from MAUI app package raw assets.
	/// URLs are in the form "asset://Collections/conceptcars/deepzoom/{id}_files/{level}/{col}_{row}.jpg".
	/// </summary>
	private class AppPackageTileFetcher : ITileFetcher
	{
		public async Task<SKBitmap?> FetchTileAsync(string url, CancellationToken ct = default)
		{
			try
			{
				var path = url.Replace("asset://", "");
				using var stream = await FileSystem.OpenAppPackageFileAsync(path);
				return SKBitmap.Decode(stream);
			}
			catch
			{
				return null;
			}
		}

		public void Dispose() { }
	}
}
