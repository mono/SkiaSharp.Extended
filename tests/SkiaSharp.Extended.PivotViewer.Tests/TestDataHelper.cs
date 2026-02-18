namespace SkiaSharp.Extended.PivotViewer.Tests;

internal static class TestDataHelper
{
    private static string GetPath(string resourceName) =>
        Path.Combine(AppContext.BaseDirectory, "TestData", resourceName);

    public static Stream GetStream(string resourceName) =>
        File.OpenRead(GetPath(resourceName));

    public static string GetString(string resourceName) =>
        File.ReadAllText(GetPath(resourceName));

    public static CxmlCollectionSource LoadCxml(string resourceName) =>
        CxmlCollectionSource.Parse(GetString(resourceName));
}
