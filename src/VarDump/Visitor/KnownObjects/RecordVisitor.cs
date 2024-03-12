using System;
using System.Linq;
using System.Reflection;
using VarDump.CodeDom.Compiler;
using VarDump.Utils;

namespace VarDump.Visitor.KnownObjects;

internal sealed class RecordVisitor(
    INextLevelVisitor nextLevelVisitor,
    ICodeWriter codeWriter,
    bool useNamedArgumentsInConstructors)
    : IKnownObjectVisitor
{
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return objectType.IsRecord();
    }

    public void Visit(object obj, Type objectType, VisitContext context)
    {
        var properties = objectType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
                                                        .Where(p => p.CanWrite);

        var argumentValues = useNamedArgumentsInConstructors
            ? properties.Select(p => (Action)(() => codeWriter.WriteNamedArgument(p.Name, () => nextLevelVisitor.Visit(ReflectionUtils.GetValue(p, obj), context))))
            : properties.Select(p => (Action)(() => nextLevelVisitor.Visit(ReflectionUtils.GetValue(p, obj), context)));

        codeWriter.WriteObjectCreate(objectType, argumentValues);
    }
}