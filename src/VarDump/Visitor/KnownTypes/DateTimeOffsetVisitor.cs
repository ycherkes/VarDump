using System;
using System.Globalization;
using VarDump.CodeDom.Common;
using VarDump.CodeDom.Compiler;

namespace VarDump.Visitor.KnownTypes;

internal sealed class DateTimeOffsetVisitor : IKnownObjectVisitor
{
    private readonly IObjectVisitor _rootObjectVisitor;
    private readonly ICodeGenerator _codeGenerator;
    private readonly DateTimeInstantiation _dateTimeInstantiation;

    public DateTimeOffsetVisitor(IObjectVisitor rootObjectVisitor, ICodeGenerator codeGenerator, DateTimeInstantiation dateTimeInstantiation)
    {
        _rootObjectVisitor = rootObjectVisitor;
        _codeGenerator = codeGenerator;
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
        var dateTimeOffsetCodeTypeReference = new CodeTypeReference(typeof(DateTimeOffset));

        if (dateTimeOffset == DateTimeOffset.MaxValue)
        {
            _codeGenerator.GenerateFieldReference(nameof(DateTimeOffset.MaxValue), () => _codeGenerator.GenerateTypeReference(dateTimeOffsetCodeTypeReference));
            return;
        }

        if (dateTimeOffset == DateTimeOffset.MinValue)
        {
            _codeGenerator.GenerateFieldReference(nameof(DateTimeOffset.MinValue), () => _codeGenerator.GenerateTypeReference(dateTimeOffsetCodeTypeReference));
            return;
        }

        if (_dateTimeInstantiation == DateTimeInstantiation.Parse)
        {
            _codeGenerator.GenerateMethodInvoke(
                () => _codeGenerator.GenerateMethodReference(
                    () => _codeGenerator.GenerateTypeReference(dateTimeOffsetCodeTypeReference), nameof(DateTimeOffset.ParseExact)),
                [
                    () => _codeGenerator.GeneratePrimitive(dateTimeOffset.ToString("O")),
                    () => _codeGenerator.GeneratePrimitive("O"),
                    () => _codeGenerator.GenerateFieldReference(nameof(CultureInfo.InvariantCulture),
                        () => _codeGenerator.GenerateTypeReference(new CodeTypeReference(typeof(CultureInfo)))),
                    () => _codeGenerator.GenerateFieldReference(nameof(DateTimeStyles.RoundtripKind),
                        () => _codeGenerator.GenerateTypeReference(new CodeTypeReference(typeof(DateTimeStyles))))
                ]);

            return;
        }

        var lessThanMillisecondTicks = dateTimeOffset.Ticks % TimeSpan.TicksPerMillisecond;

        if (lessThanMillisecondTicks == 0)
        {
            GenerateObjectCreateAction();
            return;
        }

        _codeGenerator.GenerateMethodInvoke(() => _codeGenerator.GenerateMethodReference(GenerateObjectCreateAction, nameof(DateTimeOffset.AddTicks)), [() => _codeGenerator.GeneratePrimitive(lessThanMillisecondTicks)]);

        void GenerateObjectCreateAction() => _codeGenerator.GenerateObjectCreateAndInitialize(dateTimeOffsetCodeTypeReference,
            [
                GenerateYearAction,
                GenerateMontAction,
                GenerateDayAction,
                GenerateHourAction,
                GenerateMinuteAction,
                GenerateSecondAction,
                GenerateMillisecondAction,
                GenerateOffsetAction],
            []);

        void GenerateYearAction() => _codeGenerator.GeneratePrimitive(dateTimeOffset.Year);
        void GenerateMontAction() => _codeGenerator.GeneratePrimitive(dateTimeOffset.Month);
        void GenerateDayAction() => _codeGenerator.GeneratePrimitive(dateTimeOffset.Day);
        void GenerateHourAction() => _codeGenerator.GeneratePrimitive(dateTimeOffset.Hour);
        void GenerateMinuteAction() => _codeGenerator.GeneratePrimitive(dateTimeOffset.Minute);
        void GenerateSecondAction() => _codeGenerator.GeneratePrimitive(dateTimeOffset.Second);
        void GenerateMillisecondAction() => _codeGenerator.GeneratePrimitive(dateTimeOffset.Millisecond);
        void GenerateOffsetAction() => _rootObjectVisitor.Visit(dateTimeOffset.Offset);
    }
}