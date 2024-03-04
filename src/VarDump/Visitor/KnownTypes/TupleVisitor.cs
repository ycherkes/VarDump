using System;
using System.Linq;
using VarDump.CodeDom.Compiler;
using VarDump.Extensions;
using VarDump.Utils;

namespace VarDump.Visitor.KnownTypes;

internal sealed class TupleVisitor : IKnownObjectVisitor
{
    private readonly IObjectVisitor _rootObjectVisitor;
    private readonly ICodeWriter _codeWriter;

    public TupleVisitor(IObjectVisitor rootObjectVisitor, ICodeWriter codeWriter)
    {
        _rootObjectVisitor = rootObjectVisitor;
        _codeWriter = codeWriter;
    }

    public string Id => "Tuple";
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return objectType.IsTuple();
    }

    public void Visit(object o, Type objectType)
    {
        if (_rootObjectVisitor.IsVisited(o))
        {
            _codeWriter.WriteCircularReferenceDetected();
            return;
        }

        _rootObjectVisitor.PushVisited(o);

        try
        {
            var propertyValues = objectType.GetProperties().Select(p => ReflectionUtils.GetValue(p, o)).Select(v => (Action)(() => _rootObjectVisitor.Visit(v)));

            _codeWriter.WriteObjectCreateAndInitialize(objectType,
                propertyValues,
                []);
        }
        finally
        {
            _rootObjectVisitor.PopVisited();
        }
    }
}