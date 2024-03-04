using System;
using System.Linq;
using VarDump.CodeDom.Compiler;
using VarDump.Extensions;
using VarDump.Utils;

namespace VarDump.Visitor.KnownTypes;

internal sealed class KeyValuePairVisitor : IKnownObjectVisitor
{
    private readonly IObjectVisitor _rootObjectVisitor;
    private readonly ICodeWriter _codeWriter;

    public KeyValuePairVisitor(IObjectVisitor rootObjectVisitor, ICodeWriter codeWriter)
    {
        _rootObjectVisitor = rootObjectVisitor;
        _codeWriter = codeWriter;
    }

    public string Id => "KeyValuePair";
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return objectType.IsKeyValuePair();
    }

    public void Visit(object obj, Type objectType)
    {
        var propertyValues = objectType.GetProperties().Select(p => ReflectionUtils.GetValue(p, obj)).Select(v => (Action)(() => _rootObjectVisitor.Visit(v)));

        _codeWriter.WriteObjectCreateAndInitialize(objectType,
            propertyValues,
            []);
    }
}