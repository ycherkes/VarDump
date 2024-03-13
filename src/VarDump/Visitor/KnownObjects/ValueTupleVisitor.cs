using System;
using System.Linq;
using VarDump.CodeDom.Compiler;
using VarDump.Utils;

namespace VarDump.Visitor.KnownObjects;

internal sealed class ValueTupleVisitor(INextDepthVisitor nextDepthVisitor, ICodeWriter codeWriter, DumpOptions options) : IKnownObjectVisitor
{
    public string Id => "ValueTuple";

    public DumpOptions Options => options;

    public bool IsSuitableFor(object obj, Type objectType)
    {
        return objectType.IsValueTuple();
    }

    public void Visit(object obj, Type objectType, VisitContext context)
    {
        var objectFields = objectType.GetFields();

        var constructorArguments = options.UseNamedArgumentsInConstructors
            ? objectFields.Select(f => (Action)(() => codeWriter.WriteNamedArgument(f.Name.ToLowerInvariant(), () => nextDepthVisitor.Visit(ReflectionUtils.GetValue(f, obj), context))))
            : objectFields.Select(f => (Action)(() => nextDepthVisitor.Visit(ReflectionUtils.GetValue(f, obj), context)));

        codeWriter.WriteValueTupleCreate(constructorArguments);
    }
}