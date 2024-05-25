using VarDump.Visitor;
using Xunit;

namespace VarDump.UnitTests;

public class CircularReferenceSpec
{
    [Fact]
    public void DumpAssemblyCSharp_ShouldNotThrowStackOverflowOrOutOfMemoryException()
    {
        var assembly = typeof(int).Assembly;

        var dumper = new CSharpDumper(new DumpOptions { IgnoreReadonlyProperties = false});
        _  = dumper.Dump(assembly);
    }

    [Fact]
    public void DumpAssemblyVisualBasic_ShouldNotThrowStackOverflowOrOutOfMemoryException()
    {
        var assembly = typeof(int).Assembly;

        var dumper = new VisualBasicDumper(new DumpOptions { IgnoreReadonlyProperties = false });
        _ = dumper.Dump(assembly);
    }
}