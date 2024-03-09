using System;
using System.Linq;
using VarDump.CodeDom.Compiler;
using VarDump.Utils;

namespace VarDump.Visitor.KnownTypes;

internal sealed class KeyValuePairVisitor(IRootObjectVisitor rootObjectVisitor, ICodeWriter codeWriter)
    : IKnownObjectVisitor
{
    public string Id => "KeyValuePair";
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return objectType.IsKeyValuePair();
    }

    public void Visit(object obj, Type objectType, VisitContext context)
    {
        var propertyValues = objectType.GetProperties().Select(p => (Action)(() => rootObjectVisitor.Visit(ReflectionUtils.GetValue(p, obj), context)));

        codeWriter.WriteObjectCreate(objectType, propertyValues);
    }
}