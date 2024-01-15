using System;
using System.Linq;
using VarDump.CodeDom.Common;
using VarDump.Utils;

namespace VarDump.Visitor.KnownTypes;

internal sealed class KeyValuePairVisitor : IKnownObjectVisitor
{
    private readonly IObjectVisitor _rootObjectVisitor;
    private readonly CodeTypeReferenceOptions _typeReferenceOptions;

    public KeyValuePairVisitor(DumpOptions options, IObjectVisitor rootObjectVisitor)
    {
        _rootObjectVisitor = rootObjectVisitor;
        _typeReferenceOptions = options.UseTypeFullName
            ? CodeTypeReferenceOptions.FullTypeName
            : CodeTypeReferenceOptions.ShortTypeName;
    }

    public string Id => "KeyValuePair";
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return objectType.IsKeyValuePair();
    }

    public CodeExpression Visit(object obj, Type objectType)
    {
        var propertyValues = objectType.GetProperties().Select(p => ReflectionUtils.GetValue(p, obj)).Select(_rootObjectVisitor.Visit);
        return new CodeObjectCreateExpression(new CodeTypeReference(objectType, _typeReferenceOptions),
            propertyValues);
    }
}