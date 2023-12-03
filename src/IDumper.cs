using System.IO;

namespace VarDumpExtended;

public interface IDumper
{
    string Dump(object obj);

    void Dump(object obj, TextWriter textWriter);
}