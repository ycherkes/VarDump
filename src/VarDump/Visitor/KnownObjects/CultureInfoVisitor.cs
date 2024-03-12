using System;
using System.Globalization;
using VarDump.CodeDom.Compiler;

namespace VarDump.Visitor.KnownObjects;

internal sealed class CultureInfoVisitor(ICodeWriter codeWriter, bool useNamedArgumentsInConstructors) : IKnownObjectVisitor
{
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is CultureInfo;
    }

    public void Visit(object obj, Type objectType, VisitContext context)
    {
        if (useNamedArgumentsInConstructors)
        {
            codeWriter.WriteObjectCreate(typeof(CultureInfo), [() => codeWriter.WriteNamedArgument("name", () => codeWriter.WritePrimitive(obj.ToString()))]);
        }
        else
        {
            codeWriter.WriteObjectCreate(typeof(CultureInfo), [() => codeWriter.WritePrimitive(obj.ToString())]);
        }
    }
}