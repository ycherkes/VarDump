using System;
using System.Net;
using VarDump.CodeDom.Compiler;
using VarDump.Extensions;

namespace VarDump.Visitor.KnownTypes;

internal sealed class IPAddressVisitor : IKnownObjectVisitor
{
    private readonly ICodeWriter _codeWriter;

    public IPAddressVisitor(ICodeWriter codeWriter)
    {
        _codeWriter = codeWriter;
    }

    public string Id => nameof(IPAddress);
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is IPAddress;
    }

    public void Visit(object obj, Type objectType)
    {
        var ipAddress = (IPAddress)obj;

        _codeWriter.WriteMethodInvoke(
            () => _codeWriter.WriteMethodReference(
                () => _codeWriter.WriteTypeReference(objectType), nameof(IPAddress.Parse)),
            [
                () => _codeWriter.WritePrimitive(ipAddress.ToString())
            ]);
    }
}