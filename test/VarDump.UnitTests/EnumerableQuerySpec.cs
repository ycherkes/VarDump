using System.Linq;
using VarDump.Visitor;
using Xunit;

namespace VarDump.UnitTests;

public class EnumerableQuerySpec
{
    [Fact]
    public void DumpEnumerableQueryCSharp()
    {
        var query = new[] { 5, 6 }.AsQueryable();

        var dumper = new CSharpDumper();

        var result = dumper.Dump(query);

        Assert.Equal(
            """
            var enumerableQueryOfInt = new int[]
            {
                5,
                6
            }.AsQueryable();
            
            """, result, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void DumpEnumerableQueryCollectionExpressionCSharp()
    {
        var query = new[] { 5, 6 }.AsQueryable();

        var dumper = new CSharpDumper(new DumpOptions { CollectionLiteralStyle = CollectionLiteralStyle.Expression });

        var result = dumper.Dump(query);

        // Queryable must be output as an initializer regardless of the CollectionLiteralStyle
        Assert.Equal(
            """
            var enumerableQueryOfInt = new int[]
            {
                5,
                6
            }.AsQueryable();

            """, result, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void DumpEnumerableQueryVb()
    {
        var query = Enumerable.Range(5, 2).AsQueryable();

        var dumper = new VisualBasicDumper();

        var result = dumper.Dump(query);

        Assert.Equal(
            """
            Dim enumerableQueryOfInteger = New Integer(){
                5,
                6
            }.AsQueryable()
            
            """, result, ignoreLineEndingDifferences: true);
    }
}