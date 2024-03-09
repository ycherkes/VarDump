using System;
using System.Linq;
using VarDump.CodeDom.Compiler;
using VarDump.Extensions;
using VarDump.Utils;

namespace VarDump.Visitor.KnownTypes;

internal sealed class TupleVisitor(IRootObjectVisitor rootObjectVisitor, ICodeWriter codeWriter) : IKnownObjectVisitor
{
    public string Id => "Tuple";
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return objectType.IsTuple();
    }

    public void Visit(object o, Type objectType, VisitContext context)
    {
        if (context.IsVisited(o))
        {
            codeWriter.WriteCircularReferenceDetected();
            return;
        }

        context.PushVisited(o);

        try
        {
            var propertyValues = objectType.GetProperties().Select(p => (Action)(() => rootObjectVisitor.Visit(ReflectionUtils.GetValue(p, o), context)));

            codeWriter.WriteObjectCreate(objectType, propertyValues);
        }
        finally
        {
            context.PopVisited();
        }
    }
}