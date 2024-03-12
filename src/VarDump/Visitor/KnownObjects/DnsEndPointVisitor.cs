using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using VarDump.CodeDom.Compiler;

namespace VarDump.Visitor.KnownObjects;

internal sealed class DnsEndPointVisitor(INextLevelVisitor nextLevelVisitor, ICodeWriter codeWriter) : IKnownObjectVisitor
{
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is DnsEndPoint;
    }

    public void Visit(object obj, Type objectType, VisitContext context)
    {
        var dnsEndPoint = (DnsEndPoint)obj;

        codeWriter.WriteObjectCreate(typeof(DnsEndPoint), GetConstructorArguments());
        return;

        IEnumerable<Action> GetConstructorArguments()
        {
            yield return () => codeWriter.WritePrimitive(dnsEndPoint.Host);
            yield return () => codeWriter.WritePrimitive(dnsEndPoint.Port);
            if (dnsEndPoint.AddressFamily != AddressFamily.Unspecified)
            {
                yield return () => nextLevelVisitor.Visit(dnsEndPoint.AddressFamily, context);
            }
        }
    }
}