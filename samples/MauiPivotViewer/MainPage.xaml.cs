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
            // Load the concept cars collection from bundled resources
            using var stream = await FileSystem.OpenAppPackageFileAsync("conceptcars/conceptcars.cxml");

            // Parse the collection using CxmlCollectionSource
            var source = await CxmlCollectionSource.LoadAsync(stream);

            // Bind to the PivotViewer
            pivotViewer.ItemsSource = source.Items.ToList();
            pivotViewer.PivotProperties = source.ItemProperties.ToList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load collection: {ex}");
        }
    }
}
