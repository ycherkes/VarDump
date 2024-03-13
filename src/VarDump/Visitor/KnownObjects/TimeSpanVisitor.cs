using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using VarDump.CodeDom.Compiler;

namespace VarDump.Visitor.KnownObjects;

internal sealed class TimeSpanVisitor(
    ICodeWriter codeWriter,
    DumpOptions options)
    : IKnownObjectVisitor
{
    private static readonly Dictionary<TimeSpan, string> SpecialValuesDictionary = new()
    {
        { TimeSpan.MaxValue, nameof(TimeSpan.MaxValue) },
        { TimeSpan.MinValue, nameof(TimeSpan.MinValue) },
        { TimeSpan.Zero, nameof(TimeSpan.Zero) }
    };

    public string Id => nameof(TimeSpan);

    public DumpOptions Options => options;

    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is TimeSpan;
    }

    public void Visit(object obj, Type objectType, VisitContext context)
    {
        var timeSpan = (TimeSpan)obj;

        if (SpecialValuesDictionary.TryGetValue(timeSpan, out var name))
        {
            codeWriter.WriteFieldReference(name, () => codeWriter.WriteType(objectType));

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
            codeWriter.WriteMethodInvoke(() => codeWriter.WriteMethodReference(
                () => codeWriter.WriteType(objectType), nonZeroValues[0].Key), 
                [() => codeWriter.WritePrimitive(nonZeroValues[0].Value)]);
            
            return;
        }

        if (timeSpan.Ticks % TimeSpan.TicksPerMillisecond != 0)
        {
            codeWriter.WriteMethodInvoke(
                () => codeWriter.WriteMethodReference(
                    () => codeWriter.WriteType(objectType), nameof(TimeSpan.FromTicks)),
                [() => codeWriter.WritePrimitive(timeSpan.Ticks)]);

            return;
        }

        if (options.DateTimeInstantiation == DateTimeInstantiation.Parse)
        {
            codeWriter.WriteMethodInvoke(() => codeWriter.WriteMethodReference(
                    () => codeWriter.WriteType(objectType), nameof(TimeSpan.ParseExact)),
                [
                    () => codeWriter.WritePrimitive(timeSpan.ToString("c")),
                    () => codeWriter.WritePrimitive("c"),
                    () => codeWriter.WriteFieldReference(nameof(CultureInfo.InvariantCulture),
                        () => codeWriter.WriteType(typeof(CultureInfo))),
                    () => codeWriter.WriteFieldReference(nameof(TimeSpanStyles.None),
                        () => codeWriter.WriteType(typeof(TimeSpanStyles)))
                ]);

            return;
        }

        Action writeSeconds;
        Action writeMinutes;
        Action writeHours;
        Action writeDays;
        Action writeMilliseconds;

        if (options.UseNamedArgumentsInConstructors)
        {
            writeSeconds = () => codeWriter.WriteNamedArgument("seconds", () => codeWriter.WritePrimitive(timeSpan.Seconds));
            writeMinutes = () => codeWriter.WriteNamedArgument("minutes", () => codeWriter.WritePrimitive(timeSpan.Minutes));
            writeHours = () => codeWriter.WriteNamedArgument("hours", () => codeWriter.WritePrimitive(timeSpan.Hours));
            writeDays = () => codeWriter.WriteNamedArgument("days", () => codeWriter.WritePrimitive(timeSpan.Days));
            writeMilliseconds = () => codeWriter.WriteNamedArgument("milliseconds", () => codeWriter.WritePrimitive(timeSpan.Milliseconds));
        }
        else
        {
            writeSeconds = () => codeWriter.WritePrimitive(timeSpan.Seconds);
            writeMinutes = () => codeWriter.WritePrimitive(timeSpan.Minutes);
            writeHours = () => codeWriter.WritePrimitive(timeSpan.Hours);
            writeDays = () => codeWriter.WritePrimitive(timeSpan.Days);
            writeMilliseconds = () => codeWriter.WritePrimitive(timeSpan.Milliseconds);
        }

        if (timeSpan is { Days: 0, Milliseconds: 0 })
        {
            codeWriter.WriteObjectCreate(objectType, [writeHours, writeMinutes, writeSeconds]);
            return;
        }

        if (timeSpan.Milliseconds == 0)
        {
            codeWriter.WriteObjectCreate(objectType, [writeDays, writeHours, writeMinutes, writeSeconds]);
        }

        codeWriter.WriteObjectCreate(objectType, [writeDays, writeHours, writeMinutes, writeSeconds, writeMilliseconds]);
    }
}