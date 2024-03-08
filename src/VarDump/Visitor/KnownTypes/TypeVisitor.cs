﻿using System;
using VarDump.CodeDom.Compiler;

namespace VarDump.Visitor.KnownTypes;

internal sealed class TypeVisitor(ICodeWriter codeWriter) : IKnownObjectVisitor
{
    public string Id => nameof(Type);

    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is Type;
    }

    public void Visit(object obj, Type objectType, VisitContext context)
    {
       codeWriter.WriteTypeOf((Type)obj);
    }
}