using System;
using System.Collections.Generic;
using VarDump.CodeDom.Compiler;

namespace VarDump.Visitor.KnownObjects;

internal sealed class UriVisitor(ICodeWriter codeWriter, DumpOptions options) : IKnownObjectVisitor
{
    public string Id => nameof(Uri);

    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is Uri;
    }

    public void ConfigureOptions(Action<DumpOptions> configure)
    {
        options = options.Clone();
        configure?.Invoke(options);
    }

    public void Visit(object obj, Type objectType, VisitContext context)
    {
        var uri = (Uri)obj;

        var constructorArguments = options.UseNamedArgumentsInConstructors
            ? GetNamedConstructorArguments()
            : GetConstructorArguments();

        codeWriter.WriteObjectCreate(typeof(Uri), constructorArguments);

        return;

        IEnumerable<Action> GetNamedConstructorArguments()
        {
            yield return () => codeWriter.WriteNamedArgument("uriString", WriteUriString);
            if (!uri.IsAbsoluteUri)
            {
                yield return () => codeWriter.WriteNamedArgument("uriKind", WriteUriKind);
            }
        }

        IEnumerable<Action> GetConstructorArguments()
        {
            yield return WriteUriString;
            if (!uri.IsAbsoluteUri)
            {
                yield return WriteUriKind;
            }
        }

        void WriteUriString() => codeWriter.WritePrimitive(uri.OriginalString);
        void WriteUriKind() => codeWriter.WriteFieldReference(nameof(UriKind.Relative), () => codeWriter.WriteType(typeof(UriKind)));
    }
}