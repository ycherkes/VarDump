using System;
using System.Globalization;
using VarDump.CodeDom.Compiler;
using VarDump.Extensions;

namespace VarDump.Visitor.KnownTypes;

internal sealed class DateTimeOffsetVisitor : IKnownObjectVisitor
{
    private readonly IObjectVisitor _rootObjectVisitor;
    private readonly ICodeWriter _codeWriter;
    private readonly DateTimeInstantiation _dateTimeInstantiation;

    public DateTimeOffsetVisitor(IObjectVisitor rootObjectVisitor, ICodeWriter codeWriter, DateTimeInstantiation dateTimeInstantiation)
    {
        _rootObjectVisitor = rootObjectVisitor;
        _codeWriter = codeWriter;
        _dateTimeInstantiation = dateTimeInstantiation;
    }

    public string Id => nameof(DateTimeOffset);
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is DateTimeOffset;
    }

    public void Visit(object obj, Type objectType)
    {
        var dateTimeOffset = (DateTimeOffset)obj;

        if (dateTimeOffset == DateTimeOffset.MaxValue)
        {
            _codeWriter.WriteFieldReference(nameof(DateTimeOffset.MaxValue), () => _codeWriter.WriteTypeReference(objectType));
            return;
        }

        if (dateTimeOffset == DateTimeOffset.MinValue)
        {
            _codeWriter.WriteFieldReference(nameof(DateTimeOffset.MinValue), () => _codeWriter.WriteTypeReference(objectType));
            return;
        }

        if (_dateTimeInstantiation == DateTimeInstantiation.Parse)
        {
            _codeWriter.WriteMethodInvoke(
                () => _codeWriter.WriteMethodReference(
                    () => _codeWriter.WriteTypeReference(objectType), nameof(DateTimeOffset.ParseExact)),
                [
                    () => _codeWriter.WritePrimitive(dateTimeOffset.ToString("O")),
                    () => _codeWriter.WritePrimitive("O"),
                    () => _codeWriter.WriteFieldReference(nameof(CultureInfo.InvariantCulture),
                        () => _codeWriter.WriteTypeReference(typeof(CultureInfo))),
                    () => _codeWriter.WriteFieldReference(nameof(DateTimeStyles.RoundtripKind),
                        () => _codeWriter.WriteTypeReference(typeof(DateTimeStyles)))
                ]);

            return;
        }

        var lessThanMillisecondTicks = dateTimeOffset.Ticks % TimeSpan.TicksPerMillisecond;

        if (lessThanMillisecondTicks == 0)
        {
            WriteObjectCreateAction();
            return;
        }

        _codeWriter.WriteMethodInvoke(() => _codeWriter.WriteMethodReference(WriteObjectCreateAction, nameof(DateTimeOffset.AddTicks)), [() => _codeWriter.WritePrimitive(lessThanMillisecondTicks)]);

        void WriteObjectCreateAction() => _codeWriter.WriteObjectCreateAndInitialize(objectType,
            [
                WriteYearAction,
                WriteMontAction,
                WriteDayAction,
                WriteHourAction,
                WriteMinuteAction,
                WriteSecondAction,
                WriteMillisecondAction,
                WriteOffsetAction],
            []);

        void WriteYearAction() => _codeWriter.WritePrimitive(dateTimeOffset.Year);
        void WriteMontAction() => _codeWriter.WritePrimitive(dateTimeOffset.Month);
        void WriteDayAction() => _codeWriter.WritePrimitive(dateTimeOffset.Day);
        void WriteHourAction() => _codeWriter.WritePrimitive(dateTimeOffset.Hour);
        void WriteMinuteAction() => _codeWriter.WritePrimitive(dateTimeOffset.Minute);
        void WriteSecondAction() => _codeWriter.WritePrimitive(dateTimeOffset.Second);
        void WriteMillisecondAction() => _codeWriter.WritePrimitive(dateTimeOffset.Millisecond);
        void WriteOffsetAction() => _rootObjectVisitor.Visit(dateTimeOffset.Offset);
    }
}