using System;
using VarDump.CodeDom.Compiler;
using VarDump.Extensions;

namespace VarDump.Visitor.KnownTypes;

internal sealed class VersionVisitor : IKnownObjectVisitor
{
    private readonly ICodeWriter _codeWriter;

    public VersionVisitor(ICodeWriter codeWriter)
    {
        _codeWriter = codeWriter;
    }

    public string Id => nameof(Version);

    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is Version;
    }

    public void Visit(object obj, Type objectType)
    {
        var version  = (Version)obj;
        _codeWriter.WriteObjectCreateAndInitialize(typeof(Version), 
            [
                () => _codeWriter.WritePrimitive(version.ToString())
            ], []);
    }
}