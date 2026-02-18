using SkiaSharp;
using SkiaSharp.Extended.DeepZoom;
using SkiaSharp.Extended.UI.Maui.DeepZoom;

namespace SkiaSharpDemo.Demos;

public partial class DeepZoomPage : ContentPage
{
	public DeepZoomPage()
	{
		InitializeComponent();
		LoadBundledDzi();
	}

	private async void LoadBundledDzi()
	{
		try
		{
			statusLabel.Text = "Loading Deep Zoom image...";

			// Load the bundled DZI descriptor from raw assets
			using var stream = await FileSystem.OpenAppPackageFileAsync("TestData/143.dzi");
			using var reader = new StreamReader(stream);
			var dziXml = await reader.ReadToEndAsync();

			// The tile files are at TestData/143_files/{level}/{col}_{row}.jpg
			var tileSource = DziTileSource.Parse(dziXml, "asset://TestData/143_files/");

			// Load with a fetcher that reads from MAUI app package
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
	/// URLs are in the form "asset://TestData/143_files/{level}/{col}_{row}.jpg".
	/// </summary>
	private class AppPackageTileFetcher : ITileFetcher
	{
		public async Task<SKBitmap?> FetchTileAsync(string url, CancellationToken ct = default)
		{
			try
			{
				// Strip the "asset://" prefix to get the raw asset path
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
