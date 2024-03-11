using System;
using VarDump.CodeDom.Compiler;

namespace VarDump.Visitor.KnownObjects;

internal sealed class VersionVisitor(ICodeWriter codeWriter) : IKnownObjectVisitor
{
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is Version;
    }

    public void Visit(object obj, Type objectType, VisitContext context)
    {
        codeWriter.WriteObjectCreate(typeof(Version), [() => codeWriter.WritePrimitive(obj.ToString())]);
    }
}