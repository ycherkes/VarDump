using VarDump.Visitor;
using Xunit;

namespace VarDump.UnitTests;

public class IntegralTypesSpec
{
    [Fact]
    public void DumpULongBinaryCSharp()
    {
        var dumper = new CSharpDumper(new DumpOptions
        {
            Formatting =
            {
                IntegralNumericFormat = "b"
            }
        });

        var result = dumper.Dump(ulong.MaxValue - 1);

        Assert.Equal(
            """
            var ulongValue = 0b1111111111111111111111111111111111111111111111111111111111111110ul;

            """, result);
    }

    [Fact]
    public void DumpULongBinaryUnderscoresCSharp()
    {
        var dumper = new CSharpDumper(new DumpOptions
        {
            Formatting =
            {
                IntegralNumericFormat = "b_4"
            }
        });

        var result = dumper.Dump(ulong.MaxValue - 1);

        Assert.Equal(
            """
            var ulongValue = 0b1111_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111_1110ul;
            
            """, result);
    }

    [Fact]
    public void DumpIntBinaryCSharp()
    {
        var dumper = new CSharpDumper(new DumpOptions
        {
            Formatting =
            {
                IntegralNumericFormat = "b"
            }
        });

        var result = dumper.Dump(int.MaxValue-1);

        Assert.Equal(
            """
            var intValue = 0b1111111111111111111111111111110;

            """, result);
    }

    [Fact]
    public void DumpIntArrayBinaryCSharp()
    {
        var dumper = new CSharpDumper(new DumpOptions
        {
            Formatting =
            {
                IntegralNumericFormat = "b",
                PrimitiveCollection = CollectionFormat.SingleLine
            }
        });

        var result = dumper.Dump(new[]{37,49,73});

        Assert.Equal(
            """
            var arrayOfInt = new int[]{ 0b100101, 0b110001, 0b1001001 };
            
            """, result);
    }

    [Fact]
    public void DumpByteHexCSharp()
    {
        var dumper = new CSharpDumper(new DumpOptions
        {
            Formatting =
            {
                IntegralNumericFormat = "X2"
            }
        });

        var result = dumper.Dump((byte)15);

        Assert.Equal(
            """
            var byteValue = 0X0F;

            """, result);
    }

    [Fact]
    public void DumpIntegerHexUnderscoreCSharp()
    {
        var dumper = new CSharpDumper(new DumpOptions
        {
            Formatting =
            {
                IntegralNumericFormat = "X8_4"
            }
        });

        var result = dumper.Dump(0X0001_E240);

        Assert.Equal(
            """
            var intValue = 0X0001_E240;

            """, result);
    }

    [Fact]
    public void DumpULongBinaryVb()
    {
        var dumper = new VisualBasicDumper(new DumpOptions
        {
            Formatting =
            {
                IntegralNumericFormat = "B"
            }
        });

        var result = dumper.Dump(ulong.MaxValue - 1);

        Assert.Equal(
            """
            Dim uLongValue = &B1111111111111111111111111111111111111111111111111111111111111110UL
            
            """, result);
    }

    [Fact]
    public void DumpIntBinaryVb()
    {
        var dumper = new VisualBasicDumper(new DumpOptions
        {
            Formatting =
            {
                IntegralNumericFormat = "B"
            }
        });

        var result = dumper.Dump(int.MaxValue - 1);

        Assert.Equal(
            """
            Dim integerValue = &B1111111111111111111111111111110
            
            """, result);
    }

    [Fact]
    public void DumpByteHexVb()
    {
        var dumper = new VisualBasicDumper(new DumpOptions
        {
            Formatting =
            {
                IntegralNumericFormat = "X2"
            }
        });

        var result = dumper.Dump((byte)15);

        Assert.Equal(
            """
            Dim byteValue = &H0F

            """, result);
    }

    [Fact]
    public void DumpIntegerHexUnderscoreVb()
    {
        var dumper = new VisualBasicDumper(new DumpOptions
        {
            Formatting =
            {
                IntegralNumericFormat = "X8_4"
            }
        });

        var result = dumper.Dump(0X0001_E240);

        Assert.Equal(
            """
            Dim integerValue = &H0001_E240

            """, result);
    }
}