using VarDump;
using Xunit;

namespace UnitTests;

public class NullableSpec
{
    [Fact]
    public void DumpNullableEnumCSharp()
    {
        MyEnum? value = MyEnum.TestValue;

        var dumper = new CSharpDumper();

        var result = dumper.Dump(value);

        Assert.Equal(
            @"var myEnum = MyEnum.TestValue;
", result);
    }

    [Fact]
    public void DumpNullableEnumVb()
    {
        MyEnum? value = MyEnum.TestValue;

        var dumper = new VisualBasicDumper();

        var result = dumper.Dump(value);

        Assert.Equal(
            @"Dim myEnumValue = MyEnum.TestValue
", result);
    }

    enum MyEnum
    {
        TestValue
    }

}