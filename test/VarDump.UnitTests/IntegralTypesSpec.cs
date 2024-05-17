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
                IntegralNumericFormat = new IntegralNumericFormat
                {
                    Format = NumericFormat.Binary
                }
            }
        });

        var result = dumper.Dump(ulong.MaxValue - 1);

        Assert.Equal(
            """
            var ulongValue = 0b1111111111111111111111111111111111111111111111111111111111111110ul;

            """, result);
    }

    [Fact]
    public void DumpIntBinaryCSharp()
    {
        var dumper = new CSharpDumper(new DumpOptions
        {
            Formatting =
            {
                IntegralNumericFormat = new IntegralNumericFormat
                {
                    Format = NumericFormat.Binary
                }
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
                IntegralNumericFormat = new IntegralNumericFormat
                {
                    Format = NumericFormat.Binary
                },
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
                IntegralNumericFormat = new IntegralNumericFormat
                {
                    Format = NumericFormat.HexadecimalUpperCase,
                    Precision = 2
                }
            }
        });

        var result = dumper.Dump((byte)15);

        Assert.Equal(
            """
            var byteValue = 0X0F;

            """, result);
    }

    [Fact]
    public void DumpULongBinaryVb()
    {
        var dumper = new VisualBasicDumper(new DumpOptions
        {
            Formatting =
            {
                IntegralNumericFormat = new IntegralNumericFormat
                {
                    Format = NumericFormat.Binary
                }
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
                IntegralNumericFormat = new IntegralNumericFormat
                {
                    Format = NumericFormat.Binary
                }
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
                IntegralNumericFormat = new IntegralNumericFormat
                {
                    Format = NumericFormat.HexadecimalUpperCase,
                    Precision = 2
                }
            }
        });

        var result = dumper.Dump((byte)15);

        Assert.Equal(
            """
            Dim byteValue = &H0F

            """, result);
    }
}