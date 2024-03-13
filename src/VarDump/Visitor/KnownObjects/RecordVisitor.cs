using System;
using System.Linq;
using System.Reflection;
using VarDump.CodeDom.Compiler;
using VarDump.Utils;

namespace VarDump.Visitor.KnownObjects;

internal sealed class RecordVisitor(
    INextDepthVisitor nextDepthVisitor,
    ICodeWriter codeWriter,
    DumpOptions options)
    : IKnownObjectVisitor
{
    public string Id => "Record";

    public DumpOptions Options => options;

    public bool IsSuitableFor(object obj, Type objectType)
    {
        return objectType.IsRecord();
    }

    public void Visit(object obj, Type objectType, VisitContext context)
    {
        var properties = objectType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
                                                        .Where(p => p.CanWrite);

        var argumentValues = options.UseNamedArgumentsInConstructors
            ? properties.Select(p => (Action)(() => codeWriter.WriteNamedArgument(p.Name, () => nextDepthVisitor.Visit(ReflectionUtils.GetValue(p, obj), context))))
            : properties.Select(p => (Action)(() => nextDepthVisitor.Visit(ReflectionUtils.GetValue(p, obj), context)));

        codeWriter.WriteObjectCreate(objectType, argumentValues);
    }
}