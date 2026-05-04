extern alias Gdi;
extern alias Skia;

using System;
using System.IO;
using BenchmarkDotNet.Attributes;
using GdiDrawing = Gdi::System.Drawing;
using SkiaDrawing = Skia::System.Drawing;

namespace SkiaSharp.Extended.Drawing.Common.Benchmarks;

[MemoryDiagnoser]
public class FillBenchmarks
{
    private GdiDrawing.Bitmap _gdiBmp = null!;
    private GdiDrawing.Graphics _gdiG = null!;
    private GdiDrawing.SolidBrush _gdiRedBrush = null!;
    private GdiDrawing.SolidBrush _gdiBlueBrush = null!;

    private SkiaDrawing.Bitmap _skiaBmp = null!;
    private SkiaDrawing.Graphics _skiaG = null!;
    private SkiaDrawing.SolidBrush _skiaRedBrush = null!;
    private SkiaDrawing.SolidBrush _skiaBlueBrush = null!;

    [GlobalSetup]
    public void Setup()
    {
        _gdiBmp = new GdiDrawing.Bitmap(500, 500);
        _gdiG = GdiDrawing.Graphics.FromImage(_gdiBmp);
        _gdiG.SmoothingMode = Gdi::System.Drawing.Drawing2D.SmoothingMode.None;
        _gdiRedBrush = new GdiDrawing.SolidBrush(GdiDrawing.Color.Red);
        _gdiBlueBrush = new GdiDrawing.SolidBrush(GdiDrawing.Color.Blue);

        _skiaBmp = new SkiaDrawing.Bitmap(500, 500);
        _skiaG = SkiaDrawing.Graphics.FromImage(_skiaBmp);
        _skiaG.SmoothingMode = Skia::System.Drawing.Drawing2D.SmoothingMode.None;
        _skiaRedBrush = new SkiaDrawing.SolidBrush(SkiaDrawing.Color.Red);
        _skiaBlueBrush = new SkiaDrawing.SolidBrush(SkiaDrawing.Color.Blue);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _gdiRedBrush?.Dispose(); _gdiBlueBrush?.Dispose();
        _gdiG?.Dispose(); _gdiBmp?.Dispose();
        _skiaRedBrush?.Dispose(); _skiaBlueBrush?.Dispose();
        _skiaG?.Dispose(); _skiaBmp?.Dispose();
    }

    [Benchmark(Baseline = true)]
    public void FillRectangle_GDI()
    {
        for (int i = 0; i < 100; i++)
            _gdiG.FillRectangle(_gdiRedBrush, 10 + i, 10 + i, 200, 200);
    }

    [Benchmark]
    public void FillRectangle_Skia()
    {
        for (int i = 0; i < 100; i++)
            _skiaG.FillRectangle(_skiaRedBrush, 10 + i, 10 + i, 200, 200);
    }

    [Benchmark(Baseline = false)]
    public void FillEllipse_GDI()
    {
        for (int i = 0; i < 100; i++)
            _gdiG.FillEllipse(_gdiBlueBrush, 10 + i, 10 + i, 200, 200);
    }

    [Benchmark]
    public void FillEllipse_Skia()
    {
        for (int i = 0; i < 100; i++)
            _skiaG.FillEllipse(_skiaBlueBrush, 10 + i, 10 + i, 200, 200);
    }

    [Benchmark(Baseline = false)]
    public void Clear_GDI()
    {
        for (int i = 0; i < 100; i++)
            _gdiG.Clear(GdiDrawing.Color.White);
    }

    [Benchmark]
    public void Clear_Skia()
    {
        for (int i = 0; i < 100; i++)
            _skiaG.Clear(SkiaDrawing.Color.White);
    }
}
