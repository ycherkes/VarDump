using System;
using System.Linq;
using System.Reflection;
using VarDump.CodeDom.Compiler;
using VarDump.Extensions;
using VarDump.Utils;

namespace VarDump.Visitor.KnownTypes;

internal sealed class RecordVisitor : IKnownObjectVisitor
{
    private readonly IObjectVisitor _rootObjectVisitor;
    private readonly ICodeWriter _codeWriter;
    private readonly bool _useNamedArgumentsForReferenceRecordTypes;

    public RecordVisitor(IObjectVisitor rootObjectVisitor, ICodeWriter codeWriter, bool useNamedArgumentsForReferenceRecordTypes)
    {
        _rootObjectVisitor = rootObjectVisitor;
        _codeWriter = codeWriter;
        _useNamedArgumentsForReferenceRecordTypes = useNamedArgumentsForReferenceRecordTypes;
    }

    public string Id => "Record";
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return objectType.IsRecord();
    }

    public void Visit(object obj, Type objectType)
    {
        var properties = objectType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic).Where(p => p.CanWrite);
        var argumentValues = _useNamedArgumentsForReferenceRecordTypes
            ? properties.Select(p => (Action)(() => _codeWriter.WriteNamedArgument(p.Name, () => _rootObjectVisitor.Visit(ReflectionUtils.GetValue(p, obj)))))
            : properties.Select(p => ReflectionUtils.GetValue(p, obj)).Select(value => (Action)(() => _rootObjectVisitor.Visit(value)));

        _codeWriter.WriteObjectCreateAndInitialize(objectType,
            argumentValues,
            []);
    }
}