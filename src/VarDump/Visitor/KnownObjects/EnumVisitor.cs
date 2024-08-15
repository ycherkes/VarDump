using System;
using System.Linq;
using VarDump.CodeDom.Compiler;

namespace VarDump.Visitor.KnownObjects;

internal sealed class EnumVisitor(ICodeWriter codeWriter) : IKnownObjectVisitor
{
    public string Id => nameof(Enum);

    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is Enum;
    }

    public void ConfigureOptions(Action<DumpOptions> configure)
    {
    }

    public void Visit(object obj, Type objectType, VisitContext context)
    {
        var values = obj.ToString().Split([','], StringSplitOptions.RemoveEmptyEntries);

        if (values.Length == 1)
        {
            var value = values[0].Trim();
            var firstChar = value[0];
            if ((char.IsDigit(firstChar) || firstChar == '-') && value.Skip(1).All(char.IsDigit))
            {
                var longValue = long.Parse(value);
                if (longValue == 0)
                {
                    codeWriter.WritePrimitive(0);
                }
                else
                {
                    object renderValueObject = longValue is <= int.MaxValue and >= int.MinValue
                        ? (int)longValue
                        : longValue;

                    if (longValue > 0)
                    {
                        codeWriter.WriteCast(objectType, () => codeWriter.WritePrimitive(renderValueObject));
                    }
                    else
                    {
                        codeWriter.WriteCast(objectType, () => codeWriter.WriteCast(typeof(object), () => codeWriter.WritePrimitive(renderValueObject)));
                    }
                }
            }
            else
            {
                codeWriter.WriteFieldReference(value, () => codeWriter.WriteType(objectType));
            }
            return;
        }

        var actions = values.Select(v => (Action)(() => codeWriter.WriteFieldReference(v.Trim(), () => codeWriter.WriteType(objectType))));

        codeWriter.WriteFlagsBitwiseOrOperator(actions);
    }
}