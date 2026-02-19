using SkiaSharp.Extended.PivotViewer;

namespace MauiPivotViewer;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, EventArgs e)
    {
        try
        {
            Console.WriteLine("[PivotViewer] Loading concept cars collection...");

            // Load the concept cars collection from bundled resources
            using var stream = await FileSystem.OpenAppPackageFileAsync("Collections/conceptcars/conceptcars.cxml");

            // Parse the collection using CxmlCollectionSource
            var source = await CxmlCollectionSource.LoadAsync(stream);

            Console.WriteLine($"[PivotViewer] Loaded {source.Items.Count} items, {source.ItemProperties.Count} properties");

            // Bind to the PivotViewer
            pivotViewer.ItemsSource = source.Items.ToList();
            pivotViewer.PivotProperties = source.ItemProperties.ToList();

            Console.WriteLine("[PivotViewer] Collection bound to view");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PivotViewer] Failed to load collection: {ex}");
            Title = $"Error: {ex.Message}";
        }
    }
}
