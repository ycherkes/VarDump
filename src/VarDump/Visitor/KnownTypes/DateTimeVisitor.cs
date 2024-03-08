using System;
using System.Globalization;
using VarDump.CodeDom.Compiler;

namespace VarDump.Visitor.KnownTypes;

internal sealed class DateTimeVisitor(
    ICodeWriter codeWriter,
    DateTimeInstantiation dateTimeInstantiation,
    DateKind dateKind)
    : IKnownObjectVisitor
{
    public string Id => nameof(DateTime);
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is DateTime;
    }

    public void Visit(object obj, Type objectType, VisitContext context)
    {
        var dateTime = (DateTime)obj;

        if (dateTime == DateTime.MaxValue)
        {
            codeWriter.WriteFieldReference(nameof(DateTime.MaxValue), () => codeWriter.WriteType(objectType));
            return;
        }

        if (dateTime == DateTime.MinValue)
        {
            codeWriter.WriteFieldReference(nameof(DateTime.MinValue), () => codeWriter.WriteType(objectType));
            return;
        }

        if (dateKind == DateKind.ConvertToUtc)
        {
            dateTime = dateTime.ToUniversalTime();
        }

        if (dateTimeInstantiation == DateTimeInstantiation.Parse)
        {
            codeWriter.WriteMethodInvoke(
                () => codeWriter.WriteMethodReference(
                    () => codeWriter.WriteType(objectType), nameof(DateTime.ParseExact)),
                [
                    () => codeWriter.WritePrimitive(dateTime.ToString("O")),
                    () => codeWriter.WritePrimitive("O"),
                    () => codeWriter.WriteFieldReference(nameof(CultureInfo.InvariantCulture),
                        () => codeWriter.WriteType(typeof(CultureInfo))),
                    () => codeWriter.WriteFieldReference(nameof(DateTimeStyles.RoundtripKind),
                        () => codeWriter.WriteType(typeof(DateTimeStyles)))
                ]);

            return;
        }

        var lessThanMillisecondTicks = dateTime.Ticks % TimeSpan.TicksPerMillisecond;

        if (lessThanMillisecondTicks == 0)
        {
            WriteObjectCreate();
            return;
        }

        codeWriter.WriteMethodInvoke(() => codeWriter.WriteMethodReference(WriteObjectCreate, nameof(DateTime.AddTicks)), [() => codeWriter.WritePrimitive(lessThanMillisecondTicks)]);

        void WriteObjectCreate() => codeWriter.WriteObjectCreate(objectType, 
            [
                WriteYear, 
                WriteMonth, 
                WriteDay, 
                WriteHour, 
                WriteMinute, 
                WriteSecond, 
                WriteMillisecond, 
                WriteKind
            ]);

        void WriteKind() => codeWriter.WriteFieldReference(dateTime.Kind.ToString(), () => codeWriter.WriteType(typeof(DateTimeKind)));
        void WriteYear() => codeWriter.WritePrimitive(dateTime.Year);
        void WriteMonth() => codeWriter.WritePrimitive(dateTime.Month);
        void WriteDay() => codeWriter.WritePrimitive(dateTime.Day);
        void WriteHour() => codeWriter.WritePrimitive(dateTime.Hour);
        void WriteMinute() => codeWriter.WritePrimitive(dateTime.Minute);
        void WriteSecond() => codeWriter.WritePrimitive(dateTime.Second);
        void WriteMillisecond() => codeWriter.WritePrimitive(dateTime.Millisecond);
    }
}