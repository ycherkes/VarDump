using System;
using System.IO;
using VarDump.Visitor;
using Xunit;

namespace VarDump.UnitTests;

public class NewLineOptionSpec
{
    [Theory]
    [InlineData("\n")]
    [InlineData("\r\n")]
    public void CSharpDumperUsesConfiguredNewLine(string newLine)
    {
        var dumper = new CSharpDumper(new DumpOptions
        {
            NewLine = newLine
        });

        var result = dumper.Dump(new[] { 1, 2 });

        AssertUsesOnlyNewLine(result, newLine);
    }

    [Theory]
    [InlineData("\n")]
    [InlineData("\r\n")]
    public void VisualBasicDumperUsesConfiguredNewLine(string newLine)
    {
        var dumper = new VisualBasicDumper(new DumpOptions
        {
            NewLine = newLine
        });

        var result = dumper.Dump(new[] { 1, 2 });

        AssertUsesOnlyNewLine(result, newLine);
    }

    [Fact]
    public void DumpToTextWriterUsesConfiguredNewLineAndRestoresOriginalNewLine()
    {
        using var writer = new StringWriter
        {
            NewLine = "\r\n"
        };
        var dumper = new CSharpDumper(new DumpOptions
        {
            NewLine = "\n"
        });

        dumper.Dump(new[] { 1, 2 }, writer);

        var result = writer.ToString();
        AssertUsesOnlyNewLine(result, "\n");
        Assert.Equal("\r\n", writer.NewLine, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void DumpOptionsCloneCopiesNewLine()
    {
        var options = new DumpOptions
        {
            NewLine = "\n"
        };

        var clone = options.Clone();

        Assert.Equal("\n", clone.NewLine, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void CSharpDumperRejectsNullNewLine()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new CSharpDumper(new DumpOptions
        {
            NewLine = null!
        }));

        Assert.Equal("NewLine", exception.ParamName, ignoreLineEndingDifferences: true);
    }

    private static void AssertUsesOnlyNewLine(string text, string expectedNewLine)
    {
        Assert.Contains(expectedNewLine, text);

        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] == '\r')
            {
                Assert.Equal("\r\n", expectedNewLine, ignoreLineEndingDifferences: true);
                Assert.True(i + 1 < text.Length && text[i + 1] == '\n', "Carriage return must be followed by line feed.");
                i++;
            }
            else if (text[i] == '\n')
            {
                Assert.Equal("\n", expectedNewLine, ignoreLineEndingDifferences: true);
            }
        }
    }
}
