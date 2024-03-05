using System;
using System.Linq;
using VarDump.CodeDom.Compiler;
using VarDump.Utils;

namespace VarDump.Visitor.KnownTypes;

internal sealed class KeyValuePairVisitor(IObjectVisitor rootObjectVisitor, ICodeWriter codeWriter)
    : IKnownObjectVisitor
{
    public string Id => "KeyValuePair";
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return objectType.IsKeyValuePair();
    }

    public void Visit(object obj, Type objectType)
    {
        var propertyValues = objectType.GetProperties().Select(p => ReflectionUtils.GetValue(p, obj)).Select(v => (Action)(() => rootObjectVisitor.Visit(v)));

        codeWriter.WriteObjectCreate(objectType, propertyValues);
    }
}