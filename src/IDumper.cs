using VarDump.Visitor;

namespace VarDump
{
    public interface IDumper
    {
        string Dump(object obj, DumpOptions options = null);
    }
}
