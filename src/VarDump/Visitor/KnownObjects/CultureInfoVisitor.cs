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
            codeWriter.WriteObjectCreate(objectType, [() => codeWriter.WriteNamedArgument("name", WriteCultureName)]);
        }
        else
        {
            codeWriter.WriteObjectCreate(objectType, [WriteCultureName]);
        }

        return;

        void WriteCultureName() => codeWriter.WritePrimitive(obj.ToString());
    }
}