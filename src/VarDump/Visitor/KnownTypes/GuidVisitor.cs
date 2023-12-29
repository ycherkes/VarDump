using System;
using VarDump.CodeDom.Common;

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
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is Guid;
    }

    public CodeExpression Visit(object obj, Type objectType)
    {
        var guid = (Guid)obj;
        return new CodeObjectCreateExpression(new CodeTypeReference(typeof(Guid), _typeReferenceOptions),
            new CodePrimitiveExpression(guid.ToString("D")));
    }
}