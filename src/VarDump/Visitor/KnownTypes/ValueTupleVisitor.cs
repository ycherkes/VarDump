﻿using System;
using System.Linq;
using VarDump.CodeDom.Compiler;
using VarDump.Utils;

namespace VarDump.Visitor.KnownTypes;

internal sealed class ValueTupleVisitor(IObjectVisitor rootObjectVisitor, ICodeWriter codeWriter) : IKnownObjectVisitor
{
    public string Id => "ValueTuple";
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return objectType.IsValueTuple();
    }

    public void Visit(object obj, Type objectType)
    {
        var propertyValues = objectType.GetFields().Select(p => ReflectionUtils.GetValue(p, obj)).Select(v => (Action)(() => rootObjectVisitor.Visit(v)));

        codeWriter.WriteValueTupleCreate(propertyValues);
    }
}