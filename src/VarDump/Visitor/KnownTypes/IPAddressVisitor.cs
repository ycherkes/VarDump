using System;
using System.Net;
using VarDump.CodeDom.Common;
using VarDump.CodeDom.Compiler;

namespace VarDump.Visitor.KnownTypes;

internal sealed class IPAddressVisitor : IKnownObjectVisitor
{
    private readonly IDotnetCodeGenerator _codeGenerator;

    public IPAddressVisitor(IDotnetCodeGenerator codeGenerator)
    {
        _codeGenerator = codeGenerator;
    }

    public string Id => nameof(IPAddress);
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is IPAddress;
    }

    public void Visit(object obj, Type objectType)
    {
        var ipAddress = (IPAddress)obj;

        _codeGenerator.GenerateMethodInvoke(
            () => _codeGenerator.GenerateMethodReference(
                () => _codeGenerator.GenerateTypeReference(new CodeDotnetTypeReference(typeof(IPAddress))), nameof(IPAddress.Parse)),
            [
                () => _codeGenerator.GeneratePrimitive(ipAddress.ToString())
            ]);
    }
}