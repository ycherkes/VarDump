using VarDump.Visitor;

namespace VarDump.Extensions;

public static class VarDumpExtensions
{
    public static Func<DumpOptions, IDumper> VarDumpFactory { get; set; } = VarDumpFactories.CSharp;
    public static DumpOptions DefaultDumpOptions { get; set; } = new();

    public static string Dump(this object obj)
    {
        return VarDumpFactory(DefaultDumpOptions).Dump(obj);
    }

    public static string Dump(this object obj, DumpOptions options)
    {
        return VarDumpFactory(options).Dump(obj);
    }

    public static void Dump(this object obj, DumpOptions options, TextWriter textWriter)
    {
        VarDumpFactory(options).Dump(obj, textWriter);
    }
}

public class VarDumpFactories
{
    public static readonly Func<DumpOptions, IDumper> CSharp = options => new CSharpDumper(options);
    public static readonly Func<DumpOptions, IDumper> VisualBasic = options => new VisualBasicDumper(options);
}