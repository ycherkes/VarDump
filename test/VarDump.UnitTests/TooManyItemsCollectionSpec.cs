using System.Collections.Generic;
using System.Linq;
using VarDump.UnitTests.TestModel;
using VarDump.Visitor;
using Xunit;

namespace VarDump.UnitTests;

public class TooManyItemsCollectionSpec
{
    [Fact]
    public void DumpTooLongCollectionCSharp()
    {
        var collection = new List<int> { 1, 2, 3 }.AsReadOnly();

        var dumper = new CSharpDumper(new DumpOptions { MaxCollectionSize = 2 });

        var result = dumper.Dump(collection);

        Assert.Equal("""
                     var readOnlyCollectionOfInt = new List<int>
                     {
                         1,
                         2,
                         // Too many items (> 2). Consider increasing the MaxCollectionSize option.
                     }.AsReadOnly();
                     
                     """, result);
    }

    [Fact]
    public void DumpTooLongDictionaryCSharp()
    {
        var dict = new[]
        {
            new Person{ Age = 32, FirstName = "Bob"},
            new Person{ Age = 23, FirstName = "Alice"},
            new Person{ Age = 13, FirstName = "Silvia"},
        }.ToDictionary(x => x.FirstName);

        var dumper = new CSharpDumper(new DumpOptions { MaxCollectionSize = 2 });

        var result = dumper.Dump(dict);

        Assert.Equal("""
                     var dictionaryOfPerson = new Dictionary<string, Person>
                     {
                         {
                             "Bob",
                             new Person
                             {
                                 FirstName = "Bob",
                                 Age = 32
                             }
                         },
                         {
                             "Alice",
                             new Person
                             {
                                 FirstName = "Alice",
                                 Age = 23
                             }
                         },
                         // Too many items (> 2). Consider increasing the MaxCollectionSize option.
                     };
                     
                     """, result);
    }

    [Fact]
    public void DumpTooLongAnonymousDictionaryCSharp()
    {
        var dict = new[]
        {
            new { Age = 32, FirstName = "Bob"},
            new { Age = 23, FirstName = "Alice"},
            new { Age = 13, FirstName = "Silvia"},
        }.ToDictionary(x => x.FirstName);

        var dumper = new CSharpDumper(new DumpOptions { MaxCollectionSize = 2 });

        var result = dumper.Dump(dict);

        Assert.Equal("""
                     var dictionaryOfAnonymousType = new []
                     {
                         new 
                         {
                             Key = "Bob",
                             Value = new 
                             {
                                 Age = 32,
                                 FirstName = "Bob"
                             }
                         },
                         new 
                         {
                             Key = "Alice",
                             Value = new 
                             {
                                 Age = 23,
                                 FirstName = "Alice"
                             }
                         },
                         // Too many items (> 2). Consider increasing the MaxCollectionSize option.
                     }.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                     
                     """, result);
    }

    [Fact]
    public void DumpTooLongGroupingCollectionCSharp()
    {
        var grouping = new[]
        {
            new Person{ Age = 32, FirstName = "Bob"},
            new Person{ Age = 23, FirstName = "Alice"},
            new Person{ Age = 13, FirstName = "Silvia"},
        }.GroupBy(x => x.FirstName).ToArray();

        var dumper = new CSharpDumper(new DumpOptions { MaxCollectionSize = 2 });

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
                },
                // Too many items (> 2). Consider increasing the MaxCollectionSize option.
            }.GroupBy(grp => grp.Key, grp => grp.Element).ToArray();
            
            """, result);
    }

    [Fact]
    public void DumpTooLongCollectionVisualBasic()
    {
        var collection = new List<int> { 1, 2, 3 }.AsReadOnly();

        var dumper = new VisualBasicDumper(new DumpOptions { MaxCollectionSize = 2 });

        var result = dumper.Dump(collection);

        Assert.Equal("""
                     Dim readOnlyCollectionOfInteger = New List(Of Integer) From {
                         1,
                         2,
                         'Too many items (> 2). Consider increasing the MaxCollectionSize option.
                     }.AsReadOnly()
                     
                     """, result);
    }
}