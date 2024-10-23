using System.Linq;
using Xunit;

namespace VarDump.UnitTests;

public class EnumerableQuerySpec
{
    [Fact]
    public void DumpEnumerableQueryCSharp()
    {
        var query = Enumerable.Range(5, 2).AsQueryable().AsEnumerable();

        var dumper = new CSharpDumper();

        var result = dumper.Dump(query);

        Assert.Equal(
            """
            var enumerableQueryOfInt = new int[]
            {
                5,
                6
            };
            
            """, result);
    }

    [Fact]
    public void DumpEnumerableQueryVb()
    {
        var query = Enumerable.Range(5, 2).AsQueryable().AsEnumerable();

        var dumper = new VisualBasicDumper();

        var result = dumper.Dump(query);

        Assert.Equal(
            """
            Dim enumerableQueryOfInteger = New Integer(){
                5,
                6
            }
            
            """, result);
    }
}