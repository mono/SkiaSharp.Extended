using System.Drawing;
using BenchmarkDotNet.Attributes;

namespace SkiaSharp.Extended.Drawing.Common.Benchmarks;

[MemoryDiagnoser]
public class TextBenchmarks : BenchmarkBase
{
    [Benchmark]
    public void DrawString_100()
    {
        for (int i = 0; i < 100; i++)
            Graphics.DrawString("Hello BenchmarkDotNet!", DefaultFont, Brushes.Black, 10, 10 + i * 3);
    }

    [Benchmark]
    public void MeasureString_1000()
    {
        for (int i = 0; i < 1000; i++)
            Graphics.MeasureString("Measure this text", DefaultFont);
    }
}
