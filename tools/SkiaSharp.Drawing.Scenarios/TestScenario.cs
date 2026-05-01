using System;

namespace SkiaSharp.Drawing.Scenarios;

/// <summary>
/// A named drawing scenario that can be executed on any IDrawingSurface.
/// </summary>
public sealed class TestScenario
{
    public string Name { get; }
    public int Width { get; }
    public int Height { get; }
    public string Category { get; }
    public Action<IDrawingSurface> Draw { get; }

    public TestScenario(string name, string category, int width, int height, Action<IDrawingSurface> draw)
    {
        Name = name;
        Category = category;
        Width = width;
        Height = height;
        Draw = draw;
    }
}
