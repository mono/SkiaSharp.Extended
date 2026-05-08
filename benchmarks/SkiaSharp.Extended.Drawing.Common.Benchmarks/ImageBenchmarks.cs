extern alias Gdi;
extern alias Skia;

using System;
using System.Drawing;
using System.IO;
using BenchmarkDotNet.Attributes;
using GdiDrawing = Gdi::System.Drawing;
using GdiImaging = Gdi::System.Drawing.Imaging;
using GdiDrawing2D = Gdi::System.Drawing.Drawing2D;
using SkiaDrawing = Skia::System.Drawing;
using SkiaImaging = Skia::System.Drawing.Imaging;
using SkiaDrawing2D = Skia::System.Drawing.Drawing2D;

namespace SkiaSharp.Extended.Drawing.Common.Benchmarks;

[MemoryDiagnoser]
public class ImageBenchmarks
{
    private GdiDrawing.Bitmap _gdiBmp = null!;
    private SkiaDrawing.Bitmap _skiaBmp = null!;

    [GlobalSetup]
    public void Setup()
    {
        _gdiBmp = new GdiDrawing.Bitmap(500, 500);
        _skiaBmp = new SkiaDrawing.Bitmap(500, 500);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _gdiBmp?.Dispose();
        _skiaBmp?.Dispose();
    }

    [Benchmark(Baseline = true)]
    public void SetPixel_GDI()
    {
        for (int i = 0; i < 10000; i++)
            _gdiBmp.SetPixel(i % 500, i / 500 % 500, Color.Red);
    }

    [Benchmark]
    public void SetPixel_Skia()
    {
        for (int i = 0; i < 10000; i++)
            _skiaBmp.SetPixel(i % 500, i / 500 % 500, Color.Red);
    }

    [Benchmark(Baseline = false)]
    public void GetPixel_GDI()
    {
        for (int i = 0; i < 10000; i++)
            _gdiBmp.GetPixel(i % 500, i / 500 % 500);
    }

    [Benchmark]
    public void GetPixel_Skia()
    {
        for (int i = 0; i < 10000; i++)
            _skiaBmp.GetPixel(i % 500, i / 500 % 500);
    }

    [Benchmark(Baseline = false)]
    public void CreateDispose_GDI()
    {
        for (int i = 0; i < 100; i++)
        {
            using var bmp = new GdiDrawing.Bitmap(200, 200);
            using var g = GdiDrawing.Graphics.FromImage(bmp);
        }
    }

    [Benchmark]
    public void CreateDispose_Skia()
    {
        for (int i = 0; i < 100; i++)
        {
            using var bmp = new SkiaDrawing.Bitmap(200, 200);
            using var g = SkiaDrawing.Graphics.FromImage(bmp);
        }
    }

    [Benchmark(Baseline = false)]
    public void SavePng_GDI()
    {
        using var ms = new MemoryStream();
        _gdiBmp.Save(ms, Gdi::System.Drawing.Imaging.ImageFormat.Png);
    }

    [Benchmark]
    public void SavePng_Skia()
    {
        using var ms = new MemoryStream();
        _skiaBmp.Save(ms, Skia::System.Drawing.Imaging.ImageFormat.Png);
    }
}
