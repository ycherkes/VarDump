using System;
using VarDump.Visitor;
using Xunit;

namespace VarDump.UnitTests;

public class ValueTupleArraySpec
{
    [Fact]
    public void DumpValueTupleArrayVisualBasic()
    {
        var tupleArray = new ValueTuple<int, string>[]
        {
            new(1, "First"),
            new(2, "Second")
        };

        var dumper = new VisualBasicDumper();

        var result = dumper.Dump(tupleArray);

        Assert.Equal(
            """
            Dim arrayOfValueTuple = New ValueTuple(Of Integer, String)(){
                (1, "First"),
                (2, "Second")
            }
            
            """, result);
    }

    [Fact]
    public void DumpValueTupleArrayWithNamedArgumentsVisualBasic()
    {
        var tupleArray = new ValueTuple<int, string>[]
        {
            new(1, "First"),
            new(2, "Second")
        };

        var dumper = new VisualBasicDumper(new DumpOptions { UseNamedArgumentsInConstructors = true });

        var result = dumper.Dump(tupleArray);

        Assert.Equal(
            """
            Dim arrayOfValueTuple = New ValueTuple(Of Integer, String)(){
                (item1:=1, item2:="First"),
                (item1:=2, item2:="Second")
            }
            
            """, result);
    }

    [Fact]
    public void DumpValueTupleArrayCSharp()
    {
        var tupleArray = new ValueTuple<int, string>[]
        {
            new(1, "First"),
            new(2, "Second")
        };

        var dumper = new CSharpDumper();

        var result = dumper.Dump(tupleArray);

        Assert.Equal(
            """
            var arrayOfValueTuple = new ValueTuple<int, string>[]
            {
                (1, "First"),
                (2, "Second")
            };
            
            """, result);
    }

    [Fact]
    public void DumpValueTupleArrayWithNamedArgumentsCSharp()
    {
        var tupleArray = new ValueTuple<int, string>[]
        {
            new(1, "First"),
            new(2, "Second")
        };

        var dumper = new CSharpDumper(new DumpOptions{ UseNamedArgumentsInConstructors = true});

        var result = dumper.Dump(tupleArray);

        Assert.Equal(
            """
            var arrayOfValueTuple = new ValueTuple<int, string>[]
            {
                (item1: 1, item2: "First"),
                (item1: 2, item2: "Second")
            };
            
            """, result);
    }
}