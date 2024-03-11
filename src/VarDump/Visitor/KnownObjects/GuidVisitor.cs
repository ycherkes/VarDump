using System;
using VarDump.CodeDom.Compiler;

namespace VarDump.Visitor.KnownObjects;

internal sealed class GuidVisitor(ICodeWriter codeWriter) : IKnownObjectVisitor
{
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is Guid;
    }

    public void Visit(object obj, Type objectType, VisitContext context)
    {
        var guid = (Guid)obj;

        codeWriter.WriteObjectCreate(objectType, [() => codeWriter.WritePrimitive(guid.ToString("D"))]);
    }
}