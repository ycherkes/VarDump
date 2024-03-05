using System;
using System.Linq;
using VarDump.CodeDom.Compiler;
using VarDump.Extensions;
using VarDump.Utils;

namespace VarDump.Visitor.KnownTypes;

internal sealed class TupleVisitor(IObjectVisitor rootObjectVisitor, ICodeWriter codeWriter) : IKnownObjectVisitor
{
    public string Id => "Tuple";
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return objectType.IsTuple();
    }

    public void Visit(object o, Type objectType)
    {
        if (rootObjectVisitor.IsVisited(o))
        {
            codeWriter.WriteCircularReferenceDetected();
            return;
        }

        rootObjectVisitor.PushVisited(o);

        try
        {
            var propertyValues = objectType.GetProperties().Select(p => ReflectionUtils.GetValue(p, o)).Select(v => (Action)(() => rootObjectVisitor.Visit(v)));

            codeWriter.WriteObjectCreate(objectType, propertyValues);
        }
        finally
        {
            rootObjectVisitor.PopVisited();
        }
    }
}