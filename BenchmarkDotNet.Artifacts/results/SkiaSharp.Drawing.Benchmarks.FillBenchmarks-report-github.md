```

BenchmarkDotNet v0.14.0, macOS 26.4.1 (25E253) [Darwin 25.4.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 10.0.201
  [Host]   : .NET 10.0.5 (10.0.526.15411), Arm64 RyuJIT AdvSIMD
  ShortRun : .NET 10.0.5 (10.0.526.15411), Arm64 RyuJIT AdvSIMD

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method    | Mean     | Error     | StdDev    | Allocated |
|---------- |---------:|----------:|----------:|----------:|
| Clear_100 | 1.537 ms | 0.2084 ms | 0.0114 ms |         - |
