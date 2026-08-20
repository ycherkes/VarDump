using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using VarDump.UnitTests.TestModel;
using VarDump.Visitor;
using Xunit;

namespace VarDump.UnitTests;

public class ReadonlyCollectionInitializerSpec
{
    [Fact]
    public void DumpReadonlyPropertyCollectionInitializerCSharp()
    {
        var owner = new ReadonlyCatCollectionOwner
        {
            Cats =
            {
                new Cat { Name = "Sylvester", Age = 8 },
                new Cat { Name = "Whiskers", Age = 2 },
                new Cat { Name = "Sasha", Age = 14 }
            }
        };

        var dumper = new CSharpDumper(new DumpOptions
        {
            CollectionLiteralStyle = CollectionLiteralStyle.Initializer
        });

        var result = dumper.Dump(owner);

        Assert.Equal(
            """
            var readonlyCatCollectionOwner = new ReadonlyCatCollectionOwner
            {
                Cats =
                {
                    new Cat
                    {
                        Age = 8,
                        Name = "Sylvester"
                    },
                    new Cat
                    {
                        Age = 2,
                        Name = "Whiskers"
                    },
                    new Cat
                    {
                        Age = 14,
                        Name = "Sasha"
                    }
                }
            };

            """, result, ignoreLineEndingDifferences: true);
        AssertCompiles(result);
    }

    [Fact]
    public void DumpReadonlyPropertyDictionaryInitializerCSharp()
    {
        var owner = new CatDictionaryOwner
        {
            Cats =
            {
                { "Sylvester", new Cat { Name = "Sylvester", Age = 8 } },
                { "Whiskers", new Cat { Name = "Whiskers", Age = 2 } },
                { "Sasha", new Cat { Name = "Sasha", Age = 14 } }
            }
        };

        var dumper = new CSharpDumper(new DumpOptions
        {
            CollectionLiteralStyle = CollectionLiteralStyle.Initializer
        });

        var result = dumper.Dump(owner);

        Assert.Equal(
            """
            var catDictionaryOwner = new CatDictionaryOwner
            {
                Cats =
                {
                    {
                        "Sylvester",
                        new Cat
                        {
                            Age = 8,
                            Name = "Sylvester"
                        }
                    },
                    {
                        "Whiskers",
                        new Cat
                        {
                            Age = 2,
                            Name = "Whiskers"
                        }
                    },
                    {
                        "Sasha",
                        new Cat
                        {
                            Age = 14,
                            Name = "Sasha"
                        }
                    }
                }
            };

            """, result, ignoreLineEndingDifferences: true);
        AssertCompiles(result);
    }

    [Fact]
    public void SkipReadonlyPropertyCollectionInitializerVisualBasic()
    {
        var owner = new ReadonlyCatCollectionOwner
        {
            Cats =
            {
                new Cat { Name = "Sylvester", Age = 8 }
            }
        };

        var dumper = new VisualBasicDumper(new DumpOptions
        {
            CollectionLiteralStyle = CollectionLiteralStyle.Initializer
        });

        var result = dumper.Dump(owner);

        Assert.Equal(
            "Dim readonlyCatCollectionOwnerValue = New ReadonlyCatCollectionOwner()\r\n",
            result,
            ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void SkipReadonlyPropertyDictionaryInitializerVisualBasic()
    {
        var owner = new CatDictionaryOwner
        {
            Cats =
            {
                ["Sylvester"] = new Cat { Name = "Sylvester", Age = 8 }
            }
        };

        var dumper = new VisualBasicDumper(new DumpOptions
        {
            CollectionLiteralStyle = CollectionLiteralStyle.Initializer
        });

        var result = dumper.Dump(owner);

        Assert.Equal(
            "Dim catDictionaryOwnerValue = New CatDictionaryOwner()\r\n",
            result,
            ignoreLineEndingDifferences: true);
    }

    private static void AssertCompiles(string statement)
    {
        var source = $$"""
                       using System.Collections.Generic;
                       using VarDump.UnitTests.TestModel;

                       public static class GeneratedReadonlyCollectionInitializer
                       {
                           public static void Create()
                           {
                       {{statement}}
                           }
                       }
                       """;
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => !assembly.IsDynamic && !string.IsNullOrEmpty(assembly.Location))
            .GroupBy(assembly => assembly.Location, StringComparer.OrdinalIgnoreCase)
            .Select(group => MetadataReference.CreateFromFile(group.Key));
        var compilation = CSharpCompilation.Create(
            "GeneratedReadonlyCollectionInitializer_" + Guid.NewGuid().ToString("N"),
            [CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest))],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();

        Assert.True(errors.Length == 0,
            string.Join(Environment.NewLine, errors.Select(error => error.ToString()))
            + Environment.NewLine + source);
    }
}
