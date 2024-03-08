using System;
using System.Collections.Generic;
using VarDump.CodeDom.Compiler;

namespace VarDump.Visitor.KnownTypes;

internal sealed class UriVisitor(ICodeWriter codeWriter) : IKnownObjectVisitor
{
    public string Id => nameof(Uri);
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is Uri;
    }

    public void Visit(object obj, Type objectType, VisitContext context)
    {
        var uri = (Uri)obj;

        codeWriter.WriteObjectCreate(typeof(Uri), GetConstructorArguments());

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