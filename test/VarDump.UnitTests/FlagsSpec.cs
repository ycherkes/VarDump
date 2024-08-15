using System;
using Xunit;

namespace VarDump.UnitTests;

public class FlagsSpec
{
    [Fact]
    public void DumpFlagsCSharp()
    {
        var flagsVar = TestEnum.First | TestEnum.Third;

        var dumper = new CSharpDumper();

        var result = dumper.Dump(flagsVar);

        Assert.Equal("var testEnum = TestEnum.First | TestEnum.Third;\r\n", result);
    }

    [Fact]
    public void DumpFlagsCSharp_Zero()
    {
        TestEnum flagsVar = 0;

        var dumper = new CSharpDumper();

        var result = dumper.Dump(flagsVar);

        Assert.Equal("var testEnum = 0;\r\n", result);
    }

    [Fact]
    public void DumpEnumCSharp_Minus54()
    {
        TestEnum flagsVar = (TestEnum)(object)-54;

        var dumper = new CSharpDumper();

        var result = dumper.Dump(flagsVar);

        Assert.Equal("var testEnum = (TestEnum)(object)-54;\r\n", result);
    }

    [Fact]
    public void DumpFlagsVb()
    {
        var flagsVar = TestEnum.Second | TestEnum.Third;

        var dumper = new VisualBasicDumper();

        var result = dumper.Dump(flagsVar);

        Assert.Equal("Dim testEnumValue = TestEnum.Second Or TestEnum.Third\r\n", result);
    }

    [Fact]
    public void DumpFlagsVB_Zero()
    {
        TestEnum flagsVar = 0;

        var dumper = new VisualBasicDumper();

        var result = dumper.Dump(flagsVar);

        Assert.Equal("Dim testEnumValue = 0\r\n", result);
    }

    [Fact]
    public void DumpEnumVb_Minus54()
    {
        TestEnum flagsVar = (TestEnum)(object)-54;

        var dumper = new VisualBasicDumper();

        var result = dumper.Dump(flagsVar);

        Assert.Equal("Dim testEnumValue = CType(CType(-54, Object), TestEnum)\r\n", result);
    }

    [Flags]
    private enum TestEnum
    {
        Minus55 = -55,
        First = 1,
        Second = 2,
        Third = 4
    }
}