using System;
using VarDump;
using Xunit;

namespace UnitTests
{
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
    BirthDate = new DateTime(1964, 6, 19, 0, 0, 0, 0, DateTimeKind.Utc)
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
    .BirthDate = New Date(1964, 6, 19, 0, 0, 0, 0, DateTimeKind.Utc)
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
}