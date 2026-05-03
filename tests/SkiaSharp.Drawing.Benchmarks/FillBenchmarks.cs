using System.Drawing;
using System.Drawing.Drawing2D;
using BenchmarkDotNet.Attributes;

namespace SkiaSharp.Drawing.Benchmarks;

[MemoryDiagnoser]
public class FillBenchmarks : BenchmarkBase
{
    [Benchmark]
    public void FillRectangle_100()
    {
        for (int i = 0; i < 100; i++)
            Graphics.FillRectangle(RedBrush, 10 + i, 10 + i, 200, 200);
    }

    [Benchmark]
    public void FillEllipse_100()
    {
        for (int i = 0; i < 100; i++)
            Graphics.FillEllipse(BlueBrush, 10 + i, 10 + i, 200, 200);
    }

    [Benchmark]
    public void FillPolygon_100()
    {
        var points = new PointF[] { new(50, 10), new(90, 80), new(10, 80) };
        for (int i = 0; i < 100; i++)
            Graphics.FillPolygon(RedBrush, points);
    }

    [Benchmark]
    public void Clear_100()
    {
        for (int i = 0; i < 100; i++)
            Graphics.Clear(Color.White);
    }

    [Benchmark]
    public void FillPie_100()
    {
        for (int i = 0; i < 100; i++)
            Graphics.FillPie(RedBrush, 50, 50, 200, 200, 0, 90);
    }
}
