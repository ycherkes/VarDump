using System;
using VarDump.CodeDom.Common;
using VarDump.Visitor.Descriptors;

namespace VarDump.Visitor.KnownTypes;

internal sealed class GuidVisitor : IKnownObjectVisitor
{
    private readonly CodeTypeReferenceOptions _typeReferenceOptions;

    public GuidVisitor(DumpOptions options)
    {
        _typeReferenceOptions = options.UseTypeFullName
            ? CodeTypeReferenceOptions.FullTypeName
            : CodeTypeReferenceOptions.ShortTypeName;
    }

    public string Id => nameof(Guid);
    public bool IsSuitableFor(IValueDescriptor valueDescriptor)
    {
        return valueDescriptor.Value is Guid;
    }

    public CodeExpression Visit(IValueDescriptor valueDescriptor)
    {
        var guid = (Guid)valueDescriptor.Value;
        return new CodeObjectCreateExpression(new CodeTypeReference(typeof(Guid), _typeReferenceOptions),
            new CodePrimitiveExpression(guid.ToString("D")));
    }
}