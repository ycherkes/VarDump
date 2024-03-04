using System;
using System.Collections.Generic;
using VarDump.CodeDom.Compiler;
using VarDump.Extensions;

namespace VarDump.Visitor.KnownTypes;

internal sealed class UriVisitor : IKnownObjectVisitor
{
    private readonly ICodeWriter _codeWriter;

    public UriVisitor(ICodeWriter codeWriter)
    {
        _codeWriter = codeWriter;
    }

    public string Id => nameof(Uri);
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is Uri;
    }

    public void Visit(object obj, Type objectType)
    {
        var uri = (Uri)obj;

        _codeWriter.WriteObjectCreateAndInitialize(typeof(Uri), GetConstructorArguments(), []);

        IEnumerable<Action> GetConstructorArguments()
        {
            yield return () => _codeWriter.WritePrimitive(uri.OriginalString);
            if (!uri.IsAbsoluteUri)
            {
                yield return () => _codeWriter.WriteFieldReference(nameof(UriKind.Relative), () => _codeWriter.WriteTypeReference(typeof(UriKind)));
            }
        }
    }
}