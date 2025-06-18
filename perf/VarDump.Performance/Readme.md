// * Summary *

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.3775)
12th Gen Intel Core i7-1255U, 1 CPU, 12 logical and 10 physical cores
.NET SDK 9.0.203
  [Host]     : .NET 9.0.4 (9.0.425.16305), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.4 (9.0.425.16305), X64 RyuJIT AVX2


| Method                 | Mean      | Error     | StdDev    | Median    | Gen0       | Gen1      | Gen2      | Allocated |
|----------------------- |----------:|----------:|----------:|----------:|-----------:|----------:|----------:|----------:|
| CSharpDumper_Perf      | 224.61 ms |  4.412 ms |  7.842 ms | 225.16 ms | 65000.0000 | 8000.0000 | 1000.0000 |  422.3 MB |
| VisualBasicDumper_Perf | 216.99 ms |  4.340 ms |  9.058 ms | 217.54 ms | 65000.0000 | 7000.0000 | 1000.0000 | 422.39 MB |
| ObjectDumperNet_Perf   | 690.60 ms | 13.501 ms | 23.646 ms | 691.57 ms | 33000.0000 | 7000.0000 | 1000.0000 | 226.74 MB |
| MsJson_Perf            |  21.71 ms |  0.428 ms |  0.922 ms |  21.34 ms |   312.5000 |  312.5000 |  312.5000 |   26.7 MB |
| NewtonsoftJson_Perf    |  52.78 ms |  0.678 ms |  0.601 ms |  53.04 ms |  6555.5556 | 3888.8889 | 1222.2222 |  58.94 MB |

// * Warnings *
MultimodalDistribution
  BenchmarkCustomObject.VisualBasicDumper_Perf: Default -> It seems that the distribution can have several modes (mValue = 3.09)

// * Hints *
Outliers
  BenchmarkCustomObject.CSharpDumper_Perf: Default      -> 3 outliers were removed (247.51 ms..253.17 ms)
  BenchmarkCustomObject.VisualBasicDumper_Perf: Default -> 1 outlier  was  removed (246.62 ms)
  BenchmarkCustomObject.ObjectDumperNet_Perf: Default   -> 3 outliers were removed (793.40 ms..857.01 ms)
  BenchmarkCustomObject.MsJson_Perf: Default            -> 7 outliers were removed (24.64 ms..35.40 ms)
  BenchmarkCustomObject.NewtonsoftJson_Perf: Default    -> 1 outlier  was  removed (55.22 ms)

// * Legends *
  Mean      : Arithmetic mean of all measurements
  Error     : Half of 99.9% confidence interval
  StdDev    : Standard deviation of all measurements
  Median    : Value separating the higher half of all measurements (50th percentile)
  Gen0      : GC Generation 0 collects per 1000 operations
  Gen1      : GC Generation 1 collects per 1000 operations
  Gen2      : GC Generation 2 collects per 1000 operations
  Allocated : Allocated memory per single operation (managed only, inclusive, 1KB = 1024B)
  1 ms      : 1 Millisecond (0.001 sec)

// * Diagnostic Output - MemoryDiagnoser *