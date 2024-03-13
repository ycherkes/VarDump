using System;
using VarDump.CodeDom.Compiler;
using VarDump.Extensions;
using VarDump.Utils;

namespace VarDump.Visitor.KnownObjects;

internal sealed class DateOnlyVisitor(ICodeWriter codeWriter, DumpOptions dumpOptions) : IKnownObjectVisitor
{
    public string Id => "DateOnly";

    public DumpOptions Options => dumpOptions;

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

        if (dumpOptions.DateTimeInstantiation == DateTimeInstantiation.Parse)
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
        
        if (dumpOptions.UseNamedArgumentsInConstructors)
        {
            codeWriter.WriteObjectCreate(objectType, 
            [
                () => codeWriter.WriteNamedArgument("year", WriteYear),
                () => codeWriter.WriteNamedArgument("month", WriteMonth),
                () => codeWriter.WriteNamedArgument("day", WriteDay)
            ]);
        }
        else
        {
            codeWriter.WriteObjectCreate(objectType,
            [
                WriteYear,
                WriteMonth,
                WriteDay
            ]);
        }

        return;

        void WriteYear() => codeWriter.WritePrimitive(dateTime.Year);
        void WriteMonth() => codeWriter.WritePrimitive(dateTime.Month);
        void WriteDay() => codeWriter.WritePrimitive(dateTime.Day);
    }
}