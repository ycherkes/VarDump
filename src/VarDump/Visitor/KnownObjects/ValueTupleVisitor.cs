using System;
using System.Linq;
using VarDump.CodeDom.Compiler;
using VarDump.Utils;

namespace VarDump.Visitor.KnownObjects;

internal sealed class ValueTupleVisitor(IRootVisitor rootVisitor, ICodeWriter codeWriter) : IKnownObjectVisitor
{
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return objectType.IsValueTuple();
    }

    public void Visit(object obj, Type objectType, VisitContext context)
    {
        var propertyValues = objectType.GetFields().Select(f => (Action)(() => rootVisitor.Visit(ReflectionUtils.GetValue(f, obj), context)));

        codeWriter.WriteValueTupleCreate(propertyValues);
    }
}