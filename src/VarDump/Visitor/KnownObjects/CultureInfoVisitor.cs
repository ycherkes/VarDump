using System;
using System.Globalization;
using VarDump.CodeDom.Compiler;

namespace VarDump.Visitor.KnownObjects;

internal sealed class CultureInfoVisitor(ICodeWriter codeWriter) : IKnownObjectVisitor
{
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is CultureInfo;
    }

    public void Visit(object obj, Type objectType, VisitContext context)
    {
        codeWriter.WriteObjectCreate(typeof(CultureInfo), [() => codeWriter.WritePrimitive(obj.ToString())]);
    }
}