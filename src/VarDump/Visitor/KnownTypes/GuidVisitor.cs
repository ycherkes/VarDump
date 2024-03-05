using System;
using VarDump.CodeDom.Compiler;

namespace VarDump.Visitor.KnownTypes;

internal sealed class GuidVisitor(ICodeWriter codeWriter) : IKnownObjectVisitor
{
    public string Id => nameof(Guid);
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is Guid;
    }

    public void Visit(object obj, Type objectType)
    {
        var guid = (Guid)obj;

        codeWriter.WriteObjectCreate(objectType, [() => codeWriter.WritePrimitive(guid.ToString("D"))]);
    }
}