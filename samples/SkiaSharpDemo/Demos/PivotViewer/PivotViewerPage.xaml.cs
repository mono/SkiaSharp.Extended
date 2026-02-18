using SkiaSharp.Extended.PivotViewer;
using SkiaSharp.Extended.UI.Maui.PivotViewer;

namespace SkiaSharpDemo.Demos;

public partial class PivotViewerPage : ContentPage
{
	private static readonly (string Name, string Path)[] BundledCollections =
	[
		("Concept Cars (298 items)", "Collections/conceptcars/conceptcars.cxml"),
		("Nigerian States (33 items)", "Collections/nigeria-state/nigeria_state.cxml"),
	];

	public PivotViewerPage()
	{
		InitializeComponent();

		foreach (var (name, _) in BundledCollections)
			collectionPicker.Items.Add(name);

		collectionPicker.SelectedIndex = 0;
	}

	private void OnCollectionPickerChanged(object? sender, EventArgs e)
	{
		if (collectionPicker.SelectedIndex >= 0)
			LoadBundledCollection(BundledCollections[collectionPicker.SelectedIndex].Path);
	}

	private void OnLoadUrlClicked(object? sender, EventArgs e)
	{
		var url = urlEntry.Text?.Trim();
		if (!string.IsNullOrEmpty(url))
			LoadFromUrl(url);
		else
			statusLabel.Text = "Enter a CXML URL first";
	}

	private async void LoadBundledCollection(string assetPath)
	{
		try
		{
			statusLabel.Text = "Loading collection...";

			using var stream = await FileSystem.OpenAppPackageFileAsync(assetPath);
			var source = await CxmlCollectionSource.LoadAsync(stream);

			if (source.State == CxmlCollectionState.Loaded)
			{
				pivotViewerView.LoadCollection(source);
				var types = string.Join(", ", source.ItemProperties
					.Select(p => p.GetType().Name.Replace("PivotViewer", "").Replace("Property", ""))
					.Distinct().OrderBy(t => t));
				statusLabel.Text = $"\"{source.Name}\" — {source.Items.Count} items, " +
				                   $"{source.ItemProperties.Count} facets ({types})";
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
		pivotViewerView.Dispose();
	}
}
