using System;
using VarDump.CodeDom.Common;
using VarDump.Utils;
using VarDump.Visitor.Descriptors;

namespace VarDump.Visitor.KnownTypes;

internal sealed class TimeOnlyVisitor : IKnownObjectVisitor
{
    private readonly CodeTypeReferenceOptions _typeReferenceOptions;
    private readonly DateTimeInstantiation _dateTimeInstantiation;

    public TimeOnlyVisitor(DumpOptions options)
    {
        _dateTimeInstantiation = options.DateTimeInstantiation;
        _typeReferenceOptions = options.UseTypeFullName
            ? CodeTypeReferenceOptions.FullTypeName
            : CodeTypeReferenceOptions.ShortTypeName;
    }

    public string Id => "TimeOnly";
    public bool IsSuitableFor(IValueDescriptor valueDescriptor)
    {
        return valueDescriptor.Type.IsTimeOnly();
    }

    public CodeExpression Visit(IValueDescriptor valueDescriptor)
    {
        var timeOnlyCodeTypeReference = new CodeTypeReference(valueDescriptor.Type, _typeReferenceOptions);
        var ticks = (long?)valueDescriptor.Type.GetProperty("Ticks")?.GetValue(valueDescriptor.Value);

        if (ticks == null)
        {
            return CodeDomUtils.GetErrorDetectedExpression("Wrong TimeOnly struct");
        }

        if (ticks == 863999999999)
            return new CodeFieldReferenceExpression
            (
                new CodeTypeReferenceExpression(timeOnlyCodeTypeReference),
                nameof(DateTime.MaxValue)
            );

        if (ticks == 0)
            return new CodeFieldReferenceExpression
            (
                new CodeTypeReferenceExpression(timeOnlyCodeTypeReference),
                nameof(DateTime.MinValue)
            );

        var timeSpan = TimeSpan.FromTicks(ticks.Value);

        if (timeSpan.Ticks % TimeSpan.TicksPerMillisecond != 0)
            return new CodeMethodInvokeExpression
            (
                new CodeMethodReferenceExpression(
                    new CodeTypeReferenceExpression(timeOnlyCodeTypeReference),
                    nameof(TimeSpan.FromTicks)),
                new CodePrimitiveExpression(ticks.Value)
            );

        if (_dateTimeInstantiation == DateTimeInstantiation.Parse)
        {
            return new CodeMethodInvokeExpression
            (
                new CodeMethodReferenceExpression(
                    new CodeTypeReferenceExpression(timeOnlyCodeTypeReference),
                    nameof(DateTime.ParseExact)),
                new CodePrimitiveExpression($"{timeSpan:c}"),
                new CodePrimitiveExpression("O")
            );
        }

        var hour = new CodePrimitiveExpression(timeSpan.Hours);
        var minute = new CodePrimitiveExpression(timeSpan.Minutes);
        var second = new CodePrimitiveExpression(timeSpan.Seconds);
        var millisecond = new CodePrimitiveExpression(timeSpan.Milliseconds);

        return new CodeObjectCreateExpression(timeOnlyCodeTypeReference, hour, minute, second, millisecond);
    }
}