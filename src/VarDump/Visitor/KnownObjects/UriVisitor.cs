using System;
using System.Collections.Generic;
using VarDump.CodeDom.Compiler;

namespace VarDump.Visitor.KnownObjects;

internal sealed class UriVisitor(ICodeWriter codeWriter, DumpOptions options) : IKnownObjectVisitor
{
    public string Id => nameof(Uri);

    public DumpOptions Options => options;

    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is Uri;
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
            yield return () => codeWriter.WriteNamedArgument("uriString", () => codeWriter.WritePrimitive(uri.OriginalString));
            if (!uri.IsAbsoluteUri)
            {
                yield return () => codeWriter.WriteNamedArgument("uriKind", () => codeWriter.WriteFieldReference(nameof(UriKind.Relative), () => codeWriter.WriteType(typeof(UriKind))));
            }
        }

        IEnumerable<Action> GetConstructorArguments()
        {
            yield return () => codeWriter.WritePrimitive(uri.OriginalString);
            if (!uri.IsAbsoluteUri)
            {
                yield return () => codeWriter.WriteFieldReference(nameof(UriKind.Relative), () => codeWriter.WriteType(typeof(UriKind)));
            }
        }
    }
}