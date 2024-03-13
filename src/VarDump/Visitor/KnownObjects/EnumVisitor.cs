using System;
using System.Linq;
using VarDump.CodeDom.Compiler;

namespace VarDump.Visitor.KnownObjects;

internal sealed class EnumVisitor(ICodeWriter codeWriter, DumpOptions options) : IKnownObjectVisitor
{
    public string Id => nameof(Enum);

    public DumpOptions Options => options;

    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is Enum;
    }

    public void Visit(object obj, Type objectType, VisitContext context)
    {
        var values = obj.ToString().Split([','], StringSplitOptions.RemoveEmptyEntries);

        if (values.Length == 1)
        {
            codeWriter.WriteFieldReference(values[0].Trim(), () => codeWriter.WriteType(objectType));
            return;
        }

        var actions = values.Select(v => (Action)(() => codeWriter.WriteFieldReference(v.Trim(), () => codeWriter.WriteType(objectType))));

        codeWriter.WriteFlagsBitwiseOrOperator(actions);
    }
}