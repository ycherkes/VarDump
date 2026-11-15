# Running isolated benchmarks

The benchmark entry point accepts BenchmarkDotNet command-line filters. The project multi-targets `net10.0` and `net472`, so pass the target framework explicitly with `-f`.

```powershell
# List the methods selected by a filter.
dotnet run -c Release -f net10.0 --project perf/VarDump.Performance/VarDump.Performance.csproj -- --filter "*BenchmarkCustomObject*" --list flat

# Run only VarDump's C# custom-object workload.
dotnet run -c Release -f net10.0 --project perf/VarDump.Performance/VarDump.Performance.csproj -- --filter "*BenchmarkCustomObject.CSharpDumper_Perf*"

# Run every benchmark (14 methods: 7 for custom objects, 7 for anonymous objects; about 7 minutes).
dotnet run -c Release -f net10.0 --project perf/VarDump.Performance/VarDump.Performance.csproj -- --filter "*"
```

`*_Perf` writes to and returns a string. `*_TextWriterNull` renders the same object graph into `TextWriter.Null`, isolating traversal and formatting from the final output-string allocation.

## Profiling workloads

These commands run the same static benchmark input directly, without BenchmarkDotNet's generated child process. The optional final argument is the iteration count (default: 10).

```powershell
dotnet run -c Release -f net10.0 --project perf/VarDump.Performance/VarDump.Performance.csproj -- --profile-custom-csharp 10
dotnet run -c Release -f net10.0 --project perf/VarDump.Performance/VarDump.Performance.csproj -- --profile-anonymous-csharp 10
```

## Low-noise run (recommended for comparisons)

`perf/run-precise-benchmarks.ps1` runs all benchmarks with `PreciseConfig`: 3 separate processes (launches), 5 warmup + 30 measured iterations, 4 operations per iteration, non-concurrent workstation GC, and the process pinned to the P-cores (`VARDUMP_BENCH_AFFINITY`, default mask `C03C03` = logical processors 0, 1, 10-13, 22, 23 on Core Ultra 9 275HX). It takes about 15 minutes. Close other applications and keep the laptop plugged in while it runs.

`InvocationCount` is fixed instead of `IterationTime`: the first (jitting) call of each workload takes about 770 ms, which makes BenchmarkDotNet skip its pilot stage and silently fall back to 1 operation per iteration.

## Latest results (2026-11-15, low-noise config)

Environment: BenchmarkDotNet v0.15.8, Windows 11 (10.0.26300.9974), Intel Core Ultra 9 275HX 2.70GHz (24 logical and 24 physical cores), .NET SDK 10.0.403, .NET 10.0.14 (X64 RyuJIT x86-64-v3), `PreciseConfig` job, `MemoryDiagnoser` enabled.

Each workload is an array of 10,000 objects with 16 properties and one nested object. Allocated is per single operation. Gen0/1/2 columns are collections per 1000 operations and depend on the invocation count, so compare them only within this table.

### Custom objects (`BenchmarkCustomObject`)

| Method                           | Mean      | Error    | StdDev    | Median    | Gen0       | Gen1      | Gen2     | Allocated |
|--------------------------------- |----------:|---------:|----------:|----------:|-----------:|----------:|---------:|----------:|
| CSharpDumper_Perf                | 111.59 ms | 2.102 ms |  5.789 ms | 110.34 ms |  2000.0000 | 1250.0000 | 750.0000 | 207.03 MB |
| VisualBasicDumper_Perf           | 111.22 ms | 2.374 ms |  6.579 ms | 109.27 ms |  2000.0000 | 1250.0000 | 750.0000 | 206.26 MB |
| CSharpDumper_TextWriterNull      |  56.22 ms | 1.462 ms |  4.027 ms |  55.26 ms |  7750.0000 |         - |        - | 139.68 MB |
| VisualBasicDumper_TextWriterNull |  59.69 ms | 0.732 ms |  2.042 ms |  59.63 ms |  7500.0000 |         - |        - | 138.23 MB |
| ObjectDumperNet_Perf             | 486.57 ms | 9.073 ms | 25.141 ms | 481.40 ms |  2500.0000 | 1250.0000 | 750.0000 | 239.26 MB |
| MsJson_Perf                      |  14.11 ms | 0.512 ms |  1.418 ms |  14.07 ms |          - |         - |        - |  27.06 MB |
| NewtonsoftJson_Perf              |  37.64 ms | 0.718 ms |  2.002 ms |  37.62 ms |   750.0000 |  750.0000 | 750.0000 |   59.9 MB |

### Anonymous objects (`BenchmarkAnonymousObject`)

