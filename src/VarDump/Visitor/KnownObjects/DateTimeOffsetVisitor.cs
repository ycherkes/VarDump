using System;
using System.Globalization;
using VarDump.CodeDom.Compiler;

namespace VarDump.Visitor.KnownObjects;

internal sealed class DateTimeOffsetVisitor(
    INextDepthVisitor nextDepthVisitor,
    ICodeWriter codeWriter,
    DumpOptions dumpOptions)
    : IKnownObjectVisitor
{
    public string Id => nameof(DateTimeOffset);

    public DumpOptions Options => dumpOptions;

    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is DateTimeOffset;
    }

    public void Visit(object obj, Type objectType, VisitContext context)
    {
        var dateTimeOffset = (DateTimeOffset)obj;

        if (dateTimeOffset == DateTimeOffset.MaxValue)
        {
            codeWriter.WriteFieldReference(nameof(DateTimeOffset.MaxValue), () => codeWriter.WriteType(objectType));
            return;
        }

        if (dateTimeOffset == DateTimeOffset.MinValue)
        {
            codeWriter.WriteFieldReference(nameof(DateTimeOffset.MinValue), () => codeWriter.WriteType(objectType));
            return;
        }

        if (dumpOptions.DateTimeInstantiation == DateTimeInstantiation.Parse)
        {
            codeWriter.WriteMethodInvoke(
                () => codeWriter.WriteMethodReference(
                    () => codeWriter.WriteType(objectType), nameof(DateTimeOffset.ParseExact)),
                [
                    () => codeWriter.WritePrimitive(dateTimeOffset.ToString("O")),
                    () => codeWriter.WritePrimitive("O"),
                    () => codeWriter.WriteFieldReference(nameof(CultureInfo.InvariantCulture),
                        () => codeWriter.WriteType(typeof(CultureInfo))),
                    () => codeWriter.WriteFieldReference(nameof(DateTimeStyles.RoundtripKind),
                        () => codeWriter.WriteType(typeof(DateTimeStyles)))
                ]);

            return;
        }

        var lessThanMillisecondTicks = dateTimeOffset.Ticks % TimeSpan.TicksPerMillisecond;

        if (lessThanMillisecondTicks == 0)
        {
            WriteObjectCreate();
            return;
        }

        codeWriter.WriteMethodInvoke(
            () => codeWriter.WriteMethodReference(WriteObjectCreate, nameof(DateTimeOffset.AddTicks)), 
            [() => codeWriter.WritePrimitive(lessThanMillisecondTicks)]);

        return;

        void WriteObjectCreate()
        {
            if(dumpOptions.UseNamedArgumentsInConstructors)
            {
                codeWriter.WriteObjectCreate(objectType,
                [
                    () => codeWriter.WriteNamedArgument("year", WriteYear),
                    () => codeWriter.WriteNamedArgument("month", WriteMonth),
                    () => codeWriter.WriteNamedArgument("day", WriteDay),
                    () => codeWriter.WriteNamedArgument("hour", WriteHour),
                    () => codeWriter.WriteNamedArgument("minute", WriteMinute),
                    () => codeWriter.WriteNamedArgument("second", WriteSecond),
                    () => codeWriter.WriteNamedArgument("millisecond", WriteMillisecond),
                    () => codeWriter.WriteNamedArgument("offset", WriteOffset)
                ]);
            }
            else
            {
                codeWriter.WriteObjectCreate(objectType,
                [
                    WriteYear,
                    WriteMonth,
                    WriteDay,
                    WriteHour,
                    WriteMinute,
                    WriteSecond,
                    WriteMillisecond,
                    WriteOffset
                ]);
            }
        }

        void WriteYear() => codeWriter.WritePrimitive(dateTimeOffset.Year);
        void WriteMonth() => codeWriter.WritePrimitive(dateTimeOffset.Month);
        void WriteDay() => codeWriter.WritePrimitive(dateTimeOffset.Day);
        void WriteHour() => codeWriter.WritePrimitive(dateTimeOffset.Hour);
        void WriteMinute() => codeWriter.WritePrimitive(dateTimeOffset.Minute);
        void WriteSecond() => codeWriter.WritePrimitive(dateTimeOffset.Second);
        void WriteMillisecond() => codeWriter.WritePrimitive(dateTimeOffset.Millisecond);
        void WriteOffset() => nextDepthVisitor.Visit(dateTimeOffset.Offset, context);
    }
}