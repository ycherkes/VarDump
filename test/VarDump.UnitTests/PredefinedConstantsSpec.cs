using System;
using VarDump.Visitor;
using Xunit;

namespace VarDump.UnitTests;

public class PredefinedConstantsSpec
{
    [Fact]
    public void DumpMaxValueFloatCSharp()
    {
        const float max = float.MaxValue;

        var dumper = new CSharpDumper();

        var result = dumper.Dump(max);

        Assert.Equal("var floatValue = float.MaxValue;\r\n", result);
    }
    
    [Fact]
    public void DumpMaxValueIntegerNoPredefinedConstantsCSharp()
    {
        const int max = int.MaxValue;

        var dumper = new CSharpDumper(new DumpOptions { UsePredefinedConstants = false });

        var result = dumper.Dump(max);

        Assert.Equal("var intValue = 2147483647;\r\n", result);
    }

    [Fact]
    public void DumpMaxValueDateTimeNoPredefinedConstantsCSharp()
    {
        var max = DateTime.MaxValue;

        var dumper = new CSharpDumper(new DumpOptions { UsePredefinedConstants = false });

        var result = dumper.Dump(max);

        Assert.Equal("var dateTime = DateTime.ParseExact(\"9999-12-31T23:59:59.9999999\", \"O\", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);\r\n", result);
    }

    [Fact]
    public void DumpMinValueFloatCSharp()
    {
        const float min = float.MinValue;

        var dumper = new CSharpDumper();
        var result = dumper.Dump(min);

        Assert.Equal("var floatValue = float.MinValue;\r\n", result);
    }

    [Fact]
    public void DumpNaNValueFloatCSharp()
    {
        const float nan = float.NaN;

        var dumper = new CSharpDumper();
        var result = dumper.Dump(nan);

        Assert.Equal("var floatValue = float.NaN;\r\n", result);
    }

    [Fact]
    public void DumpMaxValueSingleVb()
    {
        const float max = float.MaxValue;

        var dumper = new VisualBasicDumper();
        var result = dumper.Dump(max);

        Assert.Equal("Dim singleValue = Single.MaxValue\r\n", result);
    }

    [Fact]
    public void DumpZeroValueByteCSharp()
    {
        const byte zero = 0;

        var dumper = new CSharpDumper();

        var result = dumper.Dump(zero);

        Assert.Equal("var byteValue = 0;\r\n", result);
    }

    [Fact]
    public void DumpZeroValueByteVb()
    {
        const byte zero = 0;

        var dumper = new VisualBasicDumper();

        var result = dumper.Dump(zero);

        Assert.Equal("Dim byteValue = 0\r\n", result);
    }

    [Fact]
    public void DumpZeroValueUShortCSharp()
    {
        const ushort zero = 0;

        var dumper = new CSharpDumper();

        var result = dumper.Dump(zero);

        Assert.Equal("var ushortValue = 0;\r\n", result);
    }

    [Fact]
    public void DumpZeroValueUShortVb()
    {
        const ushort zero = 0;

        var dumper = new VisualBasicDumper();

        var result = dumper.Dump(zero);

        Assert.Equal("Dim uShortValue = 0US\r\n", result);
    }

    [Fact]
    public void DumpMaxValueIntegerNoPredefinedConstantsVb()
    {
        const int max = int.MaxValue;

        var dumper = new VisualBasicDumper(new DumpOptions { UsePredefinedConstants = false });

        var result = dumper.Dump(max);

        Assert.Equal("Dim integerValue = 2147483647\r\n", result);
    }

    [Fact]
    public void DumpMaxValueDateTimeNoPredefinedConstantsVb()
    {
        var max = DateTime.MaxValue;

        var dumper = new VisualBasicDumper(new DumpOptions { UsePredefinedConstants = false });

        var result = dumper.Dump(max);

        Assert.Equal("Dim dateValue = Date.ParseExact(\"9999-12-31T23:59:59.9999999\", \"O\", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)\r\n", result);
    }
}