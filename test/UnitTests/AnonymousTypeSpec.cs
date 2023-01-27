using VarDump;
using Xunit;

namespace UnitTests
{
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
    }
}