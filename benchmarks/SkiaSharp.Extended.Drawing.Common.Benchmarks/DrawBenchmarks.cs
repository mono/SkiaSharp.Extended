extern alias Gdi;
extern alias Skia;

using BenchmarkDotNet.Attributes;
using GdiDrawing = Gdi::System.Drawing;
using SkiaDrawing = Skia::System.Drawing;

namespace SkiaSharp.Extended.Drawing.Common.Benchmarks;

[MemoryDiagnoser]
public class DrawBenchmarks
{
    private GdiDrawing.Bitmap _gdiBmp = null!;
    private GdiDrawing.Graphics _gdiG = null!;
    private GdiDrawing.Pen _gdiPen1 = null!;
    private GdiDrawing.Pen _gdiPen3 = null!;

    private SkiaDrawing.Bitmap _skiaBmp = null!;
    private SkiaDrawing.Graphics _skiaG = null!;
    private SkiaDrawing.Pen _skiaPen1 = null!;
    private SkiaDrawing.Pen _skiaPen3 = null!;

    [GlobalSetup]
    public void Setup()
    {
        _gdiBmp = new GdiDrawing.Bitmap(500, 500);
        _gdiG = GdiDrawing.Graphics.FromImage(_gdiBmp);
        _gdiPen1 = new GdiDrawing.Pen(GdiDrawing.Color.Black, 1);
        _gdiPen3 = new GdiDrawing.Pen(GdiDrawing.Color.Black, 3);

        _skiaBmp = new SkiaDrawing.Bitmap(500, 500);
        _skiaG = SkiaDrawing.Graphics.FromImage(_skiaBmp);
        _skiaPen1 = new SkiaDrawing.Pen(SkiaDrawing.Color.Black, 1);
        _skiaPen3 = new SkiaDrawing.Pen(SkiaDrawing.Color.Black, 3);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _gdiPen1?.Dispose(); _gdiPen3?.Dispose();
        _gdiG?.Dispose(); _gdiBmp?.Dispose();
        _skiaPen1?.Dispose(); _skiaPen3?.Dispose();
        _skiaG?.Dispose(); _skiaBmp?.Dispose();
    }

    [Benchmark(Baseline = true)]
    public void DrawLine_GDI()
    {
        for (int i = 0; i < 1000; i++)
            _gdiG.DrawLine(_gdiPen1, 0, i % 500, 499, i % 500);
    }

    [Benchmark]
    public void DrawLine_Skia()
    {
        for (int i = 0; i < 1000; i++)
            _skiaG.DrawLine(_skiaPen1, 0, i % 500, 499, i % 500);
    }

    [Benchmark(Baseline = false)]
    public void DrawRectangle_GDI()
    {
        for (int i = 0; i < 100; i++)
            _gdiG.DrawRectangle(_gdiPen1, 10 + i, 10 + i, 200, 200);
    }

    [Benchmark]
    public void DrawRectangle_Skia()
    {
        for (int i = 0; i < 100; i++)
            _skiaG.DrawRectangle(_skiaPen1, 10 + i, 10 + i, 200, 200);
    }

    [Benchmark(Baseline = false)]
    public void DrawEllipse_GDI()
    {
        for (int i = 0; i < 100; i++)
            _gdiG.DrawEllipse(_gdiPen3, 10 + i, 10 + i, 200, 200);
    }

    [Benchmark]
    public void DrawEllipse_Skia()
    {
        for (int i = 0; i < 100; i++)
            _skiaG.DrawEllipse(_skiaPen3, 10 + i, 10 + i, 200, 200);
    }

    [Benchmark(Baseline = false)]
    public void DrawBezier_GDI()
    {
        for (int i = 0; i < 100; i++)
            _gdiG.DrawBezier(_gdiPen1, 10f, 50f, 100f, 10f, 200f, 90f, 300f, 50f);
    }

    [Benchmark]
    public void DrawBezier_Skia()
    {
        for (int i = 0; i < 100; i++)
            _skiaG.DrawBezier(_skiaPen1, 10f, 50f, 100f, 10f, 200f, 90f, 300f, 50f);
    }
}
