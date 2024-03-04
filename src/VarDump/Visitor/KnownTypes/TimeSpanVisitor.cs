using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using VarDump.CodeDom.Compiler;
using VarDump.Extensions;

namespace VarDump.Visitor.KnownTypes;

internal sealed class TimeSpanVisitor : IKnownObjectVisitor
{
    private readonly ICodeWriter _codeWriter;
    private readonly DateTimeInstantiation _dateTimeInstantiation;

    public TimeSpanVisitor(ICodeWriter codeWriter, DateTimeInstantiation dateTimeInstantiation)
    {
        _codeWriter = codeWriter;
        _dateTimeInstantiation = dateTimeInstantiation;
    }

    public string Id => nameof(TimeSpan);
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is TimeSpan;
    }

    public void Visit(object obj, Type objectType)
    {
        var timeSpan = (TimeSpan)obj;

        var specialValuesDictionary = new Dictionary<TimeSpan, string>
        {
            { TimeSpan.MaxValue, nameof(TimeSpan.MaxValue) },
            { TimeSpan.MinValue, nameof(TimeSpan.MinValue) },
            { TimeSpan.Zero, nameof(TimeSpan.Zero) }
        };

        if (specialValuesDictionary.TryGetValue(timeSpan, out var name))
        {
            _codeWriter.WriteFieldReference(name, () => _codeWriter.WriteTypeReference(objectType));

            return;
        }

        var valuesCollection = new Dictionary<string, long>
        {
            { nameof(TimeSpan.FromDays), timeSpan.Days },
            { nameof(TimeSpan.FromHours), timeSpan.Hours },
            { nameof(TimeSpan.FromMinutes), timeSpan.Minutes },
            { nameof(TimeSpan.FromSeconds), timeSpan.Seconds },
            { nameof(TimeSpan.FromMilliseconds), timeSpan.Milliseconds }
        };

        var nonZeroValues = valuesCollection.Where(v => v.Value > 0).ToArray();

        if (nonZeroValues.Length == 1)
        {
            _codeWriter.WriteMethodInvoke(() => _codeWriter.WriteMethodReference(
                () => _codeWriter.WriteTypeReference(objectType), nonZeroValues[0].Key), 
                [() => _codeWriter.WritePrimitive(nonZeroValues[0].Value)]);
            
            return;
        }

        if (timeSpan.Ticks % TimeSpan.TicksPerMillisecond != 0)
        {
            _codeWriter.WriteMethodInvoke(
                () => _codeWriter.WriteMethodReference(
                    () => _codeWriter.WriteTypeReference(objectType), nameof(TimeSpan.FromTicks)),
                [() => _codeWriter.WritePrimitive(timeSpan.Ticks)]);

            return;
        }

        if (_dateTimeInstantiation == DateTimeInstantiation.Parse)
        {
            _codeWriter.WriteMethodInvoke(() => _codeWriter.WriteMethodReference(
                    () => _codeWriter.WriteTypeReference(objectType), nameof(TimeSpan.ParseExact)),
                [
                    () => _codeWriter.WritePrimitive(timeSpan.ToString("c")),
                    () => _codeWriter.WritePrimitive("c"),
                    () => _codeWriter.WriteFieldReference(nameof(CultureInfo.InvariantCulture),
                        () => _codeWriter.WriteTypeReference(typeof(CultureInfo))),
                    () => _codeWriter.WriteFieldReference(nameof(TimeSpanStyles.None),
                        () => _codeWriter.WriteTypeReference(typeof(TimeSpanStyles)))
                ]);

            return;
        }

        if (timeSpan is { Days: 0, Milliseconds: 0 })
        {
            _codeWriter.WriteObjectCreateAndInitialize(objectType, [WriteHoursAction, WriteMinutesAction, WriteSecondsAction], []);
            return;
        }

        if (timeSpan.Milliseconds == 0)
        {
            _codeWriter.WriteObjectCreateAndInitialize(objectType, [WriteDaysAction, WriteHoursAction, WriteMinutesAction, WriteSecondsAction], []);
        }

        _codeWriter.WriteObjectCreateAndInitialize(objectType, [WriteDaysAction, WriteHoursAction, WriteMinutesAction, WriteSecondsAction, WriteMillisecondsAction], []);

        void WriteSecondsAction() => _codeWriter.WritePrimitive(timeSpan.Seconds);
        void WriteMinutesAction() => _codeWriter.WritePrimitive(timeSpan.Minutes);
        void WriteHoursAction() => _codeWriter.WritePrimitive(timeSpan.Hours);
        void WriteDaysAction() => _codeWriter.WritePrimitive(timeSpan.Days);
        void WriteMillisecondsAction() => _codeWriter.WritePrimitive(timeSpan.Milliseconds);
    }
}