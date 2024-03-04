using System.IO;
using Xunit;

namespace VarDump.UnitTests;

public class CustomNewLineSpec
{
    [Fact]
    public void DumpWithModifiedNewLine()
    {
        var obj = new
        {
            Level1 = new
            {
                Level2 = new
                {
                    Level3 = "Level3"
                }
            }
        };

        using var stringWriter = new StringWriter();
        stringWriter.NewLine = "\n";

        var dumper = new CSharpDumper();

        dumper.Dump(obj, stringWriter);
        var result = stringWriter.ToString();

        Assert.Equal("var anonymousType = new \n{\n    Level1 = new \n    {\n        Level2 = new \n        {\n            Level3 = \"Level3\"\n        }\n    }\n};\n", result);
    }
}