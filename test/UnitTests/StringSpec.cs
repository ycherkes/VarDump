using VarDumpExtended;
using Xunit;

namespace UnitTests;

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
}