using System.Diagnostics;
using VarDump;
using VarDump.Extensions.TextWriters;
using VarDump.Visitor;

public static class VarDumpExtensions
{
    public static Func<DumpOptions, IDumper> VarDumpFactory { get; set; } = VarDumpFactories.CSharp;
    public static DumpOptions DefaultDumpOptions { get; set; } = new();
    private static TextWriter DebugTextWriter { get; } = new DebugTextWriter();
    private static TextWriter TraceTextWriter { get; } = new TraceTextWriter();

    [Obsolete("Please use DumpText instead. In next version this method will dump to console.")]
    public static string Dump(this object obj)
    {
        return DumpText(obj);
    }

    [Obsolete("Please use DumpText instead. In next version this method will dump to console.")]
    public static string Dump(this object obj, DumpOptions options)
    {
        return DumpText(obj, options);
    }

    public static string DumpText(this object obj)
    {
        return VarDumpFactory(DefaultDumpOptions).Dump(obj);
    }

    public static string DumpText(this object obj, DumpOptions options)
    {
        return VarDumpFactory(options).Dump(obj);
    }

    public static void Dump(this object obj, DumpOptions options, TextWriter textWriter)
    {
        VarDumpFactory(options).Dump(obj, textWriter);
    }

    public static void DumpConsole(this object obj)
    {
        VarDumpFactory(DefaultDumpOptions).Dump(obj, Console.Out);
    }

    public static void DumpConsole(this object obj, DumpOptions options)
    {
        VarDumpFactory(options).Dump(obj, Console.Out);
    }

    public static void DumpDebug(this object obj)
    {
        VarDumpFactory(DefaultDumpOptions).Dump(obj, DebugTextWriter);
    }

    public static void DumpDebug(this object obj, DumpOptions options)
    {
        VarDumpFactory(options).Dump(obj, DebugTextWriter);
    }

    public static void DumpTrace(this object obj)
    {
        VarDumpFactory(DefaultDumpOptions).Dump(obj, TraceTextWriter);
    }

    public static void DumpTrace(this object obj, DumpOptions options)
    {
        VarDumpFactory(options).Dump(obj, TraceTextWriter);
    }
}
