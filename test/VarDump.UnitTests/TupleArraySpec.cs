using System;
using VarDump.Visitor;
using Xunit;

namespace VarDump.UnitTests;

public class TupleArraySpec
{
    [Fact]
    public void DumpTupleArrayVisualBasic()
    {
        var tupleArray = new Tuple<int, string>[]
        {
            new(1, "First"),
            new(2, "Second")
        };

        var dumper = new VisualBasicDumper();

        var result = dumper.Dump(tupleArray);

        Assert.Equal(
            """
            Dim arrayOfTuple = New Tuple(Of Integer, String)(){
                New Tuple(Of Integer, String)(1, "First"),
                New Tuple(Of Integer, String)(2, "Second")
            }
            
            """, result);
    }

    [Fact]
    public void DumpTupleArrayWithNamedArgumentsVisualBasic()
    {
        var tupleArray = new Tuple<int, string>[]
        {
            new(1, "First"),
            new(2, "Second")
        };

        var dumper = new VisualBasicDumper(new DumpOptions { UseNamedArgumentsInConstructors = true });

        var result = dumper.Dump(tupleArray);

        Assert.Equal(
            """
            Dim arrayOfTuple = New Tuple(Of Integer, String)(){
                New Tuple(Of Integer, String)(item1:=1, item2:="First"),
                New Tuple(Of Integer, String)(item1:=2, item2:="Second")
            }
            
            """, result);
    }

    [Fact]
    public void DumpTupleArrayCSharp()
    {
        var tupleArray = new Tuple<int, string>[]
        {
            new(1, "First"),
            new(2, "Second")
        };

        var dumper = new CSharpDumper();

        var result = dumper.Dump(tupleArray);

        Assert.Equal(
            """
            var arrayOfTuple = new Tuple<int, string>[]
            {
                new Tuple<int, string>(1, "First"),
                new Tuple<int, string>(2, "Second")
            };
            
            """, result);
    }

    [Fact]
    public void DumpTupleArrayWithNamedArgumentsCSharp()
    {
        var tupleArray = new Tuple<int, string>[]
        {
            new(1, "First"),
            new(2, "Second")
        };

        var dumper = new CSharpDumper(new DumpOptions{ UseNamedArgumentsInConstructors = true});

        var result = dumper.Dump(tupleArray);
        
        Assert.Equal(
            """
            var arrayOfTuple = new Tuple<int, string>[]
            {
                new Tuple<int, string>(item1: 1, item2: "First"),
                new Tuple<int, string>(item1: 2, item2: "Second")
            };
            
            """, result);
    }
}