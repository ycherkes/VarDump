using System;
using VarDump.CodeDom.Common;
using VarDump.Utils;
using VarDump.Visitor.Descriptors;

namespace VarDump.Visitor.KnownTypes;

internal sealed class DateOnlyVisitor : IKnownObjectVisitor
{
    private readonly CodeTypeReferenceOptions _typeReferenceOptions;
    private readonly DateTimeInstantiation _dateTimeInstantiation;

    public DateOnlyVisitor(DumpOptions options)
    {
        _dateTimeInstantiation = options.DateTimeInstantiation;
        _typeReferenceOptions = options.UseTypeFullName
            ? CodeTypeReferenceOptions.FullTypeName
            : CodeTypeReferenceOptions.ShortTypeName;
    }

    public string Id => "DateOnly";
    public bool IsSuitableFor(IValueDescriptor valueDescriptor)
    {
        return valueDescriptor.Type.IsDateOnly();
    }

    public CodeExpression Visit(IValueDescriptor valueDescriptor)
    {
        var dateOnlyCodeTypeReference = new CodeTypeReference(valueDescriptor.Type, _typeReferenceOptions);
        var dayNumber = (int?)valueDescriptor.Type.GetProperty("DayNumber")?.GetValue(valueDescriptor.Value);

        if (dayNumber == null)
        {
            return CodeDomUtils.GetErrorDetectedExpression("Wrong DateOnly struct");
        }

        if (dayNumber == 3652058U)
            return new CodeFieldReferenceExpression
            (
                new CodeTypeReferenceExpression(dateOnlyCodeTypeReference),
                nameof(DateTime.MaxValue)
            );

        if (dayNumber == 1)
            return new CodeFieldReferenceExpression
            (
                new CodeTypeReferenceExpression(dateOnlyCodeTypeReference),
                nameof(DateTime.MinValue)
            );

        var dateTime = new DateTime((long)dayNumber * 864000000000L);

        if (_dateTimeInstantiation == DateTimeInstantiation.Parse)
        {
            return new CodeMethodInvokeExpression
            (
                new CodeMethodReferenceExpression(
                    new CodeTypeReferenceExpression(dateOnlyCodeTypeReference),
                    nameof(DateTime.ParseExact)),
                new CodePrimitiveExpression($"{dateTime:yyyy-MM-dd}"),
                new CodePrimitiveExpression("O")
            );
        }

        var year = new CodePrimitiveExpression(dateTime.Year);
        var month = new CodePrimitiveExpression(dateTime.Month);
        var day = new CodePrimitiveExpression(dateTime.Day);

        return new CodeObjectCreateExpression(dateOnlyCodeTypeReference, year, month, day);
    }
}