using System.Collections.Generic;
using VarDump.Visitor;
using Xunit;

namespace VarDump.UnitTests;

public class KeyValuePairArraySpec
{
    [Fact]
    public void DumpKeyValuePairArrayVisualBasic()
    {
        var kvpArray = new KeyValuePair<int, string>[]
        {
            new(1, "First"),
            new(2, "Second")
        };

        var dumper = new VisualBasicDumper();

        var result = dumper.Dump(kvpArray);

        Assert.Equal(
            """
            Dim arrayOfKeyValuePair = New KeyValuePair(Of Integer, String)(){
                New KeyValuePair(Of Integer, String)(1, "First"),
                New KeyValuePair(Of Integer, String)(2, "Second")
            }

            """, result);
    }

    [Fact]
    public void DumpKeyValuePairArrayWithNamedArgumentsVisualBasic()
    {
        var kvpArray = new KeyValuePair<int, string>[]
        {
            new(1, "First"),
            new(2, "Second")
        };

        var dumper = new VisualBasicDumper(new DumpOptions { UseNamedArgumentsInConstructors = true });

        var result = dumper.Dump(kvpArray);

        Assert.Equal(
            """
            Dim arrayOfKeyValuePair = New KeyValuePair(Of Integer, String)(){
                New KeyValuePair(Of Integer, String)(key:=1, value:="First"),
                New KeyValuePair(Of Integer, String)(key:=2, value:="Second")
            }
            
            """, result);
    }

    [Fact]
    public void DumpKeyValuePairArrayCSharp()
    {
        var kvpArray = new KeyValuePair<int, string>[]
        {
            new(1, "First"),
            new(2, "Second")
        };

        var dumper = new CSharpDumper();

        var result = dumper.Dump(kvpArray);

        Assert.Equal(
            """
            var arrayOfKeyValuePair = new KeyValuePair<int, string>[]
            {
                new KeyValuePair<int, string>(1, "First"),
                new KeyValuePair<int, string>(2, "Second")
            };

            """, result);
    }

    [Fact]
    public void DumpKeyValuePairArrayWithNamedArgumentsCSharp()
    {
        var kvpArray = new KeyValuePair<int, string>[]
        {
            new(1, "First"),
            new(2, "Second")
        };

        var dumper = new CSharpDumper(new DumpOptions{ UseNamedArgumentsInConstructors = true});

        var result = dumper.Dump(kvpArray);

        Assert.Equal(
            """
            var arrayOfKeyValuePair = new KeyValuePair<int, string>[]
            {
                new KeyValuePair<int, string>(key: 1, value: "First"),
                new KeyValuePair<int, string>(key: 2, value: "Second")
            };
            
            """, result);
    }
}