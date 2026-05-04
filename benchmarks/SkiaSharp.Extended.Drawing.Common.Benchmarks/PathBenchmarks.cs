extern alias Gdi;
extern alias Skia;

using BenchmarkDotNet.Attributes;
using GdiDrawing = Gdi::System.Drawing;
using GdiDrawing2D = Gdi::System.Drawing.Drawing2D;
using SkiaDrawing = Skia::System.Drawing;
using SkiaDrawing2D = Skia::System.Drawing.Drawing2D;

namespace SkiaSharp.Extended.Drawing.Common.Benchmarks;

[MemoryDiagnoser]
public class PathBenchmarks
{
    private GdiDrawing.Bitmap _gdiBmp = null!;
    private GdiDrawing.Graphics _gdiG = null!;
    private GdiDrawing.SolidBrush _gdiBrush = null!;

    private SkiaDrawing.Bitmap _skiaBmp = null!;
    private SkiaDrawing.Graphics _skiaG = null!;
    private SkiaDrawing.SolidBrush _skiaBrush = null!;

    [GlobalSetup]
    public void Setup()
    {
        _gdiBmp = new GdiDrawing.Bitmap(500, 500);
        _gdiG = GdiDrawing.Graphics.FromImage(_gdiBmp);
        _gdiBrush = new GdiDrawing.SolidBrush(GdiDrawing.Color.Red);

        _skiaBmp = new SkiaDrawing.Bitmap(500, 500);
        _skiaG = SkiaDrawing.Graphics.FromImage(_skiaBmp);
        _skiaBrush = new SkiaDrawing.SolidBrush(SkiaDrawing.Color.Red);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _gdiBrush?.Dispose(); _gdiG?.Dispose(); _gdiBmp?.Dispose();
        _skiaBrush?.Dispose(); _skiaG?.Dispose(); _skiaBmp?.Dispose();
    }

    [Benchmark(Baseline = true)]
    public void PathFill_GDI()
    {
        for (int i = 0; i < 100; i++)
        {
            using var path = new GdiDrawing2D.GraphicsPath();
            path.AddRectangle(new GdiDrawing.RectangleF(10, 10, 200, 200));
            path.AddEllipse(50, 50, 100, 100);
            _gdiG.FillPath(_gdiBrush, path);
        }
    }

    [Benchmark]
    public void PathFill_Skia()
    {
        for (int i = 0; i < 100; i++)
        {
            using var path = new SkiaDrawing2D.GraphicsPath();
            path.AddRectangle(new SkiaDrawing.RectangleF(10, 10, 200, 200));
            path.AddEllipse(50, 50, 100, 100);
            _skiaG.FillPath(_skiaBrush, path);
        }
    }
}
