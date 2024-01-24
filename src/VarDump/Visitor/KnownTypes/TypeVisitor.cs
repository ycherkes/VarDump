using System;
using VarDump.CodeDom.Common;
using VarDump.Visitor.Descriptors;

namespace VarDump.Visitor.KnownTypes;

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

    public bool IsSuitableFor(IValueDescriptor valueDescriptor)
    {
        return valueDescriptor.Value is Type;
    }

    public CodeExpression Visit(IValueDescriptor valueDescriptor)
    {
        var type = (Type)valueDescriptor.Value;
        return new CodeTypeOfExpression(new CodeTypeReference(type, _typeReferenceOptions));
    }
}