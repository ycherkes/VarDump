using System;
using VarDump.CodeDom.Common;
using VarDump.CodeDom.Compiler;
using VarDump.Utils;

namespace VarDump.Visitor.KnownTypes;

internal sealed class TimeOnlyVisitor : IKnownObjectVisitor
{
    private readonly ICodeGenerator _codeGenerator;
    private readonly DateTimeInstantiation _dateTimeInstantiation;

    public TimeOnlyVisitor(ICodeGenerator codeGenerator, DateTimeInstantiation dateTimeInstantiation)
    {
        _codeGenerator = codeGenerator;
        _dateTimeInstantiation = dateTimeInstantiation;
    }

    public string Id => "TimeOnly";
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return objectType.IsTimeOnly();
    }

    public void Visit(object timeOnly, Type objectType)
    {
        var timeOnlyCodeTypeReference = new CodeTypeReference(objectType);
        var ticks = (long?)objectType.GetProperty("Ticks")?.GetValue(timeOnly);

        if (ticks == null)
        {
            _codeGenerator.WriteErrorDetected("Wrong TimeOnly struct");
            return;
        }

        if (ticks == 863999999999)
        {
            _codeGenerator.GenerateFieldReference(nameof(DateTime.MaxValue), () => _codeGenerator.GenerateTypeReference(timeOnlyCodeTypeReference));

            return;
        }

        if (ticks == 0)
        {
            _codeGenerator.GenerateFieldReference(nameof(DateTime.MinValue), () => _codeGenerator.GenerateTypeReference(timeOnlyCodeTypeReference));

            return;
        }

        var timeSpan = TimeSpan.FromTicks(ticks.Value);

        if (timeSpan.Ticks % TimeSpan.TicksPerMillisecond != 0)
        {
            _codeGenerator.GenerateMethodInvoke(
                () => _codeGenerator.GenerateMethodReference(
                    () => _codeGenerator.GenerateTypeReference(timeOnlyCodeTypeReference), nameof(TimeSpan.FromTicks)),
                [
                    () => _codeGenerator.GeneratePrimitive(ticks.Value)
                ]);

            return;
        }

        if (_dateTimeInstantiation == DateTimeInstantiation.Parse)
        {
            _codeGenerator.GenerateMethodInvoke(
                () => _codeGenerator.GenerateMethodReference(
                    () => _codeGenerator.GenerateTypeReference(timeOnlyCodeTypeReference), nameof(DateTime.ParseExact)),
                [
                    () => _codeGenerator.GeneratePrimitive($"{timeSpan:c}"),
                    () => _codeGenerator.GeneratePrimitive("O")
                ]);

            return;
        }

        _codeGenerator.GenerateObjectCreateAndInitialize(timeOnlyCodeTypeReference,
            [
                () => _codeGenerator.GeneratePrimitive(timeSpan.Hours),
                () => _codeGenerator.GeneratePrimitive(timeSpan.Minutes),
                () => _codeGenerator.GeneratePrimitive(timeSpan.Seconds),
                () => _codeGenerator.GeneratePrimitive(timeSpan.Milliseconds)
            ],
            []);
    }
}