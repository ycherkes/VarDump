using System;
using System.Linq;
using VarDumpExtended.CodeDom.Common;
using VarDumpExtended.Utils;

namespace VarDumpExtended.Visitor.KnownTypes;

internal sealed class ValueTupleVisitor : IKnownObjectVisitor
{
    private readonly IObjectVisitor _rootObjectVisitor;

    public ValueTupleVisitor(IObjectVisitor rootObjectVisitor)
    {
        _rootObjectVisitor = rootObjectVisitor;
    }
    public string Id => "ValueTuple";
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return objectType.IsValueTuple();
    }

    public CodeExpression Visit(object obj, Type objectType)
    {
        var propertyValues = objectType.GetFields().Select(p => ReflectionUtils.GetValue(p, obj)).Select(_rootObjectVisitor.Visit);

        return new CodeValueTupleCreateExpression(propertyValues.ToArray());
    }
}