using System;
using System.Linq;
using VarDump.CodeDom.Common;

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
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is Enum;
    }

    public CodeExpression Visit(object obj, Type objectType)
    {
        var values = obj.ToString().Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

        if (values.Length == 1)
        {
            return new CodeFieldReferenceExpression(
                new CodeTypeReferenceExpression(new CodeTypeReference(obj.GetType(), _typeReferenceOptions)), obj.ToString());
        }

        var expressions = values.Select(v => (CodeExpression)new CodeFieldReferenceExpression(
            new CodeTypeReferenceExpression(new CodeTypeReference(obj.GetType(), _typeReferenceOptions)), v.Trim())).ToArray();

        var bitwiseOrExpression = new CodeFlagsBinaryOperatorExpression(CodeBinaryOperatorType.BitwiseOr, expressions);

        return bitwiseOrExpression;
    }
}