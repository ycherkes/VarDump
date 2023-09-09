using System;
using System.Globalization;
using VarDump.CodeDom.Common;

namespace VarDump.Visitor.KnownTypes;

internal sealed class DateTimeVisitor : IKnownObjectVisitor
{
    private readonly CodeTypeReferenceOptions _typeReferenceOptions;
    private readonly DateTimeInstantiation _dateTimeInstantiation;
    private readonly DateKind _dateKind;

    public DateTimeVisitor(DumpOptions options)
    {
        _typeReferenceOptions = options.UseTypeFullName
            ? CodeTypeReferenceOptions.FullTypeName
            : CodeTypeReferenceOptions.ShortTypeName;

        _dateTimeInstantiation = options.DateTimeInstantiation;
        _dateKind = options.DateKind;
    }

    public string Id => nameof(DateTime);
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is DateTime;
    }

    public CodeExpression Visit(object obj, Type objectType)
    {
        var dateTime = (DateTime)obj;
        var dateTimeCodeTypeReference = new CodeTypeReference(typeof(DateTime), _typeReferenceOptions);

        if (dateTime == DateTime.MaxValue)
            return new CodeFieldReferenceExpression
            (
                new CodeTypeReferenceExpression(dateTimeCodeTypeReference),
                nameof(DateTime.MaxValue)
            );

        if (dateTime == DateTime.MinValue)
            return new CodeFieldReferenceExpression
            (
                new CodeTypeReferenceExpression(dateTimeCodeTypeReference),
                nameof(DateTime.MinValue)
            );

        if (_dateKind == DateKind.ConvertToUtc)
        {
            dateTime = dateTime.ToUniversalTime();
        }

        if (_dateTimeInstantiation == DateTimeInstantiation.Parse)
        {
            return new CodeMethodInvokeExpression
            (
                new CodeMethodReferenceExpression(
                    new CodeTypeReferenceExpression(dateTimeCodeTypeReference),
                    nameof(DateTime.ParseExact)),
                new CodePrimitiveExpression(dateTime.ToString("O")),
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

        var kind = new CodeFieldReferenceExpression
        (
            new CodeTypeReferenceExpression(new CodeTypeReference(typeof(DateTimeKind), _typeReferenceOptions)),
            dateTime.Kind.ToString()
        );

        var year = new CodePrimitiveExpression(dateTime.Year);
        var month = new CodePrimitiveExpression(dateTime.Month);
        var day = new CodePrimitiveExpression(dateTime.Day);
        var hour = new CodePrimitiveExpression(dateTime.Hour);
        var minute = new CodePrimitiveExpression(dateTime.Minute);
        var second = new CodePrimitiveExpression(dateTime.Second);
        var millisecond = new CodePrimitiveExpression(dateTime.Millisecond);

        var createDateTimeExpression = new CodeObjectCreateExpression(dateTimeCodeTypeReference, year,
            month, day, hour, minute, second, millisecond, kind);

        var lessThanMillisecondTicks = dateTime.Ticks % TimeSpan.TicksPerMillisecond;

        if (lessThanMillisecondTicks == 0)
            return createDateTimeExpression;

        return new CodeMethodInvokeExpression
        (
            new CodeMethodReferenceExpression(
                createDateTimeExpression,
                nameof(DateTime.AddTicks)),
            new CodePrimitiveExpression(lessThanMillisecondTicks)
        );
    }
}