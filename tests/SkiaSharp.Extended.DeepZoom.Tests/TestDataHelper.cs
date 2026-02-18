using System.Reflection;
using SkiaSharp.Extended.DeepZoom;

namespace SkiaSharp.Extended.DeepZoom.Tests;

/// <summary>Helper for loading embedded test data resources.</summary>
internal static class TestDataHelper
{
    private static readonly Assembly TestAssembly = typeof(TestDataHelper).Assembly;

    public static Stream GetStream(string resourceName)
    {
        string fullName = $"SkiaSharp.Extended.DeepZoom.Tests.TestData.{resourceName}";
        var stream = TestAssembly.GetManifestResourceStream(fullName);
        if (stream == null)
        {
            // Try finding it by suffix
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
}
