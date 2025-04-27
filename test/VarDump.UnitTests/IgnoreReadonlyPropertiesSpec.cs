using System.Reflection;
using System.Runtime.Serialization;
using VarDump.Visitor;
using Xunit;

namespace VarDump.UnitTests;

public class IgnoreReadonlyPropertiesSpec
{
    [Fact]
    public void IgnoreReadonlyPropertiesCSharp()
    {
        var subjectDescriptor = new SubjectDescriptor("ukn", "identifier");

        var dumper = new CSharpDumper();

        var result = dumper.Dump(subjectDescriptor);

        Assert.Equal("var subjectDescriptor = new SubjectDescriptor();\r\n", result);
    }

    [Fact]
    public void WriteReadonlyPropertiesCSharp()
    {
        var subjectDescriptor = new SubjectDescriptor("ukn", "identifier");

        var dumper = new CSharpDumper(new DumpOptions{IgnoreReadonlyProperties = false});

        var result = dumper.Dump(subjectDescriptor);

        Assert.Equal("""
                     var subjectDescriptor = new SubjectDescriptor
                     {
                         SubjectType = "ukn",
                         Identifier = "identifier"
                     };
                     
                     """, result);
    }

    [Fact]
    public void WritePropertiesWithPrivateSettersCSharp()
    {
        var subjectDescriptor = new SubjectDescriptor("ukn", "identifier");

        var dumper = new CSharpDumper(new DumpOptions { GetPropertiesBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance });

        var result = dumper.Dump(subjectDescriptor);

        Assert.Equal("""
                     var subjectDescriptor = new SubjectDescriptor
                     {
                         SubjectType = "ukn",
                         Identifier = "identifier"
                     };

                     """, result);
    }

    private struct SubjectDescriptor(string subjectType, string identifier)
    {
        [DataMember]
        public string SubjectType { get; private set; } = subjectType;

        [DataMember]
        public string Identifier { get; private set; } = identifier;
    }
}

