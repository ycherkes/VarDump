using VarDump;
using VarDump.Visitor;

public class VarDumpFactories
{
    public static readonly Func<DumpOptions, IDumper> CSharp = options => new CSharpDumper(options);
    public static readonly Func<DumpOptions, IDumper> VisualBasic = options => new VisualBasicDumper(options);
}