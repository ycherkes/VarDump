using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using VarDump.CodeDom.Compiler;

namespace VarDump.Visitor.KnownTypes;

internal sealed class DnsEndPointVisitor(IObjectVisitor rootObjectVisitor, ICodeWriter codeWriter) : IKnownObjectVisitor
{
    public string Id => nameof(DnsEndPoint);
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is DnsEndPoint;
    }

    public void Visit(object obj, Type objectType)
    {
        var dnsEndPoint = (DnsEndPoint)obj;

        codeWriter.WriteObjectCreate(typeof(DnsEndPoint), GetConstructorArguments());

        IEnumerable<Action> GetConstructorArguments()
        {
            yield return () => codeWriter.WritePrimitive(dnsEndPoint.Host);
            yield return () => codeWriter.WritePrimitive(dnsEndPoint.Port);
            if (dnsEndPoint.AddressFamily != AddressFamily.Unspecified)
            {
                yield return () => rootObjectVisitor.Visit(dnsEndPoint.AddressFamily);
            }
        }
    }
}