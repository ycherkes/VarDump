using VarDump.Visitor;
using Xunit;

namespace VarDump.UnitTests;

public class TypeNamingPolicySpec
{
    [Theory]
    [InlineData(TypeNamingPolicy.ShortName,
        """
        var inner = new Inner
        {
            Name = "Nested value"
        };

        """)]
    [InlineData(TypeNamingPolicy.NestedQualified,
        """
        var inner = new TypeNamingPolicySpec.NestedParent.Inner
        {
            Name = "Nested value"
        };

        """)]
    [InlineData(TypeNamingPolicy.FullName,
        """
        var inner = new VarDump.UnitTests.TypeNamingPolicySpec.NestedParent.Inner
        {
            Name = "Nested value"
        };

        """)]
    public void DumpNestedTypeNamesAccordingToPolicy(TypeNamingPolicy policy, string expected)
    {
        var dumper = new CSharpDumper(new DumpOptions
        {
            TypeNamePolicy = policy
        });

        var result = dumper.Dump(new NestedParent.Inner
        {
            Name = "Nested value"
        });

        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }

    private class NestedParent
    {
        public class Inner
        {
            public string Name { get; set; }
        }
    }
}
