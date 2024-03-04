using System;
using VarDump.CodeDom.Compiler;
using VarDump.Extensions;

namespace VarDump.Visitor.KnownTypes;

internal sealed class TypeVisitor : IKnownObjectVisitor
{
    private readonly ICodeWriter _codeWriter;
    public TypeVisitor(ICodeWriter codeWriter)
    {
        _codeWriter = codeWriter;
    }

    public string Id => nameof(Type);

    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is Type;
    }

    public void Visit(object obj, Type objectType)
    {
       _codeWriter.WriteTypeOf((Type)obj);
    }
}