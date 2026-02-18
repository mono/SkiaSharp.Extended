namespace SkiaSharp.Extended.DeepZoom.Tests;

/// <summary>Helper for loading test data files from the output directory.</summary>
internal static class TestDataHelper
{
    private static string GetPath(string resourceName) =>
        Path.Combine(AppContext.BaseDirectory, "TestData", resourceName);

    public static Stream GetStream(string resourceName) =>
        File.OpenRead(GetPath(resourceName));

    public static string GetString(string resourceName) =>
        File.ReadAllText(GetPath(resourceName));
}
