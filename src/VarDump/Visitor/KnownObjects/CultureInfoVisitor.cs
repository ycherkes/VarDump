using System;
using System.Globalization;
using VarDump.CodeDom.Compiler;

namespace VarDump.Visitor.KnownObjects;

internal sealed class CultureInfoVisitor(ICodeWriter codeWriter, DumpOptions dumpOptions) : IKnownObjectVisitor
{
    public string Id => nameof(CultureInfo);

    public DumpOptions Options => dumpOptions;

    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is CultureInfo;
    }

    public void Visit(object obj, Type objectType, VisitContext context)
    {
        if (dumpOptions.UseNamedArgumentsInConstructors)
        {
            codeWriter.WriteObjectCreate(typeof(CultureInfo), [() => codeWriter.WriteNamedArgument("name", () => codeWriter.WritePrimitive(obj.ToString()))]);
        }
        else
        {
            codeWriter.WriteObjectCreate(typeof(CultureInfo), [() => codeWriter.WritePrimitive(obj.ToString())]);
        }
    }
}