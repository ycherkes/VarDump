using System;
using VarDump.CodeDom.Compiler;
using VarDump.Extensions;
using VarDump.Utils;

namespace VarDump.Visitor.KnownTypes;

internal sealed class TimeOnlyVisitor : IKnownObjectVisitor
{
    private readonly ICodeWriter _codeWriter;
    private readonly DateTimeInstantiation _dateTimeInstantiation;

    public TimeOnlyVisitor(ICodeWriter codeWriter, DateTimeInstantiation dateTimeInstantiation)
    {
        _codeWriter = codeWriter;
        _dateTimeInstantiation = dateTimeInstantiation;
    }

    public string Id => "TimeOnly";
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return objectType.IsTimeOnly();
    }

    public void Visit(object timeOnly, Type objectType)
    {
        var ticks = (long?)objectType.GetProperty("Ticks")?.GetValue(timeOnly);

        if (ticks == null)
        {
            _codeWriter.WriteErrorDetected("Wrong TimeOnly struct");
            return;
        }

        if (ticks == 863999999999)
        {
            _codeWriter.WriteFieldReference(nameof(DateTime.MaxValue), () => _codeWriter.WriteTypeReference(objectType));

            return;
        }

        if (ticks == 0)
        {
            _codeWriter.WriteFieldReference(nameof(DateTime.MinValue), () => _codeWriter.WriteTypeReference(objectType));

            return;
        }

        var timeSpan = TimeSpan.FromTicks(ticks.Value);

        if (timeSpan.Ticks % TimeSpan.TicksPerMillisecond != 0)
        {
            _codeWriter.WriteMethodInvoke(
                () => _codeWriter.WriteMethodReference(
                    () => _codeWriter.WriteTypeReference(objectType), nameof(TimeSpan.FromTicks)),
                [
                    () => _codeWriter.WritePrimitive(ticks.Value)
                ]);

            return;
        }

        if (_dateTimeInstantiation == DateTimeInstantiation.Parse)
        {
            _codeWriter.WriteMethodInvoke(
                () => _codeWriter.WriteMethodReference(
                    () => _codeWriter.WriteTypeReference(objectType), nameof(DateTime.ParseExact)),
                [
                    () => _codeWriter.WritePrimitive($"{timeSpan:c}"),
                    () => _codeWriter.WritePrimitive("O")
                ]);

            return;
        }

        _codeWriter.WriteObjectCreateAndInitialize(objectType,
            [
                () => _codeWriter.WritePrimitive(timeSpan.Hours),
                () => _codeWriter.WritePrimitive(timeSpan.Minutes),
                () => _codeWriter.WritePrimitive(timeSpan.Seconds),
                () => _codeWriter.WritePrimitive(timeSpan.Milliseconds)
            ],
            []);
    }
}