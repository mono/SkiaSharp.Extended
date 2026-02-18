using SkiaSharp.Extended.PivotViewer;
using SkiaSharp.Extended.UI.Maui.PivotViewer;

namespace SkiaSharpDemo.Demos;

public partial class PivotViewerPage : ContentPage
{
	public PivotViewerPage()
	{
		InitializeComponent();
		LoadBundledCxml();
	}

	private void OnLoadClicked(object? sender, EventArgs e)
	{
		var url = urlEntry.Text?.Trim();
		if (!string.IsNullOrEmpty(url))
			LoadFromUrl(url);
	}

	private void OnBuiltInSampleClicked(object? sender, EventArgs e)
	{
		LoadBundledCxml();
	}

	private void OnRemoteSampleClicked(object? sender, EventArgs e)
	{
		var url = urlEntry.Text?.Trim();
		if (!string.IsNullOrEmpty(url))
			LoadFromUrl(url);
		else
			statusLabel.Text = "Enter a CXML URL first";
	}

	private async void LoadBundledCxml()
	{
		try
		{
			statusLabel.Text = "Loading ski resorts collection...";

			// Load the bundled CXML from app raw assets
			using var stream = await FileSystem.OpenAppPackageFileAsync("TestData/simple_ski.cxml");
			var source = await CxmlCollectionSource.LoadAsync(stream);

			if (source.State == CxmlCollectionState.Loaded)
			{
				pivotViewerView.LoadCollection(source);
				statusLabel.Text = $"Loaded \"{source.Name}\" — {source.Items.Count} items, " +
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

	private async void LoadFromUrl(string url)
	{
		try
		{
			statusLabel.Text = $"Loading from URL...";
			using var httpClient = new HttpClient();

			var uri = new Uri(url);
			var source = await CxmlCollectionSource.LoadAsync(uri, httpClient);

			if (source.State == CxmlCollectionState.Loaded)
			{
				pivotViewerView.LoadCollection(source);
				statusLabel.Text = $"Loaded \"{source.Name}\" — {source.Items.Count} items, " +
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
		pivotViewerView.Dispose();
	}
}
