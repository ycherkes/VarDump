using System.Diagnostics;
using System.Text;

namespace VarDump.Extensions.TextWriters;

// see original version MIT licensed https://github.com/MoaidHathot/Dumpify/blob/main/src/Dumpify/Outputs/TextWriters/TraceTextWriter.cs
internal class TraceTextWriter : TextWriter
{
    public override Encoding Encoding { get; } = Encoding.UTF8;

    public override void Write(char value)
        => Trace.Write(value);

    public override void WriteLine()
        => Trace.WriteLine("");

    public override void Write(string? value)
        => Trace.Write(value);

    public override void WriteLine(string? value)
        => Trace.WriteLine(value);
}