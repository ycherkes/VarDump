using System;
using VarDumpExtended.CodeDom.Common;

namespace VarDumpExtended.Visitor.KnownTypes;

internal sealed class TypeVisitor : IKnownObjectVisitor
{
    private readonly CodeTypeReferenceOptions _typeReferenceOptions;

    public TypeVisitor(DumpOptions options)
    {
        _typeReferenceOptions = options.UseTypeFullName
            ? CodeTypeReferenceOptions.FullTypeName
            : CodeTypeReferenceOptions.ShortTypeName;
    }

    public string Id => nameof(Type);

    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is Type;
    }

    public CodeExpression Visit(object obj, Type objectType)
    {
        var type = (Type)obj;
        return new CodeTypeOfExpression(new CodeTypeReference(type, _typeReferenceOptions));
    }
}