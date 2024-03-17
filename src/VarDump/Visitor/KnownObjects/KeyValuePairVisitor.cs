using System;
using System.Linq;
using VarDump.CodeDom.Compiler;
using VarDump.Utils;

namespace VarDump.Visitor.KnownObjects;

internal sealed class KeyValuePairVisitor(INextDepthVisitor nextDepthVisitor, ICodeWriter codeWriter, DumpOptions options) : IKnownObjectVisitor
{
    public string Id => "KeyValuePair";

    public bool IsSuitableFor(object obj, Type objectType)
    {
        return objectType.IsKeyValuePair();
    }

    public void ConfigureOptions(Action<DumpOptions> configure)
    {
        options = options.Clone();
        configure?.Invoke(options);
    }

    public void Visit(object obj, Type objectType, VisitContext context)
    {
        var objectProperties = objectType.GetProperties();

        var constructorArguments = options.UseNamedArgumentsInConstructors
            ? objectProperties.Select(p => (Action)(() => codeWriter.WriteNamedArgument(p.Name.ToLowerInvariant(), () => nextDepthVisitor.Visit(ReflectionUtils.GetValue(p, obj), context))))
            : objectProperties.Select(p => (Action)(() => nextDepthVisitor.Visit(ReflectionUtils.GetValue(p, obj), context)));

        codeWriter.WriteObjectCreate(objectType, constructorArguments);
    }
}