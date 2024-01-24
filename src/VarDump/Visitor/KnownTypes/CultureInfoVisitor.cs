using System.Globalization;
using VarDump.CodeDom.Common;
using VarDump.Visitor.Descriptors;

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
    public bool IsSuitableFor(IValueDescriptor valueDescriptor)
    {
        return valueDescriptor.Value is CultureInfo;
    }

    public CodeExpression Visit(IValueDescriptor valueDescriptor)
    {
        var cultureInfo = (CultureInfo)valueDescriptor.Value;
        return new CodeObjectCreateExpression(new CodeTypeReference(typeof(CultureInfo), _typeReferenceOptions),
            new CodePrimitiveExpression(cultureInfo.ToString()));
    }
}