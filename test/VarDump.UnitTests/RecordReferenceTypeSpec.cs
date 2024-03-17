#if NET6_0_OR_GREATER
using Xunit;

namespace VarDump.UnitTests;

public class RecordReferenceTypeSpec
{
    [Fact]
    public void DumpRecordWithConstructorCSharp()
    {
        var person = new Person("Boris", "Johnson");

        var dumper = new CSharpDumper();

        var result = dumper.Dump(person);

        Assert.Equal("var person = new Person(\"Boris\", \"Johnson\");\r\n", result);
    }

    [Fact]
    public void DumpRecordWithConstructorVb()
    {
        var person = new Person("Boris", "Johnson");

        var dumper = new VisualBasicDumper();

        var result = dumper.Dump(person);

        Assert.Equal("Dim personValue = New Person(\"Boris\", \"Johnson\")\r\n", result);
    }

    [Fact]
    public void DumpRecordWithoutConstructorCSharp()
    {
        var person1 = new Person1
        {
            FirstName = "Boris",
            LastName = "Johnson"
        };

        var dumper = new CSharpDumper();

        var result = dumper.Dump(person1);

        Assert.Equal(
            @"var person1 = new Person1
{
    FirstName = ""Boris"",
    LastName = ""Johnson""
};
", result);
    }

    [Fact]
    public void DumpRecordWithoutConstructorVb()
    {
        var person1 = new Person1
        {
            FirstName = "Boris",
            LastName = "Johnson"
        };

        var dumper = new VisualBasicDumper();

        var result = dumper.Dump(person1);

        Assert.Equal(
            @"Dim person1Value = New Person1 With {
    .FirstName = ""Boris"",
    .LastName = ""Johnson""
}
", result);
    }

    private record Person(string FirstName, string LastName)
    {
        public string FullName => $"{FirstName} {LastName}";
    }

    public record Person2(string FirstName, string LastName, string Id)
    {
        internal string Id { get; init; } = Id;
    }


    private record Person1
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
#endif