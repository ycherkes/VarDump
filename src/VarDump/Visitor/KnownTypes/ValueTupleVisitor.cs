using System;
using System.Linq;
using VarDump.CodeDom.Compiler;
using VarDump.Utils;

namespace VarDump.Visitor.KnownTypes;

internal sealed class ValueTupleVisitor : IKnownObjectVisitor
{
    private readonly IObjectVisitor _rootObjectVisitor;
    private readonly ICodeWriter _codeWriter;

    public ValueTupleVisitor(IObjectVisitor rootObjectVisitor, ICodeWriter codeWriter)
    {
        _rootObjectVisitor = rootObjectVisitor;
        _codeWriter = codeWriter;
    }
    public string Id => "ValueTuple";
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return objectType.IsValueTuple();
    }

    public void Visit(object obj, Type objectType)
    {
        var propertyValues = objectType.GetFields().Select(p => ReflectionUtils.GetValue(p, obj)).Select(v => (Action)(() => _rootObjectVisitor.Visit(v)));

        _codeWriter.WriteValueTupleCreate(propertyValues);
    }
}