using System;
using System.Linq;
using VarDump.CodeDom.Compiler;
using VarDump.Utils;

namespace VarDump.Visitor.KnownObjects;

internal sealed class KeyValuePairVisitor(INextLevelVisitor nextLevelVisitor, ICodeWriter codeWriter)
    : IKnownObjectVisitor
{
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return objectType.IsKeyValuePair();
    }

    public void Visit(object obj, Type objectType, VisitContext context)
    {
        var propertyValues = objectType.GetProperties().Select(p => (Action)(() => nextLevelVisitor.Visit(ReflectionUtils.GetValue(p, obj), context)));

        codeWriter.WriteObjectCreate(objectType, propertyValues);
    }
}