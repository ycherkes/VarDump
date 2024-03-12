using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using VarDump.CodeDom.Compiler;

namespace VarDump.Visitor.KnownObjects;

internal sealed class DnsEndPointVisitor(INextDepthVisitor nextDepthVisitor, ICodeWriter codeWriter, bool useNamedArgumentsInConstructors) : IKnownObjectVisitor
{
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is DnsEndPoint;
    }

    public void Visit(object obj, Type objectType, VisitContext context)
    {
        var dnsEndPoint = (DnsEndPoint)obj;

        var constructorArguments = useNamedArgumentsInConstructors
            ? GetNamedConstructorArguments()
            : GetConstructorArguments();

        codeWriter.WriteObjectCreate(typeof(DnsEndPoint), constructorArguments);
        return;

        IEnumerable<Action> GetConstructorArguments()
        {
            yield return () => codeWriter.WritePrimitive(dnsEndPoint.Host);
            yield return () => codeWriter.WritePrimitive(dnsEndPoint.Port);
            if (dnsEndPoint.AddressFamily != AddressFamily.Unspecified)
            {
                yield return () => nextDepthVisitor.Visit(dnsEndPoint.AddressFamily, context);
            }
        }

        IEnumerable<Action> GetNamedConstructorArguments()
        {
            yield return () => codeWriter.WriteNamedArgument("host", () => codeWriter.WritePrimitive(dnsEndPoint.Host));
            yield return () => codeWriter.WriteNamedArgument("port", () => codeWriter.WritePrimitive(dnsEndPoint.Port));
            if (dnsEndPoint.AddressFamily != AddressFamily.Unspecified)
            {
                yield return () => codeWriter.WriteNamedArgument("addressFamily", () => nextDepthVisitor.Visit(dnsEndPoint.AddressFamily, context));
            }
        }
    }
}