#nullable enable
using System.Collections.Generic;
using VarDump.Visitor;
using Xunit;

namespace VarDump.UnitTests;

public class ReferenceNullabilitySpec
{
    private class Foo1<T> where T : class?
    {
        public T? Value { get; set; } 
    }
    
    private class Foo
    {
        public Dictionary<int, object?> RefNullableDictionary { get; set; } = new() { { 1, null } };
        //public List<object?> RefNullableList { get; set; } = [null];
        //public Dictionary<int, object> Dictionary { get; set; } = new() { { 1, 1 } };
        public IReadOnlyCollection<object?> List { get; set; } = new List<object?>
        {
            1
        }.AsReadOnly();
    }

    [Fact]
    public void DumpCsharp()
    {
        var obj = new 
        { 
            List = new List<object?>
            {
                1
            }
        };

        var dop = DumpOptions.Default.Clone();
        dop.WritablePropertiesOnly = false;
        var dumper = new CSharpDumper(dop);

        var result = dumper.Dump(obj);

        Assert.Equal("""
                     var foo = new Foo
                     {
                         List = new List<object?>
                         {
                             1
                         }.AsReadOnly()
                     };
                     
                     """, result);
    }

    [Fact]
    public void DumpVisualBasic()
    {
        var obj = new Foo();

        var dop = DumpOptions.Default.Clone();
        dop.WritablePropertiesOnly = false;
        var dumper = new VisualBasicDumper(dop);

        var result = dumper.Dump(obj);

        Assert.Equal("""
                     Dim fooValue = New Foo With {
                         .List = New List(Of Object?) From {
                             1
                         }.AsReadOnly()
                     }
                     
                     """, result);
    }

    [Fact(Skip = "Doesn't work")]
    public void DumpCsharp1()
    {
        var obj = new{ foo = new Foo1<string?>{ Value = null }};

        var dop = DumpOptions.Default.Clone();
        dop.WritablePropertiesOnly = false;
        var dumper = new CSharpDumper(dop);

        var result = dumper.Dump(obj);

        Assert.Equal("""
                     
                     """, result);
    }
}