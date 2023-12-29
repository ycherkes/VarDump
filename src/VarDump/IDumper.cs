using System.IO;

namespace VarDump;

public interface IDumper
{
    string Dump(object obj);

    void Dump(object obj, TextWriter textWriter);
}