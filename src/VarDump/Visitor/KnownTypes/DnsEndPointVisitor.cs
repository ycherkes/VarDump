using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using VarDump.CodeDom.Compiler;
using VarDump.Extensions;

namespace VarDump.Visitor.KnownTypes;

internal sealed class DnsEndPointVisitor : IKnownObjectVisitor
{
    private readonly IObjectVisitor _rootObjectVisitor;
    private readonly ICodeWriter _codeWriter;

    public DnsEndPointVisitor(IObjectVisitor rootObjectVisitor, ICodeWriter codeWriter)
    {
        _rootObjectVisitor = rootObjectVisitor;
        _codeWriter = codeWriter;
    }

    public string Id => nameof(DnsEndPoint);
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is DnsEndPoint;
    }

    public void Visit(object obj, Type objectType)
    {
        var dnsEndPoint = (DnsEndPoint)obj;

        _codeWriter.WriteObjectCreateAndInitialize(typeof(DnsEndPoint), GetConstructorArguments(), []);

        IEnumerable<Action> GetConstructorArguments()
        {
            yield return () => _codeWriter.WritePrimitive(dnsEndPoint.Host);
            yield return () => _codeWriter.WritePrimitive(dnsEndPoint.Port);
            if (dnsEndPoint.AddressFamily != AddressFamily.Unspecified)
            {
                yield return () => _rootObjectVisitor.Visit(dnsEndPoint.AddressFamily);
            }
        }
    }
}