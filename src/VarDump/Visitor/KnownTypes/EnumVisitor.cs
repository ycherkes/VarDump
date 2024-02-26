using System;
using System.Linq;
using VarDump.CodeDom.Common;
using VarDump.CodeDom.Compiler;

namespace VarDump.Visitor.KnownTypes;

internal sealed class EnumVisitor : IKnownObjectVisitor
{
    private readonly ICodeGenerator _codeGenerator;

    public EnumVisitor(ICodeGenerator codeGenerator)
    {
        _codeGenerator = codeGenerator;
    }

    public string Id => nameof(Enum);
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is Enum;
    }

    public void Visit(object obj, Type objectType)
    {
        var values = obj.ToString().Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

        if (values.Length == 1)
        {
            _codeGenerator.GenerateFieldReference(values[0].Trim(), () => _codeGenerator.GenerateTypeReference(new CodeTypeReference(obj.GetType())));
            return;
        }

        var actions = values.Select(v => (Action)(() => _codeGenerator.GenerateFieldReference(v.Trim(), () => _codeGenerator.GenerateTypeReference(new CodeTypeReference(obj.GetType())))));

        _codeGenerator.GenerateFlagsBinaryOperator(CodeBinaryOperatorType.BitwiseOr, actions);
    }
}