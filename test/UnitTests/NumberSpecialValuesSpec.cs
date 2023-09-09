using VarDumpExtended;
using Xunit;

namespace UnitTests;

public class NumberSpecialValuesSpec
{
    [Fact]
    public void DumpMaxValueFloatCsharp()
    {
        const float max = float.MaxValue;

        var dumper = new CSharpDumper();

        var result = dumper.Dump(max);

        Assert.Equal("var floatValue = float.MaxValue;\r\n", result);
    }

    [Fact]
    public void DumpMinValueFloatCsharp()
    {
        const float min = float.MinValue;

        var dumper = new CSharpDumper();
        var result = dumper.Dump(min);

        Assert.Equal("var floatValue = float.MinValue;\r\n", result);
    }

    [Fact]
    public void DumpNaNValueFloatCsharp()
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
    public void DumpZeroValueByteCsharp()
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
    public void DumpZeroValueUShortCsharp()
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
}