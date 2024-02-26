﻿using System;
using VarDump.CodeDom.Common;
using VarDump.CodeDom.Compiler;

namespace VarDump.Visitor.KnownTypes;

internal sealed class GuidVisitor : IKnownObjectVisitor
{
    private readonly ICodeGenerator _codeGenerator;

    public GuidVisitor(ICodeGenerator codeGenerator)
    {
        _codeGenerator = codeGenerator;
    }

    public string Id => nameof(Guid);
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is Guid;
    }

    public void Visit(object obj, Type objectType)
    {
        var guid = (Guid)obj;

        _codeGenerator.GenerateObjectCreateAndInitialize(new CodeTypeReference(typeof(Guid)),
            [
                () => _codeGenerator.GeneratePrimitive(guid.ToString("D"))
            ], 
            []);
    }
}