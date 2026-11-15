using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Environments;

namespace VarDump.Performance;

/// <summary>
/// Low-noise configuration: fixed number of iterations, 4 operations per iteration, several
/// independent processes (averages out per-process effects such as P/E-core placement),
/// non-concurrent workstation GC and optional CPU affinity.
/// Set VARDUMP_BENCH_AFFINITY (hex mask, e.g. FF) to pin the benchmark process to chosen cores.
/// </summary>
public class PreciseConfig : ManualConfig
{
    public PreciseConfig()
    {
        var job = Job.Default
            .WithGcServer(false)
            .WithGcConcurrent(false)
            .WithLaunchCount(3)
            .WithWarmupCount(5)
            .WithIterationCount(30)
            // Fixed operation count instead of IterationTime: the first (jitting) call takes longer than
            // any reasonable IterationTime, so BenchmarkDotNet skips the pilot stage and falls back to
            // 1 op per iteration, which yields <100 ms iterations and per-iteration GC pause noise.
            .WithInvocationCount(4)
            .WithUnrollFactor(1);

        var affinity = Environment.GetEnvironmentVariable("VARDUMP_BENCH_AFFINITY");
        if (!string.IsNullOrWhiteSpace(affinity))
        {
            var mask = Convert.ToInt64(affinity, 16);
            job = job.WithAffinity((IntPtr)mask);
        }

        AddJob(job);
    }
}
