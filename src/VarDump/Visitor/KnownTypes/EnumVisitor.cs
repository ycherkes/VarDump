using System;
using System.Linq;
using VarDump.CodeDom.Common;
using VarDump.Visitor.Descriptors;

namespace VarDump.Visitor.KnownTypes;

internal sealed class EnumVisitor : IKnownObjectVisitor
{
    private readonly CodeTypeReferenceOptions _typeReferenceOptions;

    public EnumVisitor(DumpOptions options)
    {
        _typeReferenceOptions = options.UseTypeFullName
            ? CodeTypeReferenceOptions.FullTypeName
            : CodeTypeReferenceOptions.ShortTypeName;
    }

    public string Id => nameof(Enum);
    public bool IsSuitableFor(IValueDescriptor valueDescriptor)
    {
        return valueDescriptor.Value is Enum;
    }

    public CodeExpression Visit(IValueDescriptor valueDescriptor)
    {
        var values = valueDescriptor.Value.ToString().Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

        if (values.Length == 1)
        {
            return new CodeFieldReferenceExpression(
                new CodeTypeReferenceExpression(new CodeTypeReference(valueDescriptor.Value.GetType(), _typeReferenceOptions)), valueDescriptor.Value.ToString());
        }

        var expressions = values.Select(v => (CodeExpression)new CodeFieldReferenceExpression(
            new CodeTypeReferenceExpression(new CodeTypeReference(valueDescriptor.Type, _typeReferenceOptions)), v.Trim())).ToArray();

        var bitwiseOrExpression = new CodeFlagsBinaryOperatorExpression(CodeBinaryOperatorType.BitwiseOr, expressions);

        return bitwiseOrExpression;
    }
}