using System;
using System.Linq;
using VarDump.CodeDom.Compiler;
using VarDump.Extensions;
using VarDump.Utils;

namespace VarDump.Visitor.KnownObjects;

internal sealed class TupleVisitor(INextDepthVisitor nextDepthVisitor, ICodeWriter codeWriter, DumpOptions options) : IKnownObjectVisitor
{
    public string Id => "Tuple";

    public DumpOptions Options => options;

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
            var propertyValues = objectType.GetProperties().Select(p => (Action)(() => nextDepthVisitor.Visit(ReflectionUtils.GetValue(p, o), context)));

            codeWriter.WriteObjectCreate(objectType, propertyValues);
        }
        finally
        {
            context.PopVisited();
        }
    }
}