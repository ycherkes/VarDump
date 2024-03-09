﻿using System;
using VarDump.CodeDom.Compiler;
using VarDump.Extensions;
using VarDump.Utils;

namespace VarDump.Visitor.KnownTypes;

internal sealed class TimeOnlyVisitor(ICodeWriter codeWriter, DateTimeInstantiation dateTimeInstantiation)
    : IKnownObjectVisitor
{
    public string Id => "TimeOnly";
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return objectType.IsTimeOnly();
    }

    public void Visit(object timeOnly, Type objectType, VisitContext context)
    {
        var ticks = (long?)objectType.GetProperty("Ticks")?.GetValue(timeOnly);

        switch (ticks)
        {
            case null:
                codeWriter.WriteErrorDetected("Wrong TimeOnly struct");
                return;
            case 863999999999:
                codeWriter.WriteFieldReference(nameof(DateTime.MaxValue), () => codeWriter.WriteType(objectType));
                return;
            case 0:
                codeWriter.WriteFieldReference(nameof(DateTime.MinValue), () => codeWriter.WriteType(objectType));
                return;
        }

        var timeSpan = TimeSpan.FromTicks(ticks.Value);

        if (timeSpan.Ticks % TimeSpan.TicksPerMillisecond != 0)
        {
            codeWriter.WriteMethodInvoke(
                () => codeWriter.WriteMethodReference(
                    () => codeWriter.WriteType(objectType), nameof(TimeSpan.FromTicks)),
                [
                    () => codeWriter.WritePrimitive(ticks.Value)
                ]);

            return;
        }

        if (dateTimeInstantiation == DateTimeInstantiation.Parse)
        {
            codeWriter.WriteMethodInvoke(
                () => codeWriter.WriteMethodReference(
                    () => codeWriter.WriteType(objectType), nameof(DateTime.ParseExact)),
                [
                    () => codeWriter.WritePrimitive($"{timeSpan:c}"),
                    () => codeWriter.WritePrimitive("O")
                ]);

            return;
        }

        codeWriter.WriteObjectCreate(objectType,
            [
                () => codeWriter.WritePrimitive(timeSpan.Hours),
                () => codeWriter.WritePrimitive(timeSpan.Minutes),
                () => codeWriter.WritePrimitive(timeSpan.Seconds),
                () => codeWriter.WritePrimitive(timeSpan.Milliseconds)
            ]);
    }
}