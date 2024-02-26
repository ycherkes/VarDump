using System;
using System.Linq;
using VarDump.CodeDom.Common;
using VarDump.CodeDom.Compiler;
using VarDump.Utils;

namespace VarDump.Visitor.KnownTypes;

internal sealed class TupleVisitor : IKnownObjectVisitor
{
    private readonly IObjectVisitor _rootObjectVisitor;
    private readonly ICodeGenerator _codeGenerator;

    public TupleVisitor(IObjectVisitor rootObjectVisitor, ICodeGenerator codeGenerator)
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
            CodeDomUtils.WriteCircularReferenceDetectedExpression(_codeGenerator);
            return;
        }

        _rootObjectVisitor.PushVisited(o);

        try
        {
            var propertyValues = objectType.GetProperties().Select(p => ReflectionUtils.GetValue(p, o)).Select(v => (Action)(() => _rootObjectVisitor.Visit(v)));

            _codeGenerator.GenerateObjectCreateAndInitialize(new CodeTypeReference(objectType),
                propertyValues,
                []);
        }
        finally
        {
            _rootObjectVisitor.PopVisited();
        }
    }
}