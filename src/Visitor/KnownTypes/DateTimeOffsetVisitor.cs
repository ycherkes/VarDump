using System;
using System.Globalization;
using VarDumpExtended.CodeDom.Common;

namespace VarDumpExtended.Visitor.KnownTypes;

internal sealed class DateTimeOffsetVisitor : IKnownObjectVisitor
{
    private readonly IObjectVisitor _rootObjectVisitor;
    private readonly CodeTypeReferenceOptions _typeReferenceOptions;
    private readonly DateTimeInstantiation _dateTimeInstantiation;

    public DateTimeOffsetVisitor(DumpOptions options, IObjectVisitor rootObjectVisitor)
    {
        _rootObjectVisitor = rootObjectVisitor;
        _typeReferenceOptions = options.UseTypeFullName
            ? CodeTypeReferenceOptions.FullTypeName
            : CodeTypeReferenceOptions.ShortTypeName;

        _dateTimeInstantiation = options.DateTimeInstantiation;
    }

    public string Id => nameof(DateTimeOffset);
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is DateTimeOffset;
    }

    public CodeExpression Visit(object obj, Type objectType)
    {
        var dateTimeOffset = (DateTimeOffset)obj;
        var dateTimeOffsetCodeTypeReference = new CodeTypeReference(typeof(DateTimeOffset), _typeReferenceOptions);

        if (dateTimeOffset == DateTimeOffset.MaxValue)
            return new CodeFieldReferenceExpression
            (
                new CodeTypeReferenceExpression(dateTimeOffsetCodeTypeReference),
                nameof(DateTimeOffset.MaxValue)
            );

        if (dateTimeOffset == DateTimeOffset.MinValue)
            return new CodeFieldReferenceExpression
            (
                new CodeTypeReferenceExpression(dateTimeOffsetCodeTypeReference),
                nameof(DateTimeOffset.MinValue)
            );

        if (_dateTimeInstantiation == DateTimeInstantiation.Parse)
        {
            return new CodeMethodInvokeExpression
            (
                new CodeMethodReferenceExpression(
                    new CodeTypeReferenceExpression(dateTimeOffsetCodeTypeReference),
                    nameof(DateTimeOffset.ParseExact)),
                new CodePrimitiveExpression(dateTimeOffset.ToString("O")),
                new CodePrimitiveExpression("O"),
                new CodeFieldReferenceExpression(
                    new CodeTypeReferenceExpression(
                        new CodeTypeReference(typeof(CultureInfo), _typeReferenceOptions)),
                        nameof(CultureInfo.InvariantCulture)),
                new CodeFieldReferenceExpression(
                    new CodeTypeReferenceExpression(
                        new CodeTypeReference(typeof(DateTimeStyles), _typeReferenceOptions)),
                        nameof(DateTimeStyles.RoundtripKind))
            );
        }

        var offsetExpression = _rootObjectVisitor.Visit(dateTimeOffset.Offset);

        var year = new CodePrimitiveExpression(dateTimeOffset.Year);
        var month = new CodePrimitiveExpression(dateTimeOffset.Month);
        var day = new CodePrimitiveExpression(dateTimeOffset.Day);
        var hour = new CodePrimitiveExpression(dateTimeOffset.Hour);
        var minute = new CodePrimitiveExpression(dateTimeOffset.Minute);
        var second = new CodePrimitiveExpression(dateTimeOffset.Second);
        var millisecond = new CodePrimitiveExpression(dateTimeOffset.Millisecond);

        var createDateTimeOffsetExpression = new CodeObjectCreateExpression(dateTimeOffsetCodeTypeReference, year, month, day, hour, minute, second,
            millisecond, offsetExpression);

        var lessThanMillisecondTicks = dateTimeOffset.Ticks % TimeSpan.TicksPerMillisecond;

        if (lessThanMillisecondTicks == 0)
            return createDateTimeOffsetExpression;

        return new CodeMethodInvokeExpression
        (
            new CodeMethodReferenceExpression(
                createDateTimeOffsetExpression,
                nameof(DateTimeOffset.AddTicks)),
            new CodePrimitiveExpression(lessThanMillisecondTicks)
        );
    }
}