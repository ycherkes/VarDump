using System.Linq;
using VarDump.UnitTests.TestModel;
using Xunit;

namespace VarDump.UnitTests;

public class GroupingCollectionSpec
{
    [Fact]
    public void DumpGroupingCollectionVisualBasic()
    {
        var grouping = new[]
        {
            new Person{ Age = 32, FirstName = "Bob"},
            new Person{ Age = 23, FirstName = "Alice"}
        }.ToLookup(x => x.FirstName);

        var dumper = new VisualBasicDumper();

        var result = dumper.Dump(grouping);

        Assert.Equal(
            """
            Dim lookupOfGroupingOfPerson = {
                New With {
                    .Key = "Bob",
                    .Element = New Person With {
                        .FirstName = "Bob",
                        .Age = 32
                    }
                },
                New With {
                    .Key = "Alice",
                    .Element = New Person With {
                        .FirstName = "Alice",
                        .Age = 23
                    }
                }
            }.ToLookup(Function (grp) grp.Key, Function (grp) grp.Element)

            """, result);
    }

    [Fact]
    public void DumpGroupingCollectionCSharp()
    {
        var grouping = new[]
        {
            new Person{ Age = 32, FirstName = "Bob"},
            new Person{ Age = 23, FirstName = "Alice"}
        }.GroupBy(x => x.FirstName).ToArray();

        var dumper = new CSharpDumper();

        var result = dumper.Dump(grouping);

        Assert.Equal(
            """
            var arrayOfGroupingOfPerson = new []
            {
                new 
                {
                    Key = "Bob",
                    Element = new Person
                    {
                        FirstName = "Bob",
                        Age = 32
                    }
                },
                new 
                {
                    Key = "Alice",
                    Element = new Person
                    {
                        FirstName = "Alice",
                        Age = 23
                    }
                }
            }.GroupBy(grp => grp.Key, grp => grp.Element).ToArray();

            """, result);
    }

    [Fact]
    public void DumpSingleGroupingValueCSharp()
    {
        var grouping = new[] { new Person { Age = 32, FirstName = "Bob" } }
            .GroupBy(x => x.FirstName)
            .Single();

        var dumper = new CSharpDumper();

        var result = dumper.Dump(grouping);

        Assert.Equal(
            """
            var groupingOfPerson = new []
            {
                new 
                {
                    Key = "Bob",
                    Element = new Person
                    {
                        FirstName = "Bob",
                        Age = 32
                    }
                }
            }.GroupBy(grp => grp.Key, grp => grp.Element).Single();

            """, result);
    }
}