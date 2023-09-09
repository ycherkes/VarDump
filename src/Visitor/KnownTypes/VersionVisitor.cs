using System;
using VarDumpExtended.CodeDom.Common;

namespace VarDumpExtended.Visitor.KnownTypes;

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

    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is Version;
    }

    public CodeExpression Visit(object obj, Type objectType)
    {
        var version  = (Version)obj;
        return new CodeObjectCreateExpression(new CodeTypeReference(typeof(Version), _typeReferenceOptions),
            new CodePrimitiveExpression(version.ToString()));
    }
}