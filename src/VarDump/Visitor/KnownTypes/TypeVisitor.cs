using System;
using VarDump.CodeDom.Common;
using VarDump.CodeDom.Compiler;

namespace VarDump.Visitor.KnownTypes;

internal sealed class TypeVisitor : IKnownObjectVisitor
{
    private readonly ICodeGenerator _codeGenerator;
    public TypeVisitor(ICodeGenerator codeGenerator)
    {
        _codeGenerator = codeGenerator;
    }

    public string Id => nameof(Type);

    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is Type;
    }

    public void Visit(object obj, Type objectType)
    {
        var type = (Type)obj;
        
        _codeGenerator.GenerateTypeOf(new CodeTypeReference(type));
    }
}