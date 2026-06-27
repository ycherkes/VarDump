using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Threading.Tasks;
using VarDump.Visitor;
using Xunit;

namespace VarDump.UnitTests;

public class TimeSpanSpec
{
    [Fact]
    public async Task DumpTimeSpanZeroCSharp()
    {
        var timeSpan = TimeSpan.Zero;

        var dumper = new CSharpDumper(new DumpOptions
        {
            TypeNamePolicy = TypeNamingPolicy.ShortName,
            GenerateVariableInitializer = false
        });

        var result = dumper.Dump(timeSpan);

        Assert.Contains("TimeSpan.Zero", result);

        var evaluatedResult = await CSharpScript.EvaluateAsync<TimeSpan>(result, ScriptOptions.Default.WithImports("System"));

        Assert.Equal(timeSpan, evaluatedResult);
    }

    [Fact]
    public async Task DumpTimeSpanNonZeroCSharp()
    {
        var timeSpan = TimeSpan.FromHours(3) + TimeSpan.FromMinutes(21) + TimeSpan.FromSeconds(5);

        var dumper = new CSharpDumper(new DumpOptions
        {
            TypeNamePolicy = TypeNamingPolicy.ShortName,
            GenerateVariableInitializer = false
        });

        var result = dumper.Dump(timeSpan);

        Assert.Contains("TimeSpan", result);

        // Verify the generated code is valid by evaluating it with all needed imports
        var evaluatedResult = await CSharpScript.EvaluateAsync<TimeSpan>(result, 
            ScriptOptions.Default.WithImports("System", "System.Globalization"));

        Assert.Equal(timeSpan, evaluatedResult);
    }

    [Fact]
    public void DumpObjectWithTimeSpanPropertyIgnoresDefault()
    {
        var config = new ConfigWithDuration
        {
            Timeout = TimeSpan.Zero
        };

        var dumper = new CSharpDumper(new DumpOptions
        {
            IgnoreDefaultValues = true,
            IgnoreNullValues = false
        });

        var result = dumper.Dump(config);

        Assert.DoesNotContain("Timeout", result);
    }

    [Fact]
    public void DumpObjectWithTimeSpanPropertyIncludesNonDefault()
    {
        var config = new ConfigWithDuration
        {
            Timeout = TimeSpan.FromMinutes(30)
        };

        var dumper = new CSharpDumper(new DumpOptions());

        var result = dumper.Dump(config);

        Assert.Contains("Timeout", result);
    }

    private sealed class ConfigWithDuration
    {
        public TimeSpan Timeout { get; set; }
    }
}
