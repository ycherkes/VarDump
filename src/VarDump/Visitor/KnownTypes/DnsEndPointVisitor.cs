using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using VarDump.CodeDom.Common;
using VarDump.CodeDom.Compiler;

namespace VarDump.Visitor.KnownTypes;

internal sealed class DnsEndPointVisitor : IKnownObjectVisitor
{
    private readonly IObjectVisitor _rootObjectVisitor;
    private readonly IDotnetCodeGenerator _codeGenerator;

    public DnsEndPointVisitor(IObjectVisitor rootObjectVisitor, IDotnetCodeGenerator codeGenerator)
    {
        _rootObjectVisitor = rootObjectVisitor;
        _codeGenerator = codeGenerator;
    }

    public string Id => nameof(DnsEndPoint);
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is DnsEndPoint;
    }

    public void Visit(object obj, Type objectType)
    {
        var dnsEndPoint = (DnsEndPoint)obj;

        _codeGenerator.GenerateObjectCreateAndInitialize(new CodeDotnetTypeReference(typeof(DnsEndPoint)), GetConstructorArguments(), []);

        IEnumerable<Action> GetConstructorArguments()
        {
            yield return () => _codeGenerator.GeneratePrimitive(dnsEndPoint.Host);
            yield return () => _codeGenerator.GeneratePrimitive(dnsEndPoint.Port);
            if (dnsEndPoint.AddressFamily != AddressFamily.Unspecified)
            {
                yield return () => _rootObjectVisitor.Visit(dnsEndPoint.AddressFamily);
            }
        }
    }
}