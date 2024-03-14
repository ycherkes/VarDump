﻿using System;
using VarDump.CodeDom.Compiler;

namespace VarDump.Visitor.KnownObjects;

internal sealed class VersionVisitor(ICodeWriter codeWriter, DumpOptions options) : IKnownObjectVisitor
{
    public string Id => nameof(Version);

    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is Version;
    }

    public void ConfigureOptions(Action<DumpOptions> configure)
    {
        options = options.Clone();
        configure?.Invoke(options);
    }

    public void Visit(object obj, Type objectType, VisitContext context)
    {
        if (options.UseNamedArgumentsInConstructors)
        {
            codeWriter.WriteObjectCreate(typeof(Version), [() => codeWriter.WriteNamedArgument("version", WriteVersionString)]);
        }
        else
        {
            codeWriter.WriteObjectCreate(typeof(Version), [WriteVersionString]);
        }

        return;

        void WriteVersionString() => codeWriter.WritePrimitive(obj.ToString());
    }
}