using System;
using System.Globalization;
using VarDump.CodeDom.Common;

namespace VarDump.Visitor.KnownTypes;

internal sealed class CultureInfoVisitor : IKnownObjectVisitor
{
    private readonly CodeTypeReferenceOptions _typeReferenceOptions;

    public CultureInfoVisitor(DumpOptions options)
    {
        _typeReferenceOptions = options.UseTypeFullName
            ? CodeTypeReferenceOptions.FullTypeName
            : CodeTypeReferenceOptions.ShortTypeName;
    }

    public string Id => nameof(CultureInfo);
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is CultureInfo;
    }

    public CodeExpression Visit(object obj, Type objectType)
    {
        var cultureInfo = (CultureInfo)obj;
        return new CodeObjectCreateExpression(new CodeTypeReference(typeof(CultureInfo), _typeReferenceOptions),
            new CodePrimitiveExpression(cultureInfo.ToString()));
    }
}