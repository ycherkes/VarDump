using System;
using System.Linq;
using VarDump.CodeDom.Compiler;
using VarDump.Utils;

namespace VarDump.Visitor.KnownTypes;

internal sealed class ValueTupleVisitor(IRootObjectVisitor rootObjectVisitor, ICodeWriter codeWriter) : IKnownObjectVisitor
{
    public string Id => "ValueTuple";
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return objectType.IsValueTuple();
    }

    public void Visit(object obj, Type objectType, VisitContext context)
    {
        var propertyValues = objectType.GetFields().Select(f => (Action)(() => rootObjectVisitor.Visit(ReflectionUtils.GetValue(f, obj), context)));

        codeWriter.WriteValueTupleCreate(propertyValues);
    }
}