| Method                           | Mean      | Error    | StdDev    | Gen0       | Gen1      | Gen2     | Allocated |
|--------------------------------- |----------:|---------:|----------:|-----------:|----------:|---------:|----------:|
| CSharpDumper_Perf                | 130.51 ms | 2.939 ms |  7.996 ms |  3000.0000 | 1500.0000 | 750.0000 | 267.94 MB |
| VisualBasicDumper_Perf           | 131.25 ms | 2.411 ms |  6.559 ms |  2750.0000 | 1250.0000 | 750.0000 | 268.01 MB |
| CSharpDumper_TextWriterNull      |  67.55 ms | 1.186 ms |  3.267 ms | 11000.0000 |         - |        - | 198.46 MB |
| VisualBasicDumper_TextWriterNull |  66.76 ms | 0.897 ms |  2.501 ms | 10750.0000 |         - |        - | 197.08 MB |
| ObjectDumperNet_Perf             | 450.22 ms | 4.857 ms | 13.538 ms |  2000.0000 | 1250.0000 | 750.0000 | 226.67 MB |
| MsJson_Perf                      |  14.22 ms | 0.476 ms |  1.318 ms |          - |         - |        - |   26.7 MB |
| NewtonsoftJson_Perf              |  36.79 ms | 0.665 ms |  1.852 ms |   750.0000 |  750.0000 | 750.0000 |  58.94 MB |

### Notes

- C# and Visual Basic dumpers perform the same (within 1-4%, inside the error bars) for both `*_Perf` and `*_TextWriterNull`. The 20% gap between them in the default-config run below (111.86 ms vs 134.06 ms) was measurement noise.
- StdDev of the VarDump `*_Perf` rows dropped from 10-17 ms to 6-8 ms (about 5-6% of the mean), but the machine is still not silent, so treat differences below roughly 5-10% as noise. For a before/after comparison of a code change, run this configuration on both versions on the same day.
- Writing the result into a string costs about as much as the dump itself: `*_Perf` is roughly 2x `*_TextWriterNull`. About two thirds (custom objects) to three quarters (anonymous objects) of the allocated memory remains when the output is discarded into `TextWriter.Null`.
- Compared to the JSON serializers, VarDump generates code and is therefore several times slower and allocates several times more than System.Text.Json and Newtonsoft.Json; compared to ObjectDumper.NET it is roughly 3.5-4.5 times faster.

## Default-config run (2026-11-15)

Same machine and runtime, default BenchmarkDotNet job (no affinity, concurrent GC, auto-tuned iteration count). Noisy: StdDev of VarDump `*_Perf` is 10-17 ms and most VarDump distributions are reported as bimodal or multimodal. Kept to show the effect of the low-noise configuration.

| Benchmark                        | Custom Mean | Custom StdDev | Anonymous Mean | Anonymous StdDev |
|--------------------------------- |------------:|--------------:|---------------:|-----------------:|
| CSharpDumper_Perf                |   111.86 ms |      10.20 ms |      136.21 ms |         10.78 ms |
| VisualBasicDumper_Perf           |   134.06 ms |      16.34 ms |      135.48 ms |         16.66 ms |
| CSharpDumper_TextWriterNull      |    80.26 ms |       6.59 ms |       68.29 ms |          3.07 ms |
| VisualBasicDumper_TextWriterNull |    79.59 ms |       7.87 ms |       69.89 ms |          3.81 ms |
| ObjectDumperNet_Perf             |   602.06 ms |       4.51 ms |      425.73 ms |         19.53 ms |
| MsJson_Perf                      |    16.13 ms |       0.54 ms |       12.96 ms |          0.57 ms |
| NewtonsoftJson_Perf              |    45.23 ms |       1.49 ms |       42.27 ms |          2.96 ms |
## Previous result (.NET 9, 2025)

Kept for reference only. It was measured with BenchmarkDotNet v0.14.0 on Windows 11 (10.0.26100.3775), 12th Gen Intel Core i7-1255U, .NET SDK 9.0.203 / .NET 9.0.4, before the `*_TextWriterNull` benchmarks existed. The hardware, runtime and benchmark input differ from the latest results, so the two tables should not be used to measure how VarDump itself changed.

| Method                 | Mean      | Error     | StdDev    | Median    | Gen0       | Gen1      | Gen2      | Allocated |
|----------------------- |----------:|----------:|----------:|----------:|-----------:|----------:|----------:|----------:|
| CSharpDumper_Perf      | 224.61 ms |  4.412 ms |  7.842 ms | 225.16 ms | 65000.0000 | 8000.0000 | 1000.0000 |  422.3 MB |
| VisualBasicDumper_Perf | 216.99 ms |  4.340 ms |  9.058 ms | 217.54 ms | 65000.0000 | 7000.0000 | 1000.0000 | 422.39 MB |
| ObjectDumperNet_Perf   | 690.60 ms | 13.501 ms | 23.646 ms | 691.57 ms | 33000.0000 | 7000.0000 | 1000.0000 | 226.74 MB |
| MsJson_Perf            |  21.71 ms |  0.428 ms |  0.922 ms |  21.34 ms |   312.5000 |  312.5000 |  312.5000 |   26.7 MB |
| NewtonsoftJson_Perf    |  52.78 ms |  0.678 ms |  0.601 ms |  53.04 ms |  6555.5556 | 3888.8889 | 1222.2222 |  58.94 MB |
