using System;
using System.Collections.Generic;
using VarDump.CodeDom.Common;
using VarDump.CodeDom.Compiler;

namespace VarDump.Visitor.KnownTypes;

internal sealed class UriVisitor : IKnownObjectVisitor
{
    private readonly IDotnetCodeGenerator _codeGenerator;

    public UriVisitor(IDotnetCodeGenerator codeGenerator)
    {
        _codeGenerator = codeGenerator;
    }

    public string Id => nameof(Uri);
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is Uri;
    }

    public void Visit(object obj, Type objectType)
    {
        var uri = (Uri)obj;

        _codeGenerator.GenerateObjectCreateAndInitialize(new CodeDotnetTypeReference(typeof(Uri)), GetConstructorArguments(), []);

        IEnumerable<Action> GetConstructorArguments()
        {
            yield return () => _codeGenerator.GeneratePrimitive(uri.OriginalString);
            if (!uri.IsAbsoluteUri)
            {
                yield return () => _codeGenerator.GenerateFieldReference(nameof(UriKind.Relative), () => _codeGenerator.GenerateTypeReference(new CodeDotnetTypeReference(typeof(UriKind))));
            }
        }
    }
}