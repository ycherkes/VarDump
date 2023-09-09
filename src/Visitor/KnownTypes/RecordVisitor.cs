using System;
using System.Linq;
using System.Reflection;
using VarDumpExtended.CodeDom.Common;
using VarDumpExtended.Utils;

namespace VarDumpExtended.Visitor.KnownTypes;

internal sealed class RecordVisitor : IKnownObjectVisitor
{
    private readonly IObjectVisitor _rootObjectVisitor;
    private readonly bool _useNamedArgumentsForReferenceRecordTypes;
    private readonly CodeTypeReferenceOptions _typeReferenceOptions;

    public RecordVisitor(DumpOptions options, IObjectVisitor rootObjectVisitor)
    {
        _rootObjectVisitor = rootObjectVisitor;
        _useNamedArgumentsForReferenceRecordTypes = options.UseNamedArgumentsForReferenceRecordTypes;
        _typeReferenceOptions = options.UseTypeFullName
            ? CodeTypeReferenceOptions.FullTypeName
            : CodeTypeReferenceOptions.ShortTypeName;
    }

    public string Id => "Record";
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return objectType.IsRecord();
    }

    public CodeExpression Visit(object obj, Type objectType)
    {
        var properties = objectType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic).Where(p => p.CanWrite);
        var argumentValues = _useNamedArgumentsForReferenceRecordTypes 
            ? properties.Select(p => (CodeExpression)new CodeNamedArgumentExpression(p.Name, _rootObjectVisitor.Visit(ReflectionUtils.GetValue(p, obj))))
            : properties.Select(p => ReflectionUtils.GetValue(p, obj)).Select(_rootObjectVisitor.Visit);

        return new CodeObjectCreateExpression(
            new CodeTypeReference(objectType, _typeReferenceOptions),
            argumentValues.ToArray());
    }
}