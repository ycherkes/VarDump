using System;
using VarDump.CodeDom.Compiler;
using VarDump.Extensions;
using VarDump.Utils;

namespace VarDump.Visitor.KnownObjects;

internal sealed class DateOnlyVisitor(ICodeWriter codeWriter, DateTimeInstantiation dateTimeInstantiation, bool useNamedArgumentsInConstructors) : IKnownObjectVisitor
{
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return objectType.IsDateOnly();
    }

    public void Visit(object dateOnly, Type objectType, VisitContext context)
    {
        var dayNumber = (int?)objectType.GetProperty("DayNumber")?.GetValue(dateOnly);

        if (dayNumber == null)
        {
            codeWriter.WriteErrorDetected("Wrong DateOnly struct");
            return;
        }
        
        if (dayNumber == 3652058U)
        {
            codeWriter.WriteFieldReference(nameof(DateTime.MaxValue), () => codeWriter.WriteType(objectType));

            return;
        }

        if (dayNumber == 1)
        {
            codeWriter.WriteFieldReference(nameof(DateTime.MinValue), () => codeWriter.WriteType(objectType));

            return;
        }

        var dateTime = new DateTime((long)dayNumber * 864000000000L);

        if (dateTimeInstantiation == DateTimeInstantiation.Parse)
        {
            codeWriter.WriteMethodInvoke(
                () => codeWriter.WriteMethodReference(
                    () => codeWriter.WriteType(objectType), nameof(DateTimeOffset.ParseExact)),
                [
                    () => codeWriter.WritePrimitive($"{dateTime:yyyy-MM-dd}"),
                    () => codeWriter.WritePrimitive("O")
                ]);

            return;
        }
        
        if (useNamedArgumentsInConstructors)
        {
            codeWriter.WriteObjectCreate(objectType, 
            [
                () => codeWriter.WriteNamedArgument("year", () => codeWriter.WritePrimitive(dateTime.Year)),
                () => codeWriter.WriteNamedArgument("month", () => codeWriter.WritePrimitive(dateTime.Month)),
                () => codeWriter.WriteNamedArgument("day", () => codeWriter.WritePrimitive(dateTime.Day)),
            ]);
        }
        else
        {
            codeWriter.WriteObjectCreate(objectType,
            [
                () => codeWriter.WritePrimitive(dateTime.Year),
                () => codeWriter.WritePrimitive(dateTime.Month),
                () => codeWriter.WritePrimitive(dateTime.Day)
            ]);
        }
    }
}