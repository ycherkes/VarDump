using System;
using System.Linq;
using VarDump.CodeDom.Common;
using VarDump.CodeDom.Compiler;
using VarDump.Utils;

namespace VarDump.Visitor.KnownTypes;

internal sealed class ValueTupleVisitor : IKnownObjectVisitor
{
    private readonly IObjectVisitor _rootObjectVisitor;
    private readonly ICodeGenerator _codeGenerator;

    public ValueTupleVisitor(IObjectVisitor rootObjectVisitor, ICodeGenerator codeGenerator)
    {
        _rootObjectVisitor = rootObjectVisitor;
        _codeGenerator = codeGenerator;
    }
    public string Id => "ValueTuple";
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return objectType.IsValueTuple();
    }

    public void Visit(object obj, Type objectType)
    {
        var propertyValues = objectType.GetFields().Select(p => ReflectionUtils.GetValue(p, obj)).Select(v => (Action)(() => _rootObjectVisitor.Visit(v)));

        _codeGenerator.GenerateValueTupleCreate(propertyValues);
    }
}