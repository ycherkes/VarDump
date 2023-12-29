using System;
using System.Net;
using System.Net.Sockets;
using VarDump.CodeDom.Common;

namespace VarDump.Visitor.KnownTypes;

internal sealed class DnsEndPointVisitor : IKnownObjectVisitor
{
    private readonly IObjectVisitor _rootObjectVisitor;
    private readonly CodeTypeReferenceOptions _typeReferenceOptions;

    public DnsEndPointVisitor(DumpOptions options, IObjectVisitor rootObjectVisitor)
    {
        _rootObjectVisitor = rootObjectVisitor;
        _typeReferenceOptions = options.UseTypeFullName
            ? CodeTypeReferenceOptions.FullTypeName
            : CodeTypeReferenceOptions.ShortTypeName;
    }

    public string Id => nameof(DnsEndPoint);
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is DnsEndPoint;
    }

    public CodeExpression Visit(object obj, Type objectType)
    {
        var dnsEndPoint = (DnsEndPoint)obj;
        return dnsEndPoint.AddressFamily == AddressFamily.Unspecified ?
            new CodeObjectCreateExpression(new CodeTypeReference(typeof(DnsEndPoint), _typeReferenceOptions),
                new CodePrimitiveExpression(dnsEndPoint.Host),
                new CodePrimitiveExpression(dnsEndPoint.Port))
            : new CodeObjectCreateExpression(new CodeTypeReference(typeof(DnsEndPoint), _typeReferenceOptions),
                new CodePrimitiveExpression(dnsEndPoint.Host),
                new CodePrimitiveExpression(dnsEndPoint.Port),
                _rootObjectVisitor.Visit(dnsEndPoint.AddressFamily));
    }
}