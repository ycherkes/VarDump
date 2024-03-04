using System;
using VarDump.CodeDom.Compiler;
using VarDump.Extensions;

namespace VarDump.Visitor.KnownTypes;

internal sealed class GuidVisitor : IKnownObjectVisitor
{
    private readonly ICodeWriter _codeWriter;

    public GuidVisitor(ICodeWriter codeWriter)
    {
        _codeWriter = codeWriter;
    }

    public string Id => nameof(Guid);
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is Guid;
    }

    public void Visit(object obj, Type objectType)
    {
        var guid = (Guid)obj;

        _codeWriter.WriteObjectCreateAndInitialize(objectType,
            [() => _codeWriter.WritePrimitive(guid.ToString("D"))], 
            []);
    }
}