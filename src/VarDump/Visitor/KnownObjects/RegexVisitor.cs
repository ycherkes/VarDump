using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using VarDump.CodeDom.Compiler;

namespace VarDump.Visitor.KnownObjects;

internal sealed class RegexVisitor(ICodeWriter codeWriter, INextDepthVisitor nextDepthVisitor, DumpOptions options) : IKnownObjectVisitor
{
    public string Id => nameof(Regex);

    public DumpOptions Options => options;

    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is Regex;
    }

    public void Visit(object obj, Type objectType, VisitContext context)
    {
        var regex = (Regex)obj;

        var constructorArguments = options.UseNamedArgumentsInConstructors
            ? GetNamedConstructorArguments()
            : GetConstructorArguments();

        codeWriter.WriteObjectCreate(typeof(Regex), constructorArguments);

        return;

        IEnumerable<Action> GetNamedConstructorArguments()
        {
            yield return () => codeWriter.WriteNamedArgument("pattern", WritePattern);
            if (regex.Options != RegexOptions.None || regex.MatchTimeout != Timeout.InfiniteTimeSpan)
            {
                yield return () => codeWriter.WriteNamedArgument("options",WriteOptions);
                if (regex.MatchTimeout != Timeout.InfiniteTimeSpan)
                {
                    yield return () => codeWriter.WriteNamedArgument("matchTimeout", WriteMatchTimeout);
                }
            }
        }

        IEnumerable<Action> GetConstructorArguments()
        {
            yield return WritePattern;
            if (regex.Options != RegexOptions.None || regex.MatchTimeout != Timeout.InfiniteTimeSpan)
            {
                yield return WriteOptions;
                if (regex.MatchTimeout != Timeout.InfiniteTimeSpan)
                {
                    yield return WriteMatchTimeout;
                }
            }
        }

        void WritePattern() => codeWriter.WritePrimitive(regex.ToString());
        void WriteOptions() => nextDepthVisitor.Visit(regex.Options, context);
        void WriteMatchTimeout() => nextDepthVisitor.Visit(regex.MatchTimeout, context);
    }
}