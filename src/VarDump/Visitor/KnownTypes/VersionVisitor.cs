using System;
using VarDump.CodeDom.Compiler;

namespace VarDump.Visitor.KnownTypes;

internal sealed class VersionVisitor(ICodeWriter codeWriter) : IKnownObjectVisitor
{
    public string Id => nameof(Version);

    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is Version;
    }

    public void Visit(object obj, Type objectType)
    {
        codeWriter.WriteObjectCreate(typeof(Version), [() => codeWriter.WritePrimitive(obj.ToString())]);
    }
}