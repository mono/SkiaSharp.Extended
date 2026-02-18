using SkiaSharp.Extended.PivotViewer;
using SkiaSharp.Extended.UI.Maui.PivotViewer;

namespace SkiaSharpDemo.Demos;

public partial class PivotViewerPage : ContentPage
{
	private HttpClient? _httpClient;

	public PivotViewerPage()
	{
		InitializeComponent();
		// Load the built-in sample by default
		LoadBuiltInSample();
	}

	private void OnLoadClicked(object? sender, EventArgs e)
	{
		var url = urlEntry.Text?.Trim();
		if (!string.IsNullOrEmpty(url))
			LoadFromUrl(url);
	}

	private void OnBuiltInSampleClicked(object? sender, EventArgs e)
	{
		LoadBuiltInSample();
	}

	private void OnRemoteSampleClicked(object? sender, EventArgs e)
	{
		var url = urlEntry.Text?.Trim();
		if (!string.IsNullOrEmpty(url))
			LoadFromUrl(url);
		else
			statusLabel.Text = "Enter a CXML URL first";
	}

	private void LoadBuiltInSample()
	{
		try
		{
			statusLabel.Text = "Loading built-in sample...";

			// Build a sample collection in code (no file needed)
			var catProp = new PivotViewerStringProperty("Category")
			{
				DisplayName = "Category",
				Options = PivotViewerPropertyOptions.CanFilter,
			};
			var ratingProp = new PivotViewerNumericProperty("Rating")
			{
				DisplayName = "Rating",
				Options = PivotViewerPropertyOptions.CanFilter,
			};
			var colorProp = new PivotViewerStringProperty("Color")
			{
				DisplayName = "Color",
				Options = PivotViewerPropertyOptions.CanFilter,
			};

			var properties = new List<PivotViewerProperty> { catProp, ratingProp, colorProp };

			var items = new List<PivotViewerItem>();
			var categories = new[] { "Fruit", "Vegetable", "Grain", "Dairy" };
			var colors = new[] { "Red", "Green", "Yellow", "White", "Orange" };
			var names = new[]
			{
				"Apple", "Banana", "Carrot", "Broccoli", "Rice",
				"Wheat", "Milk", "Cheese", "Tomato", "Pepper",
				"Lettuce", "Orange", "Grape", "Oats", "Yogurt",
				"Corn", "Pea", "Bread", "Butter", "Spinach",
			};

			var rng = new Random(42);
			for (int i = 0; i < names.Length; i++)
			{
				var item = new PivotViewerItem(i.ToString());
				item.Add(catProp, categories[i % categories.Length]);
				item.Add(ratingProp, (double)(rng.Next(1, 6)));
				item.Add(colorProp, colors[i % colors.Length]);
				items.Add(item);
			}

			pivotViewerView.LoadItems(items, properties);
			statusLabel.Text = $"Loaded {items.Count} items — tap items, use filter pane";
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
			statusLabel.Text = $"Loading from {url}...";
			_httpClient ??= new HttpClient();

			var uri = new Uri(url);
			var source = await CxmlCollectionSource.LoadAsync(uri, _httpClient);

			if (source.State == CxmlCollectionState.Loaded)
			{
				pivotViewerView.LoadCollection(source);
				statusLabel.Text = $"Loaded \"{source.Name}\" — {source.Items.Count} items, " +
				                   $"{source.ItemProperties.Count} properties";
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
		_httpClient?.Dispose();
	}
}
