using System;
using System.Linq;
using System.Reflection;
using VarDump.CodeDom.Compiler;
using VarDump.Utils;

namespace VarDump.Visitor.KnownTypes;

internal sealed class RecordVisitor(
    IRootObjectVisitor rootObjectVisitor,
    ICodeWriter codeWriter,
    bool useNamedArguments)
    : IKnownObjectVisitor
{
    public string Id => "Record";
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return objectType.IsRecord();
    }

    public void Visit(object obj, Type objectType, VisitContext context)
    {
        var properties = objectType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
                                                        .Where(p => p.CanWrite);

        var argumentValues = useNamedArguments
            ? properties.Select(p => (Action)(() => codeWriter.WriteNamedArgument(p.Name, () => rootObjectVisitor.Visit(ReflectionUtils.GetValue(p, obj), context))))
            : properties.Select(p => (Action)(() => rootObjectVisitor.Visit(ReflectionUtils.GetValue(p, obj), context)));

        codeWriter.WriteObjectCreate(objectType, argumentValues);
    }
}