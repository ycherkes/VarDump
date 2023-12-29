using System;
using Xunit;

namespace VarDump.UnitTests;

public class FlagsSpec
{
    [Fact]
    public void DumpFlagsCsharp()
    {
        var flagsVar = TestEnum.First | TestEnum.Third;

        var dumper = new CSharpDumper();

        var result = dumper.Dump(flagsVar);

        Assert.Equal("var testEnum = TestEnum.First | TestEnum.Third;\r\n", result);
    }

    [Fact]
    public void DumpFlagsVb()
    {
        var flagsVar = TestEnum.Second | TestEnum.Third;

        var dumper = new VisualBasicDumper();

        var result = dumper.Dump(flagsVar);

        Assert.Equal("Dim testEnumValue = TestEnum.Second Or TestEnum.Third\r\n", result);
    }

    [Flags]
    private enum TestEnum
    {
        First = 1,
        Second = 2,
        Third = 4
    }
}