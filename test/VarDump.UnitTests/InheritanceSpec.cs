using System;
using System.Reflection;
using VarDump.Visitor;
using Xunit;

namespace VarDump.UnitTests;

public class InheritanceSpec
{

    [Fact]
    public void DumpClassCSharp()
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
            """
            var person = new Person
            {
                FirstName = "Boris",
                LastName = "Johnson",
                BirthDate = DateTime.ParseExact("1964-06-19T00:00:00.0000000Z", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
            };

            """, result);
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
            """
            Dim personValue = New Person With {
                .FirstName = "Boris",
                .LastName = "Johnson",
                .BirthDate = Date.ParseExact("1964-06-19T00:00:00.0000000Z", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
            }

            """, result);
    }

    [Fact]
    public void DumpAllFieldsIncludingBaseCSharp()
    {
        var dumpOptions = new DumpOptions
        {
            GetFieldsBindingFlags = BindingFlags.NonPublic | 
                                    BindingFlags.Public | 
                                    BindingFlags.Instance, 
            GetBaseClassFields = true
        };

        var derived = new DerivedClass(10, 20);

        var dumper = new CSharpDumper(dumpOptions);

        var result = dumper.Dump(derived);

        Assert.Equal(
            """
            var derivedClass = new DerivedClass
            {
                _baseNumber = 10,
                _derivedNumber = 20
            };
            
            """, result);
    }

    [Fact]
    public void DumpAllFieldsIncludingBaseVb()
    {
        var dumpOptions = new DumpOptions
        {
            GetFieldsBindingFlags = BindingFlags.NonPublic |
                                    BindingFlags.Public |
                                    BindingFlags.Instance,
            GetBaseClassFields = true
        };

        var derived = new DerivedClass(10, 20);

        var dumper = new VisualBasicDumper(dumpOptions);

        var result = dumper.Dump(derived);

        Assert.Equal(
            """
            Dim derivedClassValue = New DerivedClass With {
                ._baseNumber = 10,
                ._derivedNumber = 20
            }
            
            """, result);
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

    private class BaseClass(int baseNumber)
    {
        private int _baseNumber = baseNumber;
    }

    private class DerivedClass(int baseNumber, int derivedNumber) : BaseClass(baseNumber)
    {
        private int _derivedNumber = derivedNumber;
    }
}