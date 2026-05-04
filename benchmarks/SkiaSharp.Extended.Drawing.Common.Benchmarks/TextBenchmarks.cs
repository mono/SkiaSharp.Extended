extern alias Gdi;
extern alias Skia;

using BenchmarkDotNet.Attributes;
using GdiDrawing = Gdi::System.Drawing;
using SkiaDrawing = Skia::System.Drawing;

namespace SkiaSharp.Extended.Drawing.Common.Benchmarks;

[MemoryDiagnoser]
public class TextBenchmarks
{
    private GdiDrawing.Bitmap _gdiBmp = null!;
    private GdiDrawing.Graphics _gdiG = null!;
    private GdiDrawing.Font _gdiFont = null!;

    private SkiaDrawing.Bitmap _skiaBmp = null!;
    private SkiaDrawing.Graphics _skiaG = null!;
    private SkiaDrawing.Font _skiaFont = null!;

    [GlobalSetup]
    public void Setup()
    {
        _gdiBmp = new GdiDrawing.Bitmap(500, 500);
        _gdiG = GdiDrawing.Graphics.FromImage(_gdiBmp);
        _gdiFont = new GdiDrawing.Font("Arial", 12);

        _skiaBmp = new SkiaDrawing.Bitmap(500, 500);
        _skiaG = SkiaDrawing.Graphics.FromImage(_skiaBmp);
        _skiaFont = new SkiaDrawing.Font("Arial", 12);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _gdiFont?.Dispose(); _gdiG?.Dispose(); _gdiBmp?.Dispose();
        _skiaFont?.Dispose(); _skiaG?.Dispose(); _skiaBmp?.Dispose();
    }

    [Benchmark(Baseline = true)]
    public void DrawString_GDI()
    {
        for (int i = 0; i < 100; i++)
            _gdiG.DrawString("Hello BenchmarkDotNet!", _gdiFont, GdiDrawing.Brushes.Black, 10, 10 + i * 3);
    }

    [Benchmark]
    public void DrawString_Skia()
    {
        for (int i = 0; i < 100; i++)
            _skiaG.DrawString("Hello BenchmarkDotNet!", _skiaFont, SkiaDrawing.Brushes.Black, 10, 10 + i * 3);
    }

    [Benchmark(Baseline = false)]
    public void MeasureString_GDI()
    {
        for (int i = 0; i < 1000; i++)
            _gdiG.MeasureString("Measure this text", _gdiFont);
    }

    [Benchmark]
    public void MeasureString_Skia()
    {
        for (int i = 0; i < 1000; i++)
            _skiaG.MeasureString("Measure this text", _skiaFont);
    }
}
