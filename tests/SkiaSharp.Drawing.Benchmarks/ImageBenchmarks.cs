using System.Drawing;
using System.Drawing.Imaging;
using BenchmarkDotNet.Attributes;

namespace SkiaSharp.Drawing.Benchmarks;

[MemoryDiagnoser]
public class ImageBenchmarks : BenchmarkBase
{
    private Bitmap _sourceImage = null!;

    public override void Setup()
    {
        base.Setup();
        _sourceImage = new Bitmap(100, 100);
        using var sg = Graphics.FromImage(_sourceImage);
        sg.Clear(Color.Green);
    }

    public override void Cleanup()
    {
        _sourceImage?.Dispose();
        base.Cleanup();
    }

    [Benchmark]
    public void DrawImage_100()
    {
        for (int i = 0; i < 100; i++)
            Graphics.DrawImage(_sourceImage, i * 3, i * 3);
    }

    [Benchmark]
    public void DrawImage_Scaled_100()
    {
        for (int i = 0; i < 100; i++)
            Graphics.DrawImage(_sourceImage, new Rectangle(0, 0, 200, 200));
    }

    [Benchmark]
    public void SetPixel_10000()
    {
        for (int i = 0; i < 10000; i++)
            Bitmap.SetPixel(i % 500, i / 500 % 500, Color.Red);
    }

    [Benchmark]
    public void GetPixel_10000()
    {
        for (int i = 0; i < 10000; i++)
            Bitmap.GetPixel(i % 500, i / 500 % 500);
    }

    [Benchmark]
    public void SavePng()
    {
        using var ms = new MemoryStream();
        Bitmap.Save(ms, ImageFormat.Png);
    }

    [Benchmark]
    public void CreateDispose_100()
    {
        for (int i = 0; i < 100; i++)
        {
            using var bmp = new Bitmap(200, 200);
            using var g = Graphics.FromImage(bmp);
        }
    }
}
