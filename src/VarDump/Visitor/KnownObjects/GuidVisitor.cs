using System;
using VarDump.CodeDom.Compiler;

namespace VarDump.Visitor.KnownObjects;

internal sealed class GuidVisitor(ICodeWriter codeWriter, DumpOptions options) : IKnownObjectVisitor
{
    public string Id => nameof(Guid);

    public DumpOptions Options => options;

    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is Guid;
    }

    public void Visit(object obj, Type objectType, VisitContext context)
    {
        var guid = (Guid)obj;

        if (options.UseNamedArgumentsInConstructors)
        {
            codeWriter.WriteObjectCreate(objectType, [() => codeWriter.WriteNamedArgument("g", WriteGuidString)]);
        }
        else
        {
            codeWriter.WriteObjectCreate(objectType, [WriteGuidString]);
        }

        return;

        void WriteGuidString() => codeWriter.WritePrimitive(guid.ToString("D"));
    }
}