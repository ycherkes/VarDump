using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using VarDumpExtended.CodeDom.Common;

namespace VarDumpExtended.Visitor.KnownTypes;

internal sealed class TimeSpanVisitor : IKnownObjectVisitor
{
    private readonly CodeTypeReferenceOptions _typeReferenceOptions;
    private readonly DateTimeInstantiation _dateTimeInstantiation;

    public TimeSpanVisitor(DumpOptions options)
    {
        _typeReferenceOptions = options.UseTypeFullName
            ? CodeTypeReferenceOptions.FullTypeName
            : CodeTypeReferenceOptions.ShortTypeName;

        _dateTimeInstantiation = options.DateTimeInstantiation;
    }

    public string Id => nameof(TimeSpan);
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is TimeSpan;
    }

    public CodeExpression Visit(object obj, Type objectType)
    {
        var timeSpan = (TimeSpan)obj;

        var specialValuesDictionary = new Dictionary<TimeSpan, string>
        {
            { TimeSpan.MaxValue, nameof(TimeSpan.MaxValue) },
            { TimeSpan.MinValue, nameof(TimeSpan.MinValue) },
            { TimeSpan.Zero, nameof(TimeSpan.Zero) }
        };

        var timeSpanCodeTypeReference = new CodeTypeReference(typeof(TimeSpan), _typeReferenceOptions);

        if (specialValuesDictionary.TryGetValue(timeSpan, out var name))
            return new CodeFieldReferenceExpression
            (
                new CodeTypeReferenceExpression(timeSpanCodeTypeReference),
                name
            );

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
            return new CodeMethodInvokeExpression
            (
                new CodeMethodReferenceExpression(
                    new CodeTypeReferenceExpression(timeSpanCodeTypeReference),
                    nonZeroValues[0].Key),
                new CodePrimitiveExpression(nonZeroValues[0].Value)
            );

        if (timeSpan.Ticks % TimeSpan.TicksPerMillisecond != 0)
            return new CodeMethodInvokeExpression
            (
                new CodeMethodReferenceExpression(
                    new CodeTypeReferenceExpression(timeSpanCodeTypeReference),
                    nameof(TimeSpan.FromTicks)),
                new CodePrimitiveExpression(timeSpan.Ticks)
            );

        if (_dateTimeInstantiation == DateTimeInstantiation.Parse)
        {
            return new CodeMethodInvokeExpression
            (
                new CodeMethodReferenceExpression(
                    new CodeTypeReferenceExpression(timeSpanCodeTypeReference),
                    nameof(TimeSpan.ParseExact)),
                new CodePrimitiveExpression(timeSpan.ToString("c")),
                new CodePrimitiveExpression("c"),
                new CodeFieldReferenceExpression(
                    new CodeTypeReferenceExpression(
                        new CodeTypeReference(typeof(CultureInfo), _typeReferenceOptions)),
                    nameof(CultureInfo.InvariantCulture)),
                new CodeFieldReferenceExpression(
                    new CodeTypeReferenceExpression(
                        new CodeTypeReference(typeof(TimeSpanStyles), _typeReferenceOptions)),
                    nameof(TimeSpanStyles.None))
            );
        }

        var days = new CodePrimitiveExpression(timeSpan.Days);
        var hours = new CodePrimitiveExpression(timeSpan.Hours);
        var minutes = new CodePrimitiveExpression(timeSpan.Minutes);
        var seconds = new CodePrimitiveExpression(timeSpan.Seconds);
        var milliseconds = new CodePrimitiveExpression(timeSpan.Milliseconds);

        if (timeSpan.Days == 0 && timeSpan.Milliseconds == 0)
            return new CodeObjectCreateExpression(timeSpanCodeTypeReference, hours, minutes, seconds);

        if (timeSpan.Milliseconds == 0)
            return new CodeObjectCreateExpression(timeSpanCodeTypeReference, days, hours, minutes, seconds);

        return new CodeObjectCreateExpression(timeSpanCodeTypeReference, days, hours, minutes, seconds, milliseconds);
    }
}