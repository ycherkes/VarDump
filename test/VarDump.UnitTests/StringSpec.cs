using Xunit;
using VarDump.Visitor;

namespace VarDump.UnitTests;

public class StringSpec
{
    [Fact]
    public void DumpLongStringCSharp()
    {
        var stringVar = "C:\\temp\\postgresql-13.1-1-windows-x64-binaries\\pgsql\\pgAdmin 4\\docs\\en_US\\html\\_sources\\add_restore_point_dialog.rst.txt";

        var dumper = new CSharpDumper();

        var result = dumper.Dump(stringVar);

        Assert.DoesNotContain("+", result);
    }

    [Fact]
    public void DumpLongStringVisualBasic()
    {
        var stringVar = "C:\\temp\\postgresql-13.1-1-windows-x64-binaries\\pgsql\\pgAdmin 4\\docs\\en_US\\html\\_sources\\add_restore_point_dialog.rst.txt";

        var dumper = new VisualBasicDumper();

        var result = dumper.Dump(stringVar);

        Assert.DoesNotContain("& _", result);
    }

    [Fact]
    public void DumpStringEscapedCSharp()
    {
        var stringVar = "line1\r\npath\\file\"name\"";

        var dumper = new CSharpDumper(new DumpOptions
        {
            CSharpStringLiteralStyle = CSharpStringLiteralStyle.Escaped
        });

        var result = dumper.Dump(stringVar);

        Assert.Equal("var stringValue = \"line1\\r\\npath\\\\file\\\"name\\\"\";\r\n", result, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void DumpStringVerbatimCSharp()
    {
        var stringVar = "line1\r\n\"quoted\"";

        var dumper = new CSharpDumper(new DumpOptions
        {
            CSharpStringLiteralStyle = CSharpStringLiteralStyle.Verbatim
        });

        var result = dumper.Dump(stringVar);

        Assert.Equal("var stringValue = @\"line1\r\n\"\"quoted\"\"\";\r\n", result, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void DumpStringRawCSharp()
    {
        var objColl  = new[]
        {
            new
            {
                Description = """
                              line1
                              line2
                              """
            }
        };

        var dumper = new CSharpDumper(new DumpOptions
        {
            CSharpStringLiteralStyle = CSharpStringLiteralStyle.Raw
        });

        var result = dumper.Dump(objColl);

        Assert.Equal(""""
                     var arrayOfAnonymousType = new []
                     {
                         new 
                         {
                             Description = """
                                           line1
                                           line2
                                           """
                         }
                     };
                     
                     """", result, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void DumpStringRawWithTripleQuotesCSharp()
    {
        var stringVar = "aa\"\"\"bb";

        var dumper = new CSharpDumper(new DumpOptions
        {
            CSharpStringLiteralStyle = CSharpStringLiteralStyle.Raw
        });

        var result = dumper.Dump(stringVar);

        Assert.Equal("var stringValue = \"\"\"\"aa\"\"\"bb\"\"\"\";\r\n", result, ignoreLineEndingDifferences: true);
    }
}
