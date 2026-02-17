using BenchmarkDotNet.Running;

namespace SkiaSharp.Extended.Gif.Benchmarks;

/// <summary>
/// Benchmark runner for GIF encoder/decoder performance testing.
/// Run with: dotnet run -c Release
/// </summary>
public class Program
{
	public static void Main(string[] args)
	{
		var summary = BenchmarkRunner.Run(typeof(Program).Assembly, args: args);
		
		// Print summary
		Console.WriteLine("\nBenchmark Summary:");
		Console.WriteLine($"Total benchmarks run: {summary.Length}");
	}
}
