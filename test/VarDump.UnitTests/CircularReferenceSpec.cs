using Xunit;

namespace VarDump.UnitTests;

public class CircularReferenceSpec
{
    [Fact]
    public void DumpAssembly_ShouldNotThrowStackOverflowOrOutOfMemoryException()
    {
        var assembly = typeof(int).Assembly;

        var dumper = new CSharpDumper(new Visitor.DumpOptions { WritablePropertiesOnly = false});
        _ = dumper.Dump(assembly);
    }
}