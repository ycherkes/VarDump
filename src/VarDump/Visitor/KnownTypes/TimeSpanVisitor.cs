using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using VarDump.CodeDom.Common;
using VarDump.CodeDom.Compiler;

namespace VarDump.Visitor.KnownTypes;

internal sealed class TimeSpanVisitor : IKnownObjectVisitor
{
    private readonly ICodeGenerator _codeGenerator;
    private readonly DateTimeInstantiation _dateTimeInstantiation;

    public TimeSpanVisitor(ICodeGenerator codeGenerator, DateTimeInstantiation dateTimeInstantiation)
    {
        _codeGenerator = codeGenerator;
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

        var timeSpanCodeTypeReference = new CodeTypeReference(typeof(TimeSpan));

        if (specialValuesDictionary.TryGetValue(timeSpan, out var name))
        {
            _codeGenerator.GenerateFieldReference(name, () => _codeGenerator.GenerateTypeReference(new CodeTypeReference(objectType)));

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
            _codeGenerator.GenerateMethodInvoke(() => _codeGenerator.GenerateMethodReference(
                () => _codeGenerator.GenerateTypeReference(timeSpanCodeTypeReference), nonZeroValues[0].Key), 
                [() => _codeGenerator.GeneratePrimitive(nonZeroValues[0].Value)]);
            
            return;
        }

        if (timeSpan.Ticks % TimeSpan.TicksPerMillisecond != 0)
        {
            _codeGenerator.GenerateMethodInvoke(
                () => _codeGenerator.GenerateMethodReference(
                    () => _codeGenerator.GenerateTypeReference(timeSpanCodeTypeReference), nameof(TimeSpan.FromTicks)),
                [() => _codeGenerator.GeneratePrimitive(timeSpan.Ticks)]);

            return;
        }

        if (_dateTimeInstantiation == DateTimeInstantiation.Parse)
        {
            _codeGenerator.GenerateMethodInvoke(() => _codeGenerator.GenerateMethodReference(
                    () => _codeGenerator.GenerateTypeReference(timeSpanCodeTypeReference), nameof(TimeSpan.ParseExact)),
                [
                    () => _codeGenerator.GeneratePrimitive(timeSpan.ToString("c")),
                    () => _codeGenerator.GeneratePrimitive("c"),
                    () => _codeGenerator.GenerateFieldReference(nameof(CultureInfo.InvariantCulture),
                        () => _codeGenerator.GenerateTypeReference(new CodeTypeReference(typeof(CultureInfo)))),
                    () => _codeGenerator.GenerateFieldReference(nameof(TimeSpanStyles.None),
                        () => _codeGenerator.GenerateTypeReference(new CodeTypeReference(typeof(TimeSpanStyles))))
                ]);

            return;
        }

        if (timeSpan is { Days: 0, Milliseconds: 0 })
        {
            _codeGenerator.GenerateObjectCreateAndInitialize(timeSpanCodeTypeReference, [GenerateHoursAction, GenerateMinutesAction, GenerateSecondsAction], []);
            return;
        }

        if (timeSpan.Milliseconds == 0)
        {
            _codeGenerator.GenerateObjectCreateAndInitialize(timeSpanCodeTypeReference, [GenerateDaysAction, GenerateHoursAction, GenerateMinutesAction, GenerateSecondsAction], []);
        }

        _codeGenerator.GenerateObjectCreateAndInitialize(timeSpanCodeTypeReference, [GenerateDaysAction, GenerateHoursAction, GenerateMinutesAction, GenerateSecondsAction, GenerateMillisecondsAction], []);

        void GenerateSecondsAction() => _codeGenerator.GeneratePrimitive(timeSpan.Seconds);
        void GenerateMinutesAction() => _codeGenerator.GeneratePrimitive(timeSpan.Minutes);
        void GenerateHoursAction() => _codeGenerator.GeneratePrimitive(timeSpan.Hours);
        void GenerateDaysAction() => _codeGenerator.GeneratePrimitive(timeSpan.Days);
        void GenerateMillisecondsAction() => _codeGenerator.GeneratePrimitive(timeSpan.Milliseconds);
    }
}