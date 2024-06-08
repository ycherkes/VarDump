using System.Collections.Generic;
using System.Linq;
using VarDump.Visitor;
using VarDump.Visitor.Format;
using Xunit;

namespace VarDump.UnitTests;

public class CollectionSpec
{
    [Fact]
    public void DumpReadOnlyCollectionCSharp()
    {
        var collection = new List<int>{ 1 }.AsReadOnly();

        var dumper = new CSharpDumper();

        var result = dumper.Dump(collection);

        Assert.Equal("""
                     var readOnlyCollectionOfInt = new List<int>
                     {
                         1
                     }.AsReadOnly();

                     """, result);
    }

    [Fact]
    public void DumpReadOnlyCollectionSingleLineCSharp()
    {
        var collection = new List<int> { 1 }.AsReadOnly();

        var dumper = new CSharpDumper(new DumpOptions { PrimitiveCollectionLayout = CollectionLayout.SingleLine });

        var result = dumper.Dump(collection);

        Assert.Equal("""
                     var readOnlyCollectionOfInt = new List<int> { 1 }.AsReadOnly();
                     
                     """, result);
    }

    [Fact]
    public void DumpEnumerableRangeCSharp()
    {
        var range = Enumerable.Range(0, 3);

        var dumper = new CSharpDumper();

        var result = dumper.Dump(range);

        Assert.Equal("""
                     var rangeIteratorOfInt = new int[]
                     {
                         0,
                         1,
                         2
                     };
                     
                     """, result);
    }

    [Fact]
    public void DumpListOfListsCSharpSingleLine()
    {
        var array = new[]
        {
            new[]{ 1, 2, 3 }.ToList()
        }.ToList();

        var dumper = new CSharpDumper(new DumpOptions
        {
            PrimitiveCollectionLayout = CollectionLayout.SingleLine
        });

        var result = dumper.Dump(array);

        Assert.Equal(
            """
            var listOfListOfInt = new List<List<int>>
            {
                new List<int> { 1, 2, 3 }
            };

            """, result);
    }

    [Fact]
    public void DumpReadOnlyCollectionVisualBasic()
    {
        var collection = new List<int> { 1 }.AsReadOnly();

        var dumper = new VisualBasicDumper();

        var result = dumper.Dump(collection);

        Assert.Equal(
            """
            Dim readOnlyCollectionOfInteger = New List(Of Integer) From {
                1
            }.AsReadOnly()

            """, result);
    }

    [Fact]
    public void DumpReadOnlyCollectionSingleLineVisualBasic()
    {
        var collection = new List<int> { 1 }.AsReadOnly();

        var dumper = new VisualBasicDumper(new DumpOptions { PrimitiveCollectionLayout = CollectionLayout.SingleLine });

        var result = dumper.Dump(collection);

        Assert.Equal("""
                     Dim readOnlyCollectionOfInteger = New List(Of Integer) From { 1 }.AsReadOnly()
                     
                     """, result);
    }

    [Fact]
    public void DumpListOfListsVisualBasicSingleLine()
    {
        var array = new[]
        {
            new[]{ 1, 2, 3 }.ToList()
        }.ToList();

        var dumper = new VisualBasicDumper(new DumpOptions
        {
            PrimitiveCollectionLayout = CollectionLayout.SingleLine
        });

        var result = dumper.Dump(array);

        Assert.Equal(
            """
            Dim listOfListOfInteger = New List(Of List(Of Integer)) From {
                New List(Of Integer) From { 1, 2, 3 }
            }
            
            """, result);
    }
}