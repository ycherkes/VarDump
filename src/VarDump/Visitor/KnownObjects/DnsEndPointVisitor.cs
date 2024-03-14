using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using VarDump.CodeDom.Compiler;

namespace VarDump.Visitor.KnownObjects;

internal sealed class DnsEndPointVisitor(INextDepthVisitor nextDepthVisitor, ICodeWriter codeWriter, DumpOptions options) : IKnownObjectVisitor
{
    public string Id => nameof(DnsEndPoint);

    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is DnsEndPoint;
    }

    public void ConfigureOptions(Action<DumpOptions> configure)
    {
        options = options.Clone();
        configure?.Invoke(options);
    }

    public void Visit(object obj, Type objectType, VisitContext context)
    {
        var dnsEndPoint = (DnsEndPoint)obj;

        var constructorArguments = options.UseNamedArgumentsInConstructors
            ? GetNamedConstructorArguments()
            : GetConstructorArguments();

        codeWriter.WriteObjectCreate(typeof(DnsEndPoint), constructorArguments);

        return;

        IEnumerable<Action> GetConstructorArguments()
        {
            yield return WriteHost;
            yield return WritePort;

            if (dnsEndPoint.AddressFamily != AddressFamily.Unspecified)
            {
                yield return WriteAddressFamily;
            }
        }

        IEnumerable<Action> GetNamedConstructorArguments()
        {
            yield return () => codeWriter.WriteNamedArgument("host", WriteHost);
            yield return () => codeWriter.WriteNamedArgument("port", WritePort);

            if (dnsEndPoint.AddressFamily != AddressFamily.Unspecified)
            {
                yield return () => codeWriter.WriteNamedArgument("addressFamily", WriteAddressFamily);
            }
        }

        void WriteHost() => codeWriter.WritePrimitive(dnsEndPoint.Host);
        void WritePort() => codeWriter.WritePrimitive(dnsEndPoint.Port);
        void WriteAddressFamily() => nextDepthVisitor.Visit(dnsEndPoint.AddressFamily, context);
    }
}