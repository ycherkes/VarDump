using System.Linq;
using Xunit;

namespace VarDump.UnitTests;

public class EnumerableRangeSpec
{
    [Fact]
    public void DumpAnonymousTypeCsharp()
    {
        var range = Enumerable.Range(5, 2).Select((x, i) => new { i, x });

        var dumper = new CSharpDumper();

        var result = dumper.Dump(range);

        Assert.Equal(
            @"var selectIteratorOfAnonymousType = new []
{
    new 
    {
        i = 0,
        x = 5
    },
    new 
    {
        i = 1,
        x = 6
    }
};
", result);
    }

    [Fact]
    public void DumpAnonymousTypeVb()
    {
        var range = Enumerable.Range(5, 2).Select((i, x) => new { i, x });

        var dumper = new VisualBasicDumper();

        var result = dumper.Dump(range);

        Assert.Equal(
            @"Dim selectIteratorOfAnonymousType = {
    New With {
        .i = 5,
        .x = 0
    },
    New With {
        .i = 6,
        .x = 1
    }
}
", result);
    }
}