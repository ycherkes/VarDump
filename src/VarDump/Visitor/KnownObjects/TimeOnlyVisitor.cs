using System;
using VarDump.CodeDom.Compiler;
using VarDump.Extensions;
using VarDump.Utils;

namespace VarDump.Visitor.KnownObjects;

internal sealed class TimeOnlyVisitor(ICodeWriter codeWriter, DumpOptions options) : IKnownObjectVisitor
{
    public string Id => "TimeOnly";

    public bool IsSuitableFor(object obj, Type objectType)
    {
        return objectType.IsTimeOnly();
    }

    public void ConfigureOptions(Action<DumpOptions> configure)
    {
        options = options.Clone();
        configure?.Invoke(options);
    }

    public void Visit(object timeOnly, Type objectType, VisitContext context)
    {
        var ticks = (long?)objectType.GetProperty("Ticks")?.GetValue(timeOnly);

        if (ticks == null)
        {
            codeWriter.WriteErrorDetected("Wrong TimeOnly struct");
            return;
        }

        if (options.UsePredefinedConstants)
        {
            switch (ticks)
            {
                case 863999999999:
                    codeWriter.WriteFieldReference(nameof(DateTime.MaxValue), () => codeWriter.WriteType(objectType));
                    return;
                case 0:
                    codeWriter.WriteFieldReference(nameof(DateTime.MinValue), () => codeWriter.WriteType(objectType));
                    return;
            }
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

        if (options.DateTimeInstantiation == DateTimeInstantiation.Parse)
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

        if (options.UseNamedArgumentsInConstructors)
        {
            codeWriter.WriteObjectCreate(objectType,
            [
                () => codeWriter.WriteNamedArgument("hour", WriteHours),
                () => codeWriter.WriteNamedArgument("minute", WriteMinutes),
                () => codeWriter.WriteNamedArgument("second", WriteSeconds),
                () => codeWriter.WriteNamedArgument("millisecond", WriteMilliseconds)
            ]);
        }
        else
        {
            codeWriter.WriteObjectCreate(objectType,
            [
                WriteHours,
                WriteMinutes,
                WriteSeconds,
                WriteMilliseconds
            ]);
        }

        return;

        void WriteHours() => codeWriter.WritePrimitive(timeSpan.Hours);
        void WriteMinutes() => codeWriter.WritePrimitive(timeSpan.Minutes);
        void WriteSeconds() => codeWriter.WritePrimitive(timeSpan.Seconds);
        void WriteMilliseconds() => codeWriter.WritePrimitive(timeSpan.Milliseconds);
    }
}