using System.Drawing;
using System.Drawing.Drawing2D;
using BenchmarkDotNet.Attributes;

namespace SkiaSharp.Extended.Drawing.Common.Benchmarks;

[MemoryDiagnoser]
public class PathBenchmarks : BenchmarkBase
{
    [Benchmark]
    public void PathAddAndFill_100()
    {
        for (int i = 0; i < 100; i++)
        {
            using var path = new GraphicsPath();
            path.AddRectangle(new RectangleF(10, 10, 200, 200));
            path.AddEllipse(50, 50, 100, 100);
            Graphics.FillPath(RedBrush, path);
        }
    }

    [Benchmark]
    public void PathBezierAndDraw_100()
    {
        for (int i = 0; i < 100; i++)
        {
            using var path = new GraphicsPath();
            path.AddBezier(10, 50, 100, 10, 200, 90, 300, 50);
            path.AddBezier(300, 50, 400, 10, 450, 90, 490, 50);
            Graphics.DrawPath(BlackPen1, path);
        }
    }

    [Benchmark]
    public void PathTransform_1000()
    {
        using var path = new GraphicsPath();
        path.AddRectangle(new RectangleF(0, 0, 100, 100));
        using var matrix = new Matrix();
        for (int i = 0; i < 1000; i++)
        {
            matrix.Reset();
            matrix.Translate(i % 400, i / 400 * 10);
            matrix.Rotate(i % 360);
            path.Transform(matrix);
        }
    }
}
