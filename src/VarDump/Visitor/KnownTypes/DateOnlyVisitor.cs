using System;
using VarDump.CodeDom.Compiler;
using VarDump.Extensions;
using VarDump.Utils;

namespace VarDump.Visitor.KnownTypes;

internal sealed class DateOnlyVisitor : IKnownObjectVisitor
{
    private readonly ICodeWriter _codeWriter;
    private readonly DateTimeInstantiation _dateTimeInstantiation;

    public DateOnlyVisitor(ICodeWriter codeWriter, DateTimeInstantiation dateTimeInstantiation)
    {
        _codeWriter = codeWriter;
        _dateTimeInstantiation = dateTimeInstantiation;
    }

    public string Id => "DateOnly";
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return objectType.IsDateOnly();
    }

    public void Visit(object dateOnly, Type objectType)
    {
        var dayNumber = (int?)objectType.GetProperty("DayNumber")?.GetValue(dateOnly);

        if (dayNumber == null)
        {
            _codeWriter.WriteErrorDetected("Wrong DateOnly struct");
            return;
        }

        if (dayNumber == 3652058U)
        {
            _codeWriter.WriteFieldReference(nameof(DateTime.MaxValue), () => _codeWriter.WriteTypeReference(objectType));

            return;
        }

        if (dayNumber == 1)
        {
            _codeWriter.WriteFieldReference(nameof(DateTime.MinValue), () => _codeWriter.WriteTypeReference(objectType));

            return;
        }

        var dateTime = new DateTime((long)dayNumber * 864000000000L);

        if (_dateTimeInstantiation == DateTimeInstantiation.Parse)
        {
            _codeWriter.WriteMethodInvoke(
                () => _codeWriter.WriteMethodReference(
                    () => _codeWriter.WriteTypeReference(objectType), nameof(DateTimeOffset.ParseExact)),
                [
                    () => _codeWriter.WritePrimitive($"{dateTime:yyyy-MM-dd}"),
                    () => _codeWriter.WritePrimitive("O")
                ]);

            return;
        }

        _codeWriter.WriteObjectCreateAndInitialize(objectType,
            [
                () => _codeWriter.WritePrimitive(dateTime.Year),
                () => _codeWriter.WritePrimitive(dateTime.Month),
                () => _codeWriter.WritePrimitive(dateTime.Day)
            ],
            []);
    }
}