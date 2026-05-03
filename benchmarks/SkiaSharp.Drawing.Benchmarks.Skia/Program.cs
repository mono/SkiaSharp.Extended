using BenchmarkDotNet.Running;
using SkiaSharp.Drawing.Benchmarks;

BenchmarkSwitcher.FromAssembly(typeof(FillBenchmarks).Assembly).Run(args);
