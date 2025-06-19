using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using VarDump.UnitTests.TestModel;
using VarDump.Visitor;
using Xunit;

namespace VarDump.UnitTests;

public class DictionarySpec
{
    [Fact]
    public void DumpDictionaryVisualBasic()
    {
        var dict = new[]
        {
            new Person{ Age = 32, FirstName = "Bob"},
            new Person{ Age = 23, FirstName = "Alice"},
        }.ToDictionary(x => x.FirstName);

        var dumper = new VisualBasicDumper();

        var result = dumper.Dump(dict);

        Assert.Equal("""
                     Dim dictionaryOfPerson = New Dictionary(Of String, Person) From {
                         {
                             "Bob",
                             New Person With {
                                 .FirstName = "Bob",
                                 .Age = 32
                             }
                         },
                         {
                             "Alice",
                             New Person With {
                                 .FirstName = "Alice",
                                 .Age = 23
                             }
                         }
                     }

                     """, result);
    }

    [Fact]
    public void DumpDictionaryCSharp()
    {
        var dict = new[]
        {
            new Person{ Age = 32, FirstName = "Bob"},
            new Person{ Age = 23, FirstName = "Alice"},
        }.ToDictionary(x => x.FirstName);

        var dumper = new CSharpDumper();

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
                         }
                     };

                     """, result);
    }

    [Fact]
    public void DumpDictionaryOfAnonymousTypeCSharp()
    {
        var dict = new[]
        {
            new { Age = 32, FirstName = "Bob"},
            new { Age = 23, FirstName = "Alice"},
        }.ToDictionary(x => x.FirstName);

        var dumper = new CSharpDumper();

        var result = dumper.Dump(dict);

        Assert.Equal(
            """
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
                }
            }.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            """, result);
    }

    [Fact]
    public void DumpDictionaryOfTypeArrayCSharp()
    {
        var dict = new Dictionary<string, Type[]>
        {
            {"First", [typeof(Person)] },
            {"Second", [typeof(string)] }
        };

        var dumper = new CSharpDumper(new DumpOptions
        {
            IgnoreDefaultValues = true,
            IgnoreNullValues = true,
            MaxDepth = 5,
            TypeNamePolicy = TypeNamingPolicy.ShortName,
            DateTimeInstantiation = DateTimeInstantiation.New,
            DateKind = DateKind.ConvertToUtc
        });

        var result = dumper.Dump(dict);

        Assert.Equal("""
                     var dictionaryOfArrayOfType = new Dictionary<string, Type[]>
                     {
                         {
                             "First",
                             new Type[]
                             {
                                 typeof(Person)
                             }
                         },
                         {
                             "Second",
                             new Type[]
                             {
                                 typeof(string)
                             }
                         }
                     };

                     """, result);
    }

    [Fact]
    public void DumpImmutableDictionaryCSharp()
    {
        var immutableDictionary = new Dictionary<string, string>
        {
            { "Steeve", "Test reference" }
        }.ToImmutableDictionary();

        var dumper = new CSharpDumper();

        var result = dumper.Dump(immutableDictionary);

        Assert.Equal("""
                     var immutableDictionaryOfString = new Dictionary<string, string>
                     {
                         {
                             "Steeve",
                             "Test reference"
                         }
                     }.ToImmutableDictionary();

                     """, result);
    }

    [Fact]
    public void DumpImmutableDictionaryVb()
    {
        var immutableDictionary = new Dictionary<string, string>
        {
            { "Steeve", "Test reference" }
        }.ToImmutableDictionary();

        var dumper = new VisualBasicDumper();

        var result = dumper.Dump(immutableDictionary);

        Assert.Equal("""
                     Dim immutableDictionaryOfString = New Dictionary(Of String, String) From {
                         {
                             "Steeve",
                             "Test reference"
                         }
                     }.ToImmutableDictionary()

                     """, result);
    }
}