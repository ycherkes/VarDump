using System;
using VarDump.CodeDom.Common;
using VarDump.CodeDom.Compiler;
using VarDump.Utils;

namespace VarDump.Visitor.KnownTypes;

internal sealed class DateOnlyVisitor : IKnownObjectVisitor
{
    private readonly ICodeGenerator _codeGenerator;
    private readonly DateTimeInstantiation _dateTimeInstantiation;

    public DateOnlyVisitor(ICodeGenerator codeGenerator, DateTimeInstantiation dateTimeInstantiation)
    {
        _codeGenerator = codeGenerator;
        _dateTimeInstantiation = dateTimeInstantiation;
    }

    public string Id => "DateOnly";
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return objectType.IsDateOnly();
    }

    public void Visit(object dateOnly, Type objectType)
    {
        var dateOnlyCodeTypeReference = new CodeTypeReference(objectType);
        var dayNumber = (int?)objectType.GetProperty("DayNumber")?.GetValue(dateOnly);

        if (dayNumber == null)
        {
            CodeDomUtils.WriteErrorDetectedExpression(_codeGenerator, "Wrong DateOnly struct");
            return;
        }

        if (dayNumber == 3652058U)
        {
            _codeGenerator.GenerateFieldReference(nameof(DateTime.MaxValue), () => _codeGenerator.GenerateTypeReference(dateOnlyCodeTypeReference));

            return;
        }

        if (dayNumber == 1)
        {
            _codeGenerator.GenerateFieldReference(nameof(DateTime.MinValue), () => _codeGenerator.GenerateTypeReference(dateOnlyCodeTypeReference));

            return;
        }

        var dateTime = new DateTime((long)dayNumber * 864000000000L);

        if (_dateTimeInstantiation == DateTimeInstantiation.Parse)
        {
            _codeGenerator.GenerateMethodInvoke(
                () => _codeGenerator.GenerateMethodReference(
                    () => _codeGenerator.GenerateTypeReference(dateOnlyCodeTypeReference), nameof(DateTimeOffset.ParseExact)),
                [
                    () => _codeGenerator.GeneratePrimitive($"{dateTime:yyyy-MM-dd}"),
                    () => _codeGenerator.GeneratePrimitive("O")
                ]);

            return;
        }

        _codeGenerator.GenerateObjectCreateAndInitialize(dateOnlyCodeTypeReference,
            [
                () => _codeGenerator.GeneratePrimitive(dateTime.Year),
                () => _codeGenerator.GeneratePrimitive(dateTime.Month),
                () => _codeGenerator.GeneratePrimitive(dateTime.Day)
            ],
            []);
    }
}