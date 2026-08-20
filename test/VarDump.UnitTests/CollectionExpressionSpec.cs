using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
#if NET8_0_OR_GREATER
using System.Collections.Frozen;
#endif
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using VarDump.UnitTests.TestModel;
using VarDump.Visitor;
using VarDump.Visitor.Format;
using Xunit;

namespace VarDump.UnitTests;

public class CollectionExpressionSpec
{
    private static readonly DumpOptions ExpressionOptions = new()
    {
        CollectionLiteralStyle = CollectionLiteralStyle.Expression,
        PrimitiveCollectionLayout = CollectionLayout.SingleLine
    };

    [Fact]
    public void EmitsDirectCollectionExpressionsForSupportedTargetTypes()
    {
        var cases = new (string Name, object Value, string Expected)[]
        {
            ("empty array", Array.Empty<int>(), "int[] arrayOfInt = [];"),
            ("array", new[] { 1, 2 }, "int[] arrayOfInt = [1, 2];"),
            ("list", new List<int> { 1, 2 }, "List<int> listOfInt = [1, 2];"),
            ("set", new HashSet<int> { 1, 2 }, "HashSet<int> hashSetOfInt = [1, 2];"),
            ("immutable array", ImmutableArray.Create(1, 2),
                "ImmutableArray<int> immutableArrayOfInt = [1, 2];"),
            ("immutable list", ImmutableList.Create(1, 2),
                "ImmutableList<int> immutableListOfInt = [1, 2];")
        };

        foreach (var testCase in cases)
        {
            var result = new CSharpDumper(ExpressionOptions).Dump(testCase.Value);

            Assert.Equal(testCase.Expected + Environment.NewLine, result,
                ignoreLineEndingDifferences: true);
            AssertCompiles(testCase.Name, result);
        }
    }

    [Fact]
    public void EmitsExpressionForConstructibleNonGenericCollection()
    {
        var collection = new ArrayList { 1, "two" };

        var result = new CSharpDumper(ExpressionOptions).Dump(collection);

        Assert.Contains("ArrayList arrayListOfObject = ", result);
        Assert.DoesNotContain("new ArrayList", result);
        AssertCompiles("non-generic collection", result);
    }

    [Fact]
    public void EmitsExpressionWhenParameterlessConstructionUsesOptionalArguments()
    {
        var collection = new OptionalConstructorCollection { 1, 2 };

        var result = new CSharpDumper(ExpressionOptions).Dump(collection);

        Assert.Contains("OptionalConstructorCollection optionalConstructorCollectionOfInt = [1, 2]", result);
        Assert.DoesNotContain("new OptionalConstructorCollection", result);
        AssertCompiles("optional constructor collection", result);
    }

    [Fact]
    public void SuppliesTargetsForExpressionsUsedByConversionMethods()
    {
        var cases = new (string Name, object Value, string ExpectedFragment)[]
        {
            ("read-only collection", new List<int> { 1, 2 }.AsReadOnly(),
                "new ReadOnlyCollection<int>([1, 2])"),
            ("enumerable", Enumerable.Range(1, 2), "(int[])[1, 2]"),
            ("queryable", Queryable.AsQueryable([1, 2]), "Queryable.AsQueryable<int>([1, 2])")
        };

        foreach (var testCase in cases)
        {
            var result = new CSharpDumper(ExpressionOptions).Dump(testCase.Value);

            Assert.Contains(testCase.ExpectedFragment, result);
            AssertCompiles(testCase.Name, result);
        }
    }

#if NET8_0_OR_GREATER
    [Fact]
    public void SuppliesAnArrayTargetBeforeCreatingFrozenCollection()
    {
        var collection = new[] { 1, 2 }.ToFrozenSet();

        var result = new CSharpDumper(ExpressionOptions).Dump(collection);

        Assert.Contains("FrozenSet.ToFrozenSet<int>([1, 2])", result);
        AssertCompiles("frozen set", result);
    }
#endif

    [Fact]
    public void UsesExpressionForNonPublicEnumerableArrayFallback()
    {
        var collection = new CatNonPublicCollection
        {
            new Cat { Name = "Milo", Age = 3 }
        };

        var result = new CSharpDumper(ExpressionOptions).Dump(collection);

        Assert.Contains("var catNonPublicCollectionOfObject = (object[])", result);
        Assert.Contains("[", result);
        Assert.DoesNotContain("new object[]", result);
        AssertCompiles("non-public enumerable", result);
    }

