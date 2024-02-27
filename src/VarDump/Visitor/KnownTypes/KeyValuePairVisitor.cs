using System;
using System.Linq;
using VarDump.CodeDom.Common;
using VarDump.CodeDom.Compiler;
using VarDump.Utils;

namespace VarDump.Visitor.KnownTypes;

internal sealed class KeyValuePairVisitor : IKnownObjectVisitor
{
    private readonly IObjectVisitor _rootObjectVisitor;
    private readonly IDotnetCodeGenerator _codeGenerator;

    public KeyValuePairVisitor(IObjectVisitor rootObjectVisitor, IDotnetCodeGenerator codeGenerator)
    {
        _rootObjectVisitor = rootObjectVisitor;
        _codeGenerator = codeGenerator;
    }

    public string Id => "KeyValuePair";
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return objectType.IsKeyValuePair();
    }

    public void Visit(object obj, Type objectType)
    {
        var propertyValues = objectType.GetProperties().Select(p => ReflectionUtils.GetValue(p, obj)).Select(v => (Action)(() => _rootObjectVisitor.Visit(v)));

        _codeGenerator.GenerateObjectCreateAndInitialize(new CodeDotnetTypeReference(objectType),
            propertyValues,
            []);
    }
}