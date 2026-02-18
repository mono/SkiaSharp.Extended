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

	private AppPackageImageProvider? _imageProvider;

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

			// Parse CXML
			using var stream = await FileSystem.OpenAppPackageFileAsync(info.CxmlPath);
			var source = await CxmlCollectionSource.LoadAsync(stream);

			if (source.State != CxmlCollectionState.Loaded)
			{
				statusLabel.Text = $"Failed to load: state = {source.State}";
				return;
			}

			pivotViewerView.LoadCollection(source);

			// Set up image provider from DZC
			_imageProvider?.Dispose();
			try
			{
				using var dzcStream = await FileSystem.OpenAppPackageFileAsync(info.DzcPath);
				var dzc = DzcTileSource.Parse(dzcStream);
				// Base path is the directory containing the DZC file
				var dzcDir = info.DzcPath.Substring(0, info.DzcPath.LastIndexOf('/'));
				var fetcher = new AppPackageTileFetcher();

				_imageProvider = new AppPackageImageProvider(dzc, fetcher, dzcDir);
				pivotViewerView.Controller.ImageProvider =
					new CollectionImageProvider(dzc, fetcher, dzcDir);

				// Start loading thumbnails for visible items
				_ = LoadThumbnailsAsync(source);
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

	private async Task LoadThumbnailsAsync(CxmlCollectionSource source)
	{
		var imgProvider = pivotViewerView.Controller.ImageProvider;
		if (imgProvider == null) return;

		try
		{
			await imgProvider.LoadThumbnailsAsync(source.Items, 128);
			pivotViewerView.InvalidateSurface();
		}
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
		_imageProvider?.Dispose();
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

	/// <summary>
	/// Simple wrapper to track disposal.
	/// </summary>
	private class AppPackageImageProvider : IDisposable
	{
		private readonly DzcTileSource _dzc;
		private readonly ITileFetcher _fetcher;
		private readonly string _basePath;

		public AppPackageImageProvider(DzcTileSource dzc, ITileFetcher fetcher, string basePath)
		{
			_dzc = dzc;
			_fetcher = fetcher;
			_basePath = basePath;
		}

		public void Dispose() => _fetcher.Dispose();
	}
}
