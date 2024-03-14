﻿using System;
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

    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is TimeSpan;
    }

    public void ConfigureOptions(Action<DumpOptions> configure)
    {
        options = options.Clone();
        configure?.Invoke(options);
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
            writeSeconds = () => codeWriter.WriteNamedArgument("seconds", WriteSeconds);
            writeMinutes = () => codeWriter.WriteNamedArgument("minutes", WriteMinutes);
            writeHours = () => codeWriter.WriteNamedArgument("hours", WriteHours);
            writeDays = () => codeWriter.WriteNamedArgument("days", WriteDays);
            writeMilliseconds = () => codeWriter.WriteNamedArgument("milliseconds", WriteMilliseconds);
        }
        else
        {
            writeSeconds = WriteSeconds;
            writeMinutes = WriteMinutes;
            writeHours = WriteHours;
            writeDays = WriteDays;
            writeMilliseconds = WriteMilliseconds;
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
        return;

        void WriteSeconds() => codeWriter.WritePrimitive(timeSpan.Seconds);
        void WriteMinutes() => codeWriter.WritePrimitive(timeSpan.Minutes);
        void WriteHours() => codeWriter.WritePrimitive(timeSpan.Hours);
        void WriteDays() => codeWriter.WritePrimitive(timeSpan.Days);
        void WriteMilliseconds() => codeWriter.WritePrimitive(timeSpan.Milliseconds);
    }
}