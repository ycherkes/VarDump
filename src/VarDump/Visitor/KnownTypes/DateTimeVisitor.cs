using System;
using System.Globalization;
using VarDump.CodeDom.Common;
using VarDump.CodeDom.Compiler;

namespace VarDump.Visitor.KnownTypes;

internal sealed class DateTimeVisitor : IKnownObjectVisitor
{
    private readonly IDotnetCodeGenerator _codeGenerator;
    private readonly DateTimeInstantiation _dateTimeInstantiation;
    private readonly DateKind _dateKind;

    public DateTimeVisitor(IDotnetCodeGenerator codeGenerator, DateTimeInstantiation dateTimeInstantiation, DateKind dateKind)
    {
        _codeGenerator = codeGenerator;
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
        var dateTimeCodeTypeReference = new CodeDotnetTypeReference(typeof(DateTime));

        if (dateTime == DateTime.MaxValue)
        {
            _codeGenerator.GenerateFieldReference(nameof(DateTime.MaxValue), () => _codeGenerator.GenerateTypeReference(dateTimeCodeTypeReference));
            return;
        }

        if (dateTime == DateTime.MinValue)
        {
            _codeGenerator.GenerateFieldReference(nameof(DateTime.MinValue), () => _codeGenerator.GenerateTypeReference(dateTimeCodeTypeReference));
            return;
        }

        if (_dateKind == DateKind.ConvertToUtc)
        {
            dateTime = dateTime.ToUniversalTime();
        }

        if (_dateTimeInstantiation == DateTimeInstantiation.Parse)
        {
            _codeGenerator.GenerateMethodInvoke(
                () => _codeGenerator.GenerateMethodReference(
                    () => _codeGenerator.GenerateTypeReference(dateTimeCodeTypeReference), nameof(DateTime.ParseExact)),
                [
                    () => _codeGenerator.GeneratePrimitive(dateTime.ToString("O")),
                    () => _codeGenerator.GeneratePrimitive("O"),
                    () => _codeGenerator.GenerateFieldReference(nameof(CultureInfo.InvariantCulture),
                        () => _codeGenerator.GenerateTypeReference(new CodeDotnetTypeReference(typeof(CultureInfo)))),
                    () => _codeGenerator.GenerateFieldReference(nameof(DateTimeStyles.RoundtripKind),
                        () => _codeGenerator.GenerateTypeReference(new CodeDotnetTypeReference(typeof(DateTimeStyles))))
                ]);

            return;
        }

        var lessThanMillisecondTicks = dateTime.Ticks % TimeSpan.TicksPerMillisecond;

        if (lessThanMillisecondTicks == 0)
        {
            GenerateObjectCreateAction();
            return;
        }

        _codeGenerator.GenerateMethodInvoke(() => _codeGenerator.GenerateMethodReference(GenerateObjectCreateAction, nameof(DateTime.AddTicks)), [() => _codeGenerator.GeneratePrimitive(lessThanMillisecondTicks)]);

        void GenerateObjectCreateAction() => _codeGenerator.GenerateObjectCreateAndInitialize(dateTimeCodeTypeReference, 
            [
                GenerateYearAction, 
                GenerateMontAction, 
                GenerateDayAction, 
                GenerateHourAction, 
                GenerateMinuteAction, 
                GenerateSecondAction, 
                GenerateMillisecondAction, 
                GenerateKindAction], 
            []);
        void GenerateKindAction() => _codeGenerator.GenerateFieldReference(dateTime.Kind.ToString(), () => _codeGenerator.GenerateTypeReference(new CodeDotnetTypeReference(typeof(DateTimeKind))));
        void GenerateYearAction() => _codeGenerator.GeneratePrimitive(dateTime.Year);
        void GenerateMontAction() => _codeGenerator.GeneratePrimitive(dateTime.Month);
        void GenerateDayAction() => _codeGenerator.GeneratePrimitive(dateTime.Day);
        void GenerateHourAction() => _codeGenerator.GeneratePrimitive(dateTime.Hour);
        void GenerateMinuteAction() => _codeGenerator.GeneratePrimitive(dateTime.Minute);
        void GenerateSecondAction() => _codeGenerator.GeneratePrimitive(dateTime.Second);
        void GenerateMillisecondAction() => _codeGenerator.GeneratePrimitive(dateTime.Millisecond);
    }
}