using System.Drawing;
using BenchmarkDotNet.Attributes;

namespace SkiaSharp.Extended.Drawing.Common.Benchmarks;

[MemoryDiagnoser]
public class DrawBenchmarks : BenchmarkBase
{
    [Benchmark]
    public void DrawLine_1000()
    {
        for (int i = 0; i < 1000; i++)
            Graphics.DrawLine(BlackPen1, 0, i % 500, 499, i % 500);
    }

    [Benchmark]
    public void DrawRectangle_100()
    {
        for (int i = 0; i < 100; i++)
            Graphics.DrawRectangle(BlackPen1, 10 + i, 10 + i, 200, 200);
    }

    [Benchmark]
    public void DrawEllipse_100()
    {
        for (int i = 0; i < 100; i++)
            Graphics.DrawEllipse(BlackPen3, 10 + i, 10 + i, 200, 200);
    }

    [Benchmark]
    public void DrawArc_100()
    {
        for (int i = 0; i < 100; i++)
            Graphics.DrawArc(BlackPen1, 50, 50, 200, 200, 0, 270);
    }

    [Benchmark]
    public void DrawBezier_100()
    {
        for (int i = 0; i < 100; i++)
            Graphics.DrawBezier(BlackPen1, 10f, 50f, 100f, 10f, 200f, 90f, 300f, 50f);
    }
}
