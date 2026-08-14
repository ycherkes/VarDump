using System.Reflection;
using BenchmarkDotNet.Running;
using VarDump;
using VarDump.Performance;

if (TryRunProfileWorkload(args))
{
    return;
}

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

static bool TryRunProfileWorkload(string[] args)
{
    if (args.Length is < 1 or > 2)
    {
        return false;
    }

    var iterations = args.Length == 2 && int.TryParse(args[1], out var parsedIterations)
        ? parsedIterations
        : 10;

    if (iterations <= 0)
    {
        throw new ArgumentOutOfRangeException(nameof(args), "Iteration count must be positive.");
    }

    switch (args[0])
    {
        case "--profile-custom-csharp":
            RunProfileWorkload(typeof(BenchmarkCustomObject), new CSharpDumper(), iterations);
            return true;
        case "--profile-custom-vb":
            RunProfileWorkload(typeof(BenchmarkCustomObject), new VisualBasicDumper(), iterations);
            return true;
        case "--profile-anonymous-csharp":
            RunProfileWorkload(typeof(BenchmarkAnonymousObject), new CSharpDumper(), iterations);
            return true;
        case "--profile-anonymous-vb":
            RunProfileWorkload(typeof(BenchmarkAnonymousObject), new VisualBasicDumper(), iterations);
            return true;
        default:
            return false;
    }
}

static void RunProfileWorkload(Type benchmarkType, IDumper dumper, int iterations)
{
    var input = benchmarkType.GetField("Variable", BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null)
                ?? throw new InvalidOperationException("Benchmark input was not found.");

    string? result = null;
    for (var iteration = 0; iteration < iterations; iteration++)
    {
        result = dumper.Dump(input);
    }

    GC.KeepAlive(result);
}
