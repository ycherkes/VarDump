using System.Collections.Generic;
using System.Linq;
using UnitTests.TestModel;
using VarDump;
using VarDump.Visitor;
using Xunit;

namespace UnitTests;

public class VariableNameSpec
{
    [Fact]
    public void Dictionary()
    {
        var dict = new[]
        {
            new Person{ Age = 32, FirstName = "Bob"},
            new Person{ Age = 23, FirstName = "Alice"},
        }.ToDictionary(x => x.FirstName);

        var dumper = new CSharpDumper();

        var result = dumper.Dump(dict);

        Assert.StartsWith("var dictionaryOfPerson", result);
    }

    [Fact]
    public void DictionaryOfListOfPerson()
    {
        var dict = new[]
        {
            new Person{ Age = 32, FirstName = "Bob"},
            new Person{ Age = 23, FirstName = "Alice"},
        }.ToDictionary(x => x.FirstName, x => new List<Person> { x });

        var dumper = new CSharpDumper();

        var result = dumper.Dump(dict);

        Assert.StartsWith("var dictionaryOfListOfPerson", result);
    }

    [Fact]
    public void HashSet()
    {
        var hashSet = new HashSet<Person>
        {
            new Person{ Age = 32, FirstName = "Bob"},
            new Person{ Age = 23, FirstName = "Alice"},
        };

        var dumper = new CSharpDumper();

        var result = dumper.Dump(hashSet);

        Assert.StartsWith("var hashSetOfPerson", result);
    }

    [Fact]
    public void String()
    {
        var stringValue = "Test string value";

        var dumper = new CSharpDumper();

        var result = dumper.Dump(stringValue);

        Assert.StartsWith("var stringValue", result);
    }
        
    [Fact]
    public void DoNotGenerateVariableInitializerCSharp()
    {
        var stringValue = "Test string value";

        var dumper = new CSharpDumper(new DumpOptions
        {
            GenerateVariableInitializer = false
        });

        var result = dumper.Dump(stringValue);

        Assert.Equal("\"Test string value\"", result);
    }

    [Fact]
    public void DoNotGenerateVariableInitializerVb()
    {
        var stringValue = "Test string value";

        var dumper = new VisualBasicDumper(new DumpOptions
        {
            GenerateVariableInitializer = false
        });

        var result = dumper.Dump(stringValue);

        Assert.Equal("\"Test string value\"", result);
    }
}