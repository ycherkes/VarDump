using System;
using System.Linq;
using VarDump.CodeDom.Compiler;
using VarDump.Extensions;
using VarDump.Utils;

namespace VarDump.Visitor.KnownObjects;

internal sealed class TupleVisitor(INextDepthVisitor nextDepthVisitor, ICodeWriter codeWriter, DumpOptions options) : IKnownObjectVisitor
{
    public string Id => "Tuple";

    public bool IsSuitableFor(object obj, Type objectType)
    {
        return objectType.IsTuple();
    }

    public void ConfigureOptions(Action<DumpOptions> configure)
    {
        options = options.Clone();
        configure?.Invoke(options);
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
            var objectProperties = objectType.GetProperties();

            var constructorArguments = options.UseNamedArgumentsInConstructors 
                ? objectProperties.Select(p => (Action)(() => codeWriter.WriteNamedArgument(p.Name.ToLowerInvariant() ,() => nextDepthVisitor.Visit(ReflectionUtils.GetValue(p, o), context)))) 
                : objectProperties.Select(p => (Action)(() => nextDepthVisitor.Visit(ReflectionUtils.GetValue(p, o), context)));

            codeWriter.WriteObjectCreate(objectType, constructorArguments);
        }
        finally
        {
            context.PopVisited();
        }
    }
}