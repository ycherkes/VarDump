using System;
using System.Globalization;
using VarDump.CodeDom.Compiler;

namespace VarDump.Visitor.KnownTypes;

internal sealed class CultureInfoVisitor(ICodeWriter codeWriter) : IKnownObjectVisitor
{
    public string Id => nameof(CultureInfo);
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is CultureInfo;
    }

    public void Visit(object obj, Type objectType)
    {
        codeWriter.WriteObjectCreate(typeof(CultureInfo), [() => codeWriter.WritePrimitive(obj.ToString())]);
    }
}