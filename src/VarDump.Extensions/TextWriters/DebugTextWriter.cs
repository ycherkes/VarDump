using System.Diagnostics;
using System.Text;

namespace VarDump.Extensions.TextWriters;
// see original version MIT licensed https://github.com/MoaidHathot/Dumpify/blob/main/src/Dumpify/Outputs/TextWriters/DebugTextWriter.cs
internal class DebugTextWriter : TextWriter
{
    public override Encoding Encoding { get; } = Encoding.UTF8;

    public override void Write(char value)
        => Debug.Write(value);

    public override void WriteLine()
        => Debug.WriteLine("");

    public override void Write(string? value)
        => Debug.Write(value);

    public override void WriteLine(string? value)
        => Debug.WriteLine(value);
}