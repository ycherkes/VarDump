using System;
using VarDump.CodeDom.Compiler;

namespace VarDump.Visitor.KnownObjects;

internal sealed class GuidVisitor(ICodeWriter codeWriter, bool useNamedArgumentsInConstructors) : IKnownObjectVisitor
{
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is Guid;
    }

    public void Visit(object obj, Type objectType, VisitContext context)
    {
        var guid = (Guid)obj;

        if (useNamedArgumentsInConstructors)
        {
            codeWriter.WriteObjectCreate(objectType, [() => codeWriter.WriteNamedArgument("g", () => codeWriter.WritePrimitive(guid.ToString("D")))]);
        }
        else
        {
            codeWriter.WriteObjectCreate(objectType, [() => codeWriter.WritePrimitive(guid.ToString("D"))]);
        }
    }
}