    [Fact]
    public void KeepsInitializerForTargetsWithoutACollectionExpressionConversion()
    {
        var customCollection = new CatPublicCollection
        {
            new Cat { Name = "Milo", Age = 3 }
        };
        var dictionary = new Dictionary<string, int> { { "one", 1 } };
        var multidimensionalArray = new[,] { { 1, 2 } };

        var customResult = new CSharpDumper(ExpressionOptions).Dump(customCollection);
        var dictionaryResult = new CSharpDumper(ExpressionOptions).Dump(dictionary);
        var multidimensionalResult = new CSharpDumper(ExpressionOptions).Dump(multidimensionalArray);

        Assert.Contains("new CatPublicCollection", customResult);
        Assert.Contains("new Dictionary<string, int>", dictionaryResult);
        Assert.Contains("new int[,]", multidimensionalResult);
        AssertCompiles("custom non-generic collection", customResult);
        AssertCompiles("dictionary", dictionaryResult);
        AssertCompiles("multidimensional array", multidimensionalResult);
    }

    [Fact]
    public void KeepsInitializerForAnonymousAndGroupingCollections()
    {
        var anonymousCollection = new[] { new { Value = 1 } };
        var groupingCollection = new[] { 1, 2 }.GroupBy(value => value).ToArray();

        var anonymousResult = new CSharpDumper(ExpressionOptions).Dump(anonymousCollection);
        var groupingResult = new CSharpDumper(ExpressionOptions).Dump(groupingCollection);

        Assert.Contains("new []", anonymousResult);
        Assert.Contains("new []", groupingResult);
        AssertCompiles("anonymous collection", anonymousResult);
        AssertCompiles("grouping collection", groupingResult);
    }

    [Fact]
    public void EmitsValidExpressionsForNestedAndTruncatedCollections()
    {
        var jaggedArray = new[] { new[] { 1, 2 } };
        var owner = new CatOwner
        {
            Cats = [new Cat { Name = "Milo", Age = 3 }]
        };
        var truncated = new List<int> { 1, 2, 3 };

        var jaggedResult = new CSharpDumper(ExpressionOptions).Dump(jaggedArray);
        var ownerResult = new CSharpDumper(ExpressionOptions).Dump(owner);
        var truncatedResult = new CSharpDumper(new DumpOptions
        {
            CollectionLiteralStyle = CollectionLiteralStyle.Expression,
            PrimitiveCollectionLayout = CollectionLayout.SingleLine,
            MaxCollectionSize = 2
        }).Dump(truncated);

        Assert.DoesNotContain("new int[]", jaggedResult);
        Assert.Contains("Cats = ", ownerResult);
        Assert.DoesNotContain("Cats = new", ownerResult);
        Assert.Contains("Too many items", truncatedResult);
        AssertCompiles("jagged array", jaggedResult);
        AssertCompiles("collection property", ownerResult);
        AssertCompiles("truncated collection", truncatedResult);
    }

    [Fact]
    public void SuppliesTargetWhenVariableInitializerIsDisabled()
    {
        var result = new CSharpDumper(new DumpOptions
        {
            CollectionLiteralStyle = CollectionLiteralStyle.Expression,
            PrimitiveCollectionLayout = CollectionLayout.SingleLine,
            GenerateVariableInitializer = false
        }).Dump(new List<int> { 1, 2 });

        Assert.Equal("(List<int>)[1, 2]", result);
        AssertExpressionCompiles("initializer-free collection", result);
    }

    private static void AssertCompiles(string caseName, string statement)
    {
#if NET8_0_OR_GREATER
        const string frozenCollectionsUsing = "using System.Collections.Frozen;";
#else
        const string frozenCollectionsUsing = "";
#endif
        var source = $$"""
                       using System;
                       using System.Collections;
                       using System.Collections.Generic;
                       using System.Collections.Immutable;
                       using System.Collections.ObjectModel;
                       using System.Linq;
                       {{frozenCollectionsUsing}}
                       using VarDump.UnitTests;
                       using VarDump.UnitTests.TestModel;

                       public static class GeneratedCollectionExpression
                       {
                           public static void Create()
                           {
                       {{statement}}
                           }
                       }
                       """;

        var syntaxTree = CSharpSyntaxTree.ParseText(source,
            new CSharpParseOptions(LanguageVersion.Latest));
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => !assembly.IsDynamic && !string.IsNullOrEmpty(assembly.Location))
            .GroupBy(assembly => assembly.Location, StringComparer.OrdinalIgnoreCase)
            .Select(group => MetadataReference.CreateFromFile(group.Key));
        var compilation = CSharpCompilation.Create(
            "GeneratedCollectionExpression_" + Guid.NewGuid().ToString("N"),
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();

        Assert.True(errors.Length == 0,
            $"Generated code for '{caseName}' did not compile:{Environment.NewLine}" +
            string.Join(Environment.NewLine, errors.Select(error => error.ToString())) +
            $"{Environment.NewLine}{Environment.NewLine}{source}");
    }

    private static void AssertExpressionCompiles(string caseName, string expression)
    {
        var statement = $"object result = {expression};";
        AssertCompiles(caseName, statement);
    }

}

public sealed class OptionalConstructorCollection(int capacity = 0) : List<int>(capacity);
