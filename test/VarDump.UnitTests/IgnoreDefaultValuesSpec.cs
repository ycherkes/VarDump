using System.ComponentModel;
using VarDump.Visitor;
using Xunit;

namespace VarDump.UnitTests;

public class IgnoreDefaultValuesSpec
{
    [Fact]
    public void DumpObjectShouldRespectDefaultValueAttribute()
    {
        var config = new ConfigWithDefaultValueAttributes
        {
            Retries = 0,
            Enabled = false,
            Mode = null
        };

        var dumper = new CSharpDumper(new DumpOptions
        {
            IgnoreNullValues = false
        });

        var result = dumper.Dump(config);

        Assert.Equal("""
                     var configWithDefaultValueAttributes = new ConfigWithDefaultValueAttributes
                     {
                         Retries = 0,
                         Enabled = false,
                         Mode = null
                     };

                     """, result, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void DumpObjectShouldIgnoreTypedValueTypeDefaults()
    {
        var config = new ConfigWithUnsignedDefault
        {
            Retries = 0u
        };

        var dumper = new CSharpDumper(new DumpOptions());

        var result = dumper.Dump(config);

        Assert.Equal("""
                     var configWithUnsignedDefault = new ConfigWithUnsignedDefault();

                     """, result, ignoreLineEndingDifferences: true);
    }

    private sealed class ConfigWithDefaultValueAttributes
    {
        [DefaultValue(5)]
        public int Retries { get; set; } = 5;

        [DefaultValue(true)]
        public bool Enabled { get; set; } = true;

        [DefaultValue("strict")]
        public string Mode { get; set; } = "strict";
    }

    private sealed class ConfigWithUnsignedDefault
    {
        public uint Retries { get; set; } = 0u;
    }
}
