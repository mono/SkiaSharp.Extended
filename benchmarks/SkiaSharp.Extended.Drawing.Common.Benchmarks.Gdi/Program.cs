using BenchmarkDotNet.Running;
using SkiaSharp.Extended.Drawing.Common.Benchmarks;

BenchmarkSwitcher.FromAssembly(typeof(FillBenchmarks).Assembly).Run(args);
