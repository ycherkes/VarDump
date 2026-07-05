using System;
using System.Collections;
using VarDump.Visitor;
using Xunit;

namespace VarDump.UnitTests;

public class CircularReferenceSpec
{
    [Fact]
    public void DumpSelfReferencingObjectCSharp_ShouldWriteCircularReferenceComment()
    {
        var node = new Node { Name = "root" };
        node.Next = node;

        var dumper = new CSharpDumper();

        var result = dumper.Dump(node);

        Assert.Contains("Circular reference detected", result);
    }

    [Fact]
    public void DumpSharedReferenceObjectCSharp_ShouldNotTreatSiblingReuseAsCircularReference()
    {
        var shared = new Node { Name = "shared" };
        var root = new Root
        {
            First = shared,
            Second = shared
        };

        var dumper = new CSharpDumper();

        var result = dumper.Dump(root);

        Assert.DoesNotContain("Circular reference detected", result);
        Assert.Equal(2, CountOccurrences(result, "Name = \"shared\""));
    }

    [Fact]
    public void DumpCircularCollectionCSharp_ShouldWriteCircularReferenceComment()
    {
        var list = new ArrayList();
        list.Add(list);

        var dumper = new CSharpDumper();

        var result = dumper.Dump(list);

        Assert.Contains("Circular reference detected", result);
    }

    [Fact]
    public void DumpCircularDictionaryCSharp_ShouldWriteCircularReferenceComment()
    {
        var dictionary = new Hashtable();
        dictionary["self"] = dictionary;

        var dumper = new CSharpDumper();

        var result = dumper.Dump(dictionary);

        Assert.Contains("Circular reference detected", result);
    }

    [Fact]
    public void DumpTupleContainingCircularReferenceCSharp_ShouldWriteCircularReferenceComment()
    {
        var items = new ArrayList();
        var tuple = new Tuple<ArrayList>(items);
        items.Add(tuple);

        var dumper = new CSharpDumper();

        var result = dumper.Dump(tuple);

        Assert.Contains("Circular reference detected", result);
    }

    [Fact]
    public void DumpDeepObjectCSharp_ShouldStillRespectMaxDepth()
    {
        var node = new Node
        {
            Name = "root",
            Next = new Node { Name = "child" }
        };

        var dumper = new CSharpDumper(new DumpOptions { MaxDepth = 1 });

        var result = dumper.Dump(node);

        Assert.Contains("Max depth", result);
        Assert.DoesNotContain("Circular reference detected", result);
    }

    private static int CountOccurrences(string value, string search)
    {
        return value.Split([search], StringSplitOptions.None).Length - 1;
    }

    private sealed class Node
    {
        public string Name { get; set; }

        public Node Next { get; set; }
    }

    private sealed class Root
    {
        public Node First { get; set; }

        public Node Second { get; set; }
    }

    [Fact]
    public void DumpAssemblyCSharp_ShouldNotThrowStackOverflowOrOutOfMemoryException()
    {
        var assembly = typeof(int).Assembly;

        var dumper = new CSharpDumper(new DumpOptions { IgnoreReadonlyProperties = false});
        _  = dumper.Dump(assembly);
    }

    [Fact]
    public void DumpAssemblyVisualBasic_ShouldNotThrowStackOverflowOrOutOfMemoryException()
    {
        var assembly = typeof(int).Assembly;

        var dumper = new VisualBasicDumper(new DumpOptions { IgnoreReadonlyProperties = false });
        _ = dumper.Dump(assembly);
    }
}