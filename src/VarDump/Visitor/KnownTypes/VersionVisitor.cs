using System;
using VarDump.CodeDom.Common;
using VarDump.CodeDom.Compiler;

namespace VarDump.Visitor.KnownTypes;

internal sealed class VersionVisitor : IKnownObjectVisitor
{
    private readonly ICodeGenerator _codeGenerator;

    public VersionVisitor(ICodeGenerator codeGenerator)
    {
        _codeGenerator = codeGenerator;
    }

    public string Id => nameof(Version);

    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is Version;
    }

    public void Visit(object obj, Type objectType)
    {
        var version  = (Version)obj;
        _codeGenerator.GenerateObjectCreateAndInitialize(new CodeTypeReference(typeof(Version)), 
            [
                () => _codeGenerator.GeneratePrimitive(version.ToString())
            ], []);
    }
}