using System;
using System.Linq;
using VarDump.CodeDom.Common;
using VarDump.CodeDom.Compiler;

namespace VarDump.Visitor.KnownTypes;

internal sealed class EnumVisitor : IKnownObjectVisitor
{
    private readonly IDotnetCodeGenerator _codeGenerator;

    public EnumVisitor(IDotnetCodeGenerator codeGenerator)
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
            _codeGenerator.GenerateFieldReference(values[0].Trim(), () => _codeGenerator.GenerateTypeReference(new CodeDotnetTypeReference(obj.GetType())));
            return;
        }

        var actions = values.Select(v => (Action)(() => _codeGenerator.GenerateFieldReference(v.Trim(), () => _codeGenerator.GenerateTypeReference(new CodeDotnetTypeReference(obj.GetType())))));

        _codeGenerator.GenerateFlagsBinaryOperator(actions);
    }
}