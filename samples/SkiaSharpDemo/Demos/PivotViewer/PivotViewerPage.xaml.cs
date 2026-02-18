using SkiaSharp;
using SkiaSharp.Extended.DeepZoom;
using SkiaSharp.Extended.PivotViewer;
using SkiaSharp.Extended.UI.Maui.PivotViewer;

namespace SkiaSharpDemo.Demos;

public partial class PivotViewerPage : ContentPage
{
	private static readonly CollectionInfo[] BundledCollections =
	[
		new("Concept Cars (298 items)",
			"Collections/conceptcars/conceptcars.cxml",
			"Collections/conceptcars/deepzoom/conceptcars.dzc"),
		new("Nigerian States (33 items)",
			"Collections/nigeria-state/nigeria_state.cxml",
			"Collections/nigeria-state/deepzoom/images.dzc"),
	];

	private CancellationTokenSource? _thumbnailCts;

	public PivotViewerPage()
	{
		InitializeComponent();

		foreach (var c in BundledCollections)
			collectionPicker.Items.Add(c.Name);

		collectionPicker.SelectedIndex = 0;
	}

	private void OnCollectionPickerChanged(object? sender, EventArgs e)
	{
		if (collectionPicker.SelectedIndex >= 0)
			LoadBundledCollection(BundledCollections[collectionPicker.SelectedIndex]);
	}

	private void OnLoadUrlClicked(object? sender, EventArgs e)
	{
		var url = urlEntry.Text?.Trim();
		if (!string.IsNullOrEmpty(url))
			LoadFromUrl(url);
		else
			statusLabel.Text = "Enter a CXML URL first";
	}

	private async void LoadBundledCollection(CollectionInfo info)
	{
		try
		{
			statusLabel.Text = "Loading collection...";

			// Cancel any in-flight thumbnail loading
			_thumbnailCts?.Cancel();
			_thumbnailCts?.Dispose();
			_thumbnailCts = new CancellationTokenSource();
			var ct = _thumbnailCts.Token;

			// Parse CXML
			using var stream = await FileSystem.OpenAppPackageFileAsync(info.CxmlPath);
			var source = await CxmlCollectionSource.LoadAsync(stream);

			if (source.State != CxmlCollectionState.Loaded)
			{
				statusLabel.Text = $"Failed to load: state = {source.State}";
				return;
			}

			// Dispose previous ImageProvider before creating new one
			pivotViewerView.Controller.ImageProvider?.Dispose();
			pivotViewerView.Controller.ImageProvider = null;

			pivotViewerView.LoadCollection(source);

			// Set up image provider from DZC
			try
			{
				using var dzcStream = await FileSystem.OpenAppPackageFileAsync(info.DzcPath);
				var dzc = DzcTileSource.Parse(dzcStream);
				var dzcDir = info.DzcPath.Substring(0, info.DzcPath.LastIndexOf('/'));
				var fetcher = new AppPackageTileFetcher();

				pivotViewerView.Controller.ImageProvider =
					new CollectionImageProvider(dzc, fetcher, dzcDir);

				// Start loading thumbnails with cancellation support
				_ = LoadThumbnailsAsync(source, ct);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"DZC load warning: {ex.Message}");
			}

			var types = string.Join(", ", source.ItemProperties
				.Select(p => p.GetType().Name.Replace("PivotViewer", "").Replace("Property", ""))
				.Distinct().OrderBy(t => t));
			statusLabel.Text = $"\"{source.Name}\" — {source.Items.Count} items, " +
			                   $"{source.ItemProperties.Count} facets ({types})";
		}
		catch (Exception ex)
		{
			statusLabel.Text = $"Error: {ex.Message}";
		}
	}

	private async Task LoadThumbnailsAsync(CxmlCollectionSource source, CancellationToken ct)
	{
		var imgProvider = pivotViewerView.Controller.ImageProvider;
		if (imgProvider == null) return;

		try
		{
			await imgProvider.LoadThumbnailsAsync(source.Items, 128, ct);
			if (!ct.IsCancellationRequested)
				pivotViewerView.InvalidateSurface();
		}
		catch (OperationCanceledException) { }
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Thumbnail load: {ex.Message}");
		}
	}

	private async void LoadFromUrl(string url)
	{
		try
		{
			statusLabel.Text = "Loading from URL...";
			using var httpClient = new HttpClient();

			var uri = new Uri(url);
			var source = await CxmlCollectionSource.LoadAsync(uri, httpClient);

			if (source.State == CxmlCollectionState.Loaded)
			{
				pivotViewerView.LoadCollection(source);
				statusLabel.Text = $"\"{source.Name}\" — {source.Items.Count} items, " +
				                   $"{source.ItemProperties.Count} facets";
			}
			else
			{
				statusLabel.Text = $"Failed to load: state = {source.State}";
			}
		}
		catch (Exception ex)
		{
			statusLabel.Text = $"Error: {ex.Message}";
		}
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		_thumbnailCts?.Cancel();
		_thumbnailCts?.Dispose();
		pivotViewerView.Dispose();
	}

	private record CollectionInfo(string Name, string CxmlPath, string DzcPath);

	/// <summary>
	/// Loads tiles from MAUI app package raw assets for DZC composite tiles.
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
