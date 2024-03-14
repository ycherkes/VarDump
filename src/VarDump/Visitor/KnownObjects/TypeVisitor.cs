using System;
using VarDump.CodeDom.Compiler;

namespace VarDump.Visitor.KnownObjects;

internal sealed class TypeVisitor(ICodeWriter codeWriter) : IKnownObjectVisitor
{
    public string Id => nameof(Type);

    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is Type;
    }

    public void ConfigureOptions(Action<DumpOptions> configure)
    {
    }

    public void Visit(object obj, Type objectType, VisitContext context)
    {
       codeWriter.WriteTypeOf((Type)obj);
    }
}