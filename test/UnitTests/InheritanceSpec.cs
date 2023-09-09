using System;
using VarDump;
using Xunit;

namespace UnitTests;

public class InheritanceSpec
{

    [Fact]
    public void DumpClassCsharp()
    {
        var person = new Person
        {
            FirstName = "Boris",
            LastName = "Johnson",
            BirthDate = DateTime.SpecifyKind(new DateTime(1964, 6, 19), DateTimeKind.Utc)
        };

        var dumper = new CSharpDumper();

        var result = dumper.Dump(person);

        Assert.Equal(
            @"var person = new Person
{
    FirstName = ""Boris"",
    LastName = ""Johnson"",
    BirthDate = DateTime.ParseExact(""1964-06-19T00:00:00.0000000Z"", ""O"", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
};
", result);
    }

    [Fact]
    public void DumpClassVb()
    {
        var person = new Person
        {
            FirstName = "Boris",
            LastName = "Johnson",
            BirthDate = DateTime.SpecifyKind(new DateTime(1964, 6, 19), DateTimeKind.Utc)
        };

        var dumper = new VisualBasicDumper();

        var result = dumper.Dump(person);

        Assert.Equal(
            @"Dim personValue = New Person With {
    .FirstName = ""Boris"",
    .LastName = ""Johnson"",
    .BirthDate = Date.ParseExact(""1964-06-19T00:00:00.0000000Z"", ""O"", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
}
", result);
    }


    private class Human
    {
        public DateTime BirthDate { get; set; }
    }

    private class Person : Human
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}