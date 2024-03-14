﻿using System;
using System.Net;
using VarDump.CodeDom.Compiler;

namespace VarDump.Visitor.KnownObjects;

internal sealed class IPAddressVisitor(ICodeWriter codeWriter) : IKnownObjectVisitor
{
    public string Id => nameof(IPAddress);

    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is IPAddress;
    }

    public void ConfigureOptions(Action<DumpOptions> configure)
    {
    }

    public void Visit(object obj, Type objectType, VisitContext context)
    {
        codeWriter.WriteMethodInvoke(
            () => codeWriter.WriteMethodReference(
                () => codeWriter.WriteType(objectType), nameof(IPAddress.Parse)),
            [
                () => codeWriter.WritePrimitive(obj.ToString())
            ]);
    }
}