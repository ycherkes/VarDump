using System;
using System.Linq;
using System.Reflection;
using VarDump.CodeDom.Compiler;
using VarDump.Utils;

namespace VarDump.Visitor.KnownTypes;

internal sealed class RecordVisitor(
    IObjectVisitor rootObjectVisitor,
    ICodeWriter codeWriter,
    bool useNamedArgumentsForReferenceRecordTypes)
    : IKnownObjectVisitor
{
    public string Id => "Record";
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return objectType.IsRecord();
    }

    public void Visit(object obj, Type objectType)
    {
        var properties = objectType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic).Where(p => p.CanWrite);
        var argumentValues = useNamedArgumentsForReferenceRecordTypes
            ? properties.Select(p => (Action)(() => codeWriter.WriteNamedArgument(p.Name, () => rootObjectVisitor.Visit(ReflectionUtils.GetValue(p, obj)))))
            : properties.Select(p => ReflectionUtils.GetValue(p, obj)).Select(value => (Action)(() => rootObjectVisitor.Visit(value)));

        codeWriter.WriteArrayCreate(objectType, argumentValues);
    }
}