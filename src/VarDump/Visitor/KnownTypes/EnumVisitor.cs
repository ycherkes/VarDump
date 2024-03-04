using System;
using System.Linq;
using VarDump.CodeDom.Compiler;
using VarDump.Extensions;

namespace VarDump.Visitor.KnownTypes;

internal sealed class EnumVisitor : IKnownObjectVisitor
{
    private readonly ICodeWriter _codeWriter;

    public EnumVisitor(ICodeWriter codeWriter)
    {
        _codeWriter = codeWriter;
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
            _codeWriter.WriteFieldReference(values[0].Trim(), () => _codeWriter.WriteTypeReference(objectType));
            return;
        }

        var actions = values.Select(v => (Action)(() => _codeWriter.WriteFieldReference(v.Trim(), () => _codeWriter.WriteTypeReference(objectType))));

        _codeWriter.WriteFlagsBinaryOperator(actions);
    }
}