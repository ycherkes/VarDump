using System;
using System.Globalization;
using VarDump.CodeDom.Common;
using VarDump.CodeDom.Compiler;
using VarDump.Extensions;

namespace VarDump.Visitor.KnownTypes;

internal sealed class DateTimeVisitor : IKnownObjectVisitor
{
    private readonly ICodeWriter _codeWriter;
    private readonly DateTimeInstantiation _dateTimeInstantiation;
    private readonly DateKind _dateKind;

    public DateTimeVisitor(ICodeWriter codeWriter, DateTimeInstantiation dateTimeInstantiation, DateKind dateKind)
    {
        _codeWriter = codeWriter;
        _dateTimeInstantiation = dateTimeInstantiation;
        _dateKind = dateKind;
    }

    public string Id => nameof(DateTime);
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is DateTime;
    }

    public void Visit(object obj, Type objectType)
    {
        var dateTime = (DateTime)obj;

        if (dateTime == DateTime.MaxValue)
        {
            _codeWriter.WriteFieldReference(nameof(DateTime.MaxValue), () => _codeWriter.WriteTypeReference(objectType));
            return;
        }

        if (dateTime == DateTime.MinValue)
        {
            _codeWriter.WriteFieldReference(nameof(DateTime.MinValue), () => _codeWriter.WriteTypeReference(objectType));
            return;
        }

        if (_dateKind == DateKind.ConvertToUtc)
        {
            dateTime = dateTime.ToUniversalTime();
        }

        if (_dateTimeInstantiation == DateTimeInstantiation.Parse)
        {
            _codeWriter.WriteMethodInvoke(
                () => _codeWriter.WriteMethodReference(
                    () => _codeWriter.WriteTypeReference(objectType), nameof(DateTime.ParseExact)),
                [
                    () => _codeWriter.WritePrimitive(dateTime.ToString("O")),
                    () => _codeWriter.WritePrimitive("O"),
                    () => _codeWriter.WriteFieldReference(nameof(CultureInfo.InvariantCulture),
                        () => _codeWriter.WriteTypeReference(typeof(CultureInfo))),
                    () => _codeWriter.WriteFieldReference(nameof(DateTimeStyles.RoundtripKind),
                        () => _codeWriter.WriteTypeReference(typeof(DateTimeStyles)))
                ]);

            return;
        }

        var lessThanMillisecondTicks = dateTime.Ticks % TimeSpan.TicksPerMillisecond;

        if (lessThanMillisecondTicks == 0)
        {
            WriteObjectCreateAction();
            return;
        }

        _codeWriter.WriteMethodInvoke(() => _codeWriter.WriteMethodReference(WriteObjectCreateAction, nameof(DateTime.AddTicks)), [() => _codeWriter.WritePrimitive(lessThanMillisecondTicks)]);

        void WriteObjectCreateAction() => _codeWriter.WriteObjectCreateAndInitialize(objectType, 
            [
                WriteYearAction, 
                WriteMontAction, 
                WriteDayAction, 
                WriteHourAction, 
                WriteMinuteAction, 
                WriteSecondAction, 
                WriteMillisecondAction, 
                WriteKindAction], 
            []);
        void WriteKindAction() => _codeWriter.WriteFieldReference(dateTime.Kind.ToString(), () => _codeWriter.WriteTypeReference(typeof(DateTimeKind)));
        void WriteYearAction() => _codeWriter.WritePrimitive(dateTime.Year);
        void WriteMontAction() => _codeWriter.WritePrimitive(dateTime.Month);
        void WriteDayAction() => _codeWriter.WritePrimitive(dateTime.Day);
        void WriteHourAction() => _codeWriter.WritePrimitive(dateTime.Hour);
        void WriteMinuteAction() => _codeWriter.WritePrimitive(dateTime.Minute);
        void WriteSecondAction() => _codeWriter.WritePrimitive(dateTime.Second);
        void WriteMillisecondAction() => _codeWriter.WritePrimitive(dateTime.Millisecond);
    }
}