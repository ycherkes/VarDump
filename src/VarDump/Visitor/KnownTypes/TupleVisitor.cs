using System;
using System.Linq;
using VarDump.CodeDom.Common;
using VarDump.CodeDom.Compiler;
using VarDump.Extensions;
using VarDump.Utils;

namespace VarDump.Visitor.KnownTypes;

internal sealed class TupleVisitor : IKnownObjectVisitor
{
    private readonly IObjectVisitor _rootObjectVisitor;
    private readonly IDotnetCodeGenerator _codeGenerator;

    public TupleVisitor(IObjectVisitor rootObjectVisitor, IDotnetCodeGenerator codeGenerator)
    {
        _rootObjectVisitor = rootObjectVisitor;
        _codeGenerator = codeGenerator;
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
            _codeGenerator.GenerateCircularReferenceDetected();
            return;
        }

        _rootObjectVisitor.PushVisited(o);

        try
        {
            var propertyValues = objectType.GetProperties().Select(p => ReflectionUtils.GetValue(p, o)).Select(v => (Action)(() => _rootObjectVisitor.Visit(v)));

            _codeGenerator.GenerateObjectCreateAndInitialize(new CodeDotnetTypeReference(objectType),
                propertyValues,
                []);
        }
        finally
        {
            _rootObjectVisitor.PopVisited();
        }
    }
}