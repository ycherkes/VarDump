using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VarDump;
using VarDump.Utils;
using VarDump.Visitor;
using Xunit;

namespace UnitTests;

public class AnonymousTypeSpec
{
    [Fact]
    public void DumpAnonymousTypeCsharp()
    {
        var anonymous = new[]
        {
            new { Name = "Steeve", Age = (int?)int.MaxValue, Reference = "Test reference" },
            new { Name = "Peter", Age = (int?)null, Reference = (string)null }
        };

        var dumper = new CSharpDumper();

        var result = dumper.Dump(anonymous);

        Assert.Equal(
            @"var arrayOfAnonymousType = new []
{
    new 
    {
        Name = ""Steeve"",
        Age = (int?)int.MaxValue,
        Reference = ""Test reference""
    },
    new 
    {
        Name = ""Peter"",
        Age = (int?)null,
        Reference = (string)null
    }
};
", result);
    }

    [Fact]
    public async Task DumpAnonymousTypeCsharpAsync()
    {
        var anonymous = new[]
        {
            new { Name = "Steeve", Age = (int?)int.MaxValue, Reference = "Test reference" },
            new { Name = "Peter", Age = (int?)null, Reference = (string)null }
        };

        var dumper = new CSharpDumper();
        using var sourceWriter = new StringWriter();
        await dumper.DumpAsync(anonymous, sourceWriter);
        var result = sourceWriter.ToString();

        Assert.Equal(
            """
            var arrayOfAnonymousType = new []
            {
                new 
                {
                    Name = "Steeve",
                    Age = (int?)int.MaxValue,
                    Reference = "Test reference"
                },
                new 
                {
                    Name = "Peter",
                    Age = (int?)null,
                    Reference = (string)null
                }
            };

            """, result);
    }

    [Fact]
    public void DumpAnonymousTypeVb()
    {
        var anonymous = new[]
        {
            new { Name = "Steeve", Age = (int?)int.MaxValue, Reference = "Test reference" },
            new { Name = "Peter", Age = (int?)null, Reference = (string)null }
        };

        var dumper = new VisualBasicDumper();

        var result = dumper.Dump(anonymous);

        Assert.Equal(
            @"Dim arrayOfAnonymousType = {
    New With {
        .Name = ""Steeve"",
        .Age = CType(Integer.MaxValue, Integer?),
        .Reference = ""Test reference""
    },
    New With {
        .Name = ""Peter"",
        .Age = CType(Nothing, Integer?),
        .Reference = CType(Nothing, String)
    }
}
", result);
    }

    [Fact]
    public void DetectAnonymousType()
    {
        var expectedTypeNames = new[]
        {
            "<>f__AnonymousType0`2[<Name>j__TPar,<Type>j__TPar]",
            "<>f__AnonymousType1`2[<Key>j__TPar,<Element>j__TPar]"
        };

        var actualTypeNames = typeof(ObjectVisitor)
            .Assembly
            .GetTypes()
            .Where(ReflectionUtils.IsAnonymousType)
            .Select(x => x.ToString())
            .ToArray();


        Assert.Equal(expectedTypeNames, actualTypeNames);
    }
}