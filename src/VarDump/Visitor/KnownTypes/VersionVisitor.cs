using System;
using VarDump.CodeDom.Common;
using VarDump.Visitor.Descriptors;

namespace VarDump.Visitor.KnownTypes;

internal sealed class VersionVisitor : IKnownObjectVisitor
{
    private readonly CodeTypeReferenceOptions _typeReferenceOptions;

    public VersionVisitor(DumpOptions options)
    {
        _typeReferenceOptions = options.UseTypeFullName
            ? CodeTypeReferenceOptions.FullTypeName
            : CodeTypeReferenceOptions.ShortTypeName;
    }

    public string Id => nameof(Version);

    public bool IsSuitableFor(IValueDescriptor valueDescriptor)
    {
        return valueDescriptor.Value is Version;
    }

    public CodeExpression Visit(IValueDescriptor valueDescriptor)
    {
        var version  = (Version)valueDescriptor.Value;
        return new CodeObjectCreateExpression(new CodeTypeReference(typeof(Version), _typeReferenceOptions),
            new CodePrimitiveExpression(version.ToString()));
    }
}