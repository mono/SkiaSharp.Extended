using System.Reflection;

namespace SkiaSharp.Extended.PivotViewer.Tests;

internal static class TestDataHelper
{
    private static readonly Assembly TestAssembly = typeof(TestDataHelper).Assembly;

    public static Stream GetStream(string resourceName)
    {
        string fullName = $"SkiaSharp.Extended.PivotViewer.Tests.TestData.{resourceName}";
        var stream = TestAssembly.GetManifestResourceStream(fullName);
        if (stream == null)
        {
            var names = TestAssembly.GetManifestResourceNames();
            var match = names.FirstOrDefault(n => n.EndsWith(resourceName, StringComparison.OrdinalIgnoreCase));
            if (match != null)
                stream = TestAssembly.GetManifestResourceStream(match);
        }
        return stream ?? throw new FileNotFoundException($"Embedded resource not found: {fullName}. Available: {string.Join(", ", TestAssembly.GetManifestResourceNames())}");
    }

    public static string GetString(string resourceName)
    {
        using var stream = GetStream(resourceName);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    public static CxmlCollectionSource LoadCxml(string resourceName)
    {
        string xml = GetString(resourceName);
        return CxmlCollectionSource.Parse(xml);
    }
